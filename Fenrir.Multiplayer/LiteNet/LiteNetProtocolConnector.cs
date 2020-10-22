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
    class LiteNetProtocolConnector : IProtocolConnector, INetEventListener, IDisposable
    {
        public EventHandler<DisconnectedEventArgs> Disconnected;
        public EventHandler<NetworkErrorEventArgs> NetworkError;

        private const int ProtocolVersion = 1;

        public int Latency { get; private set; } = -1;

        private readonly LiteNetMessageReader _messageReader;
        private readonly LiteNetMessageWriter _messageWriter;
        private readonly ISerializationProvider _serializerProvider;
        private readonly IEventReceiver _eventReceiver;
        private readonly IResponseReceiver _responseReceiver;
        private readonly IResponseMap _responseMap;
        private readonly ITypeMap _typeMap;

        private readonly string _hostname;
        private readonly short _port;
        private object _connectionData;
        private string _clientId;

        private readonly NetDataWriter _netDataWriter;
        private readonly NetManager _netManager;

        private LiteNetClientPeer _peer;

        public IClientPeer Peer => _peer;

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

        private TaskCompletionSource<ConnectionResult> _connectionTcs = null;

        public LiteNetProtocolConnector(string hostname, 
            short port,
            string clientId,
            object connectionData, 
            ISerializationProvider serializerProvider, 
            IEventReceiver eventReceiver, 
            IResponseReceiver responseReceiver, 
            IResponseMap responseMap,
            ITypeMap typeMap, 
            IPv6ProtocolMode ipv6Mode)
        {
            _serializerProvider = serializerProvider;
            _eventReceiver = eventReceiver;
            _responseReceiver = responseReceiver;
            _responseMap = responseMap;

            _hostname = hostname;
            _port = port;
            _clientId = clientId;
            _connectionData = connectionData;
            _typeMap = typeMap;

            _messageReader = new LiteNetMessageReader(serializerProvider, typeMap, new RecyclableObjectPool<ByteStreamReader>());
            _messageWriter = new LiteNetMessageWriter(serializerProvider, typeMap, new RecyclableObjectPool<ByteStreamWriter>());

            _netDataWriter = new NetDataWriter();
            _netManager = new NetManager(this)
            {
                AutoRecycle = true,
                IPv6Enabled = (IPv6Mode)ipv6Mode
            };

            _netManager.Start();
        }

        public Task<ConnectionResult> Connect()
        {
            if(State != ConnectorState.Disconnected)
            {
                throw new InvalidOperationException("Can not connect while state is " + State);
            }

            _connectionTcs = new TaskCompletionSource<ConnectionResult>();
            _netManager.Connect(_hostname, _port, GetConnectionData());
            return _connectionTcs.Task;
        }

        public void Disconnect()
        {
            if(State != ConnectorState.Disconnected)
            {
                _netManager.Stop();
            }
        }

        private NetDataWriter GetConnectionData()
        {
            _netDataWriter.Reset();
            _netDataWriter.Put(ProtocolVersion); // Protocol Version
            _netDataWriter.Put(_clientId); // Client Id

            if (_connectionData != null)
            {
                // Client data type code
                ulong typeCode = _typeMap.GetTypeHash(_connectionData.GetType());
                _netDataWriter.Put(typeCode);

                // Client data deserialized
                _serializerProvider.Serialize(_connectionData, new ByteStreamWriter(_netDataWriter));
            }

            return _netDataWriter;
        }

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            if(State != ConnectorState.Connecting)
            {
                throw new InvalidOperationException("Connection succeeeded during wrong state: " + State);
            }

            _peer = new LiteNetClientPeer(peer, _messageWriter, _responseMap);
            _connectionTcs.SetResult(ConnectionResult.Successful);
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

        public void Dispose()
        {
            if(State != ConnectorState.Disconnected)
            {
                Disconnect();
            }
        }
    }
}
