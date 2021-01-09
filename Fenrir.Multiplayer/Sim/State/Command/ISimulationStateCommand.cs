namespace Fenrir.Multiplayer.Sim.State.Command
{
    public interface ISimulationStateCommand
    {
        void Apply(Simulation sim);

        void Rollback(Simulation sim);
    }
}
