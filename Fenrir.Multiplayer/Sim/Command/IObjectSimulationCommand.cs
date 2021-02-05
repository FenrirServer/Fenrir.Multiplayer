namespace Fenrir.Multiplayer.Sim.Command
{
    internal interface IObjectSimulationCommand : ISimulationCommand
    {
        ushort ObjectId { get; }
    }
}