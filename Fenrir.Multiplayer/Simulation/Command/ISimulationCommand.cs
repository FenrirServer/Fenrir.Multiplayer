using System;

namespace Fenrir.Multiplayer.Simulation.Command
{
    public interface ISimulationCommand
    {
        CommandType Type { get; }
    }
}