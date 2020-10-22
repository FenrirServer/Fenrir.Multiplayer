using System.Net;

namespace Fenrir.Multiplayer.Serialization
{
    public interface IByteStreamWriter
    {
        int Capacity { get; }

        byte[] Bytes { get; }

        int Length { get; }


        void Write(IByteStreamSerializable serializable);

        void Write(byte[] data, int offset, int length);
        void Write(bool value);
        void Write(IPEndPoint endPoint);
        void Write(string value);
        void Write(string value, int maxLength);
        void Write(byte value);
        void Write(sbyte value);
        void Write(short value);
        void Write(byte[] data);
        void Write(char value);
        void Write(uint value);
        void Write(int value);
        void Write(ulong value);
        void Write(long value);
        void Write(double value);
        void Write(float value);
        void Write(ushort value);
        void WriteArray(bool[] value);
        void WriteArray(short[] value);
        void WriteArray(ushort[] value);
        void WriteArray(uint[] value);
        void WriteArray(float[] value);
        void WriteArray(ulong[] value);
        void WriteArray(long[] value);
        void WriteArray(double[] value);
        void WriteArray(string[] value);
        void WriteArray(int[] value);
        void WriteArray(string[] value, int maxLength);
        void WriteBytesWithLength(byte[] data, int offset, int length);
        void WriteBytesWithLength(byte[] data);
        void WriteSBytesWithLength(sbyte[] data);
        void WriteSBytesWithLength(sbyte[] data, int offset, int length);
    }
}