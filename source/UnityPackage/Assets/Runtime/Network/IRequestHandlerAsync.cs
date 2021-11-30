using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Asynchronous Request Handler
    /// Invoked when server receives a request from the client and is required to sent a response.
    /// </summary>
    /// <typeparam name="TRequest">Type of request</typeparam>
    /// <typeparam name="TResponse">Type of response</typeparam>
    public interface IRequestHandlerAsync<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResponse
    {
        /// <summary>
        /// Invoked when request is received from the server
        /// </summary>
        /// <param name="request">Request object</param>
        /// <param name="peer">Remote peer</param>
        /// <returns>Task that must complete with a response for a given request</returns>
        Task<TResponse> HandleRequestAsync(TRequest request, IServerPeer peer);
    }

}
