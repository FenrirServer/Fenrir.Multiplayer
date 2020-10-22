using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    public interface IClientPeer : IPeer
    {
        void SendRequest<TRequest>(TRequest request, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered)
            where TRequest : IRequest;

        Task<IResponse> SendRequest<TRequest, TResponse>(TRequest request, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse;
    }
}
