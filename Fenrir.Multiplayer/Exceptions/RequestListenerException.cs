using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Multiplayer.Exceptions
{
    public class RequestListenerException : NetworkException
    {
        public RequestListenerException()
        {
        }

        public RequestListenerException(string message)
            : base(message)
        {
        }

        public RequestListenerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
