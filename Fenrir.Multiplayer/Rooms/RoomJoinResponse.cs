using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Room Join Response
    /// Sent back as a response for <seealso cref="RoomJoinRequest"/>
    /// </summary>
    public class RoomJoinResponse : RequestResultResponse
    {
        // Error codes and descriptions

        public const int ErrorCodeJoinFailed = 1;

        public const string ErrorMessageJoinFailed = "Failed to create or join a room with a given id";


        public static RoomJoinResponse JoinSuccess => new RoomJoinResponse(true);
        public static RoomJoinResponse JoinFailed => new RoomJoinResponse(false, ErrorCodeJoinFailed, ErrorMessageJoinFailed);

        public RoomJoinResponse(bool success) : base(success)
        {
        }

        public RoomJoinResponse(bool success, int errorCode, string reason) : base(success, errorCode, reason)
        {
        }
    }
}
