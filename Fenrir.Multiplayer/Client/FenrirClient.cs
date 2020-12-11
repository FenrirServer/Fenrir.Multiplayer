using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.LiteNet;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using Newtonsoft.Json;
using System;
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
        /// List of supported protocol connectors
        /// </summary>
        private readonly IProtocolConnector[] _supportedProtocolConnectors;

        /// <summary>
        /// Current protocol connector. Null if no connection attempt was made
        /// </summary>
        private IProtocolConnector _protocolConnector;

        /// <inheritdoc/>
        public string ClientId { get; set; }

        /// <inheritdoc/>
        public IClientPeer Peer => _protocolConnector?.Peer;

        /// <summary>
        /// Creates Fenrir Client
        /// </summary>
        /// <param name="httpClient">HttpClient to use</param>
        public FenrirClient(HttpClient httpClient)
            : this()
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Creates Fenrir Client
        /// </summary>
        /// <param name="supportedProtocolConnectors">Supported Protocols</param>
        /// <param name="httpClient">HttpClient to use</param>
        public FenrirClient(IProtocolConnector[] supportedProtocolConnectors, HttpClient httpClient)
            : this(supportedProtocolConnectors)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Creates Fenrir Client
        /// </summary>
        public FenrirClient()
            : this(new IProtocolConnector[] { new LiteNetProtocolConnector() })
        {
        }

        /// <summary>
        /// Creates Fenrir Client
        /// </summary>
        /// <param name="supportedProtocolConnectors">Supported Protocols</param>
        public FenrirClient(IProtocolConnector[] supportedProtocolConnectors)
        {
            ClientId = Guid.NewGuid().ToString();

            if(supportedProtocolConnectors == null)
            {
                throw new ArgumentNullException(nameof(supportedProtocolConnectors));
            }

            if (supportedProtocolConnectors.Length == 0)
            {
                throw new ArgumentException("Supported protocol connectors can not be empty", nameof(supportedProtocolConnectors));
            }

            _supportedProtocolConnectors = supportedProtocolConnectors;
        }

        /// <inheritdoc/>
        public async Task<ConnectionResponse> Connect(Uri serverInfoUri, object connectionRequestData = null)
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
        public async Task<ConnectionResponse> Connect(ServerInfo serverInfo, object connectionRequestData = null)
        {
            // Find best protocol
            ProtocolInfo selectedProtocolInfo = SelectProtocol(serverInfo);
            if(selectedProtocolInfo == null)
            {
                throw new FenrirClientException(
                    $"Failed to connect - no supported protocols found. Client supported protocols: " +
                    string.Join(", ", _supportedProtocolConnectors.Select(protocolConnector => protocolConnector.ProtocolType.ToString())) +
                    ", server supported protocols: " +
                    string.Join(", ", serverInfo.Protocols.Select(protocol => protocol.ProtocolType.ToString()))
                );
            }

            _protocolConnector = _supportedProtocolConnectors.Where(protocol => protocol.ProtocolType == selectedProtocolInfo.ProtocolType).First();

            // Get protocol data
            IProtocolConnectionData protocolData = (IProtocolConnectionData)selectedProtocolInfo.GetConnectionData(_protocolConnector.ConnectionDataType);

            // Connect using selected protocol
            var connectionRequest = new ClientConnectionRequest(ClientId, connectionRequestData, protocolData);
            return await _protocolConnector.Connect(connectionRequest);
        }

        private ProtocolInfo SelectProtocol(ServerInfo serverInfo)
        {
            return serverInfo.Protocols
                .Where(protocolInfo => _supportedProtocolConnectors.Any(listItem => protocolInfo.ProtocolType == listItem.ProtocolType))
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public void AddEventHandler<TEvent>(IEventHandler<TEvent> eventHandler) where TEvent : IEvent
        {
            foreach(var protocolConnector in _supportedProtocolConnectors)
            {
                protocolConnector.AddEventHandler<TEvent>(eventHandler);
            }
        }

        /// <inheritdoc/>
        public void RemoveEventHandler<TEvent>(IEventHandler<TEvent> eventHandler) where TEvent : IEvent
        {
            foreach (var protocolConnector in _supportedProtocolConnectors)
            {
                protocolConnector.RemoveEventHandler<TEvent>(eventHandler);
            }
        }

        /// <inheritdoc/>
        public void SetLogger(IFenrirLogger logger)
        {
            foreach (var protocolConnector in _supportedProtocolConnectors)
            {
                protocolConnector.SetLogger(logger);
            }
        }

        /// <inheritdoc/>
        public void SetContractSerializer(IContractSerializer contractSerializer)
        {
            foreach (var protocolConnector in _supportedProtocolConnectors)
            {
                protocolConnector.SetContractSerializer(contractSerializer);
            }
        }

        public void Dispose()
        {
            _protocolConnector.Dispose();
        }
    }
}
