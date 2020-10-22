using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    public interface IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResponse
    {
        Task<TResponse> HandleRequest(TRequest request, IPeer peer);
    }

    public interface IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        void HandleRequest(TRequest request, IPeer peer);
    }
}
