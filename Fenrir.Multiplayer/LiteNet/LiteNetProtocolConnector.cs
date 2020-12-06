using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Events;
using Fenrir.Multiplayer.Exceptions;
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

        #region Dependencies
        private readonly LiteNetMessageReader _messageReader;
        private readonly LiteNetMessageWriter _messageWriter;
        private readonly ISerializationProvider _serializerProvider;
        private readonly IEventReceiver _eventReceiver;
        private readonly IResponseReceiver _responseReceiver;
        private readonly IResponseMap _responseMap;
        private readonly ITypeMap _typeMap;
        #endregion

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

        /// <summary>
        /// TaskCompletionSource that represents connection task
        /// </summary>
        private TaskCompletionSource<ClientConnectionResult> _connectionTcs = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="serializerProvider"></param>
        /// <param name="eventReceiver"></param>
        /// <param name="responseReceiver"></param>
        /// <param name="responseMap"></param>
        /// <param name="typeMap"></param>
        public LiteNetProtocolConnector(
            ISerializationProvider serializerProvider, 
            IEventReceiver eventReceiver, 
            IResponseReceiver responseReceiver, 
            IResponseMap responseMap,
            ITypeMap typeMap)
        {
            _serializerProvider = serializerProvider;
            _eventReceiver = eventReceiver;
            _responseReceiver = responseReceiver;
            _responseMap = responseMap;

            _typeMap = typeMap;

            _messageReader = new LiteNetMessageReader(serializerProvider, typeMap, new RecyclableObjectPool<ByteStreamReader>());
            _messageWriter = new LiteNetMessageWriter(serializerProvider, typeMap, new RecyclableObjectPool<ByteStreamWriter>());

            _netDataWriter = new NetDataWriter();
        }

        ///<inheritdoc/>
        public Task<ClientConnectionResult> Connect(ClientConnectionRequest connectionRequest)
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
            _connectionTcs = new TaskCompletionSource<ClientConnectionResult>();

            // Create net manager
            _netManager = new NetManager(this)
            {
                AutoRecycle = true,
                IPv6Enabled = (IPv6Mode)protocolConnectionData.IPv6Mode
            };

            // Start net manager
            _netManager.Start();

            // Connect
            _netManager.Connect(protocolConnectionData.Hostname, protocolConnectionData.Port, GetConnectionData(connectionRequest.ClientId, connectionRequest.ConnectionRequestData));

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
                _serializerProvider.Serialize(connectionRequestData, new ByteStreamWriter(_netDataWriter));
            }

            return _netDataWriter;
        }

        #region INetEventListener Implementation
        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            if(State != ConnectorState.Connecting)
            {
                throw new InvalidOperationException("Connection succeeeded during wrong state: " + State);
            }

            _peer = new LiteNetClientPeer(peer, _messageWriter, _responseMap);
            _connectionTcs.SetResult(ClientConnectionResult.Successful);
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
                    return;
                }

                _eventReceiver.OnReceiveEvent(messageWrapper);
            }
            else if(messageWrapper.MessageType == MessageType.Response)
            {
                // Response
                IResponse response = messageWrapper.MessageData as IResponse;
                if (response == null) // Someone is trying to mess with the protocol
                {
                    return;
                }

                _responseReceiver.OnReceiveResponse(messageWrapper.RequestId, messageWrapper);
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
