using System;

namespace Fenrir.Multiplayer.Serialization
{
    public interface ISerializationProvider
    {
        void Serialize(object data, IByteStreamWriter byteStreamWriter);

        TData Deserialize<TData>(IByteStreamReader byteStreamReader)
            where TData : new();

        object Deserialize(Type type, IByteStreamReader byteStreamReader);
    }
}