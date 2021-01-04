using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.Simulation
{
    public interface ISimulation
    {
        /// <summary>
        /// Ticks simulation
        /// </summary>
        void Tick();

        /// <summary>
        /// Adds player to a simulation
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="token"></param>
        void AddPeer(IServerPeer peer, string token);

        /// <summary>
        /// Removes player from a simulation
        /// </summary>
        /// <param name="peer"></param>
        void RemovePeer(IServerPeer peer);
    }
}