using Fenrir.Multiplayer.Exceptions;
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

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// LiteNet realiable UDP protocol server listener
    /// </summary>
    class LiteNetProtocolListener : IProtocolListener, INetEventListener, IDisposable
    {
        private const int _minSupportedProtocolVersion = 1;

        private readonly SerializationProvider _serializationProvider;
        private readonly TypeMap _typeMap;
        private readonly RequestHandlerMap _requestHandlerMap;
        private readonly LiteNetMessageReader _messageReader;
        private readonly NetDataWriterPool _netDataWriterPool;
        private readonly RecyclableObjectPool<ByteStreamReader> _byteStreamReaderPool;
        private readonly RecyclableObjectPool<ByteStreamWriter> _byteStreamWriterPool;

        private IFenrirLogger _logger;

        private NetManager _netManager;

        private Action<ConnectionRequest, string> _connectionRequestHandler = null;
        private Action<IServerPeer> _connectHandler = null;
        private Action<IServerPeer> _disconnectHandler = null;

        public Network.ProtocolType ProtocolType => Network.ProtocolType.LiteNet;


        public bool IsRunning { get; private set; } = false;

        public IPv6ProtocolMode IPv6Mode { get; set; } = IPv6ProtocolMode.Disabled;

        public string BindIPv4 { get; set; } = "0.0.0.0";

        public string BindIPv6 { get; set; } = "::/0";

        public ushort Port { get; set; } = 27001;

        public ushort? PublicPort { get; set; } = null;


        public LiteNetProtocolListener()
        {
            _serializationProvider = new SerializationProvider();
            _logger = new EventBasedLogger();

            _typeMap = new TypeMap();
            _requestHandlerMap = new RequestHandlerMap();
            _messageReader = new LiteNetMessageReader(_serializationProvider, _typeMap, new RecyclableObjectPool<ByteStreamReader>());
            _byteStreamReaderPool = new RecyclableObjectPool<ByteStreamReader>();
            _byteStreamWriterPool = new RecyclableObjectPool<ByteStreamWriter>();
            _netDataWriterPool = new NetDataWriterPool();

        }

        public Task Start()
        {
            if (!IsRunning)
            {
                _netManager = new NetManager(this)
                {
                    AutoRecycle = true,
                    IPv6Enabled = (IPv6Mode)IPv6Mode
                };

                _netManager.Start(BindIPv4, BindIPv6, Port);

                IsRunning = true;
            }

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            if (IsRunning)
            {
                _netManager.Stop();
                _netManager = null;
                IsRunning = false;
            }

            return Task.CompletedTask;
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

        public void SetConnectionRequestHandler<TConnectionRequestData>(Func<ServerConnectionRequest<TConnectionRequestData>, Task<ConnectionResponse>> handler)
            where TConnectionRequestData : class, new()
        {
            if(handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "Connection handler can not be null");
            }

            // Add type to the type map
            _typeMap.AddType<TConnectionRequestData>();

            // Set handler
            _connectionRequestHandler = async (connectionRequest, clientId) =>
            {
                _logger.Trace("Received connection request from {0}, client id {1}", connectionRequest.RemoteEndPoint, clientId);

                // Read connection request data, if present
                TConnectionRequestData connectionRequestData = null;
                var netDataReader = connectionRequest.Data;
                if (netDataReader != null && !netDataReader.EndOfData)
                {
                    if(!netDataReader.TryGetULong(out ulong typeHash))
                    {
                        _logger.Debug("Rejected connection request from {0}, failed to read ulong type hash", connectionRequest.RemoteEndPoint);
                        RejectConnectionRequest(connectionRequest, "Bad connection data");
                    }

                    if(netDataReader.EndOfData)
                    {
                        _logger.Debug("Rejected connection request from {0}, no data after type hash {1}", connectionRequest.RemoteEndPoint, typeHash);
                        RejectConnectionRequest(connectionRequest, "Bad connection data");
                    }

                    ByteStreamReader byteStreamReader = _byteStreamReaderPool.Get();
                    byteStreamReader.SetNetDataReader(connectionRequest.Data);

                    try
                    {
                        connectionRequestData = _serializationProvider.Deserialize<TConnectionRequestData>(byteStreamReader);
                    }
                    catch(SerializationException e)
                    {
                        _logger.Debug("Rejected connection request from {0}, failed to deserialize connection data: {1}", connectionRequest.RemoteEndPoint, e);

                        RejectConnectionRequest(connectionRequest, "Bad connection data");
                    }
                    finally
                    {
                        _byteStreamReaderPool.Return(byteStreamReader);
                    }
                }

                // Create connection request object
                ServerConnectionRequest<TConnectionRequestData> hostConnectionRequest = new ServerConnectionRequest<TConnectionRequestData>(connectionRequest.RemoteEndPoint, clientId, connectionRequestData);

                // Invoke handler
                ConnectionResponse response = await handler(hostConnectionRequest);

                if(!response.Success)
                {
                    RejectConnectionRequest(connectionRequest, response.Reason);
                }
                else
                {
                    _logger.Trace("Accepted connection request from {0}, client id {1}", connectionRequest.RemoteEndPoint, clientId);
                    connectionRequest.Accept();
                }
            };
        }


        ///<inheritdoc/>
        public void SetConnectionHandler(Action<IServerPeer> handler)
        {
            _connectHandler = handler;
        }

        ///<inheritdoc/>
        public void SetContractSerializer(IContractSerializer contractSerializer)
        {
            _serializationProvider.SetContractSerializer(contractSerializer);
        }


        ///<inheritdoc/>
        public void SetLogger(IFenrirLogger logger)
        {
            _logger = logger;
        }

        public IProtocolConnectionData GetConnectionData()
        {
            return new LiteNetProtocolConnectionData(
                PublicPort ?? Port,
                IPv6Mode
                );
        }

        #region INetEventListener Implementation
        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
            NetDataReader netDataReader = request.Data;

            int protocolVersion = netDataReader.GetInt(); // Protocol Version
            if (protocolVersion < _minSupportedProtocolVersion)
            {
                _logger.Debug("Rejected connection request from {0}, protocol version {1} is less than supported protocol version {2}", request.RemoteEndPoint, protocolVersion, _minSupportedProtocolVersion);
                RejectConnectionRequest(request, "Outdated protocol");
                return;
            }

            string clientId = netDataReader.GetString(); // Client Id

            // Invoke custom connection request handler
            if (_connectionRequestHandler != null)
            {
                _connectionRequestHandler.Invoke(request, clientId);
            }
            else // No custom connection request handler, simply accept
            {
                request.Accept();
            }
        }


        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            _logger.Debug("Socket error from {0}: {1}", endPoint, socketError);
        }


        async void INetEventListener.OnNetworkReceive(NetPeer netPeer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            // Get LiteNet peer
            if(netPeer.Tag == null)
            {
                _logger.Trace("Received message from an uninitialized peer: {0}", netPeer.EndPoint);
                return;
            }

            var serverPeer = (LiteNetServerPeer)netPeer.Tag;

            // Get message
            MessageWrapper messageWrapper = _messageReader.ReadMessage(reader);

            // Dispatch message
            if (messageWrapper.MessageType == MessageType.Request)
            {
                // Request
                IRequest request = messageWrapper.MessageData as IRequest;
                if (request == null) // Someone is trying to mess with the protocol
                {
                    _logger.Trace("Empty request received from {0}", netPeer.EndPoint);
                    return;
                }

                // Notify request handler map
                _requestHandlerMap.OnReceiveRequest(serverPeer, messageWrapper);
            }
            else
            {
                _logger.Trace("Unsupported message type {0} received from {1}", messageWrapper.MessageType, netPeer.EndPoint);
            }
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            // Do nothing
        }

        void INetEventListener.OnPeerConnected(NetPeer netPeer)
        {
            // Create host peer
            var messageWriter = new LiteNetMessageWriter(_serializationProvider, _typeMap, _byteStreamWriterPool);
            var hostPeer = new LiteNetServerPeer(netPeer, messageWriter);
            _connectHandler?.Invoke(hostPeer);
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
            if(IsRunning)
            {
                Stop();
            }
        }
        #endregion
    }
}
