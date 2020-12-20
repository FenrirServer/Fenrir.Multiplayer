using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using LiteNetLib.Utils;
using System;

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// LiteNet Message reader
    /// Reads incoming messages and deserializes them into a message wrapper
    /// </summary>
    class LiteNetMessageReader
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
        /// Object pool of byte stream readers - used for incoming messages
        /// </summary>
        private readonly RecyclableObjectPool<ByteStreamReader> _byteStreamReaderPool;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="serializationProvider">Serialization Provider</param>
        /// <param name="typeMap">Type Map</param>
        /// <param name="byteStreamReaderPool">Object pool of Byte Stream Readers</param>
        public LiteNetMessageReader(ISerializationProvider serializationProvider, ITypeMap typeMap, RecyclableObjectPool<ByteStreamReader> byteStreamReaderPool)
        {
            _serializationProvider = serializationProvider;
            _typeMap = typeMap;
            _byteStreamReaderPool = byteStreamReaderPool;
        }

        /// <summary>
        /// Reads an incoming message from byte stream (NetDataReader) and creates a message wrapper
        /// </summary>
        /// <param name="netDataReader">LiteNet NetDataReader</param>
        /// <returns>Message Wrapper</returns>
        public MessageWrapper ReadMessage(NetDataReader netDataReader)
        {
            MessageType messageType = (MessageType)netDataReader.GetByte(); // Type of the message
            int requestId = 0;
            if(messageType == MessageType.Request || messageType == MessageType.Response)
            {
                requestId = netDataReader.GetInt(); // Request id
            }

            ulong messageTypeHash = netDataReader.GetULong(); // Type hash

            Type dataType = _typeMap.GetTypeByHash(messageTypeHash);

            ByteStreamReader byteStreamReader = _byteStreamReaderPool.Get();
            byteStreamReader.SetNetDataReader(netDataReader);

            try
            {
                object data = _serializationProvider.Deserialize(dataType, byteStreamReader); // Deserialize remaining bytes

                return new MessageWrapper()
                {
                    MessageType = messageType,
                    RequestId = requestId,
                    MessageData = data
                };
            }
            finally
            {
                byteStreamReader.SetNetDataReader(null);
                _byteStreamReaderPool.Return(byteStreamReader);
            }
        }
    }
}
