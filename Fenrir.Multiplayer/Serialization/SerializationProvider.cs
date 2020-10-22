﻿using System;
using System.Runtime.Serialization;

namespace Fenrir.Multiplayer.Serialization
{
    public class SerializationProvider : ISerializationProvider
    {
        private readonly ByteStreamSerializer _byteStreamSerializer;

        public IContractSerializer ContractSerializer { get; set; }

        public SerializationProvider()
        {
            _byteStreamSerializer = new ByteStreamSerializer();
        }

        public void Serialize(object data, IByteStreamWriter byteStreamWriter)
        {
            IByteStreamSerializable byteStreamSerializable = data as IByteStreamSerializable;
            if(byteStreamSerializable != null)
            {
                try
                {
                    byteStreamSerializable.Serialize(byteStreamWriter);
                }
                catch (Exception e)
                {
                    throw new SerializationException($"Failed to deserialize {data.GetType().Name} using {nameof(IByteStreamSerializable)}.{nameof(IByteStreamSerializable.Deserialize)}: " + e.Message, e);
                }

                return;
            }

            if(ContractSerializer != null)
            {
                try
                {
                    ContractSerializer.Serialize(data, byteStreamWriter);
                }
                catch (Exception e)
                {
                    throw new SerializationException($"Failed to deserialize {data.GetType().Name} using {ContractSerializer.GetType().Name}: " + e.Message, e);
                }
                return;
            }
            else
            {
                throw new SerializationException($"Failed to serialize {data.GetType().Name}: type does not implement {nameof(IByteStreamSerializable)} and {nameof(ContractSerializer)} is not set");
            }
        }


        public TData Deserialize<TData>(IByteStreamReader byteStreamReader)
            where TData : new()
        {
            if (typeof(IByteStreamSerializable).IsAssignableFrom(typeof(TData)))
            {
                TData data = new TData();
                IByteStreamSerializable byteStreamSerializable = (IByteStreamSerializable)data;

                try
                {
                    byteStreamSerializable.Deserialize(byteStreamReader);
                }
                catch(Exception e)
                {
                    throw new SerializationException($"Failed to deserialize {typeof(TData).Name} using {nameof(IByteStreamSerializable)}.{nameof(IByteStreamSerializable.Deserialize)}: " + e.Message, e);
                }

                return data;
            }

            if (ContractSerializer != null)
            {
                try
                {
                    return ContractSerializer.Deserialize<TData>(byteStreamReader);
                }
                catch (Exception e)
                {
                    throw new SerializationException($"Failed to deserialize {typeof(TData).Name} using {ContractSerializer.GetType().Name}: " + e.Message, e);
                }
            }
            else
            {
                throw new SerializationException($"Failed to deserialize {typeof(TData).Name}: type does not implement {nameof(IByteStreamSerializable)} and {nameof(ContractSerializer)} is not set");
            }
        }

        public object Deserialize(Type type, IByteStreamReader byteStreamReader)
        {
            if (typeof(IByteStreamSerializable).IsAssignableFrom(type))
            {
                IByteStreamSerializable byteStreamSerializable = (IByteStreamSerializable)Activator.CreateInstance(type);
                byteStreamSerializable.Deserialize(byteStreamReader);
                return byteStreamSerializable;
            }

            if (ContractSerializer != null)
            {
                return ContractSerializer.Deserialize(type, byteStreamReader);
            }
            else
            {
                throw new SerializationException($"Failed to deserialize {type.Name}: type does not implement {nameof(IByteStreamSerializable)} and {nameof(ContractSerializer)} is not set");
            }
        }
    }
}
