using Fenrir.Multiplayer.Events;
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
    /// Fenrir Networking Client.
    /// Connects to the Fenrir <seealso cref="Server.NetworkServer"/>
    /// </summary>
    public class NetworkClient : INetworkClient, IClientEventListener, IDisposable
    {
        /// <summary>
        /// Invoked when client is disconnected.
        /// Provides detailed information about disconnect in arguments object
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Invoked when network error occurs. 
        /// Provides detailed information about network error in arguments object
        /// </summary>
        public event EventHandler<NetworkErrorEventArgs> NetworkError;

        /// <summary>
        /// Fenrir Logger, used to log events
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Network serializer, used for serialization/deserialization of network messages
        /// </summary>
        private readonly INetworkSerializer _serializer;

        /// <summary>
        /// Http Client to query Server Info service
        /// </summary>
        private readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Type Hash Map
        /// </summary>
        private readonly ITypeHashMap _typeHashMap;

        /// <summary>
        /// Event handler map
        /// </summary>
        private readonly EventHandlerMap _eventHandlerMap;

        /// <summary>
        /// LiteNetProtocol connector
        /// </summary>
        private readonly LiteNetProtocolConnector _liteNetProtocolConnector;


        /// <summary>
        /// Current protocol connector. Null if no connection attempt was made
        /// </summary>
        private IProtocolConnector _protocolConnector;

        /// <inheritdoc/>
        public string ClientId { get; set; }

        /// <inheritdoc/>
        public ProtocolType EnabledProtocols { get; set; } = ProtocolType.All;

        /// <inheritdoc/>
        public IClientPeer Peer => _protocolConnector?.Peer;

        /// <inheritdoc/>
        public ConnectionState State => _protocolConnector?.State ?? ConnectionState.Disconnected;

        /// <inheritdoc/>
        public INetworkSerializer Serializer => _serializer;

        /// <summary>
        /// Time after which server request times out
        /// </summary>
        public int RequestTimeoutMs
        {
            get => _liteNetProtocolConnector.RequestTimeoutMs;
            set => _liteNetProtocolConnector.RequestTimeoutMs = value;
        }

        /// <summary>
        /// Time after which client is disconnected if no keepalive packets are received from the server
        /// </summary>
        public int DisconnectTimeoutMs
        {
            get => _liteNetProtocolConnector.DisconnectTimeoutMs;
            set => _liteNetProtocolConnector.DisconnectTimeoutMs = value;
        }

        /// <summary>
        /// Network update rate, times per second
        /// </summary>
        public int UpdateRateHz
        {
            get => (int)(1000f / _liteNetProtocolConnector.UpdateTimeMs);
            set => _liteNetProtocolConnector.UpdateTimeMs = (int)(1000f / value);
        }

        /// <summary>
        /// Interval between pings. Should be smaller than <seealso cref="DisconnectTimeoutMs"/>
        /// </summary>
        public int PingIntervalMs
        {
            get => _liteNetProtocolConnector.PingIntervalMs;
            set => _liteNetProtocolConnector.PingIntervalMs = value;
        }


        /// <summary>
        /// Simulation packet loss
        /// </summary>
        public bool SimulatePacketLoss
        {
            get => _liteNetProtocolConnector.SimulatePacketLoss;
            set => _liteNetProtocolConnector.SimulatePacketLoss = value;
        }

        /// <summary>
        /// Simulate latency
        /// </summary>
        public bool SimulateLatency
        {
            get => _liteNetProtocolConnector.SimulateLatency;
            set => _liteNetProtocolConnector.SimulateLatency = value;
        }

        /// <summary>
        /// Chance of dropping the packet, if <seealso cref="SimulatePacketLoss"/> is set to true.
        /// </summary>
        public float SimulatedPacketLossChance
        {
            get => _liteNetProtocolConnector.SimulationPacketLossChance / 100f;
            set => _liteNetProtocolConnector.SimulationPacketLossChance = (int)(value * 100f);
        }

        /// <summary>
        /// Minimum latency, if <seealso cref="SimulateLatency"/> is set to true
        /// </summary>
        public int SimulatedMinLatency
        {
            get => _liteNetProtocolConnector.SimulationMinLatency;
            set => _liteNetProtocolConnector.SimulationMinLatency = value;
        }

        /// <summary>
        /// Maximum latency, if <seealso cref="SimulateLatency"/> is set to true
        /// </summary>
        public int SimulatedMaxLatency
        {
            get => _liteNetProtocolConnector.SimulationMaxLatency;
            set => _liteNetProtocolConnector.SimulationMaxLatency = value;
        }


        /// <summary>
        /// Creates new Network Client
        /// </summary>
        public NetworkClient()
            : this(new EventBasedLogger(), new NetworkSerializer(), new HttpClient())
        {
        }


        /// <summary>
        /// Creates new Network Client
        /// </summary>
        /// <param name="logger">Logger</param>
        public NetworkClient(ILogger logger)
            : this(logger, new NetworkSerializer(), new HttpClient())
        {
        }

        /// <summary>
        /// Creates new Network Client
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="serializer">Serializer</param>
        public NetworkClient(ILogger logger, INetworkSerializer serializer)
            : this(logger, serializer, new HttpClient())
        {
        }

        /// <summary>
        /// Creates new Network Client
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="serializer">Serializer</param>
        /// <param name="httpClient">Http Client</param>
        public NetworkClient(ILogger logger, INetworkSerializer serializer, HttpClient httpClient)
        {
            ClientId = Guid.NewGuid().ToString();

            _logger = logger;
            _serializer = serializer;
            _httpClient = httpClient;

            _typeHashMap = new TypeHashMap();
            _eventHandlerMap = new EventHandlerMap(logger);
            _liteNetProtocolConnector = new LiteNetProtocolConnector(this, _serializer, _typeHashMap, _logger);
        }

        /// <inheritdoc/>
        public async Task<ConnectionResponse> Connect(Uri serverInfoUri, object connectionRequestData = null)
        {
            var serverInfo = await GetServerInfo(serverInfoUri);
            return await Connect(serverInfo, connectionRequestData);
        }

        /// <inheritdoc/>
        public async Task<ConnectionResponse> Connect(string serverInfoUri, object connectionRequestData = null)
        {
            return await Connect(new Uri(serverInfoUri), connectionRequestData);
        }

        private async Task<ServerInfo> GetServerInfo(Uri serverInfoUri)
        {
            var httpResponse = await _httpClient.GetAsync(serverInfoUri);
            string responseText = await httpResponse.Content.ReadAsStringAsync();
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new NetworkClientException($"Failed to get server info from {serverInfoUri}, server returned status code {httpResponse.StatusCode}: {responseText}");
            }

            ServerInfo serverInfo;

            try
            {
                serverInfo = JsonConvert.DeserializeObject<ServerInfo>(responseText);
            }
            catch (JsonException e)
            {
                throw new NetworkClientException($"Failed to get server info from {serverInfoUri}, failed to deserialize response: {e}", e);
            }

            return serverInfo;
        }

        /// <inheritdoc/>
        public async Task<ConnectionResponse> Connect(ServerInfo serverInfo, object connectionRequestData = null)
        {
            // Find best protocol
            _protocolConnector = SelectProtocolConnector(serverInfo);

            if(_protocolConnector == null)
            {
                throw new NetworkClientException(
                    $"Failed to connect - no supported protocols found. Client enabled protocols: " +
                    EnabledProtocols.ToString() + 
                    ", server supported protocols: " +
                    string.Join(", ", serverInfo.Protocols.Select(protocol => protocol.ProtocolType.ToString()))
                );
            }

            _protocolConnector.Disconnected += OnProtocolConnectorDisconnected;
            _protocolConnector.NetworkError += OnProtocolConnectorNetworkError;

            // Get protocol data
            ProtocolInfo protocolInfo = serverInfo.Protocols.Where(protoInfo => protoInfo.ProtocolType == _protocolConnector.ProtocolType).FirstOrDefault();
            IProtocolConnectionData protocolData = (IProtocolConnectionData)protocolInfo.GetConnectionData(_protocolConnector.ConnectionDataType);

            // Connect using selected protocol
            var connectionRequest = new ClientConnectionRequest(serverInfo.Hostname, ClientId, connectionRequestData, protocolData);
            return await _protocolConnector.Connect(connectionRequest);
        }

        /// <summary>
        /// Selects a preferred protocol connector from the list of ones available on a specific server
        /// </summary>
        /// <param name="serverInfo">Server info</param>
        /// <returns>Selected protocol connector</returns>
        private IProtocolConnector SelectProtocolConnector(ServerInfo serverInfo)
        {
            // Prefer UDP
            var liteNetProtocolInfo = serverInfo.Protocols.Where(protocolInfo => protocolInfo.ProtocolType == ProtocolType.LiteNet).FirstOrDefault();
            if(liteNetProtocolInfo != null && EnabledProtocols.HasFlag(ProtocolType.LiteNet))
            {
                return _liteNetProtocolConnector;
            }

            // If udp is not supported, use websocket
            // TODO

            return null;
        }

        /// <inheritdoc/>
        public void Disconnect()
        {
            if (State != ConnectionState.Disconnected)
            {
                _protocolConnector?.Disconnect();
            }
        }

        /// <inheritdoc/>
        public void AddEventHandler<TEvent>(IEventHandler<TEvent> eventHandler) where TEvent : IEvent
        {
            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            _typeHashMap.AddType<TEvent>();
            _eventHandlerMap.AddEventHandler<TEvent>(eventHandler);
        }

        /// <inheritdoc/>
        public void RemoveEventHandler<TEvent>(IEventHandler<TEvent> eventHandler) where TEvent : IEvent
        {
            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }

            _eventHandlerMap.RemoveEventHandler<TEvent>(eventHandler);
        }

        private void OnProtocolConnectorDisconnected(object sender, DisconnectedEventArgs e)
        {
            Disconnected?.Invoke(sender, e);
        }

        private void OnProtocolConnectorNetworkError(object sender, NetworkErrorEventArgs e)
        {
            NetworkError?.Invoke(sender, e);
        }
        
        /// <inheritdoc/>
        public void AddSerializableTypeFactory<T>(Func<T> factoryMethod) where T : IByteStreamSerializable
        {
            if(factoryMethod == null)
            {
                throw new ArgumentNullException(nameof(factoryMethod));
            }

            _serializer.AddTypeFactory<T>(factoryMethod);
        }

        /// <inheritdoc/>
        public void RemoveSerializableTypeFactory<T>() where T : IByteStreamSerializable
        {
            _serializer.RemoveTypeFactory<T>();
        }


        #region IDisposable Implementation
        public void Dispose()
        {
            // Dispose existing protocol connector
            _protocolConnector?.Dispose();
            _protocolConnector = null;
        }
        #endregion

        #region IClientEventListener Implementation
        void IClientEventListener.OnReceiveEvent(MessageWrapper messageWrapper)
        {
            _eventHandlerMap.OnReceiveEvent(messageWrapper);
        }
        #endregion
    }
}
