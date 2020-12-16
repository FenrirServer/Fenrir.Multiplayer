using System;

namespace Fenrir.Multiplayer.Exceptions
{
    /// <summary>
    /// Thrown when TypeMap encounters an error 
    /// </summary>
    public class TypeMapException : FenrirException
    {
        public TypeMapException()
        {
        }

        public TypeMapException(string message)
            : base(message)
        {
        }

        public TypeMapException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
