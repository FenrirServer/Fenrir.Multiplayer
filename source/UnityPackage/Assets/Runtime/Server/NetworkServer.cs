﻿using Fenrir.Multiplayer.LiteNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Network Server
    /// </summary>
    public class NetworkServer : INetworkServer, IServerEventListener
    {
        /// <inheritdoc/>
        public event EventHandler<ServerStatusChangedEventArgs> StatusChanged;

        /// <inheritdoc/>
        public event EventHandler<ServerPeerConnectedEventArgs> PeerConnected;

        /// <inheritdoc/>
        public event EventHandler<ServerPeerDisconnectedEventArgs> PeerDisconnected;

        /// <summary>
        /// Connection Request Handler delegate
        /// </summary>
        /// <param name="protocolVersion">Protocol version of the client</param>
        /// <param name="clientId">Unique ID of the client</param>
        /// <param name="remoteEndPoint">Remote IP</param>
        /// <param name="connectionRequestDataReader">Custom connection data</param>
        /// <returns>Connection Handler Result object that contains response and additional data</returns>
        private delegate Task<ConnectionHandlerResult> ConnectionRequestHandler(int protocolVersion, string clientId, IPEndPoint remoteEndPoint, IByteStreamReader connectionRequestDataReader);

        /// <summary>
        /// Type hash map
        /// </summary>
        private readonly ITypeHashMap _typeHashMap;

        /// <summary>
        /// Request handler map
        /// </summary>
        private readonly RequestHandlerMap _requestHandlerMap;

        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// Serializer
        /// </summary>
        public INetworkSerializer Serializer { get; private set; }


        /// <inheritdoc/>
        public string ServerId { get; set; }

        /// <inheritdoc/>
        public string Hostname { get; set; } = "127.0.0.1";

        /// <summary>
        /// IPv4 endpoint at which listener should be bound
        /// </summary>
        public string BindIPv4 { get; set; } = "0.0.0.0";

        /// <summary>
        /// IPv6 endpoint at which listener should be bound
        /// </summary>
        public string BindIPv6 { get; set; } = "::";

        /// <summary>
        /// TCP port at which a Server Info servers is bound
        /// </summary>
        public ushort ServerInfoPort { get; set; } = 27016;

        /// <summary>
        /// UDP port at which a network listener is bound
        /// </summary>
        public ushort BindPort { get; set; } = 27016;

        /// <summary>
        /// Public port. Overrides <seealso cref="BindPort"/> when reporting to the client
        /// Override this port if container maps <seealso cref="BindPort"/> to something else
        /// </summary>
        public ushort? PublicPort { get; set; } = null;

        /// <summary>
        /// Server ticks per second
        /// </summary>
        public int TickRate
        {
            get => _liteNetListener.TickRate;
            set => _liteNetListener.TickRate = value;
        }

        /// <summary>
        /// If set to true, events are polled automatically.
        /// If set to false, you must call PollEvents manually
        /// </summary>
        public bool AutoPollEvents
        {
            get => _liteNetListener.AutoPollEvents;
            set => _liteNetListener.AutoPollEvents = value;
        }

        /// <summary>
        /// If set to true, <see cref="TickRate"/> is ignored and requests handlers are invoked from multiple threads
        /// </summary>
        public bool UnsyncedEvents
        {
            get => _liteNetListener.UnsyncedEvents;
            set => _liteNetListener.UnsyncedEvents = value;
        }


        /// <inheritdoc/>
        public IEnumerable<IProtocolListener> Listeners
        {
            get
            {
                yield return _liteNetListener;
                //yield return _webSocketListener;
            }
        }


        /// <inheritdoc/>
        public int Ccu => _liteNetListener.NumPeers;

        /// <inheritdoc/>
        public ServerStatus Status => _status;

        /// <inheritdoc/>
        public bool IsRunning => Status == ServerStatus.Running;


        /// <summary>
        /// Server status
        /// </summary>
        private volatile ServerStatus _status = ServerStatus.Stopped;

        /// <summary>
        /// LiteNet Protocol
        /// </summary>
        private LiteNetProtocolListener _liteNetListener;

        /// <summary>
        /// Server info service
        /// </summary>
        private ServerInfoService _serverInfoService;

        /// <summary>
        /// Stores custom connection request handler if set up
        /// </summary>
        private ConnectionRequestHandler _connectionRequestHandler = null;

        /// <summary>
        /// Connected peers
        /// </summary>
        public IEnumerable<IServerPeer> Peers => _liteNetListener.Peers;

        /// <summary>
        /// Creates Network Server
        /// </summary>
        public NetworkServer()
            : this(new EventBasedLogger(), new NetworkSerializer())
        {
        }

        /// <summary>
        /// Creates new Network Server
        /// </summary>
        /// <param name="logger">Logger</param>
        public NetworkServer(ILogger logger)
            : this(logger, new NetworkSerializer())
        {
        }

        /// <summary>
        /// Creates new Network Server
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="serializer">Serializer</param>
        public NetworkServer(ILogger logger, INetworkSerializer serializer)
        {
            ServerId = Guid.NewGuid().ToString();

            Logger = logger;
            Serializer = serializer;

            _typeHashMap = new TypeHashMap();
            _requestHandlerMap = new RequestHandlerMap(Logger);

            // Setup protocols
            _liteNetListener = new LiteNetProtocolListener(this, Serializer, _typeHashMap, Logger);

            // Setup info service
            _serverInfoService = new ServerInfoService(this);
        }

        /// <summary>
        /// Starts Network Server
        /// </summary>
        public void Start()
        {
            if(Status != ServerStatus.Stopped)
            {
                throw new InvalidOperationException("Failed to start server, server is already " + Status);
            }

            SetStatus(ServerStatus.Starting);

            // Start server info service
            _serverInfoService.Start(ServerInfoPort);

            // Start LiteNet Protocol listener
            _liteNetListener.Start(BindIPv4, BindIPv6, BindPort, PublicPort);

            SetStatus(ServerStatus.Running);
        }

        /// <summary>
        /// Stops Network Server
        /// </summary>
        public void Stop()
        {
            if (Status != ServerStatus.Running && Status != ServerStatus.Starting)
            {
                throw new InvalidOperationException("Failed to stop server, server is already " + Status);
            }

            SetStatus(ServerStatus.Stopping);

            // Stop info service
            _serverInfoService.Stop();

            // Stop protocol listeners
            _liteNetListener.Stop();

            SetStatus(ServerStatus.Stopped);
        }

        /// <summary>
        /// Sets server status and invokes the event
        /// </summary>
        private void SetStatus(ServerStatus status)
        {
            _status = status;

            StatusChanged?.Invoke(this, new ServerStatusChangedEventArgs(status));
        }


        /// <summary>
        /// Polls network events manually (must be called if <see cref="AutoPollEvents"/> is false)
        /// </summary>
        public void PollEvents()
        {
            _liteNetListener?.PollEvents();
        }

        /// <inheritdoc/>
        public void SetConnectionRequestHandler<TConnectionRequestData>(Func<IServerConnectionRequest<TConnectionRequestData>, ConnectionResponse> handler)
            where TConnectionRequestData : class, new()
        {
            SetConnectionRequestHandlerAsync<TConnectionRequestData>((connectionRequestData) => Task.FromResult<ConnectionResponse>(handler(connectionRequestData)));
        }

        /// <inheritdoc/>
        public void SetConnectionRequestHandlerAsync<TConnectionRequestData>(Func<IServerConnectionRequest<TConnectionRequestData>, Task<ConnectionResponse>> handler) 
            where TConnectionRequestData : class, new()
        {
            if(handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            // Add type to the type map
            _typeHashMap.AddType<TConnectionRequestData>();

            // Add connection request handler delegate
            TConnectionRequestData connectionRequestData = null;
            
            // Set connection request handler
            _connectionRequestHandler = async (protocolVersion, clientId, remoteEndPoint, connectionDataReader) =>
            {
                // Result
                ConnectionHandlerResult result = new ConnectionHandlerResult();

                // If custom connection request data is present, deserialize
                if (connectionDataReader != null && !connectionDataReader.EndOfData)
                {
                    try
                    {
                        connectionRequestData = Serializer.Deserialize<TConnectionRequestData>(connectionDataReader);
                    }
                    catch (SerializationException e)
                    {
                        Logger.Debug("Rejected connection request from {0}, failed to deserialize connection data: {1}", remoteEndPoint, e);
                        result.Response = ConnectionResponse.Failed("Bad connection request data");
                        return result;
                    }
                }

                // Create connection request object
                ServerConnectionRequest<TConnectionRequestData> serverConnectionRequest = new ServerConnectionRequest<TConnectionRequestData>(remoteEndPoint, protocolVersion, clientId, connectionRequestData);

                // Invoke handler
                ConnectionResponse response;
                try
                {
                    response = await handler(serverConnectionRequest);
                    result.Response = response;
                    result.ConnectionRequestData = connectionRequestData;
                    return result;
                }
                catch (Exception e)
                {
                    Logger.Error("Unhandled exception in connection request handler : {0}", e);
                    result.Response = ConnectionResponse.Failed("Unhandled exception in connection request handler");
                    return result;
                }
            };

        }

        /// <inheritdoc/>
        public void AddRequestHandler<TRequest>(IRequestHandler<TRequest> requestHandler) 
            where TRequest : IRequest
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            _typeHashMap.AddType<TRequest>();
            _requestHandlerMap.AddRequestHandler<TRequest>(requestHandler);
        }

        /// <inheritdoc/>
        public void RemoveRequestHandler<TRequest>()
            where TRequest : IRequest
        {
            _requestHandlerMap.RemoveRequestHandler<TRequest>();
        }

        /// <inheritdoc/>
        public void AddRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            _typeHashMap.AddType<TRequest>();
            _typeHashMap.AddType<TResponse>();
            _requestHandlerMap.AddRequestHandler<TRequest, TResponse>(requestHandler);
        }


        /// <inheritdoc/>
        public void RemoveRequestHandler<TRequest, TResponse>()
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            _requestHandlerMap.RemoveRequestHandler<TRequest, TResponse>();
        }

        /// <inheritdoc/>
        public void AddRequestHandlerAsync<TRequest, TResponse>(IRequestHandlerAsync<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            _typeHashMap.AddType<TRequest>();
            _typeHashMap.AddType<TResponse>();
            _requestHandlerMap.AddRequestHandlerAsync<TRequest, TResponse>(requestHandler);
        }

        /// <inheritdoc/>
        public void RemoveRequestHandlerAsync<TRequest, TResponse>()
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            _requestHandlerMap.RemoveRequestHandlerAsync<TRequest, TResponse>();
        }

        /// <inheritdoc/>
        public void AddSerializableTypeFactory<T>(Func<T> factoryMethod) where T : IByteStreamSerializable
        {
            if(factoryMethod == null)
            {
                throw new ArgumentNullException(nameof(factoryMethod));
            }

            Serializer.AddTypeFactory<T>(factoryMethod);
        }

        /// <inheritdoc/>
        public void RemoveSerializableTypeFactory<T>() where T : IByteStreamSerializable
        {
            Serializer.RemoveTypeFactory<T>();
        }

        #region IDisposable Implementation
        /// <inheritdoc/>
        public void Dispose()
        {
            if (Status == ServerStatus.Running || Status == ServerStatus.Starting)
            {
                Stop();
            }
        }
        #endregion

        #region IServerEventListener Implementation
        async Task<ConnectionHandlerResult> IServerEventListener.HandleConnectionRequest(int protocolVersion, string clientId, IPEndPoint endPoint, IByteStreamReader connectionDataReader)
        {
            if(_connectionRequestHandler != null)
            {
                // Invoke custom request handler
                return await _connectionRequestHandler(protocolVersion, clientId, endPoint, connectionDataReader);
            }
            else
            {
                // No custom request handler
                return new ConnectionHandlerResult(ConnectionResponse.Successful, null); 
            }
        }

        void IServerEventListener.OnReceiveRequest(IServerPeer serverPeer, MessageWrapper messageWrapper)
        {
            _requestHandlerMap.OnReceiveRequest(serverPeer, messageWrapper);
        }

        void IServerEventListener.OnPeerConnected(IServerPeer serverPeer)
        {
            PeerConnected?.Invoke(this, new ServerPeerConnectedEventArgs(serverPeer));
        }

        void IServerEventListener.OnPeerDisconnected(IServerPeer serverPeer)
        {
            PeerDisconnected?.Invoke(this, new ServerPeerDisconnectedEventArgs(serverPeer));
        }
        #endregion
    }
}
