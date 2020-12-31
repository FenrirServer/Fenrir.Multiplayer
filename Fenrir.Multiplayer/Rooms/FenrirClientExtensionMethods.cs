using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Exceptions;
using System;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Fenrir Room Management - Client Extension methods
    /// </summary>
    public static class FenrirClientExtensionMethods
    {
        /// <summary>
        /// Joins the room
        /// </summary>
        /// <param name="fenrirClient">Fenrir Client</param>
        /// <param name="roomId">Room Id</param>
        /// <returns>Task that completes with join operation result</returns>
        public static async Task<RoomJoinResponse> JoinRoom(this FenrirClient fenrirClient, string roomId)
        {
            return await JoinRoomInternal(fenrirClient, roomId, null);
        }

        /// <summary>
        /// Joins the room
        /// </summary>
        /// <param name="fenrirClient">Fenrir Client</param>
        /// <param name="roomId">Room Id</param>
        /// <param name="token">Room Access Token</param>
        /// <returns>Task that completes with join operation result</returns>
        public static async Task<RoomJoinResponse> JoinRoom(this FenrirClient fenrirClient, string roomId, string token)
        {
            return await JoinRoomInternal(fenrirClient, roomId, token);
        }


        /// <summary>
        /// Leaves the room
        /// </summary>
        /// <param name="fenrirClient">Fenrir Client</param>
        /// <param name="roomId">Room Id</param>
        /// <returns>Task that completes with leave operation result</returns>
        public static async Task<RoomLeaveResponse> LeaveRoom(this FenrirClient fenrirClient, string roomId)
        {
            return await LeaveRoomInternal(fenrirClient, roomId);
        }

        private static async Task<RoomJoinResponse> JoinRoomInternal(FenrirClient fenrirClient, string roomId, string token)
        {
            if (fenrirClient.State != Network.ConnectionState.Connected)
            {
                throw new FenrirClientException("Failed to join room - not connected");
            }

            if(roomId == null)
            {
                throw new ArgumentNullException(nameof(roomId));
            }

            var joinRoomRequest = new RoomJoinRequest(roomId, token);
            return await fenrirClient.Peer.SendRequest<RoomJoinRequest, RoomJoinResponse>(joinRoomRequest);
        }

        private static async Task<RoomLeaveResponse> LeaveRoomInternal(FenrirClient fenrirClient, string roomId)
        {
            if (fenrirClient.State != Network.ConnectionState.Connected)
            {
                throw new FenrirClientException("Failed to leave room - not connected");
            }

            if (roomId == null)
            {
                throw new ArgumentNullException(nameof(roomId));
            }

            var leaveRoomRequest = new RoomLeaveRequest(roomId);
            return await fenrirClient.Peer.SendRequest<RoomLeaveRequest, RoomLeaveResponse>(leaveRoomRequest);
        }
    }
}
