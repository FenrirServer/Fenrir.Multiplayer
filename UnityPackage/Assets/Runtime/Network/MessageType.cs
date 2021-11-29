namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Type of the message
    /// </summary>
    enum MessageType : byte
    {
        /// <summary>
        /// Event
        /// Events are sent from server to client,
        /// to notify client(s) of a state change
        /// </summary>
        Event,

        /// <summary>
        /// Request
        /// Request are sent from client to server, and might require a response
        /// </summary>
        Request,

        /// <summary>
        /// Response
        /// Responses are sent back from server to client, as a result of a given request
        /// </summary>
        Response
    }
}
