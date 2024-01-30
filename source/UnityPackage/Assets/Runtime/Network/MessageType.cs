namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Type of the message
    /// </summary>
    enum MessageType
    {
        /// <summary>
        /// Raw bytes
        /// </summary>
        RawBytes = 0,

        /// <summary>
        /// Request with no response
        /// </summary>
        Request = 1,

        /// <summary>
        /// Request that requires a response
        /// </summary>
        RequestWithResponse = 2,

        /// <summary>
        /// Response
        /// Responses are sent back from server to client, as a result of a given request
        /// </summary>
        Response = 3,

        /// <summary>
        /// Event
        /// Events are sent from server to client,
        /// to notify client(s) of a state change
        /// </summary>
        Event = 4,
    }
}
