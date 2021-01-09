using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Sim.Exceptions;
using Fenrir.Multiplayer.Sim.State;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Fenrir.Multiplayer.Sim
{
    public class ServerSimulation : Simulation
    {
        /// <summary>
        /// Simulation server - gets notified of the server simulation events
        /// </summary>
        private ISimulationServerView SimulationServer { get; set; }

        /// <summary>
        /// Player owned objects by server peer. Only available if simulation is runnning on a server.
        /// </summary>
        private Dictionary<IServerPeer, SimulationObject> _players = new Dictionary<IServerPeer, SimulationObject>();

        /// <summary>
        /// True if simulation runs on the host (server)
        /// </summary>
        public override bool IsServer => true;

        /// <summary>
        /// Head of the Linked List of simulation snapshots.
        /// Server stores up to <see cref="SnapshotHistoryBufferSizeTicks"/> snapshots at a time,
        /// allowing rolling simulation back N states for command reconciliation
        /// </summary>
        private SimulationSnapshot _simulationSnapshotHistory = null;

        /// <summary>
        /// Temporary buffer that stores deleted objects until history buffer size is reached.
        /// </summary>
        private OrderedDictionary _destroyedObjectsById = new OrderedDictionary();

        /// <summary>
        /// Next object id, used to track incremented object ids
        /// </summary>
        private ushort _nextObjectId = 0;

        /// <summary>
        /// Size of the snapshot buffer, e.g. how many ticks we allow to roll back to
        /// </summary>
        public int SnapshotHistoryBufferSizeTicks { get; set; } = 5;

        /// <summary>
        /// Creates new Server Simulation
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="view">Simulation view, that is being notified of simulation events</param>
        /// <param name="serverView">Simulation view, that is being notified of simulation events</param>
        public ServerSimulation(IFenrirLogger logger, ISimulationView view, ISimulationServerView serverView)
            : base(logger, view)
        {
            if (serverView == null)
            {
                throw new ArgumentNullException(nameof(serverView));
            }

            SimulationServer = serverView;
        }

        #region Object Management
        public SimulationServerObject SpawnObject()
        {
            ushort objectId = GetNextObjectId();
            SimulationServerObject obj = new SimulationServerObject(this, Logger, objectId);
            ObjectsById.Add(objectId, obj);
            return obj;
        }

        public void DestroyObject(SimulationServerObject obj)
        {
            obj.Destroy();

            // Move object to the "destroyed" objects queue where it would live for the next N ticks until it is not needed even with the rollback
            // and can be safely removed
            ObjectsById.Remove(obj.Id);

            _destroyedObjectsById.Add(obj.Id, obj);
        }

        public new IEnumerable<SimulationServerObject> GetObjects()
        {
            IDictionaryEnumerator objectEnumerator = ObjectsById.GetEnumerator();

            while (objectEnumerator.MoveNext())
            {
                SimulationServerObject simObject = (SimulationServerObject)objectEnumerator.Value;

                // If simulation is being rolled back and object has not been created yet, skip
                if(IsRolledBack && simObject.TickCreated > CurrentTick)
                {
                    continue;
                }

                yield return simObject;
            }
        }
        #endregion


        #region Player Registration
        public void AddPlayer(IServerPeer serverPeer, string token)
        {
            if (serverPeer == null)
            {
                throw new ArgumentNullException(nameof(serverPeer));
            }

            SimulationObject playerObject = SpawnObject();
            _players.Add(serverPeer, playerObject);
            SimulationServer.PlayerJoined(this, playerObject, serverPeer, token);
        }

        public void RemovePlayer(IServerPeer serverPeer)
        {
            if (serverPeer == null)
            {
                throw new ArgumentNullException(nameof(serverPeer));
            }

            if (!_players.ContainsKey(serverPeer))
            {
                throw new Exception($"Can't remove player from Simulation, no player object found for peer {serverPeer}");
            }

            SimulationObject playerObject = _players[serverPeer];
            _players.Remove(serverPeer);

            SimulationServer.PlayerLeft(this, playerObject, serverPeer);
        }
        #endregion

        #region Tick

        public override void Tick()
        {
            // Perform basic tick
            base.Tick();

            // Check if any objects can now be finally removed from  the "destroyed" list
            IDictionaryEnumerator objectEnumerator = _destroyedObjectsById.GetEnumerator();

            while (objectEnumerator.MoveNext())
            {
                SimulationServerObject simObject = (SimulationServerObject)objectEnumerator.Value;

                if(simObject.TickDestroyed > CurrentTick + this.SnapshotHistoryBufferSizeTicks)
                {
                    _destroyedObjectsById.Remove(simObject.Id);
                }
            }
        }

        #endregion

        #region Utility Methods
        protected ushort GetNextObjectId()
        {
            if (ObjectsById.Count == ushort.MaxValue)
            {
                throw new SimulationException($"Failed to create Simulation Object Id, has reached max number of simulation objects: {ObjectsById.Count}");
            }

            // Find next unused objectid
            int maxId = _nextObjectId - 1;
            do
            {
                if (!ObjectsById.Contains(_nextObjectId))
                {
                    return _nextObjectId;
                }

                _nextObjectId++;
            }
            while (_nextObjectId != maxId);

            // This should not happen because of the check above
            throw new SimulationException($"Failed to create Simulation Object Id, total number of objects: {ObjectsById.Count}");
        }
        #endregion
    }
}
