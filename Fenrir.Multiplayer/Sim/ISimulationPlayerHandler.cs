namespace Fenrir.Multiplayer.Sim
{
    public interface ISimulationPlayerHandler
    {
        /// <summary>
        /// Invoked when player joins the simulation
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="playerObject">Player object</param>
        void PlayerAdded(Simulation simulation, SimulationObject playerObject);

        /// <summary>
        /// Invoked when player leaves the simulation
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="playerObject">Player object</param>
        void PlayerRemoved(Simulation simulation, SimulationObject playerObject);
    }
}
