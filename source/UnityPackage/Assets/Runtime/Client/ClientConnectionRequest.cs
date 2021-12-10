namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Client to server connection request.
    /// Contains data that's passed to the <seealso cref="IProtocolConnector"/>
    /// </summary>
    public class ClientConnectionRequest
    {
        /// <summary>
        /// Sever hostname
        /// </summary>
        public string Hostname { get; private set; }

        /// <summary>
        /// Server public key
        /// </summary>
        public string ServerPublicKey { get; private set; }

        /// <summary>
        /// Unique id of the client
        /// </summary>
        public string ClientId { get; private set; }

        /// <summary>
        /// Custom conection data that server connection request handler can dispatch
        /// </summary>
        public object ConnectionRequestData { get; private set; }

        /// <summary>
        /// Protocol-specific connection metadata
        /// </summary>
        public IProtocolConnectionData ProtocolConnectionData { get; private set; }

        /// <summary>
        /// Creates client connection request
        /// </summary>
        /// <param name="hostname">Hostname</param>
        /// <param name="serverPublicKey">Server public key</param>
        /// <param name="clientId">Unique Client Id</param>
        /// <param name="connectionRequestData">Connection Request Data</param>
        /// <param name="protocolConnectionData">Protocol Connection Data</param>
        public ClientConnectionRequest(string hostname, string serverPublicKey, string clientId, object connectionRequestData, IProtocolConnectionData protocolConnectionData)
        {
            Hostname = hostname;
            ServerPublicKey = serverPublicKey;
            ClientId = clientId;
            ConnectionRequestData = connectionRequestData;
            ProtocolConnectionData = protocolConnectionData;
        }
    }
}
