using System;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Message Flags
    /// </summary>
    [Flags]
    enum MessageFlags : byte
    {
        /// <summary>
        /// No specific flags
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates if message has unique request id.
        /// This is true for requests that require a response and responses.
        /// </summary>
        HasRequestId = 1,

        /// <summary>
        /// Indicates if message is encrypted
        /// </summary>
        IsEncrypted = 2,

        /// <summary>
        /// Indicates if responses should arrive in order in the selected channel
        /// </summary>
        IsOrdered = 4,
    }
}
