using Fenrir.Multiplayer.Network;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Server room factory.
    /// Implementation creates new room.
    /// If null is returned, or exception is thrown, we assume that room was not created succesfully.
    /// </summary>
    public interface IServerRoomFactory<TRoom>
        where TRoom : IServerRoom
    {
        /// <summary>
        /// Invoked when new room needs to be created
        /// </summary>
        /// <param name="peer">Peer that attempts to create a new room</param>
        /// <param name="roomId">Id of the room</param>
        /// <param name="token">(optional) custom token</param>
        /// <returns>Newly created room</returns>
        TRoom Create(IServerPeer peer, string roomId, string token);
    }
}
