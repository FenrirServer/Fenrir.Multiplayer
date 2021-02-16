using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Serialized data from/into a binary format, to be sent/received over the network
    /// </summary>
    public class NetworkSerializer : INetworkSerializer
    {
        /// <summary>
        /// Custom type serializer. If assigned, will be used for all types that
        /// do not implement <seealso cref="IByteStreamSerializable"/>
        /// </summary>
        private ITypeSerializer _typeSerializer;

        /// <summary>
        /// Custom type deserialization callbacks
        /// </summary>
        private Dictionary<Type, Func<Type, IByteStreamReader, object>> _typeDeserializers = new Dictionary<Type, Func<Type, IByteStreamReader, object>>();

        /// <summary>
        /// Custom type serialization callbacks
        /// </summary>
        private Dictionary<Type, Action<object, IByteStreamWriter>> _typeSerializers = new Dictionary<Type, Action<object, IByteStreamWriter>>();

        /// <summary>
        /// Factories for given types
        /// </summary>
        private Dictionary<Type, Func<IByteStreamSerializable>> _byteStreamSerializableTypeFactories = new Dictionary<Type, Func<IByteStreamSerializable>>();

        /// <summary>
        /// Thread-static variable to detect infinite recursion in serialization.
        /// </summary>
        [ThreadStatic]
        private static int _currentDepth = 0;

        /// <summary>
        /// Maximum serialization depth.
        /// If <see cref="Serializer"/>/<see cref="Deserialize(Type, IByteStreamReader)"/> calls reach this depth, <seealso cref="SerializationException"/> is thrown.
        /// </summary>
        public int MaxDepth { get; set; } = 100;

        /// <summary>
        /// Creates Network Serializer
        /// </summary>
        public NetworkSerializer()
        {
        }

        #region Serialize
        /// <inheritdoc/>
        public void Serialize(object data, IByteStreamWriter byteStreamWriter)
        {
            Serialize(data, data?.GetType(), byteStreamWriter);
        }

        /// <inheritdoc/>
        public void Serialize(object data, Type dataType, IByteStreamWriter byteStreamWriter)
        {
            // Increment current depth of serialization
            _currentDepth++;
            
            if(_currentDepth > MaxDepth)
            {
                throw new SerializationException($"Failed to serialize {data.GetType().Name}: maximum depth reached. Possible infinite recursion detected. If this is expected, consider increasing {nameof(MaxDepth)}");
            }

            try
            {
                SerializeInternal(data, dataType, byteStreamWriter);
            }
            finally
            {
                // Decrement current depth of serialization
                _currentDepth--;
            }
        }

        private void SerializeInternal(object data, Type dataType, IByteStreamWriter byteStreamWriter)
        {
            // Check if data is a null object.
            if(data == null)
            {
                byteStreamWriter.Write(false); // No data ahead
                return;
            }

            // If data is a reference type, write a flag indicating that we have an object ahead
            if(!dataType.IsValueType)
            {
                byteStreamWriter.Write(true); // Has data ahead
            }

            // Try to serialize known type
            if (TrySerializeKnownType(data, dataType, byteStreamWriter))
            {
                return;
            }

            // Check if object implements IByteStreamSerializable
            IByteStreamSerializable byteStreamSerializable = data as IByteStreamSerializable;
            if (byteStreamSerializable != null)
            {
                try
                {
                    byteStreamSerializable.Serialize(byteStreamWriter);
                }
                catch (Exception e)
                {
                    throw new SerializationException($"Failed to serialize {data.GetType().Name} using {nameof(IByteStreamSerializable)}.{nameof(IByteStreamSerializable.Deserialize)}: " + e.Message, e);
                }

                return;
            }

            // Check this specific type is bound to a type serializer
            if (_typeSerializers.TryGetValue(data.GetType(), out Action<object, IByteStreamWriter> serializeHandler))
            {
                serializeHandler.Invoke(data, byteStreamWriter); // Callback handles the exception
                return;
            }

            // Check if catch-all type serializer is set, if so, attmept to use it
            if (_typeSerializer != null)
            {
                try
                {
                    _typeSerializer.Serialize(data, byteStreamWriter);
                }
                catch (Exception e)
                {
                    throw new SerializationException($"Failed to serialize {data.GetType().Name} using {_typeSerializer.GetType().Name}: " + e.Message, e);
                }
                return;
            }

            // No serializer was found for this type
            throw new SerializationException($"Failed to serialize {data.GetType().Name}: type does not implement {nameof(IByteStreamSerializable)} and no type serializer is found for the type");
        }
        #endregion

        #region Deserialize
        /// <inheritdoc/>
        public TData Deserialize<TData>(IByteStreamReader byteStreamReader)
            where TData : new()
        {
            return (TData)Deserialize(typeof(TData), byteStreamReader);
        }

        /// <inheritdoc/>
        public object Deserialize(Type type, IByteStreamReader byteStreamReader)
        {            
            // Increment current depth of serialization
            _currentDepth++;

            if (_currentDepth > MaxDepth)
            {
                throw new SerializationException($"Failed to deserialize {type.Name}: maximum depth reached. Possible infinite recursion detected. If this is expected, consider increasing {nameof(MaxDepth)}");
            }

            try
            {
                return DeserializeInternal(type, byteStreamReader);
            }
            finally
            {
                // Decrement current depth of serialization
                _currentDepth--;
            }
        }

        private object DeserializeInternal(Type dataType, IByteStreamReader byteStreamReader)
        {
            // Check end of stream
            if (byteStreamReader.EndOfData)
            {
                throw new SerializationException($"Failed to deserialize {dataType.Name}, unexpected end of the stream");
            }

            // If data is a reference type, check if object lies ahead. If not, return null.
            if (!dataType.IsValueType)
            {
                bool hasObject = byteStreamReader.ReadBool();
                if (!hasObject)
                {
                    return null;
                }
            }

            // Try to deserialize as a known type
            if(TryDeserializeKnownType(dataType, byteStreamReader, out object data))
            {
                return data;
            }

            // Check if type is IByteStreamSerializable
            if (typeof(IByteStreamSerializable).IsAssignableFrom(dataType))
            {
                IByteStreamSerializable byteStreamSerializable;

                if (_byteStreamSerializableTypeFactories.TryGetValue(dataType, out Func<IByteStreamSerializable> factoryMethod))
                {
                    // Create new instance using factory method
                    byteStreamSerializable = factoryMethod();
                }
                else
                {
                    // Create new instance using activator
                    byteStreamSerializable = (IByteStreamSerializable)Activator.CreateInstance(dataType);
                }

                try
                {
                    byteStreamSerializable.Deserialize(byteStreamReader);
                }
                catch (Exception e)
                {
                    throw new SerializationException($"Failed to deserialize {dataType.Name} using {nameof(IByteStreamSerializable)}.{nameof(IByteStreamSerializable.Deserialize)}: " + e.Message, e);
                }

                return byteStreamSerializable;
            }

            // Check this specific type is bound to a type serializer
            if (_typeDeserializers.TryGetValue(dataType, out Func<Type, IByteStreamReader, object> deserializeHandler))
            {
                return deserializeHandler.Invoke(dataType, byteStreamReader); // Callback handles the exception
            }

            // Check if custom type serializer is set, fall back to it
            if (_typeSerializer != null)
            {
                try
                {
                    return _typeSerializer.Deserialize(dataType, byteStreamReader);
                }
                catch (Exception e)
                {
                    throw new SerializationException($"Failed to deserialize {dataType.Name} using {_typeSerializer.GetType().Name}: " + e.Message, e);
                }
            }

            // Nothing found
            throw new SerializationException($"Failed to deserialize {dataType.Name}: type does not implement {nameof(IByteStreamSerializable)} and no type serializer is found for the type");
        }
        #endregion

        #region Type Serializer
        /// <inheritdoc />
        public void SetTypeSerializer(ITypeSerializer typeSerializer)
        {
            _typeSerializer = typeSerializer;
        }

        /// <inheritdoc />
        public void AddTypeSerializer<T>(ITypeSerializer<T> typeSerializer)
        {
            if(typeSerializer == null)
            {
                throw new ArgumentNullException(nameof(typeSerializer));
            }

            if (_typeSerializers.ContainsKey(typeof(T)) || _typeDeserializers.ContainsKey(typeof(T)))
            {
                throw new ArgumentException($"Type Serializer is already set for the type {typeof(T).Name}");
            }

            // Serializer
            _typeSerializers.Add(typeof(T), (obj, byteStreamWriter) =>
            {
                try
                {
                    typeSerializer.Serialize((T)obj, byteStreamWriter);
                }
                catch(Exception e)
                {
                    throw new SerializationException($"Failed to serialize {typeof(T).Name} using {typeSerializer.GetType().Name}: " + e.Message, e);
                }
            });

            // Deserializer
            _typeDeserializers.Add(typeof(T), (type, byteStreamReader) =>
            {
                try
                {
                    return typeSerializer.Deserialize(byteStreamReader);
                }
                catch (Exception e)
                {
                    throw new SerializationException($"Failed to deserialize {typeof(T).Name} using {typeSerializer.GetType().Name}: " + e.Message, e);
                }
            });
        }

        /// <inheritdoc />
        public void RemoveTypeSerializer<T>()
        {
            if(_typeSerializers.ContainsKey(typeof(T)))
            {
                _typeSerializers.Remove(typeof(T));
            }

            if (_typeDeserializers.ContainsKey(typeof(T)))
            {
                _typeDeserializers.Remove(typeof(T));
            }
        }
        #endregion

        #region Primitive / Known Type Serialization
        private bool TrySerializeKnownType(object data, Type dataType, IByteStreamWriter byteStreamWriter)
        {
            Type underlyingType = Nullable.GetUnderlyingType(dataType);
            if(underlyingType != null)
            {
                byteStreamWriter.Write(true); // If we got there, data can't be null.
                dataType = underlyingType;
            }

            // Check primitive type
            if (dataType.IsPrimitive)
            {
                if (typeof(byte) == dataType)
                {
                    byteStreamWriter.Write((byte)data);
                }
                else if (typeof(sbyte) == dataType)
                {
                    byteStreamWriter.Write((sbyte)data);
                }
                else if (typeof(char) == dataType)
                {
                    byteStreamWriter.Write((char)data);
                }
                else if (typeof(short) == dataType)
                {
                    byteStreamWriter.Write((short)data);
                }
                else if (typeof(ushort) == dataType)
                {
                    byteStreamWriter.Write((ushort)data);
                }
                else if (typeof(int) == dataType)
                {
                    byteStreamWriter.Write((int)data);
                }
                else if (typeof(uint) == dataType)
                {
                    byteStreamWriter.Write((uint)data);
                }
                else if (typeof(long) == dataType)
                {
                    byteStreamWriter.Write((long)data);
                }
                else if (typeof(ulong) == dataType)
                {
                    byteStreamWriter.Write((ulong)data);
                }
                else if (typeof(bool) == dataType)
                {
                    byteStreamWriter.Write((bool)data);
                }
                else if (typeof(float) == dataType)
                {
                    byteStreamWriter.Write((float)data);
                }
                else if (typeof(double) == dataType)
                {
                    byteStreamWriter.Write((double)data);
                }
                else
                {
                    throw new SerializationException("Unknown primitive type: " + dataType.FullName);
                }

                return true;
            }
            else if (typeof(string) == dataType)
            {
                byteStreamWriter.Write((string)data);
                return true;
            }
            else if (typeof(DateTime) == dataType)
            {
                DateTime dateTime = (DateTime)data;
                byteStreamWriter.Write(dateTime.Ticks);
                return true;
            }
            else if (typeof(TimeSpan) == dataType)
            {
                TimeSpan dateTime = (TimeSpan)data;
                byteStreamWriter.Write(dateTime.Ticks);
                return true;
            }
            else if (typeof(Array).IsAssignableFrom(dataType))
            {
                Array array = data as Array;

                byteStreamWriter.Write(array.Length);

                foreach (object element in array)
                {
                    Serialize(element, byteStreamWriter);
                }

                return true;
            }
            else if (typeof(IList).IsAssignableFrom(dataType))
            {
                IList list = data as IList;

                byteStreamWriter.Write(list.Count);

                foreach (object element in list)
                {
                    Serialize(element, byteStreamWriter);
                }

                return true;
            }
            else if (typeof(IDictionary).IsAssignableFrom(dataType))
            {
                IDictionary dictionary = data as IDictionary;

                byteStreamWriter.Write(dictionary.Count);

                foreach (object key in dictionary.Keys)
                {
                    object value = dictionary[key];

                    Serialize(key, byteStreamWriter);
                    Serialize(value, byteStreamWriter);
                }

                return true;
            }

            // Unknown type
            return false;
        }

        private bool TryDeserializeKnownType(Type dataType, IByteStreamReader byteStreamReader, out object data)
        {
            data = null;

            // Check if data type is nullable
            Type underlyingType = Nullable.GetUnderlyingType(dataType);
            if(underlyingType != null)
            {
                bool hasValue = byteStreamReader.ReadBool();
                if (!hasValue)
                {
                    return true; // Nullable with no data
                }
                
                dataType = underlyingType;
            }

            if (dataType.IsPrimitive)
            {
                if (typeof(byte) == dataType)
                {
                    data = byteStreamReader.ReadByte();
                }
                else if (typeof(sbyte) == dataType)
                {
                    data = byteStreamReader.ReadSByte();
                }
                else if (typeof(char) == dataType)
                {
                    data = byteStreamReader.ReadChar();
                }
                else if (typeof(short) == dataType)
                {
                    data = byteStreamReader.ReadShort();
                }
                else if (typeof(ushort) == dataType)
                {
                    data = byteStreamReader.ReadUShort();
                }
                else if (typeof(int) == dataType)
                {
                    data = byteStreamReader.ReadInt();
                }
                else if (typeof(uint) == dataType)
                {
                    data = byteStreamReader.ReadUInt();
                }
                else if (typeof(long) == dataType)
                {
                    data = byteStreamReader.ReadLong();
                }
                else if (typeof(ulong) == dataType)
                {
                    data = byteStreamReader.ReadULong();
                }
                else if (typeof(bool) == dataType)
                {
                    data = byteStreamReader.ReadBool();
                }
                else if (typeof(float) == dataType)
                {
                    data = byteStreamReader.ReadFloat();
                }
                else if (typeof(double) == dataType)
                {
                    data = byteStreamReader.ReadDouble();
                }
                else
                {
                    throw new SerializationException("Unknown primitive type: " + dataType.FullName);
                }

                return true;
            }
            else if (typeof(string) == dataType)
            {
                data = byteStreamReader.ReadString();
                return true;
            }
            else if (typeof(DateTime) == dataType)
            {
                long ticks = byteStreamReader.ReadLong();
                data = new DateTime(ticks);
                return true;
            }
            else if (typeof(TimeSpan) == dataType)
            {
                long ticks = byteStreamReader.ReadLong();
                data = new TimeSpan(ticks);
                return true;
            }
            else if (typeof(Array).IsAssignableFrom(dataType))
            {
                int size = byteStreamReader.ReadInt();
                Type elementType = dataType.GetElementType();

                Array array = Array.CreateInstance(elementType, size) as Array;

                for (int i = 0; i < size; i++)
                {
                    array.SetValue(Deserialize(elementType, byteStreamReader), i);
                }

                data = array;
                return true;
            }
            else if (typeof(IList).IsAssignableFrom(dataType))
            {
                int size = byteStreamReader.ReadInt();

                IList list = Activator.CreateInstance(dataType) as IList;

                for (int i = 0; i < size; i++)
                {
                    list.Add(Deserialize(dataType.GenericTypeArguments[0], byteStreamReader));
                }

                data = list;
                return true;
            }
            else if (typeof(IDictionary).IsAssignableFrom(dataType))
            {
                int size = byteStreamReader.ReadInt();

                IDictionary dictionary = Activator.CreateInstance(dataType) as IDictionary;

                for (int i = 0; i < size; i++)
                {
                    object key = Deserialize(dataType.GenericTypeArguments[0], byteStreamReader);
                    object value = Deserialize(dataType.GenericTypeArguments[1], byteStreamReader);

                    dictionary.Add(key, value);
                }

                data = dictionary;
                return true;
            }

            // Unknown type
            return false;
        }

        #endregion

        #region Type Factories

        /// <inheritdoc />
        public void AddTypeFactory<T>(Func<T> factoryMethod) where T : IByteStreamSerializable
        {
            _byteStreamSerializableTypeFactories.Add(typeof(T), () => factoryMethod());
        }

        /// <inheritdoc />
        public void RemoveTypeFactory<T>() where T : IByteStreamSerializable
        {
            _byteStreamSerializableTypeFactories.Remove(typeof(T));
        }
        #endregion
    }
}
