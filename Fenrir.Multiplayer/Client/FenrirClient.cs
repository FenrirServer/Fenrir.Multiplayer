﻿using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.LiteNet;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
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
        /// List of supported protocol connectors
        /// </summary>
        private readonly List<IProtocolConnector> _supportedProtocolConnectors = new List<IProtocolConnector>();

        /// <summary>
        /// Current protocol connector. Null if no connection attempt was made
        /// </summary>
        private IProtocolConnector _protocolConnector;

        /// <inheritdoc/>
        public string ClientId { get; set; }

        /// <inheritdoc/>
        public IClientPeer Peer => _protocolConnector?.Peer;

        /// <inheritdoc/>
        public ConnectionState State => _protocolConnector?.State ?? ConnectionState.Disconnected;


        /// <summary>
        /// Creates new Fenrir Client
        /// </summary>
        public FenrirClient()
            : this(new EventBasedLogger(), new HttpClient())
        {
        }


        /// <summary>
        /// Creates new Fenrir Client
        /// </summary>
        /// <param name="logger">Logger</param>
        public FenrirClient(IFenrirLogger logger)
            : this(logger, new HttpClient())
        {
        }

        /// <summary>
        /// Creates new Fenrir Client
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="httpClient">Http Client</param>
        public FenrirClient(IFenrirLogger logger, HttpClient httpClient)
        {
            ClientId = Guid.NewGuid().ToString();
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
            var connectionRequest = new ClientConnectionRequest(serverInfo.Hostname, ClientId, connectionRequestData, protocolData);
            return await _protocolConnector.Connect(connectionRequest);
        }

        private ProtocolInfo SelectProtocol(ServerInfo serverInfo)
        {
            return serverInfo.Protocols
                .Where(protocolInfo => _supportedProtocolConnectors.Any(listItem => protocolInfo.ProtocolType == listItem.ProtocolType))
                .FirstOrDefault();
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
        public void AddProtocol(IProtocolConnector protocolConnector)
        {
            _supportedProtocolConnectors.Add(protocolConnector);
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
        public void SetContractSerializer(ITypeSerializer contractSerializer)
        {
            foreach (var protocolConnector in _supportedProtocolConnectors)
            {
                protocolConnector.SetContractSerializer(contractSerializer);
            }
        }


        public void Dispose()
        {
            // Dispose existing protocol connector
            _protocolConnector?.Dispose();
            _protocolConnector = null;
        }
    }
}
