using Fenrir.Multiplayer.Network;
using LiteNetLib;

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// LiteNet implemenation of a Server Peer
    /// </summary>
    class LiteNetServerPeer : LiteNetBasePeer, IServerPeer
    {
        /// <summary>
        /// Stores current latency (roundtrip between server and client)
        /// </summary>
        private volatile int _latency;

        /// <inheritdoc/>
        public int Latency => _latency;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="netPeer">LiteNet NetPeer</param>
        /// <param name="messageWriter">Message Writer</param>
        public LiteNetServerPeer(NetPeer netPeer, LiteNetMessageWriter messageWriter)
            : base(netPeer, messageWriter)
        {
            NetPeer.Tag = this;
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Sets latency
        /// </summary>
        /// <param name="latency">Latency</param>
        public void SetLatency(int latency)
        {
            _latency = latency;
        }
    }
}
