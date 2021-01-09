using System;

namespace Fenrir.Multiplayer.Sim.State
{
    /// <summary>
    /// Contains information about simulation state at a given tick
    /// </summary>
    class SimulationSnapshot
    {
        /// <summary>
        /// Number of server tick
        /// </summary>
        public uint NumTick { get; private set; }

        /// <summary>
        /// Server time, in milliseconds
        /// </summary>
        public DateTime ServerTime { get; private set; }

        /// <summary>
        /// Next snapshot
        /// </summary>
        public SimulationSnapshot Next { get; private set; }

        /// <summary>
        /// Previous snapshot
        /// </summary>
        public SimulationSnapshot Previous { get; private set; }


    }
}
