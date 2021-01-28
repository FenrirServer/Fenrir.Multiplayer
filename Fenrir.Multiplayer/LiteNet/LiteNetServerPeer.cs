using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
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
        /// Version of the peer protocol
        /// </summary>
        public int ProtocolVersion { get; private set; }

        /// <summary>
        /// Custom peer data object that can be assigned to each peer
        /// </summary>
        public object PeerData { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="peerId">Unique id of the client</param>
        /// <param name="protocolVersion">Peer protocol version</param>
        /// <param name="netPeer">LiteNet NetPeer</param>
        /// <param name="messageWriter">Message Writer</param>
        /// <param name="byteStreamWriterPool">Byte Stream Writer Object Pool</param>
        public LiteNetServerPeer(string peerId, int protocolVersion, NetPeer netPeer, MessageWriter messageWriter, RecyclableObjectPool<ByteStreamWriter> byteStreamWriterPool)
            : base(peerId, netPeer, messageWriter, byteStreamWriterPool)
        {
            ProtocolVersion = protocolVersion; 
        }

        /// <inheritdoc/>
        public void SendEvent<TEvent>(TEvent evt, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered)
            where TEvent : IEvent
        {
            // By default, all reliable messages are encrypted
            bool encrypted = deliveryMethod == MessageDeliveryMethod.ReliableOrdered || deliveryMethod == MessageDeliveryMethod.ReliableUnordered;         
            
            SendEvent<TEvent>(evt, encrypted, channel, deliveryMethod);
        }


        /// <inheritdoc/>
        public void SendEvent<TEvent>(TEvent evt, bool encrypted, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered) 
            where TEvent : IEvent
        {
            MessageFlags flags = encrypted ? MessageFlags.IsEncrypted : MessageFlags.None; // other flags are ignored for events
            MessageWrapper messageWrapper = MessageWrapper.WrapEvent(evt, channel, flags, deliveryMethod);
            Send(messageWrapper);
        }

        /// <inheritdoc/>
        public void SendResponse<TResponse>(TResponse response, short requestId, bool encrypted = true, byte channel = 0, bool ordered = true)
            where TResponse : IResponse
        {
            MessageDeliveryMethod deliveryMethod = ordered ? MessageDeliveryMethod.ReliableOrdered : MessageDeliveryMethod.ReliableUnordered; // Responses are always reliable

            MessageFlags flags = MessageFlags.HasRequestId; // Responses always have request id
            if(encrypted)
            {
                flags |= MessageFlags.IsEncrypted;
            }
            if(ordered)
            {
                flags |= MessageFlags.IsOrdered;
            }

            MessageWrapper messageWrapper = MessageWrapper.WrapResponse(response, requestId, channel, flags, deliveryMethod);
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


        /// <inheritdoc/>
        public void Disconnect()
        {
            NetPeer.Disconnect();
        }
    }
}
