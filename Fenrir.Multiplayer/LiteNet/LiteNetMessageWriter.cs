using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using LiteNetLib.Utils;

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
        /// Object pool of Byte Stream Writers - used to write bytes for outgoing messages
        /// </summary>
        private readonly RecyclableObjectPool<ByteStreamWriter> _byteStreamWriterPool;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="serializationProvider">Serialization Provider</param>
        /// <param name="typeMap">Type Map</param>
        /// <param name="byteStreamWriterPool">Object pool of Byte Stream Writers</param>
        public LiteNetMessageWriter(ISerializationProvider serializationProvider, ITypeMap typeMap, RecyclableObjectPool<ByteStreamWriter> byteStreamWriterPool)
        {
            _serializationProvider = serializationProvider;
            _typeMap = typeMap;
            _byteStreamWriterPool = byteStreamWriterPool;
        }

        /// <summary>
        /// Writes an outgoing message wrapper into a byte stream (NetDataWriter)
        /// </summary>
        /// <param name="netDataWriter">NetDataWriter to write into</param>
        /// <param name="messageWrapper">Message Wrapper - outgoing message</param>
        public void WriteMessage(NetDataWriter netDataWriter, MessageWrapper messageWrapper)
        {
            ulong messageTypeHash = _typeMap.GetTypeHash(messageWrapper.MessageData.GetType());

            ByteStreamWriter byteStreamWriter = _byteStreamWriterPool.Get();
            byteStreamWriter.SetNetDataWriter(netDataWriter);


            netDataWriter.Put((byte)messageWrapper.MessageType); // Type of the message
            if(messageWrapper.MessageType == MessageType.Request || messageWrapper.MessageType == MessageType.Response)
            {
                netDataWriter.Put(messageWrapper.RequestId); // Request id
            }

            netDataWriter.Put(messageTypeHash); // Type hash

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
