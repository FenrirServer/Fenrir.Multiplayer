using LiteNetLib.Utils;
using System;
using System.Net;

namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Byte Stream Reader
    /// Deserializes values from a given byte stream 
    /// </summary>
    class ByteStreamReader : IByteStreamReader, IRecyclable
    {
        /// <inheritdoc/>
        public int Position => _netDataReader.Position;

        /// <inheritdoc/>
        public bool IsNull => _netDataReader.IsNull;

        /// <inheritdoc/>
        public bool EndOfData => _netDataReader.EndOfData;

        /// <inheritdoc/>
        public int AvailableBytes => _netDataReader.AvailableBytes;

        /// <summary>
        /// Net Data Reader
        /// </summary>
        private NetDataReader _netDataReader;

        /// <summary>
        /// Creates ByteStreamReader
        /// </summary>
        public ByteStreamReader()
        {
            _netDataReader = new NetDataReader();
        }

        /// <summary>
        /// Creates ByteStreamReader form byte array
        /// </summary>
        public ByteStreamReader(byte[] bytes)
        {
            _netDataReader = new NetDataReader(bytes);
        }

        /// <summary>
        /// Creates ByteStreamReader from LiteNet NetDataWriter
        /// </summary>
        /// <param name="netDataReader"></param>
        public ByteStreamReader(NetDataReader netDataReader)
        {
            _netDataReader = netDataReader;
        }

        /// <inheritdoc/>
        public void SetNetDataReader(NetDataReader netDataReader)
        {
            _netDataReader = netDataReader;
        }

        /// <inheritdoc/>
        public T Read<T>() where T : IByteStreamSerializable, new()
        {
            var data = new T();
            data.Deserialize(this);
            return data;
        }

        /// <inheritdoc/>
        void IRecyclable.Recycle() => _netDataReader?.Clear();

        /// <inheritdoc/>
        public bool ReadBool() => _netDataReader.GetBool();

        /// <inheritdoc/>
        public bool[] ReadBoolArray() => _netDataReader.GetBoolArray();

        /// <inheritdoc/>
        public byte ReadByte() => _netDataReader.GetByte();

        /// <inheritdoc/>
        public void ReadBytes(byte[] destination, int count) => _netDataReader.GetBytes(destination, count);

        /// <inheritdoc/>
        public void ReadBytes(byte[] destination, int start, int count) => _netDataReader.GetBytes(destination, start, count);

        /// <inheritdoc/>
        public byte[] ReadBytesWithLength() => _netDataReader.GetBytesWithLength();

        /// <inheritdoc/>
        public char ReadChar() => _netDataReader.GetChar();

        /// <inheritdoc/>
        public double ReadDouble() => _netDataReader.GetDouble();

        /// <inheritdoc/>
        public double[] ReadDoubleArray() => _netDataReader.GetDoubleArray();

        /// <inheritdoc/>
        public float ReadFloat() => _netDataReader.GetFloat();

        /// <inheritdoc/>
        public float[] ReadFloatArray() => _netDataReader.GetFloatArray();

        /// <inheritdoc/>
        public int ReadInt() => _netDataReader.GetInt();

        /// <inheritdoc/>
        public int[] ReadIntArray() => _netDataReader.GetIntArray();

        /// <inheritdoc/>
        public long ReadLong() => _netDataReader.GetLong();

        /// <inheritdoc/>
        public long[] ReadLongArray() => _netDataReader.GetLongArray();

        /// <inheritdoc/>
        public IPEndPoint ReadNetEndPoint() => _netDataReader.GetNetEndPoint();

        /// <inheritdoc/>
        public byte[] ReadRemainingBytes() => _netDataReader.GetRemainingBytes();

        /// <inheritdoc/>
        public ArraySegment<byte> ReadRemainingBytesSegment() => _netDataReader.GetRemainingBytesSegment();

        /// <inheritdoc/>
        public sbyte ReadSByte() => _netDataReader.GetSByte();

        /// <inheritdoc/>
        public sbyte[] ReadSBytesWithLength() => _netDataReader.GetSBytesWithLength();

        /// <inheritdoc/>
        public short ReadShort() => _netDataReader.GetShort();

        /// <inheritdoc/>
        public short[] ReadShortArray() => _netDataReader.GetShortArray();

        /// <inheritdoc/>
        public string ReadString(int maxLength) => _netDataReader.GetString(maxLength);

        /// <inheritdoc/>
        public string ReadString() => _netDataReader.GetString();

        /// <inheritdoc/>
        public string[] ReadStringArray(int maxStringLength) => _netDataReader.GetStringArray(maxStringLength);

        /// <inheritdoc/>
        public string[] ReadStringArray() => _netDataReader.GetStringArray();

        /// <inheritdoc/>
        public uint ReadUInt() => _netDataReader.GetUInt();

        /// <inheritdoc/>
        public uint[] ReadUIntArray() => _netDataReader.GetUIntArray();

        /// <inheritdoc/>
        public ulong ReadULong() => _netDataReader.GetULong();

        /// <inheritdoc/>
        public ulong[] ReadULongArray() => _netDataReader.GetULongArray();

        /// <inheritdoc/>
        public ushort ReadUShort() => _netDataReader.GetUShort();

        /// <inheritdoc/>
        public ushort[] ReadUShortArray() => _netDataReader.GetUShortArray();

        /// <inheritdoc/>
        public bool PeekBool() => _netDataReader.PeekBool();

        /// <inheritdoc/>
        public byte PeekByte() => _netDataReader.PeekByte();

        /// <inheritdoc/>
        public char PeekChar() => _netDataReader.PeekChar();

        /// <inheritdoc/>
        public double PeekDouble() => _netDataReader.PeekDouble();

        /// <inheritdoc/>
        public float PeekFloat() => _netDataReader.PeekFloat();

        /// <inheritdoc/>
        public int PeekInt() => _netDataReader.PeekInt();

        /// <inheritdoc/>

        /// <inheritdoc/>
        public long PeekLong() => _netDataReader.PeekLong();

        /// <inheritdoc/>
        public sbyte PeekSByte() => _netDataReader.PeekSByte();

        /// <inheritdoc/>
        public short PeekShort() => _netDataReader.PeekShort();

        /// <inheritdoc/>
        public string PeekString() => _netDataReader.PeekString();

        /// <inheritdoc/>
        public string PeekString(int maxLength) => _netDataReader.PeekString(maxLength);

        /// <inheritdoc/>
        public uint PeekUInt() => _netDataReader.PeekUInt();

        /// <inheritdoc/>
        public ulong PeekULong() => _netDataReader.PeekULong();

        /// <inheritdoc/>
        public ushort PeekUShort() => _netDataReader.PeekUShort();

        /// <inheritdoc/>

        public void SetSource(byte[] source) => _netDataReader.SetSource(source);

        /// <inheritdoc/>
        public void SetSource(byte[] source, int offset) => _netDataReader.SetSource(source, offset);

        /// <inheritdoc/>
        public void SetSource(byte[] source, int offset, int maxSize) => _netDataReader.SetSource(source, offset, maxSize);

        /// <inheritdoc/>
        public void SkipBytes(int count) => _netDataReader.SkipBytes(count);

        /// <inheritdoc/>
        public bool TryReadBool(out bool result) => _netDataReader.TryGetBool(out result);

        /// <inheritdoc/>

        /// <inheritdoc/>
        public bool TryReadByte(out byte result) => _netDataReader.TryGetByte(out result);

        /// <inheritdoc/>
        public bool TryReadBytesWithLength(out byte[] result) => _netDataReader.TryGetBytesWithLength(out result);

        /// <inheritdoc/>
        public bool TryReadChar(out char result) => _netDataReader.TryGetChar(out result);

        /// <inheritdoc/>
        public bool TryReadDouble(out double result) => _netDataReader.TryGetDouble(out result);

        /// <inheritdoc/>
        public bool TryReadFloat(out float result) => _netDataReader.TryGetFloat(out result);

        /// <inheritdoc/>
        public bool TryReadInt(out int result) => _netDataReader.TryGetInt(out result);

        /// <inheritdoc/>
        public bool TryReadLong(out long result) => _netDataReader.TryGetLong(out result);

        /// <inheritdoc/>
        public bool TryReadSByte(out sbyte result) => _netDataReader.TryGetSByte(out result);

        /// <inheritdoc/>
        public bool TryReadShort(out short result) => _netDataReader.TryGetShort(out result);

        /// <inheritdoc/>
        public bool TryReadString(out string result) => _netDataReader.TryGetString(out result);

        /// <inheritdoc/>
        public bool TryReadStringArray(out string[] result) => _netDataReader.TryGetStringArray(out result);

        /// <inheritdoc/>
        public bool TryReadUInt(out uint result) => _netDataReader.TryGetUInt(out result);

        /// <inheritdoc/>
        public bool TryReadULong(out ulong result) => _netDataReader.TryGetULong(out result);

        /// <inheritdoc/>
        public bool TryReadUShort(out ushort result) => _netDataReader.TryGetUShort(out result);
    }
}
