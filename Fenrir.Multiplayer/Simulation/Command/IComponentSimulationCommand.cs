namespace Fenrir.Multiplayer.Simulation.Command
{
    interface IComponentSimulationCommand : ISimulationCommand
    {
        ushort ObjectId { get; }

        ulong ComponentTypeHash { get;  }
    }
}