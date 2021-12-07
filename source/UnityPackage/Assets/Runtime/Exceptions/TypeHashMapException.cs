using System;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Thrown when TypeHashMap encounters an error 
    /// </summary>
    public class TypeHashMapException : FenrirException
    {
        /// <summary>
        /// Creates Type Hash Map Exception
        /// </summary>
        public TypeHashMapException()
        {
        }

        /// <summary>
        /// Create Type Hash Map Exception using string message
        /// </summary>
        /// <param name="message">Exception message</param>
        public TypeHashMapException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates Type Hash Map Exception using string message and an inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public TypeHashMapException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
