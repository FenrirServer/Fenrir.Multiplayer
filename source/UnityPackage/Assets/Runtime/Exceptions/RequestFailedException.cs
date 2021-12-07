using System;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Indicates that server failed to handle client request
    /// </summary>
    public class RequestFailedException : NetworkClientException
    {
        /// <summary>
        /// Creates Request Failed Exception
        /// </summary>
        public RequestFailedException()
        {
        }

        /// <summary>
        /// Create Request Failed Exception using string message
        /// </summary>
        /// <param name="message">Exception message</param>
        public RequestFailedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates Request Failed Exception using string message and an inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public RequestFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
