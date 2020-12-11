using Fenrir.Multiplayer.Network;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Fenrir.Multiplayer.LiteNet
{
    class LiteNetBasePeer : IPeerInternal
    {
        private readonly NetDataWriter _netDataWriter;
        protected NetPeer NetPeer { get; private set; }
        protected LiteNetMessageWriter MessageWriter { get; private set; }

        public LiteNetBasePeer(NetPeer netPeer, LiteNetMessageWriter messageWriter)
        {
            NetPeer = netPeer;
            MessageWriter = messageWriter;
            _netDataWriter = new NetDataWriter();
        }

        public void Send(MessageWrapper messageWrapper)
        {
            _netDataWriter.Reset();
            MessageWriter.WriteMessage(_netDataWriter, messageWrapper);
            NetPeer.Send(_netDataWriter, messageWrapper.Channel, (DeliveryMethod)messageWrapper.DeliveryMethod);
        }
    }
}
