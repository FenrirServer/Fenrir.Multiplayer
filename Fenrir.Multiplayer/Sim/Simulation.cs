using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Sim.Command;
using Fenrir.Multiplayer.Sim.Exceptions;
using Fenrir.Multiplayer.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Sim
{
    public class Simulation
    {
        /// <summary>
        /// Delegate that describes event when simulation creates an outgoing command
        /// </summary>
        /// <param name="command">Outgoing command</param>
        public delegate void SimulationCommandHandler(ISimulationCommand command);

        /// <summary>
        /// Invokes when simulation creates an outgoing command
        /// </summary>
        public event SimulationCommandHandler CommandCreated;

        /// <summary>
        /// Invokes when simulation executes a command
        /// </summary>
        public event SimulationCommandHandler CommandExecuted;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly IFenrirLogger _logger;

        /// <summary>
        /// Simulation clock
        /// </summary>
        private readonly Clock _clock = new Clock();

        /// <summary>
        /// Simulation objects by ushort id
        ///  TODO: Every time we do object lookups we have boxing because our key is ushort. TODO: Replace with a better structure
        /// </summary>
        private OrderedDictionary _objectsById = new OrderedDictionary();

        /// <summary>
        /// Global component type hash
        /// </summary>
        private TypeHashMap _componentTypeHashMap = new TypeHashMap();

        /// <summary>
        /// Registered component factory methods by component type
        /// </summary>
        private Dictionary<Type, Func<SimulationComponent>> _componentFactories = new Dictionary<Type, Func<SimulationComponent>>();

        /// <summary>
        /// Queue of actions scheduled for the next tick
        /// </summary>
        private Queue<Action> _actionQueue = new Queue<Action>();

        /// <summary>
        /// Log of all outgoing commands, regardless of when they were sent (immediately vs bulked)
        /// </summary>
        private Queue<ISimulationCommand> _outgoingCommandLog = new Queue<ISimulationCommand>();

        /// <summary>
        /// List of buffered incoming commands, before they are processed
        /// TODO: Use sorted list to avoid sorting on every tick
        /// </summary>
        private List<ISimulationCommand> _incomingCommandBuffer = new List<ISimulationCommand>();
        
        /// <summary>
        /// Time to keep outgoing commands in the log, in ms
        /// This is basically the maximum rollback time
        /// </summary>
        public int MaxRollbackTimeMs { get; set; } = 50;


        /// <summary>
        /// Incoming command delay, in ms
        /// Indicates for how long incoming commands are being buffered before processing.
        /// For client, this allows to render the state that is slightly older than server state, allowing to normalize everyone's viewpoint.
        /// </summary>
        public int IncomingCommandDelayMs { get; set; } = 50;

        /// <summary>
        /// Next object id, used to track incremented object ids
        /// </summary>
        private ushort _nextObjectId = 0;

        /// <summary>
        /// Indicates if simulation is being rolled back
        /// </summary>
        public bool IsRolledBack { get; private set; }

        /// <summary>
        /// Indicates current time of the simulation
        /// </summary>
        public DateTime CurrentTickTime { get; private set; }

        /// <summary>
        /// True if this simulation is authority
        /// </summary>
        public bool IsAuthority { get; set; }
        
        #region Constructor
        public Simulation(IFenrirLogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger;
        }
        #endregion

        #region Component Registration
        public void RegisterComponentType<TComponent>()
            where TComponent : SimulationComponent, new()
        {
            // Register component using a primitive factory, that uses empty constructor to create a component
            RegisterComponentType<TComponent>(() => 
            {
                return new TComponent();
            });
        }

        public void RegisterComponentType<TComponent>(Func<TComponent> factoryMethod)
            where TComponent : SimulationComponent
        {
            // Register component type hash
            _componentTypeHashMap.AddType<TComponent>();

            // Register component factory
            _componentFactories.Add(typeof(TComponent), factoryMethod);
        }

        public bool ComponentRegistered<TComponent>()
            where TComponent : SimulationComponent
        {
            return ComponentRegistered(typeof(TComponent));
        }

        public bool ComponentRegistered(Type componentType)
        {
            return _componentTypeHashMap.HasTypeHash(componentType);
        }
        #endregion

        #region Object Management

        public SimulationObject SpawnObject()
        {
            if (!IsAuthority)
            {
                throw new SimulationException("Client simulation is not allowed to spawn objects directly. Please invoke server RPC using a component");
            }

            ushort objectId = GetNextObjectId();

            // TODO: Use command object pool
            var command = new SpawnObjectSimulationCommand(DateTime.UtcNow, objectId);

            // Execute command on self
            SimulationObject simObject = ExecuteSpawnObjectCommand(command);

            // Replicate to other simulations
            SendOutgoingCommand(command);

            return simObject;
        }

        private SimulationObject ExecuteSpawnObjectCommand(SpawnObjectSimulationCommand command)
        {
            // Create new object
            SimulationObject simObject = new SimulationObject(this, _logger, command.ObjectId);

            // Add to the list
            _objectsById.Add(command.ObjectId, simObject);

            // Return it
            return simObject;
        }

        public void DestroyObject(SimulationObject obj)
        {
            if (!IsAuthority)
            {
                throw new SimulationException("Client simulation is not allowed to remove objects directly. Please invoke server RPC using a component");
            }

            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            // TODO: Use command object pool
            var command = new DestroyObjectSimulationCommand(DateTime.UtcNow, obj.Id);

            // Execute command on self
            ExecuteDestroyObjectCommand(command);

            // Replicate to other simulations
            SendOutgoingCommand(command);
        }

        private void ExecuteDestroyObjectCommand(DestroyObjectSimulationCommand command)
        {
            if (!TryGetObject(command.ObjectId, out SimulationObject simObject))
            {
                throw new SimulationException($"Failed to remove object {command.ObjectId} from simulation, object not found in the simulation");
            }

            _objectsById.Remove(simObject.Id);
        }

        public void DestroyObject(ushort objectId)
        {
            if (!IsAuthority)
            {
                throw new SimulationException("Client simulation is not allowed to remove objects directly. Please invoke server RPC using a component");
            }

            if (!TryGetObject(objectId, out SimulationObject simObject))
            {
                throw new SimulationException($"Failed to remove object {objectId} from simulation, object not found in the simulation");
            }

            DestroyObject(simObject);
        }

        public IEnumerable<SimulationObject> GetObjects()
        {
            IDictionaryEnumerator objectEnumerator = _objectsById.GetEnumerator();

            while (objectEnumerator.MoveNext())
            {
                SimulationObject simObject = (SimulationObject)objectEnumerator.Value;
                
                if(IsRolledBack && simObject.TimeCreated > CurrentTickTime)
                {
                    // Rolling back simulation and object was created after our timestamp, skip
                    continue;
                }

                yield return simObject;
            }
        }

        public SimulationObject GetObject(ushort objectId)
        {
            if(!TryGetObject(objectId, out SimulationObject simObject))
            {
                return null;
            }

            return simObject;
        }

        public bool TryGetObject(ushort objectId, out SimulationObject simObject)
        {
            simObject = null;

            if (!_objectsById.Contains(objectId))
            {
                return false;
            }

            simObject = (SimulationObject)_objectsById[(object)objectId];
            return true;
        }

        public bool HasObject(ushort objectId)
        {
            return _objectsById.Contains(objectId);
        }
        #endregion

        #region Component Management
        internal TComponent AddComponent<TComponent>(SimulationObject simObject)
            where TComponent : SimulationComponent
        {
            if (!IsAuthority)
            {
                throw new SimulationException("Client simulation is not allowed to add components directly. Please invoke server RPC using a component method");
            }

            if (simObject == null)
            {
                throw new ArgumentNullException(nameof(simObject));
            }

            if(!ComponentRegistered<TComponent>())
            {
                throw new SimulationException($"Failed to add component {typeof(TComponent).Name}, component is not registered");
            }

            // Get component type hash
            ulong componentTypeHash = GetComponentTypeHash<TComponent>();

            // TODO: Use command object pool
            var command = new AddComponentSimulationCommand(DateTime.UtcNow, simObject.Id, componentTypeHash);

            // Execute command on self simulation
            TComponent component = (TComponent)ExecuteAddComponentCommand(command);

            // Replicate to other simulations
            SendOutgoingCommand(command);

            return component;
        }

        private SimulationComponent ExecuteAddComponentCommand(AddComponentSimulationCommand command)
        {
            // Try to get the object
            if(!TryGetObject(command.ObjectId, out SimulationObject simObject))
            {
                throw new ArgumentException($"Failed to add component with hash {command.ComponentTypeHash}, component type is not registered with Simulation. Please call {nameof(Simulation.RegisterComponentType)}");
            }

            // Try to get component type
            if (!_componentTypeHashMap.TryGetTypeByHash(command.ComponentTypeHash, out Type componentType))
            {
                throw new ArgumentException($"Failed to add component with hash {command.ComponentTypeHash}, component type is not registered with Simulation. Please call {nameof(Simulation.RegisterComponentType)}");
            }

            // Get component factory method
            if(!_componentFactories.TryGetValue(componentType, out Func<SimulationComponent> factoryMethod))
            {
                throw new ArgumentException($"Failed to create component of type {componentType.Name}, component factry is not registered with Simulation. Please call {nameof(Simulation.RegisterComponentType)}");
            }

            // Create new component instance
            SimulationComponent component = factoryMethod.Invoke();

            // Add component
            simObject.AddComponent(component, componentType);

            return component;
        }


        internal void RemoveComponent<TComponent>(SimulationObject simObject)
            where TComponent : SimulationComponent
        {
            if (!IsAuthority)
            {
                throw new SimulationException("Client simulation is not allowed to add components directly. Please invoke server RPC using a component method");
            }

            // Get component type hash
            ulong componentTypeHash = GetComponentTypeHash<TComponent>();

            // TODO: Use command object pool
            var command = new RemoveComponentSimulationCommand(DateTime.UtcNow, simObject.Id, componentTypeHash);

            // Execute command on self simulation
            ExecuteRemoveComponentCommand(command);

            // Replicate to other simulations
            SendOutgoingCommand(command);
        }

        private void ExecuteRemoveComponentCommand(RemoveComponentSimulationCommand command)
        {
            // Try to get the object
            if (!TryGetObject(command.ObjectId, out SimulationObject simObject))
            {
                throw new ArgumentException($"Failed to add component with hash {command.ComponentTypeHash}, component type is not registered with Simulation. Please call {nameof(Simulation.RegisterComponentType)}");
            }

            // Try to get component type
            if (!_componentTypeHashMap.TryGetTypeByHash(command.ComponentTypeHash, out Type componentType))
            {
                throw new ArgumentException($"Failed to add component with hash {command.ComponentTypeHash}, component type is not registered with Simulation. Please call {nameof(Simulation.RegisterComponentType)}");
            }

            // Remove component
            simObject.RemoveComponent(componentType);
        }
        #endregion

        #region Tick
        public virtual void Tick()
        {
            // Set tick time
            CurrentTickTime = _clock.UtcNow;

            // Check if we can tick simulation. If simulation is being rolled back for reconciliation, we can't tick.
            if(IsRolledBack)
            {
                throw new SimulationException("Can not tick simulation while simulation is being rolled back.");
            }

            // Process incoming commands sorted by time. We sort descending because it's O(1) to remove element from the end
            // TODO: This is shit, use sorted list data structure / linked list?
            lock (_incomingCommandBuffer)
            {
                // Sort commands by time, descending (newer elements in the end)
                _incomingCommandBuffer.Sort((cmd1, cmd2) => cmd2.Time.CompareTo(cmd1.Time));

                // Iterate over commands that are OLDER than sim time + delay
                for (int i = _incomingCommandBuffer.Count - 1; i >= 0; i--)
                {
                    ISimulationCommand incomingCommand = _incomingCommandBuffer[i];
                    if (DateTime.UtcNow < incomingCommand.Time + TimeSpan.FromMilliseconds(IncomingCommandDelayMs))
                    {
                        break; // This command and all commands after it are too new for us to process
                    }

                    // Enqueue command for execution during this tick
                    EnqueueAction(() => ProcessIncomingCommand(incomingCommand));

                    // Remove current element from the end
                    _incomingCommandBuffer.RemoveAt(_incomingCommandBuffer.Count - 1);
                }
            }

            // Tick enqueued actions and commands
            while (TryDequeueAction(out Action action))
            {
                action.Invoke();
            }

            // Tick simulation objects
            IDictionaryEnumerator objectEnumerator = _objectsById.GetEnumerator();

            while (objectEnumerator.MoveNext())
            {
                SimulationObject simObject = (SimulationObject)objectEnumerator.Value;
                simObject.Tick();
            }


            // Trim the rollback command log
            TrimOutgoingCommandLog();
        }

        private void TrimOutgoingCommandLog()
        {
            // Remove commands from log older than rollback time
            ISimulationCommand command;

            while(_outgoingCommandLog.Count > 0)
            {
                command = _outgoingCommandLog.Peek();
                if(DateTime.UtcNow > command.Time + TimeSpan.FromMilliseconds(MaxRollbackTimeMs))
                {
                    _outgoingCommandLog.Dequeue();
                    // TODO: Release the command. Instead of going straight to GC, return to object pool here.
                }
                else
                {
                    break; // No more old commands, current command (and all after) are new
                }
            }
        }

        private bool TryDequeueAction(out Action action)
        {
            action = null;

            lock (_actionQueue)
            {
                if (_actionQueue.Count > 0)
                {
                    action = _actionQueue.Dequeue();
                    return true;
                }
            }

            return false;
        }

        public void EnqueueAction(Action action)
        {
            if(action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock(_actionQueue)
            {
                _actionQueue.Enqueue(action);
            }
        }

        public async void ScheduleAction(Action action, int delayMs)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            await Task.Delay(delayMs);
            EnqueueAction(action);
        }

        #endregion

        #region Command Processing
        internal void SendOutgoingCommand(ISimulationCommand command)
        {
            // If this is an authority, log command to be able to roll back
            if (IsAuthority)
            {
                _outgoingCommandLog.Enqueue(command);
            }

            // Produce outgoing command
            CommandCreated?.Invoke(command);
        }

        public void IngestCommand(ISimulationCommand command)
        {
            // This method can be executed outside of tick, to schedule client commands to be processed on the next tick

            lock(_incomingCommandBuffer)
            {
                _incomingCommandBuffer.Add(command);
            }
        }

        public void ProcessIncomingCommand(ISimulationCommand command)
        {
            // Process command based on it's type
            switch (command.Type)
            {
                case CommandType.SpawnObject:
                    ExecuteSpawnObjectCommand((SpawnObjectSimulationCommand)command);
                    break;
                case CommandType.DestroyObject:
                    ExecuteDestroyObjectCommand((DestroyObjectSimulationCommand)command);
                    break;
                case CommandType.AddComponent:
                    ExecuteAddComponentCommand((AddComponentSimulationCommand)command);
                    break;
                case CommandType.RemoveComponent:
                    ExecuteRemoveComponentCommand((RemoveComponentSimulationCommand)command);
                    break;
                case CommandType.InvokeRpc:
                    break; // TODO
                case CommandType.SetComponentState:
                    break; // TODO
            }

            CommandExecuted?.Invoke(command);
        }

        #endregion

        #region ExecuteRollBack
        public void ExecuteRollBack(DateTime time, Action callback)
        {
            DateTime currentTickTime = CurrentTickTime;
            CurrentTickTime = time;
            IsRolledBack = true;

            // TODO: Restore state by traversing outgoing command buffer

            try
            {
                callback();
            }
            finally
            {
                CurrentTickTime = currentTickTime;
                IsRolledBack = false;
            }
        }

        #endregion

        #region Clock
        /// <summary>
        /// Sets simulation clock offset
        /// </summary>
        public void SetClockOffset(TimeSpan offset)
        {
            _clock.Offset = offset;
        }
        #endregion

        #region Utility Methods
        protected ushort GetNextObjectId()
        {
            if (_objectsById.Count == ushort.MaxValue)
            {
                throw new SimulationException($"Failed to create Simulation Object Id, has reached max number of simulation objects: {_objectsById.Count}");
            }

            // Find next unused objectid
            int maxId = _nextObjectId - 1;
            do
            {
                if (!_objectsById.Contains(_nextObjectId))
                {
                    return _nextObjectId;
                }

                _nextObjectId++;
            }
            while (_nextObjectId != maxId);

            // This should not happen because of the check above
            throw new SimulationException($"Failed to create Simulation Object Id, total number of objects: {_objectsById.Count}");
        }

        internal ulong GetComponentTypeHash<TComponent>()
            where TComponent : SimulationComponent
        {
            return GetComponentTypeHash(typeof(TComponent));
        }

        internal ulong GetComponentTypeHash(Type componentType)
        {
            if(componentType == null)
            {
                throw new ArgumentNullException(nameof(componentType));
            }

            return _componentTypeHashMap.GetTypeHash(componentType);
        }

        internal Type GetComponentTypeByHash(ulong hash)
        {
            return _componentTypeHashMap.GetTypeByHash(hash);
        }
        #endregion
    }
}
