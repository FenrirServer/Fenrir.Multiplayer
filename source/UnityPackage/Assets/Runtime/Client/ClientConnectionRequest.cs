using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.Client
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

        public ClientConnectionRequest(string hostname, string clientId, object connectionRequestData, IProtocolConnectionData protocolConnectionData)
        {
            Hostname = hostname;
            ClientId = clientId;
            ConnectionRequestData = connectionRequestData;
            ProtocolConnectionData = protocolConnectionData;
        }
    }
}
