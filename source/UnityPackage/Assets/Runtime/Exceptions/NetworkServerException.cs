using System;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Base Fenrir NetworkServer exception
    /// Thrown on the server
    /// </summary>
    public class NetworkServerException : FenrirException
    {
        /// <summary>
        /// Creates Network Server Exception
        /// </summary>
        public NetworkServerException()
        {
        }

        /// <summary>
        /// Create Network Server Exception using string message
        /// </summary>
        /// <param name="message">Exception message</param>
        public NetworkServerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates Network Server Exception using string message and an inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public NetworkServerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
