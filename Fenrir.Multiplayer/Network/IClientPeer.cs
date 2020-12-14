using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Client Peer
    /// </summary>
    public interface IClientPeer : IPeer
    {
        /// <summary>
        /// Sends request to the serveer
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <param name="request">Request object</param>
        /// <param name="channel">Channel Number</param>
        /// <param name="deliveryMethod">Delivery method</param>
        void SendRequest<TRequest>(TRequest request, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered)
            where TRequest : IRequest;

        /// <summary>
        /// Sends request to the server and waits for a response
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <typeparam name="TResponse">Type of response</typeparam>
        /// <param name="request">Request object</param>
        /// <param name="channel">Channel number</param>
        /// <param name="deliveryMethod">Delivery method</param>
        /// <returns>Task that completes when response is received</returns>
        Task<IResponse> SendRequest<TRequest, TResponse>(TRequest request, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse;
    }
}
