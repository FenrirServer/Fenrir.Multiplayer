namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Internal class to pass the result of user connection
    /// </summary>
    class ConnectionHandlerResult
    {
        /// <summary>
        /// Connection response
        /// </summary>
        public ConnectionResponse Response { get; set; }

        /// <summary>
        /// Custom connection request data. 
        /// If no custom connection request handler is set, always null
        /// </summary>
        public object ConnectionRequestData { get; set; }

        /// <summary>
        /// Creates empty connection handler result
        /// </summary>
        public ConnectionHandlerResult()
        {
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="response">Connection Response</param>
        /// <param name="connectionRequestData">Connection Request Data</param>
        public ConnectionHandlerResult(ConnectionResponse response, object connectionRequestData)
        {
            Response = response;
            ConnectionRequestData = connectionRequestData;
        }
    }
}
