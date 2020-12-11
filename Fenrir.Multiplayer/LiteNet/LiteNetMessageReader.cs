using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using LiteNetLib.Utils;
using System;

namespace Fenrir.Multiplayer.LiteNet
{
    class LiteNetMessageReader
    {
        private readonly ISerializationProvider _serializerProvider;
        private readonly ITypeMap _typeMap;
        private readonly RecyclableObjectPool<ByteStreamReader> _byteStreamReaderPool;

        public LiteNetMessageReader(ISerializationProvider serializerProvider, ITypeMap typeMap, RecyclableObjectPool<ByteStreamReader> byteStreamReaderPool)
        {
            _serializerProvider = serializerProvider;
            _typeMap = typeMap;
            _byteStreamReaderPool = byteStreamReaderPool;
        }

        public MessageWrapper ReadMessage(NetDataReader netDataReader)
        {
            MessageType messageType = (MessageType)netDataReader.GetByte(); // Type of the message
            ulong messageTypeHash = netDataReader.GetULong(); // Type hash
            int requestId = 0;
            if(messageType == MessageType.Request || messageType == MessageType.Response)
            {
                requestId = netDataReader.GetInt(); // Request id
            }

            Type dataType = _typeMap.GetTypeByHashInternal(messageTypeHash);

            ByteStreamReader byteStreamReader = _byteStreamReaderPool.Get();
            byteStreamReader.SetNetDataReader(netDataReader);

            try
            {
                object data = _serializerProvider.Deserialize(dataType, byteStreamReader); // Deserialize remaining bytes

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
