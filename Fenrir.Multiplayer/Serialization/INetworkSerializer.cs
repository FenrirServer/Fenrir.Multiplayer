using System;

namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Serialized data from/into a binary format, to be sent/received over the network
    /// </summary>
    public interface INetworkSerializer
    {
        /// <summary>
        /// Serializes data into a given byte stream
        /// </summary>
        /// <param name="data">Instance of a data type</param>
        /// <param name="byteStreamWriter">Byte stream writer to write values into</param>
        void Serialize(object data, IByteStreamWriter byteStreamWriter);

        /// <summary>
        /// Serializes data into a given byte stream with explicitly passed data type
        /// Use this method if object can be nullable
        /// </summary>
        /// <param name="data">Instance of a data type</param>
        /// <param name="dataType">Type of the data</param>
        /// <param name="byteStreamWriter">Byte stream writer to write values into</param>
        void Serialize(object data, Type dataType, IByteStreamWriter byteStreamWriter);


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

        /// <summary>
        /// Assigns type serializer for all unknown types
        /// </summary>
        /// <param name="typeSerializer">Type serializer</param>
        void SetTypeSerializer(ITypeSerializer typeSerializer);

        /// <summary>
        /// Sets type serializer for a given type. 
        /// If <see cref="Serialize(object, IByteStreamWriter)"/> or <see cref="Deserialize(Type, IByteStreamReader)"/> are invoked,
        /// this serializer will be used if type matches.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="typeSerializer">Type serializer</param>
        void AddTypeSerializer<T>(ITypeSerializer<T> typeSerializer);

        /// <summary>
        /// Removes type serializer for a given type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        void RemoveTypeSerializer<T>();

        /// <summary>
        /// Adds type factory for a given byte stream serializable type.
        /// If type factory is not set, <seealso cref="Activator.CreateInstance(Type)"/> is used to create a new instance.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="factoryMethod">Factory method</param>
        void AddTypeFactory<T>(Func<T> factoryMethod) where T : IByteStreamSerializable;

        /// <summary>
        /// Removes type factory for a given type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        void RemoveTypeFactory<T>() where T : IByteStreamSerializable;
    }
}