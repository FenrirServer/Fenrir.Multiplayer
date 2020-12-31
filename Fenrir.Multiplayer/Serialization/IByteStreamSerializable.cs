namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Allows classes that implement that interface to be serialized/deserialized by the byte stream serializer/deserializer
    /// </summary>
    public interface IByteStreamSerializable
    {
        /// <summary>
        /// Serializes object into a byte stream
        /// </summary>
        /// <param name="writer">Byte stream writer. Use byte stream writer to write values from your class into a byte stream.</param>
        void Serialize(IByteStreamWriter writer);

        /// <summary>
        /// Deserializes object from a byte stream
        /// </summary>
        /// <param name="reader">Byte stream reader. Use byte stream reader to read values from byte stream into your class.</param>
        void Deserialize(IByteStreamReader reader);
    }
}
