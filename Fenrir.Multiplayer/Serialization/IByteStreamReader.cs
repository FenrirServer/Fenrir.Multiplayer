using System;
using System.Net;

namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Deserializes values from a given byte stream 
    /// </summary>
    public interface IByteStreamReader
    {
        /// <summary>
        /// Current position in the stream
        /// </summary>
        int Position { get; }
        
        /// <summary>
        /// True if stream doesn't have any data, otherwise false
        /// </summary>
        bool IsNull { get; }

        /// <summary>
        /// True if no data left in the stream, otherwise false
        /// </summary>
        bool EndOfData { get; }

        /// <summary>
        /// Number of bytes available in the stream
        /// </summary>
        int AvailableBytes { get; }

        /// <summary>
        /// Reads data of a custom data type.
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <returns>Instance of type T</returns>
        T Read<T>() where T : new();

        /// <summary>
        /// Reads data of a custom data type.
        /// </summary>
        /// <param name="dataType">Type of data</param>
        /// <returns>Instance of type T</returns>
        object Read(Type dataType);

        /// <summary>
        /// Reads boolean
        /// </summary>
        /// <returns>Value</returns>
        bool ReadBool();

        /// <summary>
        /// Reads array of booleans
        /// </summary>
        /// <returns>Value</returns>
        bool[] ReadBoolArray();

        /// <summary>
        /// Reads bytes
        /// </summary>
        /// <returns>Value</returns>
        byte ReadByte();

        /// <summary>
        /// Reads bytes
        /// </summary>
        /// <returns>Value</returns>
        void ReadBytes(byte[] destination, int count);

        /// <summary>
        /// Reads bytes
        /// </summary>
        /// <returns>Value</returns>
        void ReadBytes(byte[] destination, int start, int count);

        /// <summary>
        /// Reads byte array preceded by it's length
        /// </summary>
        /// <returns>Value</returns>
        byte[] ReadBytesWithLength();

        /// <summary>
        /// Reads char
        /// </summary>
        /// <returns>Value</returns>
        char ReadChar();

        /// <summary>
        /// Reads double
        /// </summary>
        /// <returns>Value</returns>
        double ReadDouble();

        /// <summary>
        /// Reads double array
        /// </summary>
        /// <returns>Value</returns>
        double[] ReadDoubleArray();

        /// <summary>
        /// Reads float
        /// </summary>
        /// <returns>Value</returns>
        float ReadFloat();

        /// <summary>
        /// Reads float array
        /// </summary>
        /// <returns>Value</returns>
        float[] ReadFloatArray();

        /// <summary>
        /// Reads integer
        /// </summary>
        /// <returns>Value</returns>
        int ReadInt();

        /// <summary>
        /// Reads 
        /// </summary>
        /// <returns>Value</returns>
        int[] ReadIntArray();

        /// <summary>
        /// Reads long
        /// </summary>
        /// <returns>Value</returns>
        long ReadLong();

        /// <summary>
        /// Reads long array
        /// </summary>
        /// <returns>Value</returns>
        long[] ReadLongArray();

        /// <summary>
        /// Reads IPEndPoint
        /// </summary>
        /// <returns>Value</returns>
        IPEndPoint ReadNetEndPoint();

        /// <summary>
        /// Reads remaining bytes
        /// </summary>
        /// <returns>Value</returns>
        byte[] ReadRemainingBytes();

        /// <summary>
        /// Reads remaining bytes as an ArraySegment
        /// </summary>
        /// <returns>Value</returns>
        ArraySegment<byte> ReadRemainingBytesSegment();

        /// <summary>
        /// Reads signed byte
        /// </summary>
        /// <returns>Value</returns>
        sbyte ReadSByte();

        /// <summary>
        /// Reads byte array preceded by it's length
        /// </summary>
        /// <returns>Value</returns>
        sbyte[] ReadSBytesWithLength();

        /// <summary>
        /// Reads short
        /// </summary>
        /// <returns>Value</returns>
        short ReadShort();

        /// <summary>
        /// Reads short array
        /// </summary>
        /// <returns>Value</returns>
        short[] ReadShortArray();

        /// <summary>
        /// Reads string of a maximum length
        /// </summary>
        /// <returns>Value</returns>
        string ReadString(int maxLength);

        /// <summary>
        /// Reads string
        /// </summary>
        /// <returns>Value</returns>
        string ReadString();

        /// <summary>
        /// Reads string array of a maximum length
        /// </summary>
        /// <returns>Value</returns>
        string[] ReadStringArray(int maxStringLength);

        /// <summary>
        /// Reads string array
        /// </summary>
        /// <returns>Value</returns>
        string[] ReadStringArray();

        /// <summary>
        /// Reads unsigned integer
        /// </summary>
        /// <returns>Value</returns>
        uint ReadUInt();

        /// <summary>
        /// Reads unsigned integer array
        /// </summary>
        /// <returns>Value</returns>
        uint[] ReadUIntArray();

        /// <summary>
        /// Reads unsigned long
        /// </summary>
        /// <returns>Value</returns>
        ulong ReadULong();

        /// <summary>
        /// Reads 
        /// </summary>
        /// <returns>Value</returns>
        ulong[] ReadULongArray();

        /// <summary>
        /// Reads unsigned long array
        /// </summary>
        /// <returns>Value</returns>
        ushort ReadUShort();

        /// <summary>
        /// Reads unsigned short array
        /// </summary>
        /// <returns>Value</returns>
        ushort[] ReadUShortArray();

        /// <summary>
        /// Reads boolean without adjusting the position
        /// </summary>
        /// <returns>Value</returns>
        bool PeekBool();

        /// <summary>
        /// Reads byte without adjusting the position
        /// </summary>
        /// <returns>Value</returns>
        byte PeekByte();

        /// <summary>
        /// Reads char without adjusting the position 
        /// </summary>
        /// <returns>Value</returns>
        char PeekChar();

        /// <summary>
        /// Reads double without adjusting the position 
        /// </summary>
        /// <returns>Value</returns>
        double PeekDouble();

        /// <summary>
        /// Reads float without adjusting the position 
        /// </summary>
        /// <returns>Value</returns>
        float PeekFloat();

        /// <summary>
        /// Reads integer without adjusting the position  
        /// </summary>
        /// <returns>Value</returns>
        int PeekInt();

        /// <summary>
        /// Reads long without adjusting the position  
        /// </summary>
        /// <returns>Value</returns>
        long PeekLong();

        /// <summary>
        /// Reads 
        /// </summary>
        /// <returns>Value</returns>
        sbyte PeekSByte();

        /// <summary>
        /// Reads signed byte without adjusting the position  
        /// </summary>
        /// <returns>Value</returns>
        short PeekShort();

        /// <summary>
        /// Reads string without adjusting the position  
        /// </summary>
        /// <returns>Value</returns>
        string PeekString();

        /// <summary>
        /// Reads string of a maximum length without adjusting the position  
        /// </summary>
        /// <returns>Value</returns>
        string PeekString(int maxLength);

        /// <summary>
        /// Reads unsigned integer without adjusting the position   
        /// </summary>
        /// <returns>Value</returns>
        uint PeekUInt();

        /// <summary>
        /// Reads unsigned long without adjusting the position   
        /// </summary>
        /// <returns>Value</returns>
        ulong PeekULong();

        /// <summary>
        /// Reads unsigned short without adjusting the position   
        /// </summary>
        /// <returns>Value</returns>
        ushort PeekUShort();

        /// <summary>
        /// Skips given number of bytes
        /// </summary>
        /// <param name="count">Number of bytes to skip</param>
        void SkipBytes(int count);

        /// <summary>
        /// Attempts to read boolean
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadBool(out bool result);

        /// <summary>
        /// Attempts to read byte
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadByte(out byte result);

        /// <summary>
        /// Attempts to read byte array preceded by it's length
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadBytesWithLength(out byte[] result);

        /// <summary>
        /// Attempts to read char
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadChar(out char result);

        /// <summary>
        /// Attempts to read double
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadDouble(out double result);

        /// <summary>
        /// Attempts to read float
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadFloat(out float result);

        /// <summary>
        /// Attempts to read integer
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadInt(out int result);

        /// <summary>
        /// Attempts to read long
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadLong(out long result);

        /// <summary>
        /// Attempts to read signed byte
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadSByte(out sbyte result);

        /// <summary>
        /// Attempts to read short
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadShort(out short result);

        /// <summary>
        /// Attempts to read string
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadString(out string result);

        /// <summary>
        /// Attempts to read string array
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadStringArray(out string[] result);

        /// <summary>
        /// Attempts to read unsigned integer
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadUInt(out uint result);

        /// <summary>
        /// Attempts to read unsigned long
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadULong(out ulong result);

        /// <summary>
        /// Attempts to read unsigned short
        /// </summary>
        /// <param name="result">Value</param>
        /// <returns>True if read was successful, otherwise false</returns>
        bool TryReadUShort(out ushort result);
    }
}