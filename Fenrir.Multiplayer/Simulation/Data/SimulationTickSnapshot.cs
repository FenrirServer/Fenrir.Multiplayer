using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Simulation.Command;
using System;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Simulation.Data
{
    public class SimulationTickSnapshot
    {
        public uint TickNumber;

        public DateTime TickTime;

        public List<ISimulationCommand> Commands;

        // TODO List of component state changes

        public SimulationTickSnapshot()
        {
            Commands = new List<ISimulationCommand>();
        }

        public SimulationTickSnapshot(uint tickNumber, DateTime tickTime)
            : this()
        {
            TickNumber = tickNumber;
            TickTime = tickTime;
        }

        public SimulationTickSnapshot(uint tickNumber, DateTime tickTime, IEnumerable<ISimulationCommand> commands)
            : this(tickNumber, tickTime)
        {
            Commands.AddRange(commands);
        }
    }
}
