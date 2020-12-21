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

        ///<inheritdoc/>
        public int Latency { get; private set; } = -1;

        /// <summary>
        /// Message reader, used to dispatch incoming messages
        /// </summary>
        private readonly LiteNetMessageReader _messageReader;
        
        /// <summary>
        /// Message writer, used to wrap outgoing messages
        /// </summary>
        private readonly LiteNetMessageWriter _messageWriter;

        /// <summary>
        /// Pending request map. Stores request handlers until response arrives
        /// </summary>
        private readonly PendingRequestMap _pendingRequestMap;

        /// <summary>
        /// Event handler map. Stores event handlers bound to event types
        /// </summary>
        private readonly EventHandlerMap _eventHandlerMap;

        /// <summary>
        /// Serialization provider. Used for serialization of messages
        /// </summary>
        private readonly SerializationProvider _serializationProvider;

        /// <summary>
        /// Type map - stores type hashes
        /// </summary>
        private readonly TypeHashMap _typeHashMap;

        /// <summary>
        /// Logger
        /// </summary>
        private IFenrirLogger _logger;

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
        public int TickRate { get; set; } = 66;


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
        public int DisconnectTimeout
        {
            get => _netManager.DisconnectTimeout;
            set => _netManager.DisconnectTimeout = value;
        }

        ///<inheritdoc/>
        public int UpdateTime
        {
            get => _netManager.UpdateTime;
            set => _netManager.UpdateTime = value;
        }

        ///<inheritdoc/>
        public int PingInterval
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
        /// Default constructor
        /// </summary>
        /// <param name="serializationProvider">Serialization Provider</param>
        /// <param name="logger">Logger</param>
        public LiteNetProtocolConnector()
        {
            _serializationProvider = new SerializationProvider();
            _logger = new EventBasedLogger();
            _typeHashMap = new TypeHashMap();
            _eventHandlerMap = new EventHandlerMap(_logger);
            _pendingRequestMap = new PendingRequestMap(_logger);
            _messageReader = new LiteNetMessageReader(_serializationProvider, _typeHashMap, _logger, new RecyclableObjectPool<ByteStreamReader>());
            _messageWriter = new LiteNetMessageWriter(_serializationProvider, _typeHashMap, _logger, new RecyclableObjectPool<ByteStreamWriter>());

            _netDataWriter = new NetDataWriter();

            _netManager = new NetManager(this)
            {
                AutoRecycle = true,
            };
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

            // Connect
            _netManager.Connect(connectionRequest.Hostname, protocolConnectionData.Port, GetConnectionData(connectionRequest.ClientId, connectionRequest.ConnectionRequestData));

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
                    _logger?.Error("Error during server tick: " + e);
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

        private NetDataWriter GetConnectionData(string clientId, object connectionRequestData = null)
        {
            _netDataWriter.Reset();
            _netDataWriter.Put(_protocolVersion); // Protocol Version
            _netDataWriter.Put(clientId); // Client Id

            if (connectionRequestData != null)
            {
                // Client data deserialized
                _serializationProvider.Serialize(connectionRequestData, new ByteStreamWriter(_netDataWriter));
            }

            return _netDataWriter;
        }

        ///<inheritdoc/>
        public void AddEventHandler<TEvent>(IEventHandler<TEvent> eventHandler) where TEvent : IEvent
        {
            _eventHandlerMap.AddEventHandler<TEvent>(eventHandler);
        }

        ///<inheritdoc/>
        public void RemoveEventHandler<TEvent>(IEventHandler<TEvent> eventHandler) where TEvent : IEvent
        {
            _eventHandlerMap.RemoveEventHandler<TEvent>(eventHandler);
        }

        ///<inheritdoc/>
        public void SetContractSerializer(IContractSerializer contractSerializer)
        {
            _serializationProvider.SetContractSerializer(contractSerializer);
        }

        public void SetLogger(IFenrirLogger logger)
        {
            _logger = logger;
        }

        #region INetEventListener Implementation
        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            if(State != Network.ConnectionState.Connecting)
            {
                throw new InvalidOperationException("Connection succeeeded during wrong state: " + State);
            }

            _peer = new LiteNetClientPeer(peer, _messageWriter, _pendingRequestMap);
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
            NetworkError?.Invoke(this, new NetworkErrorEventArgs(endPoint, socketError));
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            // Get message
            int totalBytes = reader.AvailableBytes;
            if(!_messageReader.TryReadMessage(reader, out MessageWrapper messageWrapper))
            {
                _logger.Warning("Failed to read message of length {0} from {1}", totalBytes, peer.EndPoint);
                return;
            }

            // Dispatch message
            if(messageWrapper.MessageType == MessageType.Event)
            {
                // Event
                IEvent evt = messageWrapper.MessageData as IEvent;
                if(evt == null) // Someone is trying to tampter the protocol
                {
                    _logger.Warning("Empty event received from {0}", peer.EndPoint);
                    return;
                }

                // Invoke custom event handler
                _eventHandlerMap.OnReceiveEvent(messageWrapper);
            }
            else if(messageWrapper.MessageType == MessageType.Response)
            {
                // Response
                IResponse response = messageWrapper.MessageData as IResponse;
                if (response == null) // Someone is trying to mess with the protocol
                {
                    _logger.Warning("Empty response received from {0}", peer.EndPoint);
                    return;
                }

                // Invoke custom response hanlder
                _pendingRequestMap.OnReceiveResponse(messageWrapper.RequestId, messageWrapper);
            }
            else
            {
                _logger.Warning("Unsupported message type {0} received from {1}", messageWrapper.MessageType, peer.EndPoint);
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
