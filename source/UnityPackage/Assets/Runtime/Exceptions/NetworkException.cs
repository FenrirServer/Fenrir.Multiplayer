using System;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Base Network Exception
    /// </summary>
    public class NetworkException : FenrirException
    {
        /// <summary>
        /// Creates Network Exception
        /// </summary>
        public NetworkException()
        {
        }

        /// <summary>
        /// Create Network Exception using string message
        /// </summary>
        /// <param name="message">Exception message</param>
        public NetworkException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates Network Exception using string message and an inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public NetworkException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
