using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;
using System;
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
        private readonly ServerSimulation _simulation;

        /// <summary>
        /// Stopwatch used to measure simulation tickrate
        /// </summary>
        private readonly Stopwatch _simulationTickStopwatch;

        /// <summary>
        /// Creates new room that runs a simulation.
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="logger">Logger</param>
        /// <param name="roomId">Room id</param>
        public SimulationRoom(ServerSimulation simulation, IFenrirLogger logger, string roomId)
            : base(logger, roomId)
        {
            _simulation = simulation;
            _simulationTickStopwatch = new Stopwatch();

            // Do first simulation tick, calling this method schedule next tick and so on
            TickSimulation();
        }

        private void TickSimulation()
        {
            _simulationTickStopwatch.Start();

            try
            {
                _simulation.Tick();
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
            _simulation.EnqueueAction(() => _simulation.AddPlayer(peer, token));
        }

        protected override void OnPeerLeave(IServerPeer peer)
        {
            _simulation.EnqueueAction(() => _simulation.RemovePlayer(peer));
        }
    }
}
