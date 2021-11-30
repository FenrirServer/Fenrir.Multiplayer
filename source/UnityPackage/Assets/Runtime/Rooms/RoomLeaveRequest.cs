using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Request to leave a given room
    /// </summary>
    public class RoomLeaveRequest : IRequest<RoomLeaveResponse>, IByteStreamSerializable
    {
        /// <summary>
        /// Id of the room to leave
        /// </summary>
        public string RoomId { get; set; }

        /// <summary>
        /// Creates Room Leave Request
        /// </summary>
        public RoomLeaveRequest()
        {
        }

        /// <summary>
        /// Creates Room Leave Request
        /// </summary>
        /// <param name="roomId"></param>
        public RoomLeaveRequest(string roomId)
        {
            RoomId = roomId;
        }

        #region IByteStreamSerializable Implementation
        void IByteStreamSerializable.Serialize(IByteStreamWriter writer)
        {
            writer.Write(RoomId);
        }

        void IByteStreamSerializable.Deserialize(IByteStreamReader reader)
        {
            RoomId = reader.ReadString();
        }

        #endregion
    }
}
