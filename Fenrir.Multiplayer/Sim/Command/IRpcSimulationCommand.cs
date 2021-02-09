using System.Collections.Generic;

namespace Fenrir.Multiplayer.Sim.Command
{
    interface IRpcSimulationCommand : IComponentSimulationCommand
    {
        ulong MethodHash { get; }

        object[] Parameters { get; }
    }
}
