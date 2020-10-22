using System;

namespace Fenrir.Multiplayer.Serialization
{
    class ByteStreamSerializer
    {
        public byte[] Serialize(IByteStreamSerializable data)
        {
            var byteArrayWriter = new ByteStreamWriter();

            data.Serialize(byteArrayWriter);

            return byteArrayWriter.ToByteArray();
        }

        public TData Deserialize<TData>(byte[] bytes) where TData : class, new()
        {
            var byteArrayReader = new ByteStreamReader();

            data.Serialize(byteArrayWriter);

            return byteArrayWriter.ToByteArray();
        }

        public object Deserialize(Type type, byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
