using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Join Room Request
    /// Send by the peer to join a specific room
    /// </summary>
    public class RoomJoinRequest : IRequest<RoomJoinResponse>, IByteStreamSerializable
    {
        /// <summary>
        /// Unique id of the room
        /// </summary>
        public string RoomId { get; set; }

        /// <summary>
        /// Custom token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Creates room Join Request
        /// </summary>
        public RoomJoinRequest()
        {
        }

        /// <summary>
        /// Creates Room Join Request
        /// </summary>
        /// <param name="roomId">Unique id of the room</param>
        public RoomJoinRequest(string roomId)
        {
            RoomId = roomId;
        }

        /// <summary>
        /// Creates Room Join Request
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="token">Custom token to validate if player can join this specific room</param>
        public RoomJoinRequest(string roomId, string token)
            : this(roomId)
        {
            Token = token;
        }


        #region IByteStreamSerializable Implementation
        void IByteStreamSerializable.Deserialize(IByteStreamReader reader)
        {
            RoomId = reader.ReadString();

            if(!reader.EndOfData)
            {
                Token = reader.ReadString();
            }
        }

        void IByteStreamSerializable.Serialize(IByteStreamWriter writer)
        {
            writer.Write(RoomId);

            if(Token != null)
            {
                writer.Write(Token);
            }
        }
        #endregion
    }
}
