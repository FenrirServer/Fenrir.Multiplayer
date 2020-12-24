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
        public int Capacity => NetDataWriter.Capacity;

        ///<inheritdoc/>
        public byte[] Bytes => NetDataWriter.Data;

        ///<inheritdoc/>
        public int Length => NetDataWriter.Length;

        /// <summary>
        /// Net Data Writer
        /// </summary>
        public NetDataWriter NetDataWriter { get; private set; }

        /// <summary>
        /// Creates Byte Stream Writer
        /// </summary>
        public ByteStreamWriter()
        {
            NetDataWriter = new NetDataWriter();
        }

        /// <summary>
        /// Creates Byte Stream Writer
        /// </summary>
        /// <param name="byteStreamReader">Byte stream reader</param>
        public ByteStreamWriter(ByteStreamReader byteStreamReader)
        {
            NetDataWriter = new NetDataWriter();
        }

        /// <summary>
        /// Constructs Byte Stream Writer with LiteNet NetDataWriter
        /// </summary>
        /// <param name="netDataWriter">Net Data Writer</param>
        public ByteStreamWriter(NetDataWriter netDataWriter)
        {
            NetDataWriter = netDataWriter;
        }

        ///<inheritdoc/>
        public void SetNetDataWriter(NetDataWriter netDataWriter)
        {
            NetDataWriter = netDataWriter;
        }

        ///<inheritdoc/>
        void IRecyclable.Recycle() => NetDataWriter?.Reset();

        ///<inheritdoc/>
        public void Write(IByteStreamSerializable serializable) => serializable.Serialize(this);

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
