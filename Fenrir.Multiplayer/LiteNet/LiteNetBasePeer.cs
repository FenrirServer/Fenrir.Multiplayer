using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using LiteNetLib;

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// Base LiteNet peer
    /// </summary>
    class LiteNetBasePeer
    {
        /// <summary>
        /// LiteNet Peer
        /// </summary>
        protected NetPeer NetPeer { get; private set; }

        /// <summary>
        /// MessageWriter - serializes and write message into NetDataWriter
        /// </summary>
        protected MessageWriter MessageWriter { get; private set; }
        
        /// <summary>
        /// Object pool of byte stream writers - used to send messages (byte streams) to LiteNet
        /// </summary>
        protected RecyclableObjectPool<ByteStreamWriter> ByteStreamWriterPool { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="netPeer">LiteNet NetPeer</param>
        /// <param name="messageWriter">Message Writer</param>
        /// <param name="byteStreamWriterPool">Byte Stream Writer object pool</param>
        public LiteNetBasePeer(NetPeer netPeer, MessageWriter messageWriter, RecyclableObjectPool<ByteStreamWriter> byteStreamWriterPool)
        {
            NetPeer = netPeer;
            MessageWriter = messageWriter;
            ByteStreamWriterPool = byteStreamWriterPool;
        }

        /// <summary>
        /// Sends wrapped message to this peer
        /// </summary>
        /// <param name="messageWrapper">Message Wrapper</param>
        protected void Send(MessageWrapper messageWrapper)
        {
            var byteStreamWriter = ByteStreamWriterPool.Get();

            try
            {
                MessageWriter.WriteMessage(byteStreamWriter, messageWrapper);
                NetPeer.Send(byteStreamWriter.Bytes, messageWrapper.Channel, (DeliveryMethod)messageWrapper.DeliveryMethod);
            }
            finally
            {
                ByteStreamWriterPool.Return(byteStreamWriter);
            }
        }
    }
}
