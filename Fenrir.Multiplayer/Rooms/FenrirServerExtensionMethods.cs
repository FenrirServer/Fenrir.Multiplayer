using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Server;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Room Management Extension methods for Fenrir Server
    /// </summary>
    public static class FenrirServerExtensionMethods
    {
        /// <summary>
        /// Adds Fenrir Room Management support with given room type
        /// </summary>
        /// <typeparam name="TRoom">Type of room</typeparam>
        /// <param name="server">Fenrir Server</param>
        /// <param name="roomManager">Room Manager</param>
        public static void AddRooms<TRoom>(this FenrirServer server, ServerRoomManager<TRoom> roomManager)
            where TRoom : IServerRoom
        {
            AddRoomManagementInternal(server, roomManager);
        }

        /// <summary>
        /// Adds Fenrir Room Management support with given room type
        /// </summary>
        /// <typeparam name="TRoom">Type of room</typeparam>
        /// <param name="server">Fenrir Server</param>
        /// <param name="roomFactory">Room Factory</param>
        public static void AddRooms<TRoom>(this FenrirServer server, IServerRoomFactory<TRoom> roomFactory)
            where TRoom : IServerRoom
        {
            AddRoomManagementInternal(server, new ServerRoomManager<TRoom>(roomFactory));
        }

        /// <summary>
        /// Adds Fenrir Room Management support with given room type
        /// </summary>
        /// <typeparam name="TRoom">Type of room</typeparam>
        /// <param name="server">Fenrir Server</param>
        /// <param name="roomFactory">Room Factory</param>
        /// <param name="logger">Logger</param>
        public static void AddRooms<TRoom>(this FenrirServer server, IServerRoomFactory<TRoom> roomFactory, IFenrirLogger logger)
            where TRoom : IServerRoom
        {
            AddRoomManagementInternal(server, new ServerRoomManager<TRoom>(roomFactory, logger));
        }

        /// <summary>
        /// Adds Fenrir Room Management support with given room type
        /// </summary>
        /// <typeparam name="TRoom">Type of room</typeparam>
        /// <param name="server">Fenrir Server</param>
        /// <param name="createRoomHandler">Room Factory Method</param>
        public static void AddRooms<TRoom>(this FenrirServer server, ServerRoomManager<TRoom>.CreateRoomHandler  createRoomHandler)
            where TRoom : IServerRoom
        {
            AddRoomManagementInternal(server, new ServerRoomManager<TRoom>(createRoomHandler));
        }

        /// <summary>
        /// Adds Fenrir Room Management support with given room type
        /// </summary>
        /// <typeparam name="TRoom">Type of room</typeparam>
        /// <param name="server">Fenrir Server</param>
        /// <param name="createRoomHandler">Room Factory Method</param>
        /// <param name="logger">Logger</param>
        public static void AddRooms<TRoom>(this FenrirServer server, ServerRoomManager<TRoom>.CreateRoomHandler createRoomHandler, IFenrirLogger logger)
            where TRoom : IServerRoom
        {
            AddRoomManagementInternal(server, new ServerRoomManager<TRoom>(createRoomHandler, logger));
        }

        private static void AddRoomManagementInternal<TRoom>(FenrirServer server, ServerRoomManager<TRoom> roomManager)
            where TRoom : IServerRoom
        {
            server.AddRequestHandler<RoomJoinRequest, RoomJoinResponse>(roomManager);
            server.AddRequestHandler<RoomLeaveRequest, RoomLeaveResponse>(roomManager);
        }
    }
}
