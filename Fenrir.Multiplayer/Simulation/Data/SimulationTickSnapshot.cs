using Fenrir.Multiplayer.Simulation.Command;
using System;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Simulation.Data
{
    public class SimulationTickSnapshot
    {
        public DateTime TickTime;

        public List<ISimulationCommand> Commands = new List<ISimulationCommand>();

        // TODO List of component state changes

        public SimulationTickSnapshot()
        {
        }

    }
}
