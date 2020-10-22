using System;

namespace Fenrir.Multiplayer.Exceptions
{
    public class FenrirHostException : FenrirException
    {
        public FenrirHostException()
        {
        }

        public FenrirHostException(string message)
            : base(message)
        {
        }

        public FenrirHostException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
