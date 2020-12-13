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
        private const int ProtocolVersion = 1;

        ///<inheritdoc/>
        public int Latency { get; private set; } = -1;

        private readonly LiteNetMessageReader _messageReader;
        private readonly LiteNetMessageWriter _messageWriter;
        private readonly RequestResponseMap _requestResponseMap;
        private readonly EventHandlerMap _eventHandlerMap;
        private readonly SerializationProvider _serializationProvider;
        private readonly RequestResponseMap _responseMap;
        private readonly TypeMap _typeMap;

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

        ///<inheritdoc/>
        public ConnectorState State
        {
            get
            {
                if (_connectionTcs == null)
                {
                    return ConnectorState.Disconnected;
                }
                else if (!_connectionTcs.Task.IsCompleted)
                {
                    return ConnectorState.Connecting;
                }
                else
                {
                    return ConnectorState.Connected;
                }
            }
        }

        ///<inheritdoc/>
        public Type ConnectionDataType => typeof(LiteNetProtocolConnectionData);

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
            _typeMap = new TypeMap();
            _eventHandlerMap = new EventHandlerMap();
            _requestResponseMap = new RequestResponseMap();
            _messageReader = new LiteNetMessageReader(_serializationProvider, _typeMap, new RecyclableObjectPool<ByteStreamReader>());
            _messageWriter = new LiteNetMessageWriter(_serializationProvider, _typeMap, new RecyclableObjectPool<ByteStreamWriter>());

            _netDataWriter = new NetDataWriter();
        }

        ///<inheritdoc/>
        public Task<ConnectionResponse> Connect(ClientConnectionRequest connectionRequest)
        {
            if(State != ConnectorState.Disconnected)
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

            // Create net manager
            _netManager = new NetManager(this)
            {
                AutoRecycle = true,
                IPv6Enabled = (IPv6Mode)protocolConnectionData.IPv6Mode
            };

            // Start net manager
            _netManager.Start();

            // Connect
            _netManager.Connect(connectionRequest.Hostname, protocolConnectionData.Port, GetConnectionData(connectionRequest.ClientId, connectionRequest.ConnectionRequestData));

            return _connectionTcs.Task;
        }

        ///<inheritdoc/>
        public void Disconnect()
        {
            if(State != ConnectorState.Disconnected)
            {
                _netManager.Stop();
            }

            _netManager = null;
            _peer = null;
        }

        private NetDataWriter GetConnectionData(string clientId, object connectionRequestData = null)
        {
            _netDataWriter.Reset();
            _netDataWriter.Put(ProtocolVersion); // Protocol Version
            _netDataWriter.Put(clientId); // Client Id

            if (connectionRequestData != null)
            {
                // Client data type code
                ulong typeCode = _typeMap.GetTypeHash(connectionRequestData.GetType());
                _netDataWriter.Put(typeCode);

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
            if(State != ConnectorState.Connecting)
            {
                throw new InvalidOperationException("Connection succeeeded during wrong state: " + State);
            }

            _peer = new LiteNetClientPeer(peer, _messageWriter, _requestResponseMap);
            _connectionTcs.SetResult(ConnectionResponse.Successful);
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {

            if (State != ConnectorState.Disconnected)
            {
                throw new InvalidOperationException("Received disconnected event while not connected");
            }

            DisconnectedReason reason = (DisconnectedReason)disconnectInfo.Reason;
            SocketError socketError = disconnectInfo.SocketErrorCode;
            object data = null;

            if(disconnectInfo.AdditionalData != null)
            {
                data = _messageReader.ReadMessage(disconnectInfo.AdditionalData);
            }

            if (State == ConnectorState.Connecting)
            {
                _connectionTcs.SetException(new ConnectionFailedException("Connection failed", reason, socketError, data));
            }
            else // if(State == ConnectorState.Connected)
            {
                Disconnected?.Invoke(this, new DisconnectedEventArgs(reason, socketError, data));
            }
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            NetworkError?.Invoke(this, new NetworkErrorEventArgs(endPoint, socketError));
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            // Get message
            MessageWrapper messageWrapper = _messageReader.ReadMessage(reader);

            // Dispatch message
            if(messageWrapper.MessageType == MessageType.Event)
            {
                // Event
                IEvent evt = messageWrapper.MessageData as IEvent;
                if(evt == null) // Someone is trying to mess with the protocol
                {
                    _logger.Trace("Empty event received from {0}", peer.EndPoint);
                    return;
                }

                _eventHandlerMap.OnReceiveEvent(messageWrapper);
            }
            else if(messageWrapper.MessageType == MessageType.Response)
            {
                // Response
                IResponse response = messageWrapper.MessageData as IResponse;
                if (response == null) // Someone is trying to mess with the protocol
                {
                    _logger.Trace("Empty response received from {0}", peer.EndPoint);
                    return;
                }

                _requestResponseMap.OnReceiveResponse(messageWrapper.RequestId, messageWrapper);
            }
            else
            {
                _logger.Trace("Unsupported message type {0} received from {1}", messageWrapper.MessageType, peer.EndPoint);
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
            if(State != ConnectorState.Disconnected)
            {
                Disconnect();
            }
        }
        #endregion
    }
}
