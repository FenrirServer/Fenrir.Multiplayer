using System;

namespace Fenrir.Multiplayer.Exceptions
{
    /// <summary>
    /// Base Fenrir NetworkClient exception
    /// Thrown on the client
    /// </summary>
    public class NetworkClientException : FenrirException
    {
        public NetworkClientException()
        {
        }

        public NetworkClientException(string message)
            : base(message)
        {
        }

        public NetworkClientException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
