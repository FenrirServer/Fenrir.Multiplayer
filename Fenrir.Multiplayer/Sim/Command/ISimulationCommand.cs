using System;

namespace Fenrir.Multiplayer.Sim.Command
{
    public interface ISimulationCommand
    {
        CommandType Type { get; }
    }
}