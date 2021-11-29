using System;
using System.Net;

namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Serializes values from a given byte stream 
    /// </summary>
    public interface IByteStreamWriter
    {
        /// <summary>
        /// Maximum size of the stream
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Number of bytes in the stream
        /// </summary>
        byte[] Bytes { get; }

        /// <summary>
        /// Current length of the stream
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Writes custom object of an unknown type, using <seealso cref="INetworkSerializer"/>
        /// </summary>
        /// <param name="data">Serializable object</param>
        void Write(object data);

        /// <summary>
        /// Writes custom object of an unknown type, using <seealso cref="INetworkSerializer"/>
        /// Data type is provided explicitly.
        /// Use this method if type can be nullable
        /// </summary>
        /// <param name="data">Serializable object</param>
        /// <param name="dataType">Data type</param>
        void Write(object data, Type dataType);

        /// <summary>
        /// Writes byte array
        /// </summary>
        /// <param name="data">Value</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        void Write(byte[] data, int offset, int length);

        /// <summary>
        /// Writes boolean
        /// </summary>
        /// <param name="value">Value</param>
        void Write(bool value);

        /// <summary>
        /// Writes IPEndPoint
        /// </summary>
        /// <param name="value">Value</param>
        void Write(IPEndPoint endPoint);

        /// <summary>
        /// Writes string
        /// </summary>
        /// <param name="value">Value</param>
        void Write(string value);

        /// <summary>
        /// Writes string of a given maximum length
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="maxLength">Maximum length</param>
        void Write(string value, int maxLength);

        /// <summary>
        /// Writes byte
        /// </summary>
        /// <param name="value">Value</param>
        void Write(byte value);

        /// <summary>
        /// Writes signed byte
        /// </summary>
        /// <param name="value">Value</param>
        void Write(sbyte value);

        /// <summary>
        /// Writes short
        /// </summary>
        /// <param name="value">Value</param>
        void Write(short value);

        /// <summary>
        /// Writes byte array
        /// </summary>
        /// <param name="value">Value</param>
        void Write(byte[] data);

        /// <summary>
        /// Writes char
        /// </summary>
        /// <param name="value">Value</param>
        void Write(char value);

        /// <summary>
        /// Writes unsigned integer
        /// </summary>
        /// <param name="value">Value</param>
        void Write(uint value);

        /// <summary>
        /// Writes integer
        /// </summary>
        /// <param name="value">Value</param>
        void Write(int value);

        /// <summary>
        /// Writes unsigned long
        /// </summary>
        /// <param name="value">Value</param>
        void Write(ulong value);

        /// <summary>
        /// Writes long
        /// </summary>
        /// <param name="value">Value</param>
        void Write(long value);

        /// <summary>
        /// Writes double
        /// </summary>
        /// <param name="value">Value</param>
        void Write(double value);

        /// <summary>
        /// Writes float
        /// </summary>
        /// <param name="value">Value</param>
        void Write(float value);

        /// <summary>
        /// Writes unsigned short
        /// </summary>
        /// <param name="value">Value</param>
        void Write(ushort value);

        /// <summary>
        /// Writes array of booleans
        /// </summary>
        /// <param name="value">Value</param>
        void WriteArray(bool[] value);

        /// <summary>
        /// Writes array of shorts
        /// </summary>
        /// <param name="value">Value</param>
        void WriteArray(short[] value);

        /// <summary>
        /// Writes array of unsigned short
        /// </summary>
        /// <param name="value">Value</param>
        void WriteArray(ushort[] value);

        /// <summary>
        /// Writes array of unsigned integer
        /// </summary>
        /// <param name="value">Value</param>
        void WriteArray(uint[] value);

        /// <summary>
        /// Writes array of floats
        /// </summary>
        /// <param name="value">Value</param>
        void WriteArray(float[] value);

        /// <summary>
        /// Writes array of unsigned longs
        /// </summary>
        /// <param name="value">Value</param>
        void WriteArray(ulong[] value);

        /// <summary>
        /// Writes array of longs
        /// </summary>
        /// <param name="value">Value</param>
        void WriteArray(long[] value);

        /// <summary>
        /// Writes array of doubles
        /// </summary>
        /// <param name="value">Value</param>
        void WriteArray(double[] value);

        /// <summary>
        /// Writes array of strings
        /// </summary>
        /// <param name="value">Value</param>
        void WriteArray(string[] value);

        /// <summary>
        /// Writes array of integer
        /// </summary>
        /// <param name="value">Value</param>
        void WriteArray(int[] value);

        /// <summary>
        /// Writes array of strings of a maximum length
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="maxLength">Maximum length</param>
        void WriteArray(string[] value, int maxLength);

        /// <summary>
        /// Writes array of bytes, putting length before the buffer.
        /// </summary>
        /// <param name="data">Value</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        void WriteBytesWithLength(byte[] data, int offset, int length);

        /// <summary>
        /// Writes array of bytes preceding with length
        /// </summary>
        /// <param name="data">Value</param>
        void WriteBytesWithLength(byte[] data);

        /// <summary>
        /// Writes array of signed bytes, putting length before the buffer.
        /// </summary>
        /// <param name="data">Value</param>
        void WriteSBytesWithLength(sbyte[] data);

        /// <summary>
        /// Writes array of signed bytes, putting length before the buffer.
        /// </summary>
        /// <param name="data">Value</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        void WriteSBytesWithLength(sbyte[] data, int offset, int length);
    }
}