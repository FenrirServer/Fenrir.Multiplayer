namespace Fenrir.Multiplayer.Sim
{
    public interface ISimulationPlayerHandler
    {
        /// <summary>
        /// Invoked when player joins the simulation
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="playerObject">Player object</param>
        /// <param name="playerId">Unique id of the player</param>
        void PlayerAdded(Simulation simulation, SimulationObject playerObject, string playerId);

        /// <summary>
        /// Invoked when player leaves the simulation
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="playerObject">Player object</param>
        /// <param name="playerId">Unique id of the player</param>
        void PlayerRemoved(Simulation simulation, SimulationObject playerObject, string playerId);
    }
}
