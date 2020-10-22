using System;
using System.Net;

namespace Fenrir.Multiplayer.Serialization
{
    public interface IByteStreamReader
    {
        int Position { get; }
        bool IsNull { get; }
        bool EndOfData { get; }
        int AvailableBytes { get; }

        T Read<T>() where T : IByteStreamSerializable, new();

        bool ReadBool();
        bool[] ReadBoolArray();
        byte ReadByte();
        void ReadBytes(byte[] destination, int count);
        void ReadBytes(byte[] destination, int start, int count);
        byte[] ReadBytesWithLength();
        char ReadChar();
        double ReadDouble();
        double[] ReadDoubleArray();
        float ReadFloat();
        float[] ReadFloatArray();
        int ReadInt();
        int[] ReadIntArray();
        long ReadLong();
        long[] ReadLongArray();
        IPEndPoint ReadNetEndPoint();
        byte[] ReadRemainingBytes();
        ArraySegment<byte> ReadRemainingBytesSegment();
        sbyte ReadSByte();
        sbyte[] ReadSBytesWithLength();
        short ReadShort();
        short[] ReadShortArray();
        string ReadString(int maxLength);
        string ReadString();
        string[] ReadStringArray(int maxStringLength);
        string[] ReadStringArray();
        uint ReadUInt();
        uint[] ReadUIntArray();
        ulong ReadULong();
        ulong[] ReadULongArray();
        ushort ReadUShort();
        ushort[] ReadUShortArray();
        bool PeekBool();
        byte PeekByte();
        char PeekChar();
        double PeekDouble();
        float PeekFloat();
        int PeekInt();
        long PeekLong();
        sbyte PeekSByte();
        short PeekShort();
        string PeekString();
        string PeekString(int maxLength);
        uint PeekUInt();
        ulong PeekULong();
        ushort PeekUShort();

        void SkipBytes(int count);
        bool TryReadBool(out bool result);
        bool TryReadByte(out byte result);
        bool TryReadBytesWithLength(out byte[] result);
        bool TryReadChar(out char result);
        bool TryReadDouble(out double result);
        bool TryReadFloat(out float result);
        bool TryReadInt(out int result);
        bool TryReadLong(out long result);
        bool TryReadSByte(out sbyte result);
        bool TryReadShort(out short result);
        bool TryReadString(out string result);
        bool TryReadStringArray(out string[] result);
        bool TryReadUInt(out uint result);
        bool TryReadULong(out ulong result);
        bool TryReadUShort(out ushort result);
    }
}