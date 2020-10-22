using LiteNetLib.Utils;
using System;
using System.Net;

namespace Fenrir.Multiplayer.Serialization
{
    class ByteStreamReader : IByteStreamReader, IRecyclable
    {
        public int Position => _netDataReader.Position;
        public bool IsNull => _netDataReader.IsNull;
        public bool EndOfData => _netDataReader.EndOfData;
        public int AvailableBytes => _netDataReader.AvailableBytes;


        private NetDataReader _netDataReader;

        public ByteStreamReader()
        {
            _netDataReader = new NetDataReader();
        }

        public ByteStreamReader(NetDataReader netDataReader)
        {
            _netDataReader = netDataReader;
        }

        public void SetNetDataReader(NetDataReader netDataReader)
        {
            _netDataReader = netDataReader;
        }

        public T Read<T>() where T : IByteStreamSerializable, new()
        {
            var data = new T();
            data.Deserialize(this);
            return data;
        }

        void IRecyclable.Recycle() => _netDataReader?.Clear();

        public bool ReadBool() => _netDataReader.GetBool();
        public bool[] ReadBoolArray() => _netDataReader.GetBoolArray();
        public byte ReadByte() => _netDataReader.GetByte();
        public void ReadBytes(byte[] destination, int count) => _netDataReader.GetBytes(destination, count);
        public void ReadBytes(byte[] destination, int start, int count) => _netDataReader.GetBytes(destination, start, count);
        public byte[] ReadBytesWithLength() => _netDataReader.GetBytesWithLength();
        public char ReadChar() => _netDataReader.GetChar();
        public double ReadDouble() => _netDataReader.GetDouble();
        public double[] ReadDoubleArray() => _netDataReader.GetDoubleArray();
        public float ReadFloat() => _netDataReader.GetFloat();
        public float[] ReadFloatArray() => _netDataReader.GetFloatArray();
        public int ReadInt() => _netDataReader.GetInt();
        public int[] ReadIntArray() => _netDataReader.GetIntArray();
        public long ReadLong() => _netDataReader.GetLong();
        public long[] ReadLongArray() => _netDataReader.GetLongArray();
        public IPEndPoint ReadNetEndPoint() => _netDataReader.GetNetEndPoint();
        public byte[] ReadRemainingBytes() => _netDataReader.GetRemainingBytes();
        public ArraySegment<byte> ReadRemainingBytesSegment() => _netDataReader.GetRemainingBytesSegment();
        public sbyte ReadSByte() => _netDataReader.GetSByte();
        public sbyte[] ReadSBytesWithLength() => _netDataReader.GetSBytesWithLength();
        public short ReadShort() => _netDataReader.GetShort();
        public short[] ReadShortArray() => _netDataReader.GetShortArray();
        public string ReadString(int maxLength) => _netDataReader.GetString(maxLength);
        public string ReadString() => _netDataReader.GetString();
        public string[] ReadStringArray(int maxStringLength) => _netDataReader.GetStringArray(maxStringLength);
        public string[] ReadStringArray() => _netDataReader.GetStringArray();
        public uint ReadUInt() => _netDataReader.GetUInt();
        public uint[] ReadUIntArray() => _netDataReader.GetUIntArray();
        public ulong ReadULong() => _netDataReader.GetULong();
        public ulong[] ReadULongArray() => _netDataReader.GetULongArray();
        public ushort ReadUShort() => _netDataReader.GetUShort();
        public ushort[] ReadUShortArray() => _netDataReader.GetUShortArray();
        public bool PeekBool() => _netDataReader.PeekBool();
        public byte PeekByte() => _netDataReader.PeekByte();
        public char PeekChar() => _netDataReader.PeekChar();
        public double PeekDouble() => _netDataReader.PeekDouble();
        public float PeekFloat() => _netDataReader.PeekFloat();
        public int PeekInt() => _netDataReader.PeekInt();
        public long PeekLong() => _netDataReader.PeekLong();
        public sbyte PeekSByte() => _netDataReader.PeekSByte();
        public short PeekShort() => _netDataReader.PeekShort();
        public string PeekString() => _netDataReader.PeekString();
        public string PeekString(int maxLength) => _netDataReader.PeekString(maxLength);
        public uint PeekUInt() => _netDataReader.PeekUInt();
        public ulong PeekULong() => _netDataReader.PeekULong();
        public ushort PeekUShort() => _netDataReader.PeekUShort();

        public void SetSource(byte[] source) => _netDataReader.SetSource(source);
        public void SetSource(byte[] source, int offset) => _netDataReader.SetSource(source, offset);
        public void SetSource(byte[] source, int offset, int maxSize) => _netDataReader.SetSource(source, offset, maxSize);
        public void SkipBytes(int count) => _netDataReader.SkipBytes(count);
        public bool TryReadBool(out bool result) => _netDataReader.TryGetBool(out result);
        public bool TryReadByte(out byte result) => _netDataReader.TryGetByte(out result);
        public bool TryReadBytesWithLength(out byte[] result) => _netDataReader.TryGetBytesWithLength(out result);
        public bool TryReadChar(out char result) => _netDataReader.TryGetChar(out result);
        public bool TryReadDouble(out double result) => _netDataReader.TryGetDouble(out result);
        public bool TryReadFloat(out float result) => _netDataReader.TryGetFloat(out result);
        public bool TryReadInt(out int result) => _netDataReader.TryGetInt(out result);
        public bool TryReadLong(out long result) => _netDataReader.TryGetLong(out result);
        public bool TryReadSByte(out sbyte result) => _netDataReader.TryGetSByte(out result);
        public bool TryReadShort(out short result) => _netDataReader.TryGetShort(out result);
        public bool TryReadString(out string result) => _netDataReader.TryGetString(out result);
        public bool TryReadStringArray(out string[] result) => _netDataReader.TryGetStringArray(out result);
        public bool TryReadUInt(out uint result) => _netDataReader.TryGetUInt(out result);
        public bool TryReadULong(out ulong result) => _netDataReader.TryGetULong(out result);
        public bool TryReadUShort(out ushort result) => _netDataReader.TryGetUShort(out result);
    }
}
