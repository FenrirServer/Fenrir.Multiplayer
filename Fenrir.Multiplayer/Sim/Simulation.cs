using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Sim.Command;
using Fenrir.Multiplayer.Sim.Data;
using Fenrir.Multiplayer.Sim.Exceptions;
using Fenrir.Multiplayer.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Sim
{
    public partial class Simulation
    {
        /// <summary>
        /// Delegate that describes event when simulation creates an outgoing command
        /// </summary>
        /// <param name="command">Outgoing command</param>
        public delegate void SimulationCommandHandler(ISimulationCommand command);

        /// <summary>
        /// Delegate that describes event when simulation tick processes snapshot
        /// </summary>
        public delegate void SimulationSnapshotProcessedHandler(SimulationTickSnapshot snapshot);

        /// <summary>
        /// Invokes when simulation creates an outgoing command
        /// </summary>
        public event SimulationCommandHandler CommandCreated;

        /// <summary>
        /// Invokes when simulation executes a command
        /// </summary>
        public event SimulationCommandHandler CommandExecuted;

        /// <summary>
        /// Invoked when simulation finishes it's tick
        /// </summary>
        public event SimulationSnapshotProcessedHandler TickSnapshotProcessed;

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
        /// Component helper map. 
        /// Container component helpers used to cache component members, RPCS etc
        /// </summary>
        private Dictionary<Type, ComponentTypeWrapper> _componentTypeWrappers = new Dictionary<Type, ComponentTypeWrapper>();

        /// <summary>
        /// Registered component factory methods by component type
        /// </summary>
        private Dictionary<Type, Func<SimulationComponent>> _componentFactories = new Dictionary<Type, Func<SimulationComponent>>();

        /// <summary>
        /// Queue of actions scheduled for the next tick
        /// </summary>
        private Queue<Action> _actionQueue = new Queue<Action>();

        /// <summary>
        /// Queue of actions scheduled for the next late tick
        /// </summary>
        private Queue<Action> _lateActionQueue = new Queue<Action>();

        /// <summary>
        /// Stores current snapshot that's being built during the tick
        /// </summary>
        private SimulationTickSnapshot _currentTickSnapshot = null;

        /// <summary>
        /// Snapshot history, allowing to roll back (used by server sim)
        /// </summary>
        private Queue<SimulationTickSnapshot> _snapshotHistory = new Queue<SimulationTickSnapshot>();

        /// <summary>
        /// Incoming tick snapshots (used by client sim)
        /// </summary>
        private Queue<SimulationTickSnapshot> _incomingSnapshotBuffer = new Queue<SimulationTickSnapshot>();

        /// <summary>
        /// Tick time of the snapshot tick time. Same as _incomingSnapshotBuffer.Last().TickTime
        /// </summary>
        private DateTime _lastIncomingSnapshotTickTime;

        /// <summary>
        /// Time to keep outgoing commands in the log, in ms
        /// This is basically the maximum rollback time
        /// </summary>
        public int MaxRollbackTimeMs { get; set; } = 50;


        /// <summary>
        /// Incoming command delay, in milliseconds. AKA client interpolation time
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
        /// True if simulation is not currently processing a tick
        /// </summary>
        public bool InTick { get; private set; }

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
            if(ComponentRegistered<TComponent>())
            {
                throw new SimulationException($"Component {typeof(TComponent).Name} is already registered");
            }

            if(factoryMethod == null)
            {
                throw new ArgumentNullException(nameof(factoryMethod));
            }

            // Register component type hash
            _componentTypeHashMap.AddType<TComponent>();

            // Register component helper
            var componentTypeHelper = new ComponentTypeWrapper(typeof(TComponent));
            _componentTypeWrappers.Add(typeof(TComponent), componentTypeHelper);

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

        internal ComponentTypeWrapper GetComponentWrapper(Type componentType)
        {
            return _componentTypeWrappers[componentType];
        }
        #endregion

        #region Object Management

        public SimulationObject SpawnObject()
        {
            // Checks
            CheckAuthority();
            CheckInTick();

            ushort objectId = GetNextObjectId();

            // TODO: Use command object pool
            var command = new SpawnObjectSimulationCommand(objectId);

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
            // Checks
            CheckAuthority();
            CheckInTick();

            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            // TODO: Use command object pool
            var command = new DestroyObjectSimulationCommand(obj.Id);

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
            // Checks
            CheckAuthority();
            CheckInTick();

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

                if (IsRolledBack && simObject.TimeCreated > CurrentTickTime)
                {
                    // Rolling back simulation and object was created after our timestamp, skip
                    continue;
                }

                yield return simObject;
            }
        }

        public SimulationObject GetObject(ushort objectId)
        {
            if (!TryGetObject(objectId, out SimulationObject simObject))
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
            // Checks
            CheckAuthority();
            CheckInTick();

            if (simObject == null)
            {
                throw new ArgumentNullException(nameof(simObject));
            }

            if (!ComponentRegistered<TComponent>())
            {
                throw new SimulationException($"Failed to add component {typeof(TComponent).Name}, component is not registered");
            }

            // Get component type hash
            ulong componentTypeHash = GetComponentTypeHash<TComponent>();

            // TODO: Use command object pool
            var command = new AddComponentSimulationCommand(simObject.Id, componentTypeHash);

            // Execute command on self simulation
            TComponent component = (TComponent)ExecuteAddComponentCommand(command);

            // Replicate to other simulations
            SendOutgoingCommand(command);

            return component;
        }

        private SimulationComponent ExecuteAddComponentCommand(AddComponentSimulationCommand command)
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

            // Get component factory method
            if (!_componentFactories.TryGetValue(componentType, out Func<SimulationComponent> factoryMethod))
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
            // Checks
            CheckAuthority();
            CheckInTick();

            // Get component type hash
            ulong componentTypeHash = GetComponentTypeHash<TComponent>();

            // TODO: Use command object pool
            var command = new RemoveComponentSimulationCommand(simObject.Id, componentTypeHash);

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

        #region Rpc

        internal void InvokeClientRpc(SimulationComponent component, ulong methodHash, params object[] parameters)
        {
            // Checks
            CheckAuthority();
            CheckInTick();

            // Get component type hash
            ulong componentTypeHash = GetComponentTypeHash(component.GetType());

            // TODO: Use command object pool
            var command = new ServerRpcSimulationCommand(component.Object.Id, componentTypeHash, methodHash);

            // Replicate to other simulations
            SendOutgoingCommand(command);
        }
        #endregion


        #region Tick
        public virtual void Tick()
        {
            if(InTick)
            {
                _logger.Warning($"Called {nameof(Simulation.Tick)} while already processing a tick. Did previous tick take too long?");
                return;
            }

            // Set tick time
            CurrentTickTime = _clock.UtcNow;

            InTick = true;
            TickInternal();
            InTick = false;
        }

        private void TickInternal()
        {
            // Check if we can tick simulation. If simulation is being rolled back for reconciliation, we can't tick.
            if (IsRolledBack)
            {
                throw new SimulationException("Can not tick simulation while simulation is being rolled back.");
            }

            // Authority and using snapshot history, create new slice TODO use object pool
            if (IsAuthority)
            {
                _currentTickSnapshot = new SimulationTickSnapshot(this) { TickTime = CurrentTickTime };
            }
            else // If not authority, dispatch incoming snapshots
            {
                lock (_incomingSnapshotBuffer)
                {
                    // Iterate over snapshots that are OLDER than sim time + delay
                    while (_incomingSnapshotBuffer.Count > 0)
                    {
                        if (DateTime.UtcNow < _incomingSnapshotBuffer.Peek().TickTime + TimeSpan.FromMilliseconds(IncomingCommandDelayMs))
                        {
                            break; // Next snapshot and all snapshots after it are not ready yet to be dispatched
                        }

                        // Dispatch snapshot
                        SimulationTickSnapshot snapshot = _incomingSnapshotBuffer.Dequeue();

                        // Enqueue command for execution during this tick
                        EnqueueAction(() => ProcessTickSnapshot(snapshot));
                    }
                }
            }

            // Tick enqueued actions and incoming commands
            while (TryDequeueAction(out Action action))
            {
                try
                {
                    action.Invoke();
                }
                catch(Exception e)
                {
                    _logger.Error("Error during simulation action: " + e.ToString());
                }
            }

            // Tick simulation objects
            IDictionaryEnumerator objectEnumerator = _objectsById.GetEnumerator();

            while (objectEnumerator.MoveNext())
            {
                SimulationObject simObject = (SimulationObject)objectEnumerator.Value;

                try
                {
                    simObject.Tick();
                }
                catch(Exception e)
                {
                    _logger.Error($"Error during object {simObject.Id} Tick: " + e.ToString());
                }
            }

            // Late tick actions
            while (TryDequeueLateAction(out Action action))
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    _logger.Error("Error during simulation late action: " + e.ToString());
                }
            }

            // Late tick simulation objects
            objectEnumerator = _objectsById.GetEnumerator();

            while (objectEnumerator.MoveNext())
            {
                SimulationObject simObject = (SimulationObject)objectEnumerator.Value;

                try
                {
                    simObject.LateTick();
                }
                catch (Exception e)
                {
                    _logger.Error($"Error during object {simObject.Id} Late Tick: " + e.ToString());
                }
            }

            // Snapshot history
            if (IsAuthority)
            {
                // Save the snapshot of this tick
                _snapshotHistory.Enqueue(_currentTickSnapshot);
                _currentTickSnapshot = null;

                // Trim the rollback snapshot log
                TrimSnapshotHistory();
            }
        }

        private void TrimSnapshotHistory()
        {
            // Remove snapshots from history older than rollback time
            SimulationTickSnapshot snapshot;

            while (_snapshotHistory.Count > 0)
            {
                snapshot = _snapshotHistory.Peek();
                if (DateTime.UtcNow > snapshot.TickTime + TimeSpan.FromMilliseconds(MaxRollbackTimeMs))
                {
                    _snapshotHistory.Dequeue();
                    // TODO: Release the snapshot. Instead of going straight to GC, return to object pool here.
                }
                else
                {
                    break; // No more old snapshots
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

        private bool TryDequeueLateAction(out Action action)
        {
            action = null;

            lock (_lateActionQueue)
            {
                if (_lateActionQueue.Count > 0)
                {
                    action = _lateActionQueue.Dequeue();
                    return true;
                }
            }

            return false;
        }

        public void EnqueueAction(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (_actionQueue)
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

        public void EnqueueLateAction(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (_lateActionQueue)
            {
                _lateActionQueue.Enqueue(action);
            }
        }

        public async void ScheduleLateAction(Action action, int delayMs)
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
            // If this is an authority, log command in this snapshot
            if (IsAuthority)
            {
                _currentTickSnapshot.Commands.Add(command);
            }

            // Produce outgoing command
            CommandCreated?.Invoke(command);
        }

        public void IngestTickSnapshot(SimulationTickSnapshot snapshot)
        {
            lock(_incomingSnapshotBuffer)
            {
                // Make sure we do not ingest the same snapshot twice,
                // as server will attempt to send those until it receives an ack 

                if (snapshot.TickTime > _lastIncomingSnapshotTickTime)
                {
                    _incomingSnapshotBuffer.Enqueue(snapshot);
                    _lastIncomingSnapshotTickTime = snapshot.TickTime;
                }
            }
        }

        private void ProcessTickSnapshot(SimulationTickSnapshot snapshot)
        {
            foreach (ISimulationCommand command in snapshot.Commands)
            {
                ExecuteCommand(command);
            }

            TickSnapshotProcessed?.Invoke(snapshot);
        }

        private void ExecuteCommand(ISimulationCommand command)
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
                case CommandType.ServerRpc:
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
        private void CheckInTick()
        {
            if(!InTick)
            {
                throw new NotInTickException($"Simulation is not currently executing a Tick(). Call {nameof(Simulation.EnqueueAction)} to run an action during next tick.");
            }
        }

        private void CheckAuthority()
        {
            if (!IsAuthority)
            {
                throw new SimulationException($"Action is not allowed for non-authority simulation. Invoke comonent RPC to execute this action.");
            }
        }

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

        internal bool TryGetComponentTypeByHash(ulong hash, out Type type)
        {
            return _componentTypeHashMap.TryGetTypeByHash(hash, out type);
        }
        #endregion
    }
}
