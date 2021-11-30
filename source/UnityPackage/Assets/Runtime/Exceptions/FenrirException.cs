using System;

namespace Fenrir.Multiplayer.Exceptions
{
    /// <summary>
    /// Base Fenrir Multiplayer SDK Exception
    /// </summary>
    public class FenrirException : Exception
    {
        public FenrirException()
        {
        }

        public FenrirException(string message)
            : base(message)
        {
        }

        public FenrirException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}