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
    public class SimulationRoom : ServerRoom
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
        /// Tracks peer id -> player component
        /// </summary>
        private Dictionary<string, PlayerComponent> _playerObjects = new Dictionary<string, PlayerComponent>();


        /// <summary>
        /// Creates new room that runs a simulation.
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="logger">Logger</param>
        /// <param name="roomId">Room id</param>
        public SimulationRoom(IFenrirLogger logger, string roomId)
            : base(logger, roomId)
        {
            Simulation = new Simulation(logger);
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

        protected sealed override void OnPeerJoin(IServerPeer peer, string token)
        {
            Simulation.EnqueueAction(() =>
            {
                SimulationObject playerObject = Simulation.SpawnObject();
                PlayerComponent playerComponent = playerObject.AddComponent<PlayerComponent>();
                playerComponent.ServerPeer = peer; // TODO: Introduce parameterized AddComponent. It should take in T1, T2, T3 etc parameters and pass into component factory
                _playerObjects.Add(peer.Id, playerComponent);
                OnPlayerObjectCreated(playerObject, playerComponent);
            });
        }

        protected virtual void OnPlayerObjectCreated(SimulationObject simObject, PlayerComponent player)
        {
        }

        protected sealed override void OnPeerLeave(IServerPeer peer)
        {
            Simulation.EnqueueAction(() =>
            {
                if(!_playerObjects.TryGetValue(peer.Id, out PlayerComponent playerComponent))
                {
                    Logger.Warning($"{nameof(OnPeerLeave)} failed: no peer found with id {peer.Id}");
                    return;
                }

                SimulationObject playerObject = playerComponent.Object;

                OnBeforePlayerObjectDestroyed(playerObject, playerComponent);

                Simulation.DestroyObject(playerObject);
            });
        }

        protected virtual void OnBeforePlayerObjectDestroyed(SimulationObject simObject, PlayerComponent player)
        {
        }

        public void AcknowledgeTickSnapshot(IServerPeer peer, DateTime tickTime)
        {
            if(!_playerObjects.TryGetValue(peer.Id, out PlayerComponent playerComponent))
            {
                Logger.Warning($"{nameof(AcknowledgeTickSnapshot)} failed, no peer component found. Perhaps peer object has been destroyed");
                return;
            }

            // Schedule acknowledgement on the next tick
            Simulation.EnqueueAction(() => playerComponent.AcknowledgeTickSnapshot(tickTime));
        }
    }
}
