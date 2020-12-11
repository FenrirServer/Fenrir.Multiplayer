using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.Network;
using LiteNetLib;
using System.Threading;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.LiteNet
{
    class LiteNetClientPeer : LiteNetBasePeer, IClientPeer
    {
        private int _requestId = 0;
        private readonly RequestResponseMap _responseMap;

        public LiteNetClientPeer(NetPeer netPeer, LiteNetMessageWriter messageWriter, RequestResponseMap responseMap)
            : base(netPeer, messageWriter)
        {
            _responseMap = responseMap;
        }

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
            Task<MessageWrapper> task = _responseMap.OnSendRequest(messageWrapper);

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
