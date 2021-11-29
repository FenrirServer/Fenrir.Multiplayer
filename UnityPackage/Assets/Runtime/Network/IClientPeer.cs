using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Client Peer
    /// </summary>
    public interface IClientPeer : IPeer
    {
        /// <summary>
        /// Sends request to the server. 
        /// All reliable requests are encrypted by default. 
        /// If you wish to override this behavior, you can explicitly specify encryption.
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <param name="request">Request object</param>
        /// <param name="channel">Channel Number</param>
        /// <param name="deliveryMethod">Delivery method</param>
        void SendRequest<TRequest>(TRequest request, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered)
            where TRequest : IRequest;

        /// <summary>
        /// Sends request to the server with specified encryption
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <param name="request">Request object</param>
        /// <param name="encrypted">True if request should be encrypted, otherwise false</param>
        /// <param name="channel">Channel Number</param>
        /// <param name="deliveryMethod">Delivery method</param>
        void SendRequest<TRequest>(TRequest request, bool encrypted, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered)
            where TRequest : IRequest;

        /// <summary>
        /// Sends request to the server and waits for a response.
        /// All requests that require a response are reliable. You can not provide unreliable delivery method but you can chose to provide ordered vs unordered.
        /// All requests that require a response are encrypted by default. You can override this behavior.
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <typeparam name="TResponse">Type of response</typeparam>
        /// <param name="request">Request object</param>
        /// <param name="encrypted">True if message should be encrypted</param>
        /// <param name="channel">Channel number</param>
        /// <param name="ordered">If true, messages in the specified channel will arrive in order.</param>
        /// <returns>Task that completes when response is received</returns>
        Task<TResponse> SendRequest<TRequest, TResponse>(TRequest request, bool encrypted = true, byte channel = 0, bool ordered = true)
            where TRequest : IRequest<TResponse>
            where TResponse : class, IResponse;
    }
}
