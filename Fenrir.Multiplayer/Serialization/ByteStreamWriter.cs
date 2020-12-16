using System.Net;
using LiteNetLib.Utils;

namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Byte Stream Writer
    /// Serializes values from a given byte stream 
    /// </summary>
    class ByteStreamWriter : IByteStreamWriter, IRecyclable
    {
        ///<inheritdoc/>
        public int Capacity => _netDataWriter.Capacity;

        ///<inheritdoc/>
        public byte[] Bytes => _netDataWriter.Data;

        ///<inheritdoc/>
        public int Length => _netDataWriter.Length;

        /// <summary>
        /// Net Data Writer
        /// </summary>
        private NetDataWriter _netDataWriter;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ByteStreamWriter()
        {
            _netDataWriter = new NetDataWriter();
        }

        /// <summary>
        /// Constructs Byte Stream Writer with LiteNet NetDataWriter
        /// </summary>
        /// <param name="netDataWriter">Net Data Writer</param>
        public ByteStreamWriter(NetDataWriter netDataWriter)
        {
            _netDataWriter = netDataWriter;
        }

        ///<inheritdoc/>
        public void SetNetDataWriter(NetDataWriter netDataWriter)
        {
            _netDataWriter = netDataWriter;
        }

        ///<inheritdoc/>
        void IRecyclable.Recycle() => _netDataWriter?.Reset();

        ///<inheritdoc/>
        public void Write(IByteStreamSerializable serializable) => serializable.Serialize(this);

        ///<inheritdoc/>
        public void Write(byte[] data, int offset, int length) => _netDataWriter.Put(data, offset, length);

        ///<inheritdoc/>
        public void Write(bool value) => _netDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(IPEndPoint endPoint) => _netDataWriter.Put(endPoint);

        ///<inheritdoc/>
        public void Write(string value) => _netDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(string value, int maxLength) => _netDataWriter.Put(value, maxLength);

        ///<inheritdoc/>
        public void Write(byte value) => _netDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(sbyte value) => _netDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(short value) => _netDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(byte[] data) => _netDataWriter.Put(data);

        ///<inheritdoc/>
        public void Write(char value) => _netDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(uint value) => _netDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(int value) => _netDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(ulong value) => _netDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(long value) => _netDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(double value) => _netDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(float value) => _netDataWriter.Put(value);

        ///<inheritdoc/>
        public void Write(ushort value) => _netDataWriter.Put(value);

        ///<inheritdoc/>
        public void WriteArray(bool[] value) => _netDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(short[] value) => _netDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(ushort[] value) => _netDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(uint[] value) => _netDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(float[] value) => _netDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(ulong[] value) => _netDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(long[] value) => _netDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(double[] value) => _netDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(string[] value) => _netDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(int[] value) => _netDataWriter.PutArray(value);

        ///<inheritdoc/>
        public void WriteArray(string[] value, int maxLength) => _netDataWriter.PutArray(value, maxLength);

        ///<inheritdoc/>
        public void WriteBytesWithLength(byte[] data, int offset, int length) => _netDataWriter.PutBytesWithLength(data, offset, length);

        ///<inheritdoc/>
        public void WriteBytesWithLength(byte[] data) => _netDataWriter.PutBytesWithLength(data);

        ///<inheritdoc/>
        public void WriteSBytesWithLength(sbyte[] data) => _netDataWriter.PutSBytesWithLength(data);

        ///<inheritdoc/>
        public void WriteSBytesWithLength(sbyte[] data, int offset, int length) => _netDataWriter.PutSBytesWithLength(data, offset, length);

    }
}
