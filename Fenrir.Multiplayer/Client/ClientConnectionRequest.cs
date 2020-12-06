using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.Client
{
    public class ClientConnectionRequest
    {
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

        public ClientConnectionRequest(string clientId, object connectionRequestData, IProtocolConnectionData protocolConnectionData)
        {
            ClientId = clientId;
            ConnectionRequestData = connectionRequestData;
            ProtocolConnectionData = protocolConnectionData;
        }
    }
}
