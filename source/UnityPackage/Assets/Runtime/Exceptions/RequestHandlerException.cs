using System;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Thrown when request handler encounters an error
    /// </summary>
    public class RequestHandlerException : Exception
    {
        /// <summary>
        /// Creates Request Handler Exception
        /// </summary>
        public RequestHandlerException()
        {
        }

        /// <summary>
        /// Create Request Handler Exception using string message
        /// </summary>
        /// <param name="message">Exception message</param>
        public RequestHandlerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates Request Handler Exception using string message and an inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public RequestHandlerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}