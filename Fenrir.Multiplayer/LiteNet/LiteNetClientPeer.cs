using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using LiteNetLib;
using System;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// LiteNet Client Peer Implementation
    /// </summary>
    class LiteNetClientPeer : LiteNetBasePeer, IClientPeer
    {
        /// <summary>
        /// Numeric id of the last request
        /// </summary>
        private short _requestId = 0;

        /// <summary>
        /// Sync root for incrementing request id
        /// </summary>
        private object _requestIdLock = new object();

        /// <summary>
        /// Map of pending requests, that are waiting for a response from the server
        /// </summary>
        private readonly PendingRequestMap _pendingRequestMap;

        /// <summary>
        /// Type hash map
        /// </summary>
        private readonly ITypeHashMap _typeHashMap;

        /// <summary>
        /// Request timeout
        /// </summary>
        private readonly int _requestTimeoutMs;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="peerId">Unique id of this client to server connection</param>
        /// <param name="netPeer">LiteNet NetPeer</param>
        /// <param name="messageWriter">Message Writer</param>
        /// <param name="pendingRequestMap">Pending Request Map</param>
        /// <param name="byteStreamWriterPool">Byte Stream Writer Object Pool</param>
        public LiteNetClientPeer(string peerId, NetPeer netPeer, MessageWriter messageWriter, PendingRequestMap pendingRequestMap, ITypeHashMap typeHashMap, RecyclableObjectPool<ByteStreamWriter> byteStreamWriterPool, int requestTimeoutMs)
            : base(peerId, netPeer, messageWriter, byteStreamWriterPool)
        {
            _pendingRequestMap = pendingRequestMap;
            _requestTimeoutMs = requestTimeoutMs;
            _typeHashMap = typeHashMap;
        }

        /// <inheritdoc/>
        public void SendRequest<TRequest>(TRequest request, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered) 
            where TRequest : IRequest
        {
            // By default, all reliable messages are encrypted
            bool encrypted = deliveryMethod == MessageDeliveryMethod.ReliableOrdered || deliveryMethod == MessageDeliveryMethod.ReliableUnordered;

            SendRequest(request, encrypted, channel, deliveryMethod);
        }

        /// <inheritdoc/>
        public void SendRequest<TRequest>(TRequest request, bool encrypted, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered)
            where TRequest : IRequest
        {
            short requestId = 0; // Requests with no response, do not require a unique id
            MessageFlags flags = encrypted ? MessageFlags.IsEncrypted : MessageFlags.None; // Other flags should not be set for request with no response...
            
            MessageWrapper messageWrapper = MessageWrapper.WrapRequest(request, requestId, channel, flags, deliveryMethod);
            Send(messageWrapper);
        }

        /// <inheritdoc/>
        public async Task<TResponse> SendRequest<TRequest, TResponse>(TRequest request, bool encrypted = true, byte channel = 0, bool ordered = true)
            where TRequest : IRequest<TResponse>
            where TResponse : class, IResponse
        {
            // Register response type
            _typeHashMap.AddType<TResponse>();

            // Requests that require a response, require unique id to be tracked
            short requestId = GetNextRequestId();

            MessageDeliveryMethod deliveryMethod = ordered ? MessageDeliveryMethod.ReliableOrdered : MessageDeliveryMethod.ReliableUnordered; // Requests that require a response are always reliable
            MessageFlags flags = MessageFlags.HasRequestId;
            if (encrypted)
            {
                flags |= MessageFlags.IsEncrypted;
            }
            if(ordered)
            {
                flags |= MessageFlags.IsOrdered;
            }

            MessageWrapper messageWrapper = MessageWrapper.WrapRequest(request, requestId, channel, flags, deliveryMethod);

            // Add request awaiter to a response map
            Task<MessageWrapper> task = _pendingRequestMap.OnSendRequest(messageWrapper);

            Send(messageWrapper);
            
            // Send request and wait for response message wrapper to arrive, or timeout
            MessageWrapper responseWrapper;
            if (await Task.WhenAny(task, Task.Delay(_requestTimeoutMs)) == task)
            {
                responseWrapper = task.Result;
            }
            else
            {
                throw new RequestTimeoutException($"Request {typeof(TRequest).Name} timed out, server did not respond within {_requestTimeoutMs} ms");
            }

            // Check if request failed and no response
            if(responseWrapper.MessageData is ErrorResponse)
            {
                throw new RequestFailedException($"Server failed to process request {typeof(TRequest).Name}");
            }

            if(responseWrapper.MessageData == null)
            {
                throw new RequestFailedException($"Server failed to process request {typeof(TRequest).Name}, empty response"); // should not happen per server logic
            }

            TResponse response = responseWrapper.MessageData as TResponse;
            if(response == null)
            {
                throw new NetworkException($"Failed to dispatch response, {responseWrapper.MessageData.GetType().Name} does not implement {nameof(IResponse)}");
            }

            return response;
        }

        private short GetNextRequestId()
        {
            lock(_requestIdLock)
            {
                return ++_requestId;
            }
        }
    }
}
