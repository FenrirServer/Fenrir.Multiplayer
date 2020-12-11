using Fenrir.Multiplayer.Network;
using LiteNetLib;

namespace Fenrir.Multiplayer.LiteNet
{
    class LiteNetServerPeer : LiteNetBasePeer, IServerPeer
    {
        private volatile int _latency;

        public int Latency => _latency;

        public LiteNetServerPeer(NetPeer netPeer, LiteNetMessageWriter messageWriter)
            : base(netPeer, messageWriter)
        {
            NetPeer.Tag = this;
        }

        public void SendEvent<TEvent>(TEvent evt, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered) where TEvent : IEvent
        {
            var messageWrapper = new MessageWrapper()
            {
                MessageType = MessageType.Event,
                MessageData = evt,
                Peer = this,
                Channel = channel,
                DeliveryMethod = deliveryMethod,
            };

            Send(messageWrapper);
        }

        public void SetLatency(int latency)
        {
            _latency = latency;
        }
    }
}
