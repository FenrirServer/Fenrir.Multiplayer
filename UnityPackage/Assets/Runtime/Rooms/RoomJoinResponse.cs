using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Room Join Response
    /// Sent back as a response for <seealso cref="RoomJoinRequest"/>
    /// </summary>
    public class RoomJoinResponse : RequestResultResponse
    {
        /// <summary>
        /// Error code that indicates that joining room failed due to invalid room id
        /// </summary>
        public const int ErrorCodeJoinFailed = 1;

        /// <summary>
        /// Error message that indicates that joining room failed due to invalid room id
        /// </summary>
        public const string ErrorMessageJoinFailed = "Failed to create or join a room with a given id";

        /// <summary>
        /// Creates successful <see cref="RoomJoinResponse"/>
        /// </summary>
        public static RoomJoinResponse JoinSuccess => new RoomJoinResponse(true);

        /// <summary>
        /// Creates failed <see cref="RoomJoinResponse"/>
        /// </summary>
        public static RoomJoinResponse JoinFailed => new RoomJoinResponse(false, ErrorCodeJoinFailed, ErrorMessageJoinFailed);

        /// <summary>
        /// Creates <see cref="RoomJoinResponse"/>
        /// </summary>
        public RoomJoinResponse()
        {
        }

        /// <summary>
        /// Creates <see cref="RoomJoinResponse"/>
        /// </summary>
        /// <param name="success">Indicates if joined successfully</param>
        public RoomJoinResponse(bool success) : base(success)
        {
        }

        /// <summary>
        /// Creates <see cref="RoomJoinResponse"/>
        /// </summary>
        /// <param name="success">Indicates if joined successfully</param>
        /// <param name="errorCode">If failed to join, contains error code</param>
        /// <param name="reason">If failed to join, contains error description</param>
        public RoomJoinResponse(bool success, int errorCode, string reason) : base(success, errorCode, reason)
        {
        }
    }
}
