using Fenrir.Multiplayer.Serialization;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Sent when request handler fails on the server
    /// </summary>
    class ErrorResponse : IResponse, IByteStreamSerializable
    {
        public void Deserialize(IByteStreamReader reader)
        {
        }

        public void Serialize(IByteStreamWriter writer)
        {
        }
    }
}
