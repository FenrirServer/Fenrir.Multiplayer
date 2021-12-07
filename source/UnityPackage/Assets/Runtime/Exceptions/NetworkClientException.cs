using System;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Base Fenrir NetworkClient exception
    /// Thrown on the client
    /// </summary>
    public class NetworkClientException : FenrirException
    {
        /// <summary>
        /// Creates Network Client Exception
        /// </summary>
        public NetworkClientException()
        {
        }

        /// <summary>
        /// Create Network Client Exception using string message
        /// </summary>
        /// <param name="message">Exception message</param>
        public NetworkClientException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates Network Client Exception using string message and an inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public NetworkClientException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
