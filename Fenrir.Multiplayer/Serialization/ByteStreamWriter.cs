using LiteNetLib.Utils;
using System;
using System.Net;

namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Byte Stream Writer
    /// Serializes values from a given byte stream 
    /// </summary>
    class ByteStreamWriter : IByteStreamWriter, IRecyclable
    {
        /// <summary>
        /// Net Data Writer
        /// </summary>
        public NetDataWriter NetDataWriter { get; private set; }

        ///<inheritdoc/>
        public int Capacity => NetDataWriter.Capacity;

        ///<inheritdoc/>
        public byte[] Bytes => NetDataWriter.Data;

        ///<inheritdoc/>
        public int Length => NetDataWriter.Length;

        /// <summary>
        /// Instance of a serializer. Used to write unknown types
        /// </summary>
        private INetworkSerializer _serializer;

        /// <summary>
        /// Creates Byte Stream Writer
        /// </summary>
        /// <param name="serializer">Network Serializer, used for serializing unknown types</param>
        public ByteStreamWriter(INetworkSerializer serializer)
            : this(new NetDataWriter(), serializer)
        {
        }

        /// <summary>
        /// Creates new <see cref="ByteStreamWriter"/> with <seealso cref="INetworkSerializer"/> and <seealso cref="NetDataWriter"/>
        /// </summary>
        /// <param name="netDataWriter">Net Data Writer</param>
        /// <param name="serializer">Network Serializer, used for serializing unknown types</param>
        public ByteStreamWriter(NetDataWriter netDataWriter, INetworkSerializer serializer)
        {
            if (netDataWriter == null)
            {
                throw new ArgumentNullException(nameof(netDataWriter));
            }

            _serializer = serializer;
            NetDataWriter = netDataWriter;
        }

        ///<inheritdoc/>
        public void SetNetDataWriter(NetDataWriter netDataWriter)
        {
            NetDataWriter = netDataWriter;
        }

        ///<inheritdoc/>
        public void Recycle() => NetDataWriter?.Reset();

        ///<inheritdoc/>
        public void Write(object obj) 
        {
            if (_serializer == null)
            {
                throw new NullReferenceException($"Failed to write {obj.GetType().Name}, {nameof(ByteStreamReader)}.{nameof(_serializer)} is not set");
            }
    
            _serializer.Serialize(obj, this);
        }

        ///<inheritdoc/>
        public void Write(object obj, Type dataType)
        {
            if (_serializer == null)
            {
                throw new NullReferenceException($"Failed to write {obj.GetType().Name}, {nameof(ByteStreamReader)}.{nameof(_serializer)} is not set");
            }

            _serializer.Serialize(obj, dataType, this);
        }


        ///<inheritdoc/>
        public void Write(byte[] data, int offset, int length) => NetDataWriter.Put(data, offset, length);

        ///<inheritdoc/>
        public void Write(bool value) => NetDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(IPEndPoint endPoint) => NetDataWriter.Put(endPoint);

        ///<inheritdoc/>
        public void Write(string value) => NetDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(string value, int maxLength) => NetDataWriter.Put(value, maxLength);

        ///<inheritdoc/>
        public void Write(byte value) => NetDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(sbyte value) => NetDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(short value) => NetDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(byte[] data) => NetDataWriter.Put(data);

        ///<inheritdoc/>
        public void Write(char value) => NetDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(uint value) => NetDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(int value) => NetDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(ulong value) => NetDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(long value) => NetDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(double value) => NetDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(float value) => NetDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(ushort value) => NetDataWriter.Put(value);

        ///<inheritdoc/>
        public void WriteArray(bool[] value) => NetDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(short[] value) => NetDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(ushort[] value) => NetDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(uint[] value) => NetDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(float[] value) => NetDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(ulong[] value) => NetDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(long[] value) => NetDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(double[] value) => NetDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(string[] value) => NetDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(int[] value) => NetDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(string[] value, int maxLength) => NetDataWriter.PutArray(value, maxLength);

        ///<inheritdoc/>
        public void WriteBytesWithLength(byte[] data, int offset, int length) => NetDataWriter.PutBytesWithLength(data, offset, length);

        ///<inheritdoc/>
        public void WriteBytesWithLength(byte[] data) => NetDataWriter.PutBytesWithLength(data);

        ///<inheritdoc/>
        public void WriteSBytesWithLength(sbyte[] data) => NetDataWriter.PutSBytesWithLength(data);

        ///<inheritdoc/>
        public void WriteSBytesWithLength(sbyte[] data, int offset, int length) => NetDataWriter.PutSBytesWithLength(data, offset, length);

    }
}
