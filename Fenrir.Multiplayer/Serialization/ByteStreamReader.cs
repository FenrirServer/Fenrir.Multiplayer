using System;
using System.Net;
using LiteNetLib.Utils;

namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Deserializes values from a given byte stream 
    /// </summary>
    class ByteStreamReader : IByteStreamReader, IRecyclable
    {
        /// <summary>
        /// Net Data Reader
        /// </summary>
        public NetDataReader NetDataReader { get; private set; }

        /// <inheritdoc/>
        public int Position => NetDataReader.Position;

        /// <inheritdoc/>
        public bool IsNull => NetDataReader.IsNull;

        /// <inheritdoc/>
        public bool EndOfData => NetDataReader.EndOfData;

        /// <inheritdoc/>
        public int AvailableBytes => NetDataReader.AvailableBytes;

        /// <summary>
        /// Instance of a serializer. Used to read unknown types
        /// </summary>
        private INetworkSerializer _serializer;

        /// <summary>
        /// Creates ByteStreamReader
        /// </summary>
        /// <param name="serializer">Fenrir Serializer, used for deserializing unknown types</param>
        public ByteStreamReader(INetworkSerializer serializer)
            : this(new NetDataReader(), serializer)
        {
        }

        /// <summary>
        /// Creates byte stream reader from byte stream writer
        /// </summary>
        /// <param name="byteStreamWriter">Byte stream writer</param>
        /// <param name="serializer">Fenrir Serializer, used for deserializing unknown types</param>
        public ByteStreamReader(ByteStreamWriter byteStreamWriter, INetworkSerializer serializer = null)
            : this(new NetDataReader(byteStreamWriter.NetDataWriter), serializer)
        {
        }

        /// <summary>
        /// Creates <see cref="ByteStreamReader"/> with <seealso cref="INetworkSerializer"/> and byte stream array
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="serializer">Fenrir Serializer, used for deserializing unknown types</param>
        public ByteStreamReader(byte[] bytes, INetworkSerializer serializer = null)
            : this(new NetDataReader(bytes), serializer)
        {
        }

        /// <summary>
        /// Creates <see cref="ByteStreamReader"/> with <seealso cref="INetworkSerializer"/> and <seealso cref="NetDataReader"/>
        /// </summary>
        /// <param name="netDataReader">Net data reader</param>
        /// <param name="serializer">Fenrir Serializer, used for deserializing unknown types</param>
        public ByteStreamReader(NetDataReader netDataReader, INetworkSerializer serializer = null)
        {
            if(netDataReader == null)
            {
                throw new ArgumentNullException(nameof(netDataReader));
            }

            _serializer = serializer;
            NetDataReader = netDataReader;
        }

        /// <inheritdoc/>
        public void SetNetDataReader(NetDataReader netDataReader)
        {
            NetDataReader = netDataReader;
        }

        /// <inheritdoc/>
        public T Read<T>() where T : new()
        {
            if(_serializer == null)
            {
                throw new NullReferenceException($"Failed to read {typeof(T).Name}, {nameof(ByteStreamReader)}.{nameof(_serializer)} is not set");
            }

            return _serializer.Deserialize<T>(this);
        }

        /// <inheritdoc/>
        public object Read(Type dataType)
        {
            if (_serializer == null)
            {
                throw new NullReferenceException($"Failed to read {dataType.Name}, {nameof(ByteStreamReader)}.{nameof(_serializer)} is not set");
            }

            return _serializer.Deserialize(dataType, this);
        }


        /// <inheritdoc/>
        public void Recycle() => NetDataReader?.Clear();

        /// <inheritdoc/>
        public bool ReadBool() => NetDataReader.GetBool();

        /// <inheritdoc/>
        public bool[] ReadBoolArray() => NetDataReader.GetBoolArray();

        /// <inheritdoc/>
        public byte ReadByte() => NetDataReader.GetByte();

        /// <inheritdoc/>
        public void ReadBytes(byte[] destination, int count) => NetDataReader.GetBytes(destination, count);

        /// <inheritdoc/>
        public void ReadBytes(byte[] destination, int start, int count) => NetDataReader.GetBytes(destination, start, count);

        /// <inheritdoc/>
        public byte[] ReadBytesWithLength() => NetDataReader.GetBytesWithLength();

        /// <inheritdoc/>
        public char ReadChar() => NetDataReader.GetChar();

        /// <inheritdoc/>
        public double ReadDouble() => NetDataReader.GetDouble();

        /// <inheritdoc/>
        public double[] ReadDoubleArray() => NetDataReader.GetDoubleArray();

        /// <inheritdoc/>
        public float ReadFloat() => NetDataReader.GetFloat();

        /// <inheritdoc/>
        public float[] ReadFloatArray() => NetDataReader.GetFloatArray();

        /// <inheritdoc/>
        public int ReadInt() => NetDataReader.GetInt();

        /// <inheritdoc/>
        public int[] ReadIntArray() => NetDataReader.GetIntArray();

        /// <inheritdoc/>
        public long ReadLong() => NetDataReader.GetLong();

        /// <inheritdoc/>
        public long[] ReadLongArray() => NetDataReader.GetLongArray();

        /// <inheritdoc/>
        public IPEndPoint ReadNetEndPoint() => NetDataReader.GetNetEndPoint();

        /// <inheritdoc/>
        public byte[] ReadRemainingBytes() => NetDataReader.GetRemainingBytes();

        /// <inheritdoc/>
        public ArraySegment<byte> ReadRemainingBytesSegment() => NetDataReader.GetRemainingBytesSegment();

        /// <inheritdoc/>
        public sbyte ReadSByte() => NetDataReader.GetSByte();

        /// <inheritdoc/>
        public sbyte[] ReadSBytesWithLength() => NetDataReader.GetSBytesWithLength();

        /// <inheritdoc/>
        public short ReadShort() => NetDataReader.GetShort();

        /// <inheritdoc/>
        public short[] ReadShortArray() => NetDataReader.GetShortArray();

        /// <inheritdoc/>
        public string ReadString(int maxLength) => NetDataReader.GetString(maxLength);

        /// <inheritdoc/>
        public string ReadString() => NetDataReader.GetString();

        /// <inheritdoc/>
        public string[] ReadStringArray(int maxStringLength) => NetDataReader.GetStringArray(maxStringLength);

        /// <inheritdoc/>
        public string[] ReadStringArray() => NetDataReader.GetStringArray();

        /// <inheritdoc/>
        public uint ReadUInt() => NetDataReader.GetUInt();

        /// <inheritdoc/>
        public uint[] ReadUIntArray() => NetDataReader.GetUIntArray();

        /// <inheritdoc/>
        public ulong ReadULong() => NetDataReader.GetULong();

        /// <inheritdoc/>
        public ulong[] ReadULongArray() => NetDataReader.GetULongArray();

        /// <inheritdoc/>
        public ushort ReadUShort() => NetDataReader.GetUShort();

        /// <inheritdoc/>
        public ushort[] ReadUShortArray() => NetDataReader.GetUShortArray();

        /// <inheritdoc/>
        public bool PeekBool() => NetDataReader.PeekBool();

        /// <inheritdoc/>
        public byte PeekByte() => NetDataReader.PeekByte();

        /// <inheritdoc/>
        public char PeekChar() => NetDataReader.PeekChar();

        /// <inheritdoc/>
        public double PeekDouble() => NetDataReader.PeekDouble();

        /// <inheritdoc/>
        public float PeekFloat() => NetDataReader.PeekFloat();

        /// <inheritdoc/>
        public int PeekInt() => NetDataReader.PeekInt();

        /// <inheritdoc/>

        /// <inheritdoc/>
        public long PeekLong() => NetDataReader.PeekLong();

        /// <inheritdoc/>
        public sbyte PeekSByte() => NetDataReader.PeekSByte();

        /// <inheritdoc/>
        public short PeekShort() => NetDataReader.PeekShort();

        /// <inheritdoc/>
        public string PeekString() => NetDataReader.PeekString();

        /// <inheritdoc/>
        public string PeekString(int maxLength) => NetDataReader.PeekString(maxLength);

        /// <inheritdoc/>
        public uint PeekUInt() => NetDataReader.PeekUInt();

        /// <inheritdoc/>
        public ulong PeekULong() => NetDataReader.PeekULong();

        /// <inheritdoc/>
        public ushort PeekUShort() => NetDataReader.PeekUShort();

        /// <inheritdoc/>

        public void SetSource(byte[] source) => NetDataReader.SetSource(source);

        /// <inheritdoc/>
        public void SetSource(byte[] source, int offset) => NetDataReader.SetSource(source, offset);

        /// <inheritdoc/>
        public void SetSource(byte[] source, int offset, int maxSize) => NetDataReader.SetSource(source, offset, maxSize);

        /// <inheritdoc/>
        public void SkipBytes(int count) => NetDataReader.SkipBytes(count);

        /// <inheritdoc/>
        public bool TryReadBool(out bool result) => NetDataReader.TryGetBool(out result);

        /// <inheritdoc/>

        /// <inheritdoc/>
        public bool TryReadByte(out byte result) => NetDataReader.TryGetByte(out result);

        /// <inheritdoc/>
        public bool TryReadBytesWithLength(out byte[] result) => NetDataReader.TryGetBytesWithLength(out result);

        /// <inheritdoc/>
        public bool TryReadChar(out char result) => NetDataReader.TryGetChar(out result);

        /// <inheritdoc/>
        public bool TryReadDouble(out double result) => NetDataReader.TryGetDouble(out result);

        /// <inheritdoc/>
        public bool TryReadFloat(out float result) => NetDataReader.TryGetFloat(out result);

        /// <inheritdoc/>
        public bool TryReadInt(out int result) => NetDataReader.TryGetInt(out result);

        /// <inheritdoc/>
        public bool TryReadLong(out long result) => NetDataReader.TryGetLong(out result);

        /// <inheritdoc/>
        public bool TryReadSByte(out sbyte result) => NetDataReader.TryGetSByte(out result);

        /// <inheritdoc/>
        public bool TryReadShort(out short result) => NetDataReader.TryGetShort(out result);

        /// <inheritdoc/>
        public bool TryReadString(out string result) => NetDataReader.TryGetString(out result);

        /// <inheritdoc/>
        public bool TryReadStringArray(out string[] result) => NetDataReader.TryGetStringArray(out result);

        /// <inheritdoc/>
        public bool TryReadUInt(out uint result) => NetDataReader.TryGetUInt(out result);

        /// <inheritdoc/>
        public bool TryReadULong(out ulong result) => NetDataReader.TryGetULong(out result);

        /// <inheritdoc/>
        public bool TryReadUShort(out ushort result) => NetDataReader.TryGetUShort(out result);
    }
}
