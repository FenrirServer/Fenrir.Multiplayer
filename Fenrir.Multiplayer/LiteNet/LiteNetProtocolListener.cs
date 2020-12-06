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

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// LiteNet realiable UDP protocol server listener
    /// </summary>
    class LiteNetProtocolListener : IProtocolListener, INetEventListener, IDisposable
    {
        private const int _minSupportedProtocolVersion = 1;

        #region Dependencies
        private readonly LiteNetMessageReader _messageReader;
        private readonly ISerializationProvider _serializerProvider;
        private readonly IRequestReceiver _requestReceiver;
        private readonly IFenrirLogger _logger;
        private readonly ITypeMap _typeMap;

        private readonly NetDataWriterPool _netDataWriterPool;
        private readonly RecyclableObjectPool<ByteStreamReader> _byteStreamReaderPool;
        private readonly RecyclableObjectPool<ByteStreamWriter> _byteStreamWriterPool;
        #endregion

        private NetManager _netManager;

        private Action<ConnectionRequest, string> _connectionRequestHandler = null;
        private Action<IHostPeer> _connectHandler = null;
        private Action<IHostPeer> _disconnectHandler = null;

        public Network.ProtocolType ProtocolType => Network.ProtocolType.LiteNet;

        public IProtocolConnectionData ConnectionData { get; private set; }

        public bool IsRunning { get; private set; } = false;

        public IPv6ProtocolMode IPv6Mode { get; set; } = IPv6ProtocolMode.Disabled;

        public string HostnameIPv4 { get; set; } = "0.0.0.0";

        public string HostnameIPv6 { get; set; } = "::/0";


        public short Port { get; set; } = 27001;

        public LiteNetProtocolListener(
            ISerializationProvider serializerProvider,
            IRequestReceiver requestReceiver,
            IFenrirLogger logger,
            ITypeMap typeMap)
        {
            _serializerProvider = serializerProvider;
            _requestReceiver = requestReceiver;
            _logger = logger;
            _typeMap = typeMap;

            _messageReader = new LiteNetMessageReader(serializerProvider, typeMap, new RecyclableObjectPool<ByteStreamReader>());
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

                _netManager.Start(HostnameIPv4, HostnameIPv6, Port);

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
                        connectionRequestData = _serializerProvider.Deserialize<TConnectionRequestData>(byteStreamReader);
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


        async void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            // Get LiteNet peer
            if(peer.Tag == null)
            {
                _logger.Trace("Received message from an uninitialized peer: {0}", peer.EndPoint);
                return;
            }

            var netPeer = (LiteNetServerPeer)peer.Tag;

            // Get message
            MessageWrapper messageWrapper = _messageReader.ReadMessage(reader);

            // Dispatch message
            if (messageWrapper.MessageType == MessageType.Request)
            {
                // Request
                IRequest request = messageWrapper.MessageData as IRequest;
                if (request == null) // Someone is trying to mess with the protocol
                {
                    _logger.Trace("Empty request received from {0}", peer.EndPoint);
                    return;
                }

                MessageWrapper responseWrapper = await _requestReceiver.OnReceiveRequest(netPeer, messageWrapper);


            }
            else
            {
                _logger.Trace("Unsupported message type {0} received from {1}", messageWrapper.MessageType, peer.EndPoint);
            }
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            // Do nothing
        }

        void INetEventListener.OnPeerConnected(NetPeer netPeer)
        {
            // Create host peer
            var messageWriter = new LiteNetMessageWriter(_serializerProvider, _typeMap, _byteStreamWriterPool);
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
