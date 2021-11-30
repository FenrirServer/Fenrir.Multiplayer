using System;

namespace Fenrir.Multiplayer.Exceptions
{
    /// <summary>
    /// Indicates that server failed to handle client request
    /// </summary>
    public class RequestFailedException : NetworkClientException
    {
        public RequestFailedException()
        {
        }

        public RequestFailedException(string message)
            : base(message)
        {
        }

        public RequestFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
