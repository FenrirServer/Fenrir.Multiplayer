using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.Network;
using LiteNetLib;
using System.Threading;
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
        private int _requestId = 0;

        /// <summary>
        /// Map of pending requests, that are waiting for a response from the server
        /// </summary>
        private readonly PendingRequestMap _pendingRequestMap;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="netPeer">LiteNet NetPeer</param>
        /// <param name="messageWriter">Message Writer</param>
        /// <param name="pendingRequestMap">Pending Request Map</param>
        public LiteNetClientPeer(NetPeer netPeer, LiteNetMessageWriter messageWriter, PendingRequestMap pendingRequestMap)
            : base(netPeer, messageWriter)
        {
            _pendingRequestMap = pendingRequestMap;
        }

        /// <inheritdoc/>
        public void SendRequest<TRequest>(TRequest request, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered) where TRequest : IRequest
        {
            int requestId = Interlocked.Increment(ref _requestId);

            var messageWrapper = new MessageWrapper()
            {
                MessageType = MessageType.Request,
                RequestId = requestId,
                MessageData = request,
                Peer = this,
                Channel = channel,
                DeliveryMethod = deliveryMethod,
            };

            Send(messageWrapper);
        }

        /// <inheritdoc/>
        public async Task<IResponse> SendRequest<TRequest, TResponse>(TRequest request, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            int requestId = Interlocked.Increment(ref _requestId);

            var messageWrapper = new MessageWrapper()
            {
                MessageType = MessageType.Request,
                RequestId = requestId,
                MessageData = request,
                Peer = this,
                Channel = channel,
                DeliveryMethod = deliveryMethod,
            };

            // Add request awaiter to a response map
            Task<MessageWrapper> task = _pendingRequestMap.OnSendRequest(messageWrapper);

            Send(messageWrapper);

            MessageWrapper responseWrapper = await task;

            IResponse response = responseWrapper.MessageData as IResponse;
            if(response == null)
            {
                throw new NetworkException($"Failed to dispatch response, {responseWrapper.MessageData.GetType().Name} does not implement IResponse");
            }

            return response;
        }
    }
}
