using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.Sim
{
    public interface ISimulationServerView
    {
        /// <summary>
        /// Invoked when player joins the simulation
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="playerObject">Player object</param>
        /// <param name="serverPeer">Peer that this player is associated with</param>
        /// <param name="token">Join token provided by the client</param>
        void PlayerJoined(Simulation simulation, SimulationObject playerObject, IServerPeer serverPeer, string token);

        /// <summary>
        /// Invoked when player leaves the simulation
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="playerObject">Player object</param>
        /// <param name="serverPeer">Peer that this player is associated with</param>
        void PlayerLeft(Simulation simulation, SimulationObject playerObject, IServerPeer serverPeer);
    }
}
