using System;

namespace Fenrir.Multiplayer.Exceptions
{
    /// <summary>
    /// Base Fenrir NetworkServer exception
    /// Thrown on the server
    /// </summary>
    public class NetworkServerException : FenrirException
    {
        public NetworkServerException()
        {
        }

        public NetworkServerException(string message)
            : base(message)
        {
        }

        public NetworkServerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
