using System.Collections.Generic;

namespace Fenrir.Multiplayer.Simulation.Command
{
    interface IRpcSimulationCommand : IComponentSimulationCommand
    {
        ulong MethodHash { get; }

        object[] Parameters { get; }
    }
}
