using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Sim.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Fenrir.Multiplayer.Sim
{
    public class Simulation
    {
        /// <summary>
        /// Next object id, used to track incremented object ids
        /// </summary>
        private ushort _nextObjectId = 0;

        /// <summary>
        /// Simulation objects by ushort id
        /// </summary>
        private OrderedDictionary _objectsById = new OrderedDictionary();

        /// <summary>
        /// Global component type hash
        /// </summary>
        private TypeHashMap _componentTypeHash = new TypeHashMap();

        /// <summary>
        /// Player owned objects by player id
        /// </summary>
        private Dictionary<string, SimulationObject> _players = new Dictionary<string, SimulationObject>();

        /// <summary>
        /// Logger
        /// </summary>
        private readonly IFenrirLogger _logger;

        /// <summary>
        /// Player handler - methods are invoked when new player object is created
        /// </summary>
        private readonly ISimulationPlayerHandler _playerHandler;

        /// <summary>
        /// True if simulation runs on the host (server)
        /// </summary>
        public bool IsServer { get; set; }

        /// <summary>
        /// Creates new Server Simulation
        /// </summary>
        public Simulation(IFenrirLogger logger, ISimulationPlayerHandler playerHandler)
        {
            if(logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (playerHandler == null)
            {
                throw new ArgumentNullException(nameof(playerHandler));
            }

            _logger = logger;
            _playerHandler = playerHandler;
        }

        public void RegisterComponentType<TComponent>()
            where TComponent : SimulationComponent
        {
            _componentTypeHash.AddType<TComponent>();
        }

        public void AddPlayer(string playerId)
        {
            SimulationObject playerObject = CreateObject();
            _players.Add(playerId, playerObject);
            _playerHandler.PlayerAdded(this, playerObject);
        }

        public void RemovePlayer(string playerId)
        {
            if(!_players.ContainsKey(playerId))
            {
                throw new Exception($"Can't remove player from Simulation, no player found with id {playerId}");
            }

            SimulationObject playerObject = _players[playerId];
            _players.Remove(playerId);

            _playerHandler.PlayerRemoved(this, playerObject);
        }

        public SimulationObject CreateObject()
        {
            ushort objectId = GetNextObjectId();
            SimulationObject obj = new SimulationObject(this, objectId);
            _objectsById.Add(objectId, obj);
            return obj;
        }

        public void RemoveObject(SimulationObject obj)
        {
            if(obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if(!_objectsById.Contains(obj.Id))
            {
                throw new SimulationException($"Failed to remove object {obj.Id} from simulation, object not in simulation");
            }

            _objectsById.Remove(obj.Id);
        }

        public void RemoveObject(int objectId)
        {
            if (!_objectsById.Contains(objectId))
            {
                throw new SimulationException($"Failed to remove object {objectId} from simulation, object not in simulation");
            }

            _objectsById.Remove(objectId);
        }

        public void Tick()
        {
            // Get all sim objects
            IDictionaryEnumerator objectEnumerator = _objectsById.GetEnumerator();

            while (objectEnumerator.MoveNext())
            {
                SimulationObject simObject = (SimulationObject)objectEnumerator.Value;

                // Get all components attached to this object
                foreach (var component in simObject.GetComponents())
                {
                    try
                    {
                        component.Tick();
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Uncaught exception during component {nameof(SimulationComponent.Tick)}: {e.ToString()}");
                    }
                }
            }
        }

        private ushort GetNextObjectId()
        {
            if(_objectsById.Count == ushort.MaxValue)
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

        public ulong GetComponentTypeHash<TComponent>()
            where TComponent : SimulationComponent
        {
            return GetComponentTypeHash(typeof(TComponent));
        }

        public ulong GetComponentTypeHash(Type componentType)
        {
            return _componentTypeHash.GetTypeHash(componentType);
        }

    }
}
