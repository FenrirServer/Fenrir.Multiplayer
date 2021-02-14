using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;
using Fenrir.Multiplayer.Simulation.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Fenrir.Multiplayer.Simulation
{
    public class SimulationRoom : ServerRoom
    {
        /// <summary>
        /// Simulation tick rate, how many ticks per second
        /// </summary>
        public int TickRate { get; set; } = 66;

        /// <summary>
        /// Contains server simulation
        /// </summary>
        public NetworkSimulation Simulation { get; private set; }

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
        public SimulationRoom(ILogger logger, string roomId)
            : base(logger, roomId)
        {
            _simulationTickStopwatch = new Stopwatch();

            Simulation = new NetworkSimulation(logger) { IsAuthority = true };

            RegisterBuiltInSimulationComponents();

            // Do first simulation tick, calling this method schedule next tick and so on
            TickSimulation();
        }

        private void RegisterBuiltInSimulationComponents()
        {
            Simulation.RegisterComponentType<PlayerComponent>();
        }

        private void TickSimulation()
        {
            _simulationTickStopwatch.Start();

            double delayBetweenTicksMs = 1000d / TickRate;

            try
            {
                Simulation.Tick();
            }
            catch(Exception e)
            {
                Logger.Error("Error during simulation tick: {0}", e.ToString());
            }

            _simulationTickStopwatch.Stop();

            double timeElapsedMs = (double)_simulationTickStopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond;

            _simulationTickStopwatch.Reset();

            if (timeElapsedMs > delayBetweenTicksMs)
            {
                Logger.Warning("Simulation tick took {0} milliseconds, which is longer than tick rate {1} milliseconds. Scheduling next tick immediately", timeElapsedMs, delayBetweenTicksMs);
                Enqueue(TickSimulation);
            }
            else
            {
                Schedule(TickSimulation, delayBetweenTicksMs - timeElapsedMs);
            }
        }

        protected sealed override void OnPeerJoin(IServerPeer peer, string token)
        {
            Simulation.EnqueueAction(() =>
            {
                SimulationObject playerObject = Simulation.SpawnObject();
                PlayerComponent playerComponent = playerObject.AddComponent<PlayerComponent>();
                playerComponent.ServerPeer = peer; // TODO: Introduce parameterized AddComponent. It should take in T1, T2, T3 etc parameters and pass into component factory
                playerComponent.SendSimulationInitEvent();
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
