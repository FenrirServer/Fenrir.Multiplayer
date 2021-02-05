namespace Fenrir.Multiplayer.Sim.Command
{
    interface IComponentSimulationCommand : ISimulationCommand
    {
        ushort ObjectId { get; }

        ulong ComponentTypeHash { get;  }
    }
}