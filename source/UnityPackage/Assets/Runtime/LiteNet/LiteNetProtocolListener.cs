﻿using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

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
        /// Server event listener
        /// </summary>
        private readonly IServerEventListener _serverEventListener;

        /// <summary>
        /// Serializer
        /// </summary>
        private readonly INetworkSerializer _serializer;

        /// <summary>
        /// Type hash map
        /// </summary>
        private readonly ITypeHashMap _typeHashMap;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Message reader
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
        /// True if server is running
        /// </summary>
        private volatile bool _isRunning;

        /// <inheritdoc/>
        public ProtocolType ProtocolType => ProtocolType.LiteNet;

        /// <inheritdoc/>
        public bool IsRunning => _isRunning;


        /// <summary>
        /// IPv4 endpoint at which listener should be bound
        /// </summary>
        public string BindIPv4 { get; private set; }

        /// <summary>
        /// IPv6 endpoint at which listener should be bound
        /// </summary>
        public string BindIPv6 { get; private set; }

        /// <summary>
        /// Port at which listener should be bound
        /// </summary>
        public ushort BindPort { get; private set; }

        /// <summary>
        /// Public port. Overrides listen <seealso cref="BindPort"/> when reporting to the client
        /// Override this port if container maps <seealso cref="BindPort"/> to something else
        /// </summary>
        public ushort? PublicPort { get; private set; }

        /// <summary>
        /// Server ticks per second
        /// </summary>
        public int TickRate { get; set; } = 100;

        /// <summary>
        /// If set to true, events are polled automatically based on the <see cref="TickRate"/>
        /// If set to false, <see cref="TickRate"/> is ignored and you have to call PollEvents()
        /// </summary>
        public bool AutoPollEvents { get; set; } = true;

        /// <summary>
        /// If true, <see cref="TickRate"/> is ignored and events are invoked
        /// immediately outside of a single thread
        /// </summary>
        public bool UnsyncedEvents
        {
            get => _netManager.UnsyncedEvents;
            set => _netManager.UnsyncedEvents = value;
        }

        /// <summary>
        /// IPv6 Support mode
        /// </summary>
        public bool IPv6Enabled => _netManager.IPv6Enabled;

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

        /// <inheritdoc/>
        public IEnumerable<IServerPeer> Peers => _pendingConnections.Values.Where(tcs => tcs.Task.Status == TaskStatus.RanToCompletion && tcs.Task.Result != null).Select(tcs => (IServerPeer)tcs.Task.Result);

        /// <inheritdoc/>
        public int NumPeers => _pendingConnections.Count;

        /// <summary>
        /// In LiteNet, when UnsyncedEvents is on, calling ConnectionRequest.Accept() 
        /// could trigger INetEventListener.OnPeerConnected or other events before we had a chance to set a tag
        /// or even identify the NetPeer somehow
        /// </summary>
        private ConcurrentDictionary<NetPeer, TaskCompletionSource<LiteNetServerPeer>> _pendingConnections = new ConcurrentDictionary<NetPeer, TaskCompletionSource<LiteNetServerPeer>>();

        /// <summary>
        /// Create new LiteNet Protocol Serializer
        /// </summary>
        /// <param name="serverEventListener">Server Event Listener</param>
        /// <param name="serializer">Serializer</param>
        /// <param name="typeHashMap">Type Hash Map</param>
        /// <param name="logger">Logger</param>
        /// <exception cref="ArgumentNullException"></exception>
        public LiteNetProtocolListener(IServerEventListener serverEventListener, 
            INetworkSerializer serializer, 
            ITypeHashMap typeHashMap, 
            ILogger logger)
        {
            if(serverEventListener == null)
            {
                throw new ArgumentNullException(nameof(serverEventListener));
            }
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }
            if (typeHashMap == null)
            {
                throw new ArgumentNullException(nameof(typeHashMap));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _serverEventListener = serverEventListener;
            _serializer = serializer;
            _typeHashMap = typeHashMap;
            _logger = logger;

            _byteStreamReaderPool = new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer));
            _byteStreamWriterPool = new RecyclableObjectPool<ByteStreamWriter>(() => new ByteStreamWriter(serializer));

            _messageReader = new MessageReader(serializer, typeHashMap, logger, _byteStreamReaderPool);

            _netDataWriterPool = new NetDataWriterPool();
            _netManager = new NetManager(this)
            {
                AutoRecycle = true,
                IPv6Enabled = true,
            };
        }


        /// <summary>
        /// Starts LiteNet Protocol Listener
        /// </summary>
        /// <param name="bindIPv4">IPv4 to listen to</param>
        /// <param name="bindIPv6">IPv6 to listen to</param>
        /// <param name="bindPort">Port to listen</param>
        /// <param name="publicPort">Override port value. Use if your public port does not match bind port, e.g. when using docker port override</param>
        public void Start(string bindIPv4 = "0.0.0.0", string bindIPv6 = "::", ushort bindPort = 27016, ushort? publicPort = null)
        {
            BindIPv4 = bindIPv4;
            BindIPv6 = bindIPv6;
            BindPort = bindPort;
            PublicPort = publicPort;

            if (!_isRunning)
            {
                _netManager.Start(BindIPv4, BindIPv6, BindPort);

                _isRunning = true;

                if (!UnsyncedEvents && AutoPollEvents)
                {
                    RunEventLoop().FireAndForget(_logger);
                }
            }
        }

        /// <summary>
        /// Manually polls network events
        /// </summary>
        public void PollEvents()
        {
            try
            {
                _netManager.TriggerUpdate();
                _netManager.PollEvents();
                _netManager.TriggerUpdate();
            }
            catch (Exception e)
            {
                _logger?.Error("Error during server tick: " + e);
            }
        }


        /// <summary>
        /// Ticks the server
        /// </summary>
        private async Task RunEventLoop()
        {
            while(_isRunning)
            {
                PollEvents();
                float delaySeconds = 1f / TickRate;
                await Task.Delay((int)(delaySeconds * 1000f));
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (_isRunning)
            {
                _netManager.Stop();
                _isRunning = false;
            }
        }


        /// <inheritdoc/>
        public IProtocolConnectionData GetConnectionData()
        {
            return new LiteNetProtocolConnectionData(
                PublicPort ?? BindPort,
                IPv6Enabled
            );
        }

        #region INetEventListener Implementation
        void INetEventListener.OnConnectionRequest(ConnectionRequest connectionRequest)
        {
            HandleConnectionRequestAsync(connectionRequest).FireAndForget(_logger);
        }

        private async Task HandleConnectionRequestAsync(ConnectionRequest connectionRequest)
        {
            // Connection request data
            NetDataReader connectionNetDataReader = connectionRequest.Data;

            int protocolVersion = connectionNetDataReader.GetInt(); // Read protocol Version
            if (protocolVersion < _minSupportedProtocolVersion)
            {
                _logger.Debug("Rejected connection request from {0}, protocol version {1} is less than supported protocol version {2}", connectionRequest.RemoteEndPoint, protocolVersion, _minSupportedProtocolVersion);
                RejectConnectionRequest(connectionRequest, "Outdated protocol");
                return;
            }

            string clientId = connectionNetDataReader.GetString(); // Read client Id

            _logger.Trace("Received connection request from {0}, client id {1}", connectionRequest.RemoteEndPoint, clientId);

            // Read custom data, if present
            ByteStreamReader connectionRequestDataReader = null;

            if (!connectionNetDataReader.EndOfData)
            {
                connectionRequestDataReader = _byteStreamReaderPool.Get();
                connectionRequestDataReader.SetNetDataReader(connectionRequest.Data);
            }

            // Invoke connection request handler
            ConnectionHandlerResult connectionHandlerResult;
            try
            {
                connectionHandlerResult = await _serverEventListener.HandleConnectionRequest(protocolVersion, clientId, connectionRequest.RemoteEndPoint, connectionRequestDataReader);
            }
            finally
            {
                if (connectionRequestDataReader != null)
                {
                    _byteStreamReaderPool.Return(connectionRequestDataReader);
                }
            }

            if(connectionHandlerResult.Response.Success)
            {
                AcceptConnectionRequest(connectionRequest, protocolVersion, clientId, connectionHandlerResult.ConnectionRequestData);
            }
            else
            {
                RejectConnectionRequest(connectionRequest, connectionHandlerResult.Response.Reason);
            }
        }

        private void AcceptConnectionRequest(ConnectionRequest liteNetConnectionRequest, int protocolVersion, string clientId, object connectionRequestData)
        {
            _logger.Info("Accepting connection request from {0}", liteNetConnectionRequest.RemoteEndPoint);

            NetPeer netPeer = liteNetConnectionRequest.Accept();

            TaskCompletionSource<LiteNetServerPeer> connectionTcs = _pendingConnections.GetOrAdd(netPeer, new TaskCompletionSource<LiteNetServerPeer>());

            // Create server peer
            var messageWriter = new MessageWriter(_serializer, _typeHashMap, _logger);
            var serverPeer = new LiteNetServerPeer(clientId, protocolVersion, connectionRequestData, netPeer, messageWriter, _byteStreamWriterPool);

            connectionTcs.SetResult(serverPeer);
        }

        private void RejectConnectionRequest(ConnectionRequest connectionRequest, string reason)
        {
            _logger.Trace("Rejecting connection request from {0} with reason {1}", connectionRequest.RemoteEndPoint, reason);

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

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            _logger.Debug("Socket error from {0}: {1}", endPoint, socketError);
        }


        void INetEventListener.OnNetworkReceive(NetPeer netPeer, NetPacketReader netPacketReader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            HandleNetworkReceive(netPeer, netPacketReader, channelNumber, deliveryMethod).FireAndForget(_logger);
        }

        private async Task HandleNetworkReceive(NetPeer netPeer, NetPacketReader netPacketReader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            // Get LiteNet peer
            TaskCompletionSource<LiteNetServerPeer> connectionTcs = _pendingConnections.GetOrAdd(netPeer, new TaskCompletionSource<LiteNetServerPeer>());

            var serverPeer = await connectionTcs.Task;

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
                _logger.Warning("Failed to read message of length {0} from {1}", totalBytes, netPeer);
                return;
            }

            // Dispatch message
            if (messageWrapper.MessageType == MessageType.Request || messageWrapper.MessageType == MessageType.RequestWithResponse)
            {
                // Request
                IRequest request = messageWrapper.MessageData as IRequest;
                if (request == null) // Someone is trying to mess with the protocol
                {
                    _logger.Warning("Empty request received from {0}", netPeer);
                    return;
                }

                // Notify server
                _serverEventListener.OnReceiveRequest(serverPeer, messageWrapper);
            }
            else
            {
                _logger.Warning("Unsupported message type {0} received from {1}", messageWrapper.MessageType, netPeer);
            }

        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            // Do nothing
        }

        void INetEventListener.OnPeerConnected(NetPeer netPeer)
        {
            HandlePeerConnected(netPeer).FireAndForget(_logger);
        }

        private async Task HandlePeerConnected(NetPeer netPeer)
        {
            // Get LiteNet peer
            TaskCompletionSource<LiteNetServerPeer> connectionTcs = _pendingConnections.GetOrAdd(netPeer, new TaskCompletionSource<LiteNetServerPeer>());

            var serverPeer = await connectionTcs.Task;

            _logger.Trace("Peer connected: {0}", netPeer);

            // Notify server
            _serverEventListener.OnPeerConnected(serverPeer);
        }
        

        void INetEventListener.OnPeerDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo)
        {
            HandlePeerDisconnected(netPeer, disconnectInfo).FireAndForget(_logger);
        }

        private async Task HandlePeerDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo)
        {
            // Get LiteNet peer
            if(!_pendingConnections.TryRemove(netPeer, out var connectionTcs))
            {
                // How can this ever happen?
                _logger.Error("Failed to remove connection for peer, not connected: {0}", netPeer);
                return;
            }

            LiteNetServerPeer serverPeer = await connectionTcs.Task;

            if (serverPeer != null)
            {
                // Notify peer
                serverPeer.NotifyDisconnected();

                // Notify server
                _serverEventListener.OnPeerDisconnected(serverPeer);
            }

            _logger.Trace("Peer disconnected: {0}", netPeer);
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
