using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Client
{
    /// <summary>
    /// Fenrir Networking Client
    /// </summary>
    public class FenrirClient : IFenrirClient, IDisposable
    {
        /// <summary>
        /// Http Client to query Server Info service
        /// </summary>
        private readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// List of supported protocols
        /// </summary>
        private List<IProtocol> _supportedProtocols = new List<IProtocol>();

        /// <summary>
        /// Current protocol connector. Null if no connection attempt was made
        /// </summary>
        private IProtocolConnector _protocolConnector;

        /// <inheritdoc/>
        public string ClientId { get; set; }

        /// <inheritdoc/>
        public IClientPeer Peer => _protocolConnector?.Peer;

        /// <summary>
        /// Constructor that takes in HttpClient
        /// </summary>
        /// <param name="httpClient">HttpClient</param>
        public FenrirClient(HttpClient httpClient)
            : this()
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public FenrirClient()
        {
            ClientId = Guid.NewGuid().ToString();
        }

        /// <inheritdoc/>
        public void AddProtocol(IProtocol protocol)
        {
            _supportedProtocols.Add(protocol);
        }

        /// <inheritdoc/>
        public async Task<ClientConnectionResult> Connect(Uri serverInfoUri, object connectionRequestData = null)
        {
            var serverInfo = await GetServerInfo(serverInfoUri);
            return await Connect(serverInfo, connectionRequestData);
        }

        private async Task<ServerInfo> GetServerInfo(Uri serverInfoUri)
        {
            var httpResponse = await _httpClient.GetAsync(serverInfoUri);
            string responseText = await httpResponse.Content.ReadAsStringAsync();
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new FenrirClientException($"Failed to get server info from {serverInfoUri}, server returned status code {httpResponse.StatusCode}: {responseText}");
            }

            ServerInfo serverInfo;

            try
            {
                serverInfo = JsonConvert.DeserializeObject<ServerInfo>(responseText);
            }
            catch (JsonException e)
            {
                throw new FenrirClientException($"Failed to get server info from {serverInfoUri}, failed to deserialize response: {e}", e);
            }

            return serverInfo;
        }

        /// <inheritdoc/>
        public async Task<ClientConnectionResult> Connect(ServerInfo serverInfo, object connectionRequestData = null)
        {
            // Find best protocol
            ProtocolInfo selectedProtocolInfo = SelectProtocol(serverInfo);
            if(selectedProtocolInfo == null)
            {
                throw new FenrirClientException(
                    $"Failed to connect - no supported protocols found. Client supported protocols: " +
                    string.Join(", ", _supportedProtocols.Select(protocol => protocol.ProtocolType.ToString())) +
                    ", server supported protocols: " +
                    string.Join(", ", serverInfo.Protocols.Select(protocol => protocol.ProtocolType.ToString()))
                );
            }

            var selectedProtocol = _supportedProtocols.Where(protocol => protocol.ProtocolType == selectedProtocolInfo.ProtocolType).First();

            // Get protocol data
            IProtocolConnectionData protocolData = (IProtocolConnectionData)selectedProtocolInfo.GetConnectionData(selectedProtocol.ConnectionDataType);

            // Connect using selected protocol
            _protocolConnector = selectedProtocol.CreateConnector();
            var connectionRequest = new ClientConnectionRequest(ClientId, connectionRequestData, protocolData);
            return await _protocolConnector.Connect(connectionRequest);
        }

        private ProtocolInfo SelectProtocol(ServerInfo serverInfo)
        {
            if(_supportedProtocols.Count == 0)
            {
                throw new FenrirClientException("No protocols found. Please add at least one protocol using AddProtocol");
            }

            return serverInfo.Protocols
                .Where(protocolInfo => _supportedProtocols.Any(listItem => protocolInfo.ProtocolType == listItem.ProtocolType))
                .FirstOrDefault();
        }

        public void Dispose()
        {
            _protocolConnector.Dispose();
        }
    }
}
