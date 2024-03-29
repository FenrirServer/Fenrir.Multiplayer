﻿using LiteNetLib;
using System.Net;

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
        /// Peer id
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Remote Endpoint
        /// </summary>
        public EndPoint EndPoint => NetPeer;

        /// <summary>
        /// Average Round trip time
        /// </summary>
        public int RoundTripTime => NetPeer?.RoundTripTime ?? 0;

        /// <summary>
        /// MTU
        /// </summary>
        public int Mtu => NetPeer?.Mtu ?? 0;

        /// <summary>
        /// If set to true, outgoing messages will contain debug info.
        /// Setting this to true affects performance and should be disabled in production builds.
        /// </summary>
        public bool WriteDebugInfo { get; set; } = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="id">Unique peer id</param>
        /// <param name="netPeer">LiteNet NetPeer</param>
        /// <param name="messageWriter">Message Writer</param>
        /// <param name="byteStreamWriterPool">Byte Stream Writer object pool</param>
        public LiteNetBasePeer(string id, NetPeer netPeer, MessageWriter messageWriter, RecyclableObjectPool<ByteStreamWriter> byteStreamWriterPool)
        {
            Id = id;
            NetPeer = netPeer;
            MessageWriter = messageWriter;
            ByteStreamWriterPool = byteStreamWriterPool;

#if DEBUG
            WriteDebugInfo = true;
#endif
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

        /// <summary>
        /// Returns debug flags if <see cref="WriteDebugInfo"/> is set to true
        /// </summary>
        /// <returns>Message Flags</returns>
        protected MessageFlags GetDebugFlags()
        {
            return WriteDebugInfo ? MessageFlags.IsDebug : MessageFlags.None;
        }
    }
}
