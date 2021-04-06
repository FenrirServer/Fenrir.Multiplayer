using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Events;
using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// LiteNet protocol connector implementation
    /// Connects to the Protocol Listener using LiteNet protocol
    /// </summary>
    class LiteNetProtocolConnector : IProtocolConnector, INetEventListener, IDisposable
    {
        ///<inheritdoc/>
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        ///<inheritdoc/>
        public event EventHandler<NetworkErrorEventArgs> NetworkError;

        /// <summary>
        /// Version of the protocol
        /// </summary>
        private const int _protocolVersion = 1;

        /// <summary>
        /// Client Event Listener
        /// </summary>
        private readonly IClientEventListener _clientEventListener;

        /// <summary>
        /// Type map
        /// </summary>
        private readonly ITypeHashMap _typeHashMap;

        /// <summary>
        /// Serializer
        /// </summary>
        private readonly INetworkSerializer _serializer;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _logger;

        ///<inheritdoc/>
        public int Latency { get; private set; } = -1;

        /// <summary>
        /// Message reader, used to dispatch incoming messages
        /// </summary>
        private readonly MessageReader _messageReader;
        
        /// <summary>
        /// Message writer, used to wrap outgoing messages
        /// </summary>
        private readonly MessageWriter _messageWriter;

        /// <summary>
        /// Pending request map. Stores request handlers until response arrives
        /// </summary>
        private readonly PendingRequestMap _pendingRequestMap;

        /// <summary>
        /// Byte stream writer object pool. 
        /// Byte stream writers are taken from the pool to send messages and convert them into
        /// bytes that are being written into a socket
        /// </summary>
        private readonly RecyclableObjectPool<ByteStreamWriter> _byteStreamWriterPool;

        /// <summary>
        /// Byte stream reader pool. 
        /// Byte stream readers are used to convert bytes from socket into messages, then returned to the pool.
        /// </summary>
        private readonly RecyclableObjectPool<ByteStreamReader> _byteStreamReaderPool;

        /// <summary>
        /// LiteNet Data Writer to write outgoing data
        /// </summary>
        private readonly NetDataWriter _netDataWriter;

        /// <summary>
        /// LiteNet NetManager
        /// </summary>
        private NetManager _netManager;

        /// <summary>
        /// LiteNet peer
        /// </summary>
        private LiteNetClientPeer _peer;


        ///<inheritdoc/>
        public IClientPeer Peer => _peer;

        ///<inheritdoc/>
        public Network.ProtocolType ProtocolType => Network.ProtocolType.LiteNet;

        /// <summary>
        /// Client ticks per second
        /// </summary>
        public int TickRate { get; set; } = 60;

        /// <summary>
        /// Server request timeout
        /// While connection is stable, server request might take longer than expected.
        /// If SendRequest() takes longer than this value, <seealso cref="RequestTimeoutException"/> is thrown.
        /// </summary>
        public int RequestTimeoutMs { get; set; } = 5000;


        ///<inheritdoc/>
        public bool IsRunning => State != Network.ConnectionState.Disconnected;

        ///<inheritdoc/>
        public Network.ConnectionState State
        {
            get
            {
                if (_connectionTcs == null)
                {
                    return Network.ConnectionState.Disconnected;
                }
                else if (!_connectionTcs.Task.IsCompleted)
                {
                    return Network.ConnectionState.Connecting;
                }
                else
                {
                    return Network.ConnectionState.Connected;
                }
            }
        }
        ///<inheritdoc/>
        public Type ConnectionDataType => typeof(LiteNetProtocolConnectionData);


        ///<inheritdoc/>
        public int DisconnectTimeoutMs
        {
            get => _netManager.DisconnectTimeout;
            set => _netManager.DisconnectTimeout = value;
        }

        ///<inheritdoc/>
        public int UpdateTimeMs
        {
            get => _netManager.UpdateTime;
            set => _netManager.UpdateTime = value;
        }

        ///<inheritdoc/>
        public int PingIntervalMs
        {
            get => _netManager.PingInterval;
            set => _netManager.PingInterval = value;
        }

        ///<inheritdoc/>
        public bool SimulatePacketLoss
        {
            get => _netManager.SimulatePacketLoss;
            set => _netManager.SimulatePacketLoss = value;
        }

        ///<inheritdoc/>
        public bool SimulateLatency
        {
            get => _netManager.SimulateLatency;
            set => _netManager.SimulateLatency = value;
        }

        ///<inheritdoc/>
        public int SimulationPacketLossChance
        {
            get => _netManager.SimulationPacketLossChance;
            set => _netManager.SimulationPacketLossChance = value;
        }

        ///<inheritdoc/>
        public int SimulationMinLatency
        {
            get => _netManager.SimulationMinLatency;
            set => _netManager.SimulationMinLatency = value;
        }

        ///<inheritdoc/>
        public int SimulationMaxLatency
        {
            get => _netManager.SimulationMaxLatency;
            set => _netManager.SimulationMaxLatency = value;
        }

        /// <summary>
        /// TaskCompletionSource that represents connection task
        /// </summary>
        private TaskCompletionSource<ConnectionResponse> _connectionTcs = null;

        /// <summary>
        /// Creates <see cref="LiteNetProtocolConnector"/>
        /// </summary>
        /// <param name="serializer">Serializer</param>
        /// <param name="typeHashMap">Type Hash Map</param>
        /// <param name="logger">Logger</param>
        public LiteNetProtocolConnector(IClientEventListener clientEventListener, 
            INetworkSerializer serializer, 
            ITypeHashMap typeHashMap, 
            ILogger logger)
        {
            if (clientEventListener == null)
            {
                throw new ArgumentNullException(nameof(clientEventListener));
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

            _clientEventListener = clientEventListener;
            _serializer = serializer;
            _typeHashMap = typeHashMap;
            _logger = logger;


            _pendingRequestMap = new PendingRequestMap(_logger);

            _byteStreamWriterPool = new RecyclableObjectPool<ByteStreamWriter>(() => new ByteStreamWriter(_serializer));
            _byteStreamReaderPool = new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(_serializer));

            _messageReader = new MessageReader(_serializer, _typeHashMap, _logger, _byteStreamReaderPool);
            _messageWriter = new MessageWriter(_serializer, _typeHashMap, _logger);

            _netDataWriter = new NetDataWriter();

            _netManager = new NetManager(this)
            {
                AutoRecycle = true,
            };

            // Add default types
            _typeHashMap.AddType<ErrorResponse>();
        }

        ///<inheritdoc/>
        public Task<ConnectionResponse> Connect(ClientConnectionRequest connectionRequest)
        {
            if (State != Network.ConnectionState.Disconnected)
            {
                throw new InvalidOperationException("Can not connect while state is " + State);
            }

            // Get protocol connection data
            var protocolConnectionData = connectionRequest.ProtocolConnectionData as LiteNetProtocolConnectionData;

            if (protocolConnectionData == null)
            {
                throw new InvalidCastException($"Failed to cast {nameof(connectionRequest.ProtocolConnectionData)} to {nameof(LiteNetProtocolConnectionData)}");
            }

            // Create task completion source
            _connectionTcs = new TaskCompletionSource<ConnectionResponse>();

            // Start net manager
            _netManager.Start();

            // Start polling events
            RunEventLoop();

            // Create connection data
            var connectionRequestDataWriter = CreateConnectionRequestDataWriter(connectionRequest.ClientId, connectionRequest.ConnectionRequestData);

            // Connect
            _netManager.Connect(connectionRequest.Hostname, protocolConnectionData.Port, connectionRequestDataWriter);

            return _connectionTcs.Task;
        }

        private async void RunEventLoop()
        {
            while(IsRunning)
            {
                try
                {
                    _netManager.PollEvents();
                }
                catch(Exception e)
                {
                    _logger.Error("Error during server tick: " + e);
                }

                float delaySeconds = 1f / TickRate;
                await Task.Delay((int)(delaySeconds * 1000f));
            }
        }

        ///<inheritdoc/>
        public void Disconnect()
        {
            if(State != Network.ConnectionState.Disconnected)
            {
                _netManager.Stop();
            }

            _peer = null;
        }

        private NetDataWriter CreateConnectionRequestDataWriter(string clientId, object connectionRequestData = null)
        {
            _netDataWriter.Reset();
            _netDataWriter.Put(_protocolVersion); // Protocol Version
            _netDataWriter.Put(clientId); // Client Id

            if (connectionRequestData != null)
            {
                // Client data deserialized
                _serializer.Serialize(connectionRequestData, new ByteStreamWriter(_netDataWriter, _serializer));
            }

            return _netDataWriter;
        }

        #region INetEventListener Implementation
        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            if(State != Network.ConnectionState.Connecting)
            {
                throw new InvalidOperationException("Connection succeeeded during wrong state: " + State);
            }

            // Unfortunately right now there is no way for us to know the actual id of the server, it has to be communicated
            // We need to add Accept(NetDataWriter) in LiteNet similar to Reject to send over connection result to the client
            string peerId = Guid.NewGuid().ToString(); 

            _peer = new LiteNetClientPeer(peerId, peer, _messageWriter, _pendingRequestMap, _typeHashMap, _byteStreamWriterPool, RequestTimeoutMs);
            _connectionTcs.SetResult(ConnectionResponse.Successful);
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (State == Network.ConnectionState.Disconnected)
            {
                throw new InvalidOperationException("Received disconnected event while not connected");
            }

            DisconnectedReason reason = (DisconnectedReason)disconnectInfo.Reason;
            SocketError socketError = disconnectInfo.SocketErrorCode;

            if (State == Network.ConnectionState.Connecting)
            {
                if (reason == DisconnectedReason.ConnectionRejected)
                {
                    string rejectedReason = null;

                    if (disconnectInfo.AdditionalData != null && !disconnectInfo.AdditionalData.EndOfData)
                    {
                        rejectedReason = disconnectInfo.AdditionalData.GetString();
                    }

                    var connectionTcs = _connectionTcs;
                    _connectionTcs = null;
                    connectionTcs.SetResult(ConnectionResponse.Failed(rejectedReason));
                }
                else
                {
                    var connectionTcs = _connectionTcs;
                    _connectionTcs = null;
                    connectionTcs.SetException(new ConnectionFailedException("Connection failed", reason, socketError));
                }
            }
            else // if(State == ConnectorState.Connected)
            {
                _connectionTcs = null;
                Disconnected?.Invoke(this, new DisconnectedEventArgs(reason, socketError));
            }
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            _logger.Trace("Network error from {0}: {1}", endPoint, socketError);

            NetworkError?.Invoke(this, new NetworkErrorEventArgs(endPoint, socketError));
        }

        void INetEventListener.OnNetworkReceive(NetPeer netPeer, NetPacketReader netPacketReader, DeliveryMethod deliveryMethod)
        {
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
                _logger.Warning("Failed to read message of length {0} from {1}", totalBytes, netPeer.EndPoint);
                return;
            }

            // Dispatch message
            if(messageWrapper.MessageType == MessageType.Event)
            {
                // Event
                IEvent evt = messageWrapper.MessageData as IEvent;
                if(evt == null) // Someone is trying to tampter the protocol
                {
                    _logger.Warning("Empty event received from {0}", netPeer.EndPoint);
                    return;
                }

                // Invoke custom event handler
                _clientEventListener.OnReceiveEvent(messageWrapper);
            }
            else if(messageWrapper.MessageType == MessageType.Response)
            {
                // Response
                IResponse response = messageWrapper.MessageData as IResponse;
                if (response == null) // Someone is trying to mess with the protocol
                {
                    _logger.Warning("Empty response received from {0}", netPeer.EndPoint);
                    return;
                }

                // Invoke custom response hanlder
                _pendingRequestMap.OnReceiveResponse(messageWrapper.RequestId, messageWrapper);
            }
            else
            {
                _logger.Warning("Unsupported message type {0} received from {1}", messageWrapper.MessageType, netPeer.EndPoint);
            }
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            Latency = latency;
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            // Do nothing
        }

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
            // Do nothing
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            if(State != Network.ConnectionState.Disconnected)
            {
                Disconnect();
            }
        }
        #endregion
    }
}
