using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Simulation.Command;
using Fenrir.Multiplayer.Simulation.Data;
using Fenrir.Multiplayer.Simulation.Exceptions;
using Fenrir.Multiplayer.Simulation.Serialization;
using Fenrir.Multiplayer.Utility;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Simulation
{
    public class NetworkSimulation
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
        private readonly ILogger _logger;

        /// <summary>
        /// Network serializer
        /// </summary>
        private readonly INetworkSerializer _serializer;

        /// <summary>
        /// Simulation clock
        /// </summary>
        private readonly Clock _clock;

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
        /// Incoming tick snapshots
        /// </summary>
        private ConcurrentQueue<SimulationTickSnapshot> _incomingSnapshotBuffer = new ConcurrentQueue<SimulationTickSnapshot>();

        /// <summary>
        /// Tick number of the last incoming authority snapshot. Same as _incomingSnapshotBuffer.Last().TickNumber
        /// </summary>
        public uint LastIngestedSnapshotTickNumber { get; private set; }

        /// <summary>
        /// Ticm number of the last incoming authority snapshot that was processed
        /// </summary>
        public uint LastProcessedSnapshotTickNumber { get; private set; }

        /// <summary>
        /// Next object id, used to track incremented object ids
        /// </summary>
        private ushort _nextObjectId = 0;

        /// <summary>
        /// Rollback buffer size
        /// How many ticks we can roll back to
        /// </summary>
        public int RollbackBufferSize { get; set; } = 16;

        /// <summary>
        /// Incoming snapshot interpolation buffer size
        /// How much incoming snapshots we buffer before processing
        /// </summary>
        public int IncomingSnapshotBufferSize { get; set; } = 1;

        /// <summary>
        /// Indicates if simulation is being rolled back
        /// </summary>
        public bool IsRolledBack { get; private set; }

        /// <summary>
        /// True if simulation is not currently processing a tick
        /// </summary>
        public bool InTick { get; private set; }

        /// <summary>
        /// True if this simulation is authority
        /// </summary>
        public bool IsAuthority { get; set; }

        /// <summary>
        /// Current tick number
        /// </summary>
        public uint CurrentTickNumber { get; set; }

        /// <summary>
        /// Current tick time
        /// </summary>
        public DateTime CurrentTickTime { get; set; }

        /// <summary>
        /// Simulation tick rate
        /// </summary>
        public int TickRate { get; set; } = 60;

        /// <summary>
        /// Time per tick
        /// </summary>
        public TimeSpan TimePerTick => TimeSpan.FromMilliseconds(1000d / TickRate);

        /// <summary>
        /// Simulation clock time
        /// </summary>
        public DateTime ClockTime => _clock.UtcNow;

        /// <summary>
        /// Clock offset
        /// </summary>
        public TimeSpan ClockOffset
        {
            get => _clock.Offset;
            set => _clock.Offset = value;
        }

        /// <summary>
        /// Serializer
        /// </summary>
        internal INetworkSerializer Serializer => _serializer;

        /// <summary>
        /// Tick serializer
        /// </summary>
        internal SimulationTickSnapshotSerializer TickSerializer { get; private set; }

        #region Constructor
        public NetworkSimulation(ILogger logger)
            : this(logger, new NetworkSerializer())
        {
        }

        public NetworkSimulation(ILogger logger, INetworkSerializer serializer)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger;
            _clock = new Clock();

            TickSerializer = new SimulationTickSnapshotSerializer(this);
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
                throw new ArgumentException($"Failed to remove component from object with id {command.ObjectId}, Object Id not found");
            }

            // Try to get component type
            if (!_componentTypeHashMap.TryGetTypeByHash(command.ComponentTypeHash, out Type componentType))
            {
                throw new ArgumentException($"Failed to add component with hash {command.ComponentTypeHash}, component type is not registered with Simulation. Please call {nameof(NetworkSimulation.RegisterComponentType)}");
            }

            // Get component factory method
            if (!_componentFactories.TryGetValue(componentType, out Func<SimulationComponent> factoryMethod))
            {
                throw new ArgumentException($"Failed to create component of type {componentType.Name}, component factry is not registered with Simulation. Please call {nameof(NetworkSimulation.RegisterComponentType)}");
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
                throw new ArgumentException($"Failed to remove component from object with id {command.ObjectId}, Object Id not found");
            }

            // Try to get component type
            if (!_componentTypeHashMap.TryGetTypeByHash(command.ComponentTypeHash, out Type componentType))
            {
                throw new ArgumentException($"Failed to remove component with hash {command.ComponentTypeHash}, component type is not registered with Simulation. Please call {nameof(NetworkSimulation.RegisterComponentType)}");
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

            // Pre-process parameters (replace object references with ids)
            for (int numParam = 0; numParam < parameters.Length; numParam++)
            {
                object parameter = parameters[numParam];
                Type parameterType = parameter.GetType();

                // If type is a simulation object reference, replace with object id
                if (typeof(SimulationObject) == parameterType)
                {
                    SimulationObject simulationObject = (SimulationObject)parameter;
                    parameters[numParam] = simulationObject.Id;
                    return;
                }
                // If type is a simulation component reference, replace with component reference
                else if (typeof(SimulationComponent).IsAssignableFrom(parameterType))
                {
                    SimulationComponent simulationComponent = (SimulationComponent)parameter;
                    parameters[numParam] = new ComponentReference(simulationComponent);
                    return;
                }

            }

            // TODO: Use command object pool
            var command = new ClientRpcSimulationCommand(component.Object.Id, componentTypeHash, methodHash);

            // Replicate to other simulations
            SendOutgoingCommand(command);
        }

        private void ExecuteClientRpcCommand(ClientRpcSimulationCommand command)
        {
            // Try to get the object
            if (!TryGetObject(command.ObjectId, out SimulationObject simObject))
            {
                throw new ArgumentException($"Failed to execute RPC for object {command.ObjectId}, Object Id not found");
            }

            // Try to get component type
            if (!_componentTypeHashMap.TryGetTypeByHash(command.ComponentTypeHash, out Type componentType))
            {
                throw new ArgumentException($"Failed to execute RPC for component with hash {command.ComponentTypeHash}, component type is not registered with Simulation. Please call {nameof(NetworkSimulation.RegisterComponentType)}");
            }

            // Try to get component
            if(!simObject.TryGetComponent(componentType, out SimulationComponent component))
            {
                throw new ArgumentException($"Failed to execute RPC for component {componentType.Name}, object {simObject.Id} does not have component of a given type");
            }

            // Try to find component wrapper
            if(!_componentTypeWrappers.TryGetValue(componentType, out ComponentTypeWrapper componentTypeWrapper))
            {
                // should never happen
                throw new ArgumentException($"Failed to execute RPC for component with hash {command.ComponentTypeHash}, component type is not registered with Simulation. Please call {nameof(NetworkSimulation.RegisterComponentType)}");
            }

            componentTypeWrapper.TryInvokeClientRpc(component, command.MethodHash, command.Parameters);
        }
        #endregion

        #region Tick
        public virtual void Tick()
        {
            if(InTick)
            {
                _logger.Warning($"Called {nameof(NetworkSimulation.Tick)} while already processing a tick. Did previous tick take too long?");
                return;
            }

            InTick = true;

            try
            {
                TickInternal();
            }
            finally
            {
                InTick = false;
            }
        }

        private void TickInternal()
        {
            // Check if we can tick simulation. If simulation is being rolled back for reconciliation, we can't tick.
            if (IsRolledBack)
            {
                throw new SimulationException("Can not tick simulation while simulation is being rolled back.");
            }

            // Increment tick number
            CurrentTickNumber++;
            CurrentTickTime = _clock.UtcNow;

            // Authority and using snapshot history, create new slice TODO use object pool
            if (IsAuthority)
            {
                _currentTickSnapshot = new SimulationTickSnapshot(CurrentTickNumber, CurrentTickTime);
            }
            else // If not authority, dispatch incoming snapshots
            {

                // Dispatch snapshot
                while(_incomingSnapshotBuffer.TryDequeue(out var snapshot))
                {
                    LastProcessedSnapshotTickNumber = snapshot.TickNumber;

                    // Enqueue snapshot for processing later during this tick
                    EnqueueAction(() => ProcessTickSnapshot(snapshot));
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

                // Clear old snapshots
                while(_snapshotHistory.Count > RollbackBufferSize)
                {
                    _snapshotHistory.Dequeue();
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
            // Make sure we do not ingest the same snapshot twice,
            // as server will attempt to send those until it receives an ack 

            if (snapshot.TickNumber > LastIngestedSnapshotTickNumber)
            {
                _incomingSnapshotBuffer.Enqueue(snapshot);
                LastIngestedSnapshotTickNumber = snapshot.TickNumber;
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
                    //ExecuteServerRpcCommand((ServerRpcSimulationCommand)command);
                    break;
                case CommandType.ClientRpc:
                    ExecuteClientRpcCommand((ClientRpcSimulationCommand)command);
                    break;
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


        #region Utility Methods
        private void CheckInTick()
        {
            if(!InTick)
            {
                throw new NotInTickException($"Simulation is not currently executing a Tick(). Call {nameof(NetworkSimulation.EnqueueAction)} to run an action during next tick.");
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
