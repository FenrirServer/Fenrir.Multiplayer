namespace Fenrir.Multiplayer.Simulation.Command
{
    internal interface IObjectSimulationCommand : ISimulationCommand
    {
        ushort ObjectId { get; }
    }
}