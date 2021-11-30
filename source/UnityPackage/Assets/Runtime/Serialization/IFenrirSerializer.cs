using System;

namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Serialized data from/into a binary format, to be sent/received over the network
    /// </summary>
    public interface IFenrirSerializer
    {
        /// <summary>
        /// Serializes message into a given byte stream
        /// </summary>
        /// <param name="data">Instance of a data type</param>
        /// <param name="byteStreamWriter">Byte stream writer to write values into</param>
        void Serialize(object data, IByteStreamWriter byteStreamWriter);

        /// <summary>
        /// Deserializes message from a given byte stream
        /// </summary>
        /// <typeparam name="TData">Message data type</typeparam>
        /// <param name="byteStreamReader">Byte stream reader to read values from</param>
        /// <returns>New instance of a given type</returns>
        TData Deserialize<TData>(IByteStreamReader byteStreamReader)
            where TData : new();

        /// <summary>
        /// Deserializes message from a given byte stream
        /// </summary>
        /// <param name="type">Type of data to deserialize</param>
        /// <param name="byteStreamReader">Byte stream reader to read values from</param>
        /// <returns>New instance of a given type</returns>
        object Deserialize(Type type, IByteStreamReader byteStreamReader);
    }
}