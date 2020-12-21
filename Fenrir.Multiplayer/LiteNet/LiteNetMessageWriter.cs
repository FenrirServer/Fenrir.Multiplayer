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
        private readonly ITypeMap _typeMap;

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
        /// <param name="typeMap">Type Map</param>
        /// <param name="logger">Logger</param>
        /// <param name="byteStreamWriterPool">Object pool of Byte Stream Writers</param>
        public LiteNetMessageWriter(ISerializationProvider serializationProvider, ITypeMap typeMap, IFenrirLogger logger, RecyclableObjectPool<ByteStreamWriter> byteStreamWriterPool)
        {
            _serializationProvider = serializationProvider;
            _typeMap = typeMap;
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
            // 1. [byte] Type of the message
            netDataWriter.Put((byte)messageWrapper.MessageType); // Type of the message

            // 2. [int] Request Id (if message type is request)
            if (messageWrapper.MessageType == MessageType.Request || messageWrapper.MessageType == MessageType.Response)
            {
                netDataWriter.Put(messageWrapper.RequestId); // Request id
            }

            // 3. [ulong] Message type hash
            ulong messageTypeHash = _typeMap.GetTypeHash(messageWrapper.MessageData.GetType());

            netDataWriter.Put(messageTypeHash); // Type hash

            // 4. [byte[]] Serialized message
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
