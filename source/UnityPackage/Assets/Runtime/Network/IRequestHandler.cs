using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Request Handler
    /// Invoked when server receives a request from the client and is required to sent a response
    /// </summary>
    /// <typeparam name="TRequest">Type of request</typeparam>
    /// <typeparam name="TResponse">Type of response</typeparam>
    public interface IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResponse
    {
        /// <summary>
        /// Invoked when request is received from the server
        /// </summary>
        /// <param name="request">Request object</param>
        /// <param name="peer">Remote peer</param>
        /// <returns>Response for a given request</returns>
        TResponse HandleRequest(TRequest request, IServerPeer peer);
    }

    /// <summary>
    /// Request Handler
    /// Invoked when server receives a request from the client
    /// </summary>
    /// <typeparam name="TRequest">Type of request</typeparam>
    public interface IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        /// <summary>
        /// Invoked when request is received from the server
        /// </summary>
        /// <param name="request">Request object</param>
        /// <param name="peer">Remote peer</param>
        void HandleRequest(TRequest request, IServerPeer peer);
    }
}
