using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Room Leave Request
    /// Sent as a response to <seealso cref="RoomLeaveRequest"/>
    /// </summary>
    public class RoomLeaveResponse : RequestResultResponse
    {        
        // Error codes and descriptions

        public const int ErrorCodeInvalidRoomId = 1;

        public const string ErrorMessageInvalidRoomId = "Invalid Room Id";


        public static RoomLeaveResponse LeaveSuccess => new RoomLeaveResponse(true);
        public static RoomLeaveResponse LeaveFailed => new RoomLeaveResponse(false, ErrorCodeInvalidRoomId, ErrorMessageInvalidRoomId);

        public RoomLeaveResponse(bool success) : base(success)
        {
        }

        public RoomLeaveResponse(bool success, int errorCode, string reason) : base(success, errorCode, reason)
        {
        }
    }
}
