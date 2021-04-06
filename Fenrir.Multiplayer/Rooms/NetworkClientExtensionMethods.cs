using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Exceptions;
using System;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Fenrir Room Management - Client Extension methods
    /// </summary>
    public static class NetworkClientExtensionMethods
    {
        /// <summary>
        /// Joins the room
        /// </summary>
        /// <param name="networkClient">Network Client</param>
        /// <param name="roomId">Room Id</param>
        /// <returns>Task that completes with join operation result</returns>
        public static async Task<RoomJoinResponse> JoinRoom(this NetworkClient networkClient, string roomId)
        {
            return await JoinRoomInternal(networkClient, roomId, null);
        }

        /// <summary>
        /// Joins the room
        /// </summary>
        /// <param name="networkClient">Network Client</param>
        /// <param name="roomId">Room Id</param>
        /// <param name="token">Room Access Token</param>
        /// <returns>Task that completes with join operation result</returns>
        public static async Task<RoomJoinResponse> JoinRoom(this NetworkClient networkClient, string roomId, string token)
        {
            return await JoinRoomInternal(networkClient, roomId, token);
        }


        /// <summary>
        /// Leaves the room
        /// </summary>
        /// <param name="networkClient">Network Client</param>
        /// <param name="roomId">Room Id</param>
        /// <returns>Task that completes with leave operation result</returns>
        public static async Task<RoomLeaveResponse> LeaveRoom(this NetworkClient networkClient, string roomId)
        {
            return await LeaveRoomInternal(networkClient, roomId);
        }

        private static async Task<RoomJoinResponse> JoinRoomInternal(NetworkClient networkClient, string roomId, string token)
        {
            if (networkClient.State != Network.ConnectionState.Connected)
            {
                throw new NetworkClientException("Failed to join room - not connected");
            }

            if(roomId == null)
            {
                throw new ArgumentNullException(nameof(roomId));
            }

            var joinRoomRequest = new RoomJoinRequest(roomId, token);
            return await networkClient.Peer.SendRequest<RoomJoinRequest, RoomJoinResponse>(joinRoomRequest);
        }

        private static async Task<RoomLeaveResponse> LeaveRoomInternal(NetworkClient networkClient, string roomId)
        {
            if (networkClient.State != Network.ConnectionState.Connected)
            {
                throw new NetworkClientException("Failed to leave room - not connected");
            }

            if (roomId == null)
            {
                throw new ArgumentNullException(nameof(roomId));
            }

            var leaveRoomRequest = new RoomLeaveRequest(roomId);
            return await networkClient.Peer.SendRequest<RoomLeaveRequest, RoomLeaveResponse>(leaveRoomRequest);
        }
    }
}
