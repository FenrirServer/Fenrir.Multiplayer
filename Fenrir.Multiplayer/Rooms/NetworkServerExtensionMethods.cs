using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Server;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Room Management Extension methods for Fenrir Network Server
    /// </summary>
    public static class NetworkServerExtensionMethods
    {
        /// <summary>
        /// Adds Fenrir Room Management support with given room type
        /// </summary>
        /// <typeparam name="TRoom">Type of room</typeparam>
        /// <param name="server">Network Server</param>
        /// <param name="roomFactory">Room Factory</param>
        /// <returns>Server Room Manager</returns>
        public static ServerRoomManager<TRoom> AddRooms<TRoom>(this NetworkServer server, IServerRoomFactory<TRoom> roomFactory)
            where TRoom : IServerRoom
        {
            return new ServerRoomManager<TRoom>(roomFactory, server.Logger, server);
        }

        /// <summary>
        /// Adds Fenrir Room Management support with given room type
        /// </summary>
        /// <typeparam name="TRoom">Type of room</typeparam>
        /// <param name="server">Network Server</param>
        /// <param name="createRoomHandler">Room Factory Method</param>
        /// <returns>Server Room Manager</returns>
        public static ServerRoomManager<TRoom> AddRooms<TRoom>(this NetworkServer server, ServerRoomManager<TRoom>.CreateRoomHandler  createRoomHandler)
            where TRoom : IServerRoom
        {
            return new ServerRoomManager<TRoom>(createRoomHandler, server.Logger, server);
        }
    }
}
