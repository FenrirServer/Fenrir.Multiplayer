using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;
using Fenrir.Multiplayer.Sim.Command;
using Fenrir.Multiplayer.Sim.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Fenrir.Multiplayer.Sim
{
    public class SimulationRoom : ServerRoom, ISimulationListener
    {
        /// <summary>
        /// Simulation tick rate - ~66 times a second
        /// </summary>
        public int TickRateMs { get; set; } = 15;

        /// <summary>
        /// Contains server simulation
        /// </summary>
        protected Simulation Simulation { get; private set; }

        /// <summary>
        /// Stopwatch used to measure simulation tickrate
        /// </summary>
        private readonly Stopwatch _simulationTickStopwatch;

        /// <summary>
        /// Tracks peer id -> simulation object id
        /// </summary>
        private Dictionary<string, ushort> _playerObjects = new Dictionary<string, ushort>();

        /// <summary>
        /// Creates new room that runs a simulation.
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="logger">Logger</param>
        /// <param name="roomId">Room id</param>
        public SimulationRoom(IFenrirLogger logger, string roomId)
            : base(logger, roomId)
        {
            Simulation = new Simulation(this, logger);
            _simulationTickStopwatch = new Stopwatch();

            // Do first simulation tick, calling this method schedule next tick and so on
            TickSimulation();
        }

        private void TickSimulation()
        {
            _simulationTickStopwatch.Start();

            try
            {
                Simulation.Tick();
            }
            catch(Exception e)
            {
                Logger.Error("Error during simulation tick: {0}", e.ToString());
            }

            _simulationTickStopwatch.Stop();

            long timeElapsedMs = _simulationTickStopwatch.ElapsedMilliseconds;

            _simulationTickStopwatch.Reset();

            if (timeElapsedMs > TickRateMs)
            {
                Logger.Warning("Simulation tick took {0} milliseconds, which is longer than tick rate {1} milliseconds. Scheduling next tick immediately", timeElapsedMs, TickRateMs);
                Enqueue(TickSimulation);
            }
            else
            {
                Schedule(TickSimulation, TickRateMs);
            }
        }

        protected override void OnPeerJoin(IServerPeer peer, string token)
        {
            Simulation.EnqueueAction(() =>
            {
                SimulationObject playerObject = Simulation.SpawnObject();
                playerObject.AddComponent<PlayerComponent>();
                _playerObjects.Add(peer.Id, playerObject.Id);
            });
        }

        protected override void OnPeerLeave(IServerPeer peer)
        {
            Simulation.EnqueueAction(() =>
            {
                if(!_playerObjects.TryGetValue(peer.Id, out ushort objectId))
                {
                    Logger.Warning($"{nameof(OnPeerLeave)} failed: no peer found with id {peer.Id}");
                    return;
                }

                if(!Simulation.TryGetObject(objectId, out SimulationObject simObject))
                {
                    Logger.Warning($"{nameof(OnPeerLeave)} failed: no player object found id {objectId}, player object has been destroyed?");
                    return;
                }

                Simulation.DestroyObject(simObject);
            });
        }


        public void OnSendCommand(ISimulationCommand command)
        {
            // TODO: Send to all peers
            throw new NotImplementedException();
        }

        public void OnSendCommands(IEnumerable<ISimulationCommand> commands)
        {
            // TODO: Pack and send to all peers
            throw new NotImplementedException();
        }

        public void OnCommandExecuted(ISimulationCommand command)
        {
            // Do nothing on the server. Client will render using SimulationView?
        }
    }
}
