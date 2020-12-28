using Fenrir.Multiplayer.Network;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Server room factory.
    /// Implementation must create custom room
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
        /// <param name="room">Newly created room</param>
        /// <returns>True if room was created, otherwise false</returns>
        bool TryCreate(IServerPeer peer, string roomId, string token, out TRoom room);
    }
}
