using System;

namespace Fenrir.Multiplayer.Exceptions
{
    public class FenrirConfiguratorException : FenrirException
    {
        public FenrirConfiguratorException()
        {
        }

        public FenrirConfiguratorException(string message)
            : base(message)
        {
        }

        public FenrirConfiguratorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
