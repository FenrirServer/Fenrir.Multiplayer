using System;

namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Provides means of data serialization 
    /// </summary>
    public interface ISerializationProvider
    {
        /// <summary>
        /// Serializes data contract into a given byte stream
        /// </summary>
        /// <param name="data">Instance of a data contract</param>
        /// <param name="byteStreamWriter">Byte stream writer to write values into</param>
        void Serialize(object data, IByteStreamWriter byteStreamWriter);

        /// <summary>
        /// Deserializes data contract from a given byte stream
        /// </summary>
        /// <typeparam name="TData">Type of data contract</typeparam>
        /// <param name="byteStreamReader">Byte stream reader to read values from</param>
        /// <returns>New instance of a given type</returns>
        TData Deserialize<TData>(IByteStreamReader byteStreamReader)
            where TData : new();

        /// <summary>
        /// Deserializes data contract from a given byte stream
        /// </summary>
        /// <param name="type">Type of data contract</param>
        /// <param name="byteStreamReader">Byte stream reader to read values from</param>
        /// <returns>New instance of a given type</returns>
        object Deserialize(Type type, IByteStreamReader byteStreamReader);
    }
}