using System;

namespace Fenrir.Multiplayer.Exceptions
{
    /// <summary>
    /// Thrown when TypeHashMap encounters an error 
    /// </summary>
    public class TypeHashMapException : FenrirException
    {
        public TypeHashMapException()
        {
        }

        public TypeHashMapException(string message)
            : base(message)
        {
        }

        public TypeHashMapException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
