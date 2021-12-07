using System;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Client request timed out
    /// </summary>
    public class RequestTimeoutException : NetworkClientException
    {
        /// <summary>
        /// Creates Request Timeout Exception
        /// </summary>
        public RequestTimeoutException()
        {
        }

        /// <summary>
        /// Create Request Timeout Exception using string message
        /// </summary>
        /// <param name="message">Exception message</param>
        public RequestTimeoutException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates Request Timeout Exception using string message and an inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public RequestTimeoutException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
