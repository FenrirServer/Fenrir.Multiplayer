using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Sim.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Sim
{
    public class Simulation
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected IFenrirLogger Logger { get; private set; }

        /// <summary>
        /// Simulation client - gets notified of the client simulation events
        /// </summary>
        private ISimulationView SimulationView { get; set; }

        /// <summary>
        /// Simulation objects by ushort id
        /// </summary>
        protected OrderedDictionary ObjectsById => new OrderedDictionary();

        /// <summary>
        /// Global component type hash
        /// </summary>
        private TypeHashMap _componentTypeHash = new TypeHashMap();

        /// <summary>
        /// Queue of actions scheduled for the next tick
        /// </summary>
        private Queue<Action> _actionQueue = new Queue<Action>();

        /// <summary>
        /// True if simulation runs on the host (server)
        /// </summary>
        public virtual bool IsServer => false;

        /// <summary>
        /// Indicates if simulation is rolling back for server re-conciliation
        /// </summary>
        public bool IsRolledBack { get; private set; }

        /// <summary>
        /// Simulation tick number
        /// </summary>
        public int CurrentTick { get; private set; }

        /// <summary>
        /// Creates new Client Simulation
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="view">Simulation view, that is being notified of simulation events</param>
        public Simulation(IFenrirLogger logger, ISimulationView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            Logger = logger;
            SimulationView = view;
        }

        #region Component Registration
        public void RegisterComponentType<TComponent>()
            where TComponent : SimulationComponent
        {
            _componentTypeHash.AddType<TComponent>();
        }
        public bool ComponentRegistered<TComponent>()
            where TComponent : SimulationComponent
        {
            return _componentTypeHash.HasTypeHash(typeof(TComponent));
        }

        #endregion

        #region Object Management

        /// <summary>
        /// Creates object with a given object id
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        internal SimulationObject SpawnObject(ushort objectId)
        {
            SimulationObject obj = new SimulationObject(this, objectId);
            ObjectsById.Add(objectId, obj);
            return obj;
        }


        public void RemoveObject(SimulationObject obj)
        {
            if (!IsServer)
            {
                throw new SimulationException("Client simulation is not allowed to remove objects directly. Please invoke server RPC using a component");
            }

            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if(!ObjectsById.Contains(obj.Id))
            {
                throw new SimulationException($"Failed to remove object {obj.Id} from simulation, object not in simulation");
            }

            ObjectsById.Remove(obj.Id);
        }

        public void RemoveObject(ushort objectId)
        {
            if (!IsServer)
            {
                throw new SimulationException("Client simulation is not allowed to remove objects directly. Please invoke server RPC using a component");
            }

            if (!ObjectsById.Contains(objectId))
            {
                throw new SimulationException($"Failed to remove object {objectId} from simulation, object not in simulation");
            }

            ObjectsById.Remove(objectId);
        }

        public IEnumerable<SimulationObject> GetObjects()
        {
            IDictionaryEnumerator objectEnumerator = ObjectsById.GetEnumerator();

            while (objectEnumerator.MoveNext())
            {
                SimulationObject simObject = (SimulationObject)objectEnumerator.Value;
                yield return simObject;
            }
        }
        #endregion

        #region Tick
        public virtual void Tick()
        {
            // Check if we can tick simulation. If simulation is being rolled back for reconciliation, we can't tick.
            if(IsRolledBack)
            {
                throw new SimulationException("Can not tick simulation while simulation is being rolled back.");
            }

            // Tick enqueued actions
            while(TryDequeueAction(out Action action))
            {
                action.Invoke();
            }
            
            // Iterate over objects
            IDictionaryEnumerator objectEnumerator = ObjectsById.GetEnumerator();

            while (objectEnumerator.MoveNext())
            {
                SimulationObject simObject = (SimulationObject)objectEnumerator.Value;
                simObject.Tick();
            }


            // Increment tick
            CurrentTick++;
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

        #region Utility Methods
        public ulong GetComponentTypeHash<TComponent>()
            where TComponent : SimulationComponent
        {
            return GetComponentTypeHash(typeof(TComponent));
        }

        public ulong GetComponentTypeHash(Type componentType)
        {
            if(componentType == null)
            {
                throw new ArgumentNullException(nameof(componentType));
            }

            return _componentTypeHash.GetTypeHash(componentType);
        }
        #endregion
    }
}
