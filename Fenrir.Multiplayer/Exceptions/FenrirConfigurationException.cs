using System;

namespace Fenrir.Multiplayer.Exceptions
{
    /// <summary>
    /// Thrown when configuration is incorrect
    /// </summary>
    public class FenrirConfigurationException : FenrirException
    {
        public FenrirConfigurationException()
        {
        }

        public FenrirConfigurationException(string message)
            : base(message)
        {
        }

        public FenrirConfigurationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
