using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using LiteNetLib.Utils;

namespace Fenrir.Multiplayer.LiteNet
{
    class LiteNetMessageWriter
    {
        private readonly ISerializationProvider _serializerProvider;
        private readonly ITypeMap _typeMap;
        private readonly RecyclableObjectPool<ByteStreamWriter> _byteStreamWriterPool;

        public LiteNetMessageWriter(ISerializationProvider serializerProvider, ITypeMap typeMap, RecyclableObjectPool<ByteStreamWriter> byteStreamWriterPool)
        {
            _serializerProvider = serializerProvider;
            _typeMap = typeMap;
            _byteStreamWriterPool = byteStreamWriterPool;
        }

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
                _serializerProvider.Serialize(messageWrapper.MessageData, byteStreamWriter); // Serialize into remaining bytes
            }
            finally
            {
                byteStreamWriter.SetNetDataWriter(null);
                _byteStreamWriterPool.Return(byteStreamWriter);
            }
        }
    }
}
