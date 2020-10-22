using System.Net;
using LiteNetLib.Utils;

namespace Fenrir.Multiplayer.Serialization
{
    class ByteStreamWriter : IByteStreamWriter, IRecyclable
    {
        public int Capacity => _netDataWriter.Capacity;

        public byte[] Bytes => _netDataWriter.Data;

        public int Length => _netDataWriter.Length;


        private NetDataWriter _netDataWriter;


        public ByteStreamWriter()
        {
            _netDataWriter = new NetDataWriter();
        }
        public ByteStreamWriter(NetDataWriter netDataWriter)
        {
            _netDataWriter = netDataWriter;
        }

        public void SetNetDataWriter(NetDataWriter netDataWriter)
        {
            _netDataWriter = netDataWriter;
        }

        void IRecyclable.Recycle() => _netDataWriter?.Reset();

        public void Write(IByteStreamSerializable serializable) => serializable.Serialize(this);

        public void Write(byte[] data, int offset, int length) => _netDataWriter.Put(data, offset, length);
        public void Write(bool value) => _netDataWriter.Put(value);
        public void Write(IPEndPoint endPoint) => _netDataWriter.Put(endPoint);
        public void Write(string value) => _netDataWriter.Put(value);
        public void Write(string value, int maxLength) => _netDataWriter.Put(value, maxLength);
        public void Write(byte value) => _netDataWriter.Put(value);
        public void Write(sbyte value) => _netDataWriter.Put(value);
        public void Write(short value) => _netDataWriter.Put(value);
        public void Write(byte[] data) => _netDataWriter.Put(data);
        public void Write(char value) => _netDataWriter.Put(value);
        public void Write(uint value) => _netDataWriter.Put(value);
        public void Write(int value) => _netDataWriter.Put(value);
        public void Write(ulong value) => _netDataWriter.Put(value);
        public void Write(long value) => _netDataWriter.Put(value);
        public void Write(double value) => _netDataWriter.Put(value);
        public void Write(float value) => _netDataWriter.Put(value);
        public void Write(ushort value) => _netDataWriter.Put(value);
        public void WriteArray(bool[] value) => _netDataWriter.PutArray(value);
        public void WriteArray(short[] value) => _netDataWriter.PutArray(value);
        public void WriteArray(ushort[] value) => _netDataWriter.PutArray(value);
        public void WriteArray(uint[] value) => _netDataWriter.PutArray(value);
        public void WriteArray(float[] value) => _netDataWriter.PutArray(value);
        public void WriteArray(ulong[] value) => _netDataWriter.PutArray(value);
        public void WriteArray(long[] value) => _netDataWriter.PutArray(value);
        public void WriteArray(double[] value) => _netDataWriter.PutArray(value);
        public void WriteArray(string[] value) => _netDataWriter.PutArray(value);
        public void WriteArray(int[] value) => _netDataWriter.PutArray(value);
        public void WriteArray(string[] value, int maxLength) => _netDataWriter.PutArray(value, maxLength);
        public void WriteBytesWithLength(byte[] data, int offset, int length) => _netDataWriter.PutBytesWithLength(data, offset, length);
        public void WriteBytesWithLength(byte[] data) => _netDataWriter.PutBytesWithLength(data);
        public void WriteSBytesWithLength(sbyte[] data) => _netDataWriter.PutSBytesWithLength(data);
        public void WriteSBytesWithLength(sbyte[] data, int offset, int length) => _netDataWriter.PutSBytesWithLength(data, offset, length);

    }
}
