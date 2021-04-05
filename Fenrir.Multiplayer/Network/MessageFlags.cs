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
        /// No specific flags, message is considered raw message
        /// </summary>
        None = 0,

        /// <summary>
        /// Message is an event. 
        /// Events are sent from server to client, to notify client(s) of a state change
        /// </summary>
        IsEvent = 1,

        /// <summary>
        /// Message is a request.
        /// Requests are sent from client to server, and might optionally require a response
        /// </summary>
        IsRequest = 2,

        /// <summary>
        /// Message is a response.
        /// Responses are sent back from server to client, as a result of a given request
        /// </summary>
        /// <remarks>For the efficiency of packing, let's consider (IsEvent | IsRequest) == IsResponse </remarks>
        IsResponse = 3,

        /// <summary>
        /// Indicates if message has unique request/response id.
        /// This is true for requests that require a response and responses.
        /// </summary>
        HasUniqueId = 4,

        /// <summary>
        /// Indicates if message is encrypted
        /// </summary>
        IsEncrypted = 8,

        /// <summary>
        /// Indicates if responses should arrive in order in the selected channel
        /// </summary>
        IsOrdered = 16,
    }
}
