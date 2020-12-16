using System;

namespace Fenrir.Multiplayer.Exceptions
{
    /// <summary>
    /// Base Fenrir Server exception
    /// </summary>
    public class FenrirServerException : FenrirException
    {
        public FenrirServerException()
        {
        }

        public FenrirServerException(string message)
            : base(message)
        {
        }

        public FenrirServerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
