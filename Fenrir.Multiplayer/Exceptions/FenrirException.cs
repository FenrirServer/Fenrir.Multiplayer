using System;

namespace Fenrir.Multiplayer.Exceptions
{
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