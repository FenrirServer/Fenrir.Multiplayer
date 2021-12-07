using System;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Base Fenrir Multiplayer SDK Exception
    /// </summary>
    public class FenrirException : Exception
    {
        /// <summary>
        /// Creates Fenrir Exception
        /// </summary>
        public FenrirException()
        {
        }

        /// <summary>
        /// Create Fenrir Exception using string message
        /// </summary>
        /// <param name="message">Exception message</param>
        public FenrirException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates Fenrir Exception using string message and an inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public FenrirException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}