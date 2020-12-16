using System;

namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Data Contract Serializer
    /// Classes that implement allow for custom data contract serialization.
    /// Implement and assign to <seealso cref="ISerializationProvider"/>
    /// </summary>
    public interface IContractSerializer
    {
        /// <summary>
        /// Deserialize data contract of a given type
        /// </summary>
        /// <typeparam name="TData">Type of data contract</typeparam>
        /// <param name="byteStreamReader">Byte stream reader to read data from</param>
        /// <returns>New instance of a given type</returns>
        TData Deserialize<TData>(IByteStreamReader byteStreamReader)
            where TData : new();

        /// <summary>
        /// Deserialize data contract of a given type
        /// </summary>
        /// <param name="type">Type of data contract</param>
        /// <param name="byteStreamReader">Byte stream reader to read data from</param>
        /// <returns>New instance of a given type</returns>
        object Deserialize(Type type, IByteStreamReader byteStreamReader);

        /// <summary>
        /// Serializes data contract
        /// </summary>
        /// <param name="data">Instance of a data contract</param>
        /// <param name="byteStreamWriter">Byte stream writer to write data into</param>
        void Serialize(object data, IByteStreamWriter byteStreamWriter);
    }
}
