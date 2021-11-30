using System;

namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Data Type Serializer
    /// Allows for serialization and deserialization of custom or unknown types, such as data contracts.
    /// </summary>
    public interface ITypeSerializer
    {
        /// <summary>
        /// Deserialize data of a given type
        /// </summary>
        /// <param name="type">Type of data being deserialized</param>
        /// <param name="byteStreamReader">Byte stream reader to read data from</param>
        /// <returns>New instance of a given type</returns>
        object Deserialize(Type type, IByteStreamReader byteStreamReader);

        /// <summary>
        /// Serializes data
        /// </summary>
        /// <param name="data">Instance of a data</param>
        /// <param name="byteStreamWriter">Byte stream reader to write data into</param>
        void Serialize(object data, IByteStreamWriter byteStreamWriter);
    }

    /// <summary>
    /// Data Type Serializer
    /// Allows for serialization and deserialization of a given type of type <typeparamref name="T"/>
    /// </summary>
    public interface ITypeSerializer<T>
    {
        /// <summary>
        /// Deserialize data of a given type
        /// </summary>
        /// <param name="type">Type of data</param>
        /// <param name="byteStreamReader">Byte stream writer to read data from</param>
        /// <returns>New instance of a given type</returns>
        T Deserialize(IByteStreamReader byteStreamReader);

        /// <summary>
        /// Serializes data
        /// </summary>
        /// <param name="data">Instance of data</param>
        /// <param name="byteStreamWriter">Byte stream writer to write data into</param>
        void Serialize(T data, IByteStreamWriter byteStreamWriter);
    }
}
