namespace Fenrir.Multiplayer.Serialization
{
    public interface IByteStreamSerializable
    {
        void Serialize(IByteStreamWriter writer);

        void Deserialize(IByteStreamReader reader);
    }
}
