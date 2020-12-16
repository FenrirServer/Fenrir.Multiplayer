using Fenrir.Multiplayer.Network;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// Base LiteNet peer
    /// </summary>
    class LiteNetBasePeer : IPeerInternal
    {
        /// <summary>
        /// Net Data Writer - buffer used to send data to this peer
        /// </summary>
        private readonly NetDataWriter _netDataWriter;

        /// <summary>
        /// LiteNet Peer
        /// </summary>
        protected NetPeer NetPeer { get; private set; }

        /// <summary>
        /// MessageWriter - serializes and write message into NetDataWriter
        /// </summary>
        protected LiteNetMessageWriter MessageWriter { get; private set; }


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="netPeer">LiteNet NetPeer</param>
        /// <param name="messageWriter">Message Writer</param>
        public LiteNetBasePeer(NetPeer netPeer, LiteNetMessageWriter messageWriter)
        {
            NetPeer = netPeer;
            MessageWriter = messageWriter;
            _netDataWriter = new NetDataWriter();
        }

        /// <summary>
        /// Sends wrapped message to this peer
        /// </summary>
        /// <param name="messageWrapper">Message Wrapper</param>
        public void Send(MessageWrapper messageWrapper)
        {
            _netDataWriter.Reset();
            MessageWriter.WriteMessage(_netDataWriter, messageWrapper);
            NetPeer.Send(_netDataWriter, messageWrapper.Channel, (DeliveryMethod)messageWrapper.DeliveryMethod);
        }
    }
}
