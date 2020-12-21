using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using LiteNetLib.Utils;
using System.Runtime.Serialization;

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// LiteNet Message Writer
    /// Used to serialize outgoing messages
    /// </summary>
    class LiteNetMessageWriter
    {
        /// <summary>
        /// Serialization provider - used for serializing and deserializing messages
        /// </summary>
        private readonly ISerializationProvider _serializationProvider;

        /// <summary>
        /// Type map - contians list of types and hashes
        /// </summary>
        private readonly ITypeHashMap _typeHashMap;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly IFenrirLogger _logger;

        /// <summary>
        /// Object pool of Byte Stream Writers - used to write bytes for outgoing messages
        /// </summary>
        private readonly RecyclableObjectPool<ByteStreamWriter> _byteStreamWriterPool;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="serializationProvider">Serialization Provider</param>
        /// <param name="typeHashMap">Type Hash Map</param>
        /// <param name="logger">Logger</param>
        /// <param name="byteStreamWriterPool">Object pool of Byte Stream Writers</param>
        public LiteNetMessageWriter(ISerializationProvider serializationProvider, ITypeHashMap typeHashMap, IFenrirLogger logger, RecyclableObjectPool<ByteStreamWriter> byteStreamWriterPool)
        {
            _serializationProvider = serializationProvider;
            _typeHashMap = typeHashMap;
            _logger = logger;
            _byteStreamWriterPool = byteStreamWriterPool;
        }

        /// <summary>
        /// Writes an outgoing message wrapper into a byte stream (NetDataWriter)
        /// </summary>
        /// <param name="netDataWriter">NetDataWriter to write into</param>
        /// <param name="messageWrapper">Message Wrapper - outgoing message</param>
        public void WriteMessage(NetDataWriter netDataWriter, MessageWrapper messageWrapper)
        {
            // Message format: 
            // [8 bytes long message type hash]
            // [2 bytes short flags]
            //    [1 bit encrypted yes/no]
            //    [1 bit reserved for MessageFlags]
            //    [1 bit reserved for MessageFlags]
            //    [1 bit reserved for MessageFlags]
            //    [12 bit request id]
            // [N bytes serialized message]

            // 1. ulong Message type hash
            ulong messageTypeHash = _typeHashMap.GetTypeHash(messageWrapper.MessageData.GetType());
            netDataWriter.Put(messageTypeHash); // Type hash

            // 2. short Flags + request id
            MessageFlags messageFlags = MessageFlags.None;
            if(messageWrapper.IsEncrypted)
            {
                messageFlags |= MessageFlags.Encrypted;
            }
            ushort flags = 0;
            flags |= (ushort)(messageWrapper.RequestId << 4);
            flags |= (ushort)messageFlags;

            netDataWriter.Put(flags);

            // 3. byte[] Serialized message
            ByteStreamWriter byteStreamWriter = _byteStreamWriterPool.Get();
            byteStreamWriter.SetNetDataWriter(netDataWriter);

            try
            {
                _serializationProvider.Serialize(messageWrapper.MessageData, byteStreamWriter); // Serialize into remaining bytes
            }
            finally
            {
                byteStreamWriter.SetNetDataWriter(null);
                _byteStreamWriterPool.Return(byteStreamWriter);
            }
        }
    }
}
