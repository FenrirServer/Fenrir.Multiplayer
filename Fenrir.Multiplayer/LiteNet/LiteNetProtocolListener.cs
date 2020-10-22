using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.Host;
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
    class LiteNetProtocolListener : IProtocolListener, INetEventListener, IDisposable
    {
        private const int _minSupportedProtocolVersion = 1;

        private readonly ISerializationProvider _serializerProvider;
        private readonly IRequestReceiver _requestReceiver;
        private readonly IFenrirLogger _logger;
        private readonly ITypeMap _typeMap;

        private readonly string _hostnameIPv4;
        private readonly string _hostnameIPv6;
        private readonly short _port;

        private readonly NetDataWriterPool _netDataWriterPool;
        private readonly RecyclableObjectPool<ByteStreamReader> _byteStreamReaderPool;
        private readonly RecyclableObjectPool<ByteStreamWriter> _byteStreamWriterPool;

        private readonly NetManager _netManager;

        private Action<ConnectionRequest, string> _connectionRequestHandler = null;
        private Action<IHostPeer> _connectHandler = null;
        private Action<IHostPeer> _disconnectHandler = null;

        public LiteNetProtocolListener(string hostnameIPv4,
            string hostnameIPv6,
            short port,
            ISerializationProvider serializerProvider,
            IRequestReceiver requestReceiver,
            IFenrirLogger logger,
            ITypeMap typeMap,
            IPv6ProtocolMode ipv6Mode)
        {
            _serializerProvider = serializerProvider;
            _requestReceiver = requestReceiver;
            _logger = logger;
            _typeMap = typeMap;

            _hostnameIPv4 = hostnameIPv4;
            _hostnameIPv6 = hostnameIPv6;
            _port = port;

            _byteStreamReaderPool = new RecyclableObjectPool<ByteStreamReader>();
            _byteStreamWriterPool = new RecyclableObjectPool<ByteStreamWriter>();

            _netDataWriterPool = new NetDataWriterPool();

            _netManager = new NetManager(this)
            {
                AutoRecycle = true,
                IPv6Enabled = (IPv6Mode)ipv6Mode
            };

            _netManager.Start(_hostnameIPv4, _hostnameIPv6, _port);
        }

        public void Dispose()
        {
            _netManager.Stop();
        }

        private void RejectConnectionRequest(ConnectionRequest connectionRequest, string reason)
        {
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

        public void SetConnectionRequestHandler<TConnectionData>(Func<HostConnectionRequest<TConnectionData>, Task<ConnectionResult>> handler)
            where TConnectionData : class, new()
        {
            if(handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "Connection handler can not be null");
            }

            // Add type to the type map
            _typeMap.AddType<TConnectionData>();

            // Set handler
            _connectionRequestHandler = async (connectionRequest, clientId) =>
            {
                // Read connection request data, if present
                TConnectionData connectionData = null;
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
                        connectionData = _serializerProvider.Deserialize<TConnectionData>(byteStreamReader);
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
                HostConnectionRequest<TConnectionData> hostConnectionRequest = new HostConnectionRequest<TConnectionData>(connectionRequest.RemoteEndPoint, clientId, connectionData);

                // Invoke handler
                ConnectionResult result = await handler(hostConnectionRequest);

                if(!result.Success)
                {
                    RejectConnectionRequest(connectionRequest, result.Reason);
                }
                else
                {
                    connectionRequest.Accept();
                }
            };
        }

        public void SetConnectionHandler(Action<IHostPeer> handler)
        {
            _connectHandler = handler;
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

            _connectionRequestHandler.Invoke(request, clientId);
        }


        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            _logger.Debug("Socket error from {0}: {1}", endPoint, socketError);
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            throw new System.NotImplementedException();
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            throw new System.NotImplementedException();
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            // Do nothing
        }

        void INetEventListener.OnPeerConnected(NetPeer netPeer)
        {
            // Create host peer
            var messageWriter = new LiteNetMessageWriter(_serializerProvider, _typeMap, _byteStreamWriterPool);
            var hostPeer = new LiteNetHostPeer(netPeer, messageWriter);
            _connectHandler?.Invoke(hostPeer);
        }

        void INetEventListener.OnPeerDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo)
        {
            if(netPeer.Tag != null) 
            {
                LiteNetHostPeer hostPeer = (LiteNetHostPeer)netPeer.Tag;
                _disconnectHandler?.Invoke(hostPeer);
            }
        }
        #endregion
    }
}
