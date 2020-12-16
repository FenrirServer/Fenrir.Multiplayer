using System;

namespace Fenrir.Multiplayer.Exceptions
{
    /// <summary>
    /// Thrown when request handler encounters an error
    /// </summary>
    public class RequestHandlerException : Exception
    {
        public RequestHandlerException()
        {
        }

        public RequestHandlerException(string message) : base(message)
        {
        }

        public RequestHandlerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}