using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Multiplayer.Exceptions
{
    /// <summary>
    /// Client request timed out
    /// </summary>
    public class RequestTimeoutException : NetworkClientException
    {
        public RequestTimeoutException()
        {
        }

        public RequestTimeoutException(string message)
            : base(message)
        {
        }

        public RequestTimeoutException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
