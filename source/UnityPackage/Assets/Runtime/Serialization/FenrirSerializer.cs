using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Serialized data from/into a binary format, to be sent/received over the network
    /// </summary>
    public class FenrirSerializer : IFenrirSerializer
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
        /// Creates Fenrir serializer
        /// </summary>
        public FenrirSerializer()
        {
        }

        /// <inheritdoc/>
        public void Serialize(object data, IByteStreamWriter byteStreamWriter)
        {
            // Increment current depth of serialization
            _currentDepth++;
            
            if(_currentDepth > MaxDepth)
            {
                throw new SerializationException($"Failed to serialize {data.GetType().Name}: maximum depth reached. Possible infinite recursion detected. If this is expected, consider increasing {nameof(MaxDepth)}");
            }

            try
            {
                SerializeInternal(data, byteStreamWriter);
            }
            finally
            {
                // Decrement current depth of serialization
                _currentDepth--;
            }
        }

        private void SerializeInternal(object data, IByteStreamWriter byteStreamWriter)
        {
            // Check if data is null, if so, write false boolean
            if(data == null)
            {
                byteStreamWriter.Write(false); // Null object ahead
                return;
            }

            // Check if object implements IByteStreamSerializable
            IByteStreamSerializable byteStreamSerializable = data as IByteStreamSerializable;
            if (byteStreamSerializable != null)
            {
                try
                {
                    byteStreamWriter.Write(true); // Non-null object ahead
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
                byteStreamWriter.Write(true); // Non-null object ahead
                serializeHandler.Invoke(data, byteStreamWriter); // Callback handles the exception
                return;
            }

            // Check if catch-all type serializer is set, if so, attmept to use it
            if (_typeSerializer != null)
            {
                try
                {
                    byteStreamWriter.Write(true); // Non-null object ahead
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

        private object DeserializeInternal(Type type, IByteStreamReader byteStreamReader)
        {
            // Check if there is an object ahead
            if(byteStreamReader.EndOfData || !byteStreamReader.TryReadBool(out bool hasObject))
            {
                throw new SerializationException($"Failed to deserialize {type.Name}, unexpected end of the stream");
            }

            // Check if there is an object ahead
            if(!hasObject)
            {
                return null;
            }

            // Check if type is IByteStreamSerializable
            if (typeof(IByteStreamSerializable).IsAssignableFrom(type))
            {
                IByteStreamSerializable byteStreamSerializable = (IByteStreamSerializable)Activator.CreateInstance(type);

                try
                {
                    byteStreamSerializable.Deserialize(byteStreamReader);
                }
                catch (Exception e)
                {
                    throw new SerializationException($"Failed to deserialize {type.Name} using {nameof(IByteStreamSerializable)}.{nameof(IByteStreamSerializable.Deserialize)}: " + e.Message, e);
                }

                return byteStreamSerializable;
            }

            // Check this specific type is bound to a type serializer
            if (_typeDeserializers.TryGetValue(type, out Func<Type, IByteStreamReader, object> deserializeHandler))
            {
                return deserializeHandler.Invoke(type, byteStreamReader); // Callback handles the exception
            }

            // Check if custom type serializer is set, fall back to it
            if (_typeSerializer != null)
            {
                try
                {
                    return _typeSerializer.Deserialize(type, byteStreamReader);
                }
                catch (Exception e)
                {
                    throw new SerializationException($"Failed to deserialize {type.Name} using {_typeSerializer.GetType().Name}: " + e.Message, e);
                }
            }

            // Nothing found
            throw new SerializationException($"Failed to deserialize {type.Name}: type does not implement {nameof(IByteStreamSerializable)} and no type serializer is found for the type");
        }

        /// <summary>
        /// Assigns type serializer for all unknown types
        /// </summary>
        /// <param name="typeSerializer">Type serializer</param>
        public void SetTypeSerializer(ITypeSerializer typeSerializer)
        {
            _typeSerializer = typeSerializer;
        }

        /// <summary>
        /// Sets type serializer for a given type. 
        /// If <see cref="Serialize(object, IByteStreamWriter)"/> or <see cref="Deserialize(Type, IByteStreamReader)"/> are invoked,
        /// this serializer will be used if type matches.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="typeSerializer">Type serializer</param>
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

        /// <summary>
        /// Removes type serializer for a given type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
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
    }
}
