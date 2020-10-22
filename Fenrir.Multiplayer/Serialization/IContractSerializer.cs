using System;

namespace Fenrir.Multiplayer.Serialization
{
    public interface IContractSerializer
    {
        TData Deserialize<TData>(IByteStreamReader byteStreamReader)
            where TData : new();

        object Deserialize(Type type, IByteStreamReader byteStreamReader);

        void Serialize(object data, IByteStreamWriter byteStreamWriter);
    }
}
