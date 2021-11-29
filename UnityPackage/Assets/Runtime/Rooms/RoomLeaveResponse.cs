using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Room Leave Request
    /// Sent as a response to <seealso cref="RoomLeaveRequest"/>
    /// </summary>
    public class RoomLeaveResponse : RequestResultResponse
    {        
        /// <summary>
        /// Error code that indicates that failed to leave the room because of invalid room id
        /// </summary>
        public const int ErrorCodeInvalidRoomId = 1;

        /// <summary>
        /// Error message taht indicates that failed to leave the room because of invalid room id
        /// </summary>
        public const string ErrorMessageInvalidRoomId = "Invalid Room Id";

        /// <summary>
        /// Creates successful <see cref="RoomLeaveResponse"/>
        /// </summary>
        public static RoomLeaveResponse LeaveSuccess => new RoomLeaveResponse(true);

        /// <summary>
        /// Creates failed <see cref="RoomLeaveResponse"/>
        /// </summary>
        public static RoomLeaveResponse LeaveFailed => new RoomLeaveResponse(false, ErrorCodeInvalidRoomId, ErrorMessageInvalidRoomId);

        /// <summary>
        /// Creates empty <see cref="RoomLeaveResponse"/>
        /// </summary>
        public RoomLeaveResponse()
        {
        }

        /// <summary>
        /// Creates <see cref="RoomLeaveResponse"/>
        /// </summary>
        /// <param name="success">Indicates if leave operation was successful</param>
        public RoomLeaveResponse(bool success) : base(success)
        {
        }

        /// <summary>
        /// Creates <see cref="RoomLeaveResponse"/>
        /// </summary>
        /// <param name="success">Indicates if leave operation was successful</param>
        /// <param name="errorCode">If operation failed, contains error code</param>
        /// <param name="reason">If operation failed, contains error text</param>
        public RoomLeaveResponse(bool success, int errorCode, string reason) : base(success, errorCode, reason)
        {
        }
    }
}
