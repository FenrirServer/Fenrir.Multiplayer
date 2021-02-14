﻿using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.Server;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Runtime.Serialization;

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// LiteNet realiable UDP protocol server listener
    /// </summary>
    class LiteNetProtocolListener : IProtocolListener, INetEventListener, IDisposable
    {
        /// <summary>
        /// Minimum supported protocol version by the server
        /// </summary>
        private const int _minSupportedProtocolVersion = 1;


        /// <summary>
        /// Type map - stores type hashes
        /// </summary>
        private readonly TypeHashMap _typeHashMap;

        /// <summary>
        /// Request handler - stores event handlers bound to event types
        /// </summary>
        private readonly RequestHandlerMap _requestHandlerMap;

        /// <summary>
        /// Message reader, used to dispatch incoming messages
        /// </summary>
        private readonly MessageReader _messageReader;

        /// <summary>
        /// Object pool of NetDataWriters used to write outgoing messages
        /// </summary>
        private readonly NetDataWriterPool _netDataWriterPool;

        /// <summary>
        /// Pool if byte stream readers - used as temporary dispatch buffers
        /// </summary>
        private readonly RecyclableObjectPool<ByteStreamReader> _byteStreamReaderPool;

        /// <summary>
        /// Object pool of byte stream writers - used as temporary write buffers
        /// </summary>
        private readonly RecyclableObjectPool<ByteStreamWriter> _byteStreamWriterPool;


        /// <summary>
        /// LiteNet NetManager
        /// </summary>
        private NetManager _netManager;

        /// <summary>
        /// Stores delegate connection custom request handler if one is used
        /// </summary>
        private Func<ConnectionRequest, string, int, Task> _connectionRequestHandler = null;

        /// <summary>
        /// True if server is running
        /// </summary>
        private volatile bool _isRunning;

        /// <summary>
        /// Stores connected handler
        /// </summary>
        private Action<IServerPeer> _connectHandler = null;

        /// <summary>
        /// Stores disconnected handler
        /// </summary>
        private Action<IServerPeer> _disconnectHandler = null;

        /// <inheritdoc/>
        public Network.ProtocolType ProtocolType => Network.ProtocolType.LiteNet;

        /// <inheritdoc/>
        public bool IsRunning => _isRunning;

        /// <inheritdoc/>
        public INetworkSerializer Serializer { get; set; }

        /// <inheritdoc/>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Stores IPv6 mode 
        /// </summary>
        public IPv6ProtocolMode IPv6Mode
        {
            get => (IPv6ProtocolMode)_netManager.IPv6Enabled;
            set => _netManager.IPv6Enabled = (IPv6Mode)value;
        }

        /// <summary>
        /// IPv4 endpoint at which listener should be bound
        /// </summary>
        public string BindIPv4 { get; set; } = "0.0.0.0";

        /// <summary>
        /// IPv6 endpoint at which listener should be bound
        /// </summary>
        public string BindIPv6 { get; set; } = "::";

        /// <summary>
        /// Port at which listener should be bound
        /// </summary>
        public ushort BindPort { get; set; } = 27015;

        /// <summary>
        /// Public port. Overrides listen <seealso cref="BindPort"/> when reporting to the client
        /// Override this port if container maps <seealso cref="BindPort"/> to something else
        /// </summary>
        public ushort? PublicPort { get; set; } = null;

        /// <summary>
        /// Server ticks per second
        /// </summary>
        public int TickRate { get; set; } = 66;

        /// <inheritdoc/>
        public int DisconnectTimeout
        {
            get => _netManager.DisconnectTimeout;
            set => _netManager.DisconnectTimeout = value;
        }

        /// <inheritdoc/>
        public int UpdateTime
        {
            get => _netManager.UpdateTime;
            set => _netManager.UpdateTime = value;
        }

        /// <inheritdoc/>
        public int PingInterval
        {
            get => _netManager.PingInterval;
            set => _netManager.PingInterval = value;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public LiteNetProtocolListener()
        {
            Serializer = new NetworkSerializer();
            Logger = new EventBasedLogger();
            _typeHashMap = new TypeHashMap();
            _requestHandlerMap = new RequestHandlerMap(Logger);

            _byteStreamReaderPool = new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(Serializer));
            _byteStreamWriterPool = new RecyclableObjectPool<ByteStreamWriter>(() => new ByteStreamWriter(Serializer));

            _messageReader = new MessageReader(Serializer, _typeHashMap, Logger, _byteStreamReaderPool);
            _netDataWriterPool = new NetDataWriterPool();
            _netManager = new NetManager(this)
            {
                AutoRecycle = true,
            };
        }

        /// <summary>
        /// Creates LiteNetProtocolListener
        /// </summary>
        /// <param name="port">Port</param>
        public LiteNetProtocolListener(ushort port)
            : this()
        {
            BindPort = port;
        }

        /// <summary>
        /// Creates LiteNetProtocolListener
        /// </summary>
        /// <param name="port">Port</param>
        /// <param name="bindIPv4">IPv4 listen address</param>
        public LiteNetProtocolListener(ushort port, string bindIPv4)
            : this(port)
        {
            BindIPv4 = bindIPv4;
        }

        /// <summary>
        /// Creates LiteNetProtocolListener
        /// </summary>
        /// <param name="port">Port</param>
        /// <param name="bindIPv4">IPv4 Listen Address</param>
        /// <param name="bindIpv6">IPv6 Listen Address</param>
        /// <param name="ipv6Mode">IPv6 Mode</param>
        public LiteNetProtocolListener(ushort port, string bindIPv4, string bindIpv6, IPv6ProtocolMode ipv6Mode)
            : this(port, bindIPv4)
        {
            BindIPv6 = bindIpv6;
            IPv6Mode = ipv6Mode;
        }

        /// <inheritdoc/>
        public Task Start()
        {
            if (!_isRunning)
            {
                _netManager.Start(BindIPv4, BindIPv6, BindPort);

                _isRunning = true;

                RunEventLoop();
            }

            return Task.CompletedTask;
        }


        /// <summary>
        /// Ticks the server
        /// </summary>
        private async void RunEventLoop()
        {
            while(_isRunning)
            {
                try
                {
                    _netManager.PollEvents();
                }
                catch (Exception e)
                {
                    Logger?.Error("Error during server tick: " + e);
                }

                float delaySeconds = 1f / TickRate;
                await Task.Delay((int)(delaySeconds * 1000f));
            }
        }

        /// <inheritdoc/>
        public Task Stop()
        {
            if (_isRunning)
            {
                _netManager.Stop();
                _isRunning = false;
            }

            return Task.CompletedTask;
        }

        private void AcceptConnectionRequest(ConnectionRequest liteNetConnectionRequest, int protocolVersion, string clientId)
        {
            Logger.Trace("Accepting connection request from {0}", liteNetConnectionRequest.RemoteEndPoint);

            NetPeer netPeer = liteNetConnectionRequest.Accept();

            // Create server peer
            var messageWriter = new MessageWriter(Serializer, _typeHashMap, Logger);
            netPeer.Tag = new LiteNetServerPeer(clientId, protocolVersion, netPeer, messageWriter, _byteStreamWriterPool);
        }

        private void RejectConnectionRequest(ConnectionRequest connectionRequest, string reason)
        {
            Logger.Trace("Rejecting connection request from {0} with reason {1}", connectionRequest.RemoteEndPoint, reason);

            NetDataWriter netDataWriter = _netDataWriterPool.Get();
            try
            {
                netDataWriter.Put(reason);
                connectionRequest.Reject(netDataWriter);
            }
            finally
            {
                _netDataWriterPool.Return(netDataWriter);
            }
        }

        /// <inheritdoc/>
        public void SetConnectionRequestHandler<TConnectionRequestData>(Func<IServerConnectionRequest<TConnectionRequestData>, Task<ConnectionResponse>> handler)
            where TConnectionRequestData : class, new()
        {
            if(handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "Connection handler can not be null");
            }

            // Add type to the type map
            _typeHashMap.AddType<TConnectionRequestData>();

            // Set handler
            _connectionRequestHandler = async (connectionRequest, clientId, protocolVersion) =>
            {
                Logger.Trace("Received connection request from {0}, client id {1}", connectionRequest.RemoteEndPoint, clientId);

                // Read connection request data, if present
                TConnectionRequestData connectionRequestData = null;
                var netDataReader = connectionRequest.Data;
                if (netDataReader != null && !netDataReader.EndOfData)
                {
                    ByteStreamReader byteStreamReader = _byteStreamReaderPool.Get();
                    byteStreamReader.SetNetDataReader(connectionRequest.Data);

                    try
                    {
                        connectionRequestData = Serializer.Deserialize<TConnectionRequestData>(byteStreamReader);
                    }
                    catch(SerializationException e)
                    {
                        Logger.Debug("Rejected connection request from {0}, failed to deserialize connection data: {1}", connectionRequest.RemoteEndPoint, e);

                        RejectConnectionRequest(connectionRequest, "Bad connection data");
                    }
                    finally
                    {
                        _byteStreamReaderPool.Return(byteStreamReader);
                    }
                }

                // Create connection request object
                ServerConnectionRequest<TConnectionRequestData> serverConnectionRequest = new ServerConnectionRequest<TConnectionRequestData>(connectionRequest.RemoteEndPoint, protocolVersion, clientId, connectionRequestData);

                // Invoke handler
                ConnectionResponse response;
                try
                {
                    response = await handler(serverConnectionRequest);
                }
                catch(Exception e)
                {
                    Logger.Error("Unhandled exception in connection request handler : {0}", e);
                    RejectConnectionRequest(connectionRequest, "Unhandled exception in connection request handler");
                    return;
                }

                if(!response.Success)
                {
                    RejectConnectionRequest(connectionRequest, response.Reason);
                }
                else
                {
                    Logger.Trace("Accepted connection request from {0}, client id {1}", connectionRequest.RemoteEndPoint, clientId);
                    AcceptConnectionRequest(connectionRequest, protocolVersion, clientId);
                }
            };
        }

        ///<inheritdoc/>
        public void SetConnectionHandler(Action<IServerPeer> handler)
        {
            _connectHandler = handler;
        }

        /// <inheritdoc/>
        public IProtocolConnectionData GetConnectionData()
        {
            return new LiteNetProtocolConnectionData(
                PublicPort ?? BindPort,
                IPv6Mode
                );
        }

        /// <inheritdoc/>
        public void AddRequestHandler<TRequest>(IRequestHandler<TRequest> requestHandler) where TRequest : IRequest
        {
            if(requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            _typeHashMap.AddType<TRequest>();

            _requestHandlerMap.AddRequestHandler<TRequest>(requestHandler);
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
        public void RemoveRequestHandler<TRequest>(IRequestHandler<TRequest> requestHandler) where TRequest : IRequest
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            _requestHandlerMap.RemoveRequestHandler<TRequest>(requestHandler);
        }

        /// <inheritdoc/>
        public void RemoveRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            _requestHandlerMap.RemoveRequestHandler<TRequest, TResponse>(requestHandler);
        }

        /// <inheritdoc/>
        public void RemoveRequestHandlerAsync<TRequest, TResponse>(IRequestHandlerAsync<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            _requestHandlerMap.RemoveRequestHandlerAsync<TRequest, TResponse>(requestHandler);
        }

        /// <inheritdoc/>
        public void AddSerializableTypeFactory<T>(Func<T> factoryMethod) where T : IByteStreamSerializable
        {
            Serializer.AddTypeFactory<T>(factoryMethod);
        }

        /// <inheritdoc/>
        public void RemoveSerializableTypeFactory<T>() where T : IByteStreamSerializable
        {
            Serializer.RemoveTypeFactory<T>();
        }


        #region INetEventListener Implementation
        void INetEventListener.OnConnectionRequest(ConnectionRequest connectionRequest)
        {
            NetDataReader netDataReader = connectionRequest.Data;

            int protocolVersion = netDataReader.GetInt(); // Protocol Version
            if (protocolVersion < _minSupportedProtocolVersion)
            {
                Logger.Debug("Rejected connection request from {0}, protocol version {1} is less than supported protocol version {2}", connectionRequest.RemoteEndPoint, protocolVersion, _minSupportedProtocolVersion);
                RejectConnectionRequest(connectionRequest, "Outdated protocol");
                return;
            }

            string clientId = netDataReader.GetString(); // Client Id

            // Invoke custom connection request handler
            if (_connectionRequestHandler != null)
            {
                InvokeConnectionRequestHandler(connectionRequest, clientId, protocolVersion);
            }
            else // No custom connection request handler, simply accept
            {
                AcceptConnectionRequest(connectionRequest, protocolVersion, clientId);
            }
        }

        private async void InvokeConnectionRequestHandler(ConnectionRequest request, string clientId, int protocolVersion)
        {
            try
            {
                await _connectionRequestHandler.Invoke(request, clientId, protocolVersion);
            }
            catch(Exception e)
            {
                Logger.Error("Connection request handler failed: " + e);
            }
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Logger.Debug("Socket error from {0}: {1}", endPoint, socketError);
        }


        void INetEventListener.OnNetworkReceive(NetPeer netPeer, NetPacketReader netPacketReader, DeliveryMethod deliveryMethod)
        {
            // Get LiteNet peer
            if(netPeer.Tag == null)
            {
                Logger.Warning("Received message from an uninitialized peer: {0}", netPeer.EndPoint);
                return;
            }

            var serverPeer = (LiteNetServerPeer)netPeer.Tag;

            // Read message
            MessageWrapper messageWrapper;
            bool didReadMessage;
            int totalBytes = netPacketReader.AvailableBytes;

            ByteStreamReader byteStreamReader = _byteStreamReaderPool.Get();
            byteStreamReader.SetNetDataReader(netPacketReader);

            try
            {
                didReadMessage = _messageReader.TryReadMessage(byteStreamReader, out messageWrapper);
            }
            finally
            {
                byteStreamReader.SetNetDataReader(null); // Since netDataReader is pooled within LiteNet library, it's important to make sure we reset it.
                _byteStreamReaderPool.Return(byteStreamReader); // Calling Return will simply reset NetPacketReader but not free up so it will exist in both pools
            }

            if (!didReadMessage)
            {
                Logger.Warning("Failed to read message of length {0} from {1}", totalBytes, netPeer.EndPoint);
                return;
            }

            // Dispatch message
            if (messageWrapper.MessageType == MessageType.Request)
            {
                // Request
                IRequest request = messageWrapper.MessageData as IRequest;
                if (request == null) // Someone is trying to mess with the protocol
                {
                    Logger.Warning("Empty request received from {0}", netPeer.EndPoint);
                    return;
                }

                // Notify request handler map
                _requestHandlerMap.OnReceiveRequest(serverPeer, messageWrapper);
            }
            else
            {
                Logger.Warning("Unsupported message type {0} received from {1}", messageWrapper.MessageType, netPeer.EndPoint);
            }
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            // Do nothing
        }

        void INetEventListener.OnPeerConnected(NetPeer netPeer)
        {
            if(netPeer.Tag == null)
            {
                Logger.Error("Peer connected before connection was accepted: " + netPeer.EndPoint);
                return;
            }

            var serverPeer = (LiteNetServerPeer)netPeer.Tag;

            // Invoke connection handler
            _connectHandler?.Invoke(serverPeer);
        }

        void INetEventListener.OnPeerDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo)
        {
            if(netPeer.Tag != null) 
            {
                LiteNetServerPeer hostPeer = (LiteNetServerPeer)netPeer.Tag;
                _disconnectHandler?.Invoke(hostPeer);
            }
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer netPeer, int latency)
        {
            if (netPeer.Tag != null)
            {
                LiteNetServerPeer hostPeer = (LiteNetServerPeer)netPeer.Tag;
                hostPeer.SetLatency(latency);
            }
        }
        #endregion


        #region IDisposable Implementation
        public void Dispose()
        {
            if(_isRunning)
            {
                Stop();
            }
        }

        #endregion
    }
}
