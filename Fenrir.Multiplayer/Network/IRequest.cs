namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Identifies Request sent from client to a server
    /// </summary>
    public interface IRequest
    {
    }

    /// <summary>
    /// Identifies Request sent from client to a server,
    /// that requires a response
    /// </summary>
    /// <typeparam name="TResponse">Type of response</typeparam>
    public interface IRequest<TResponse> : IRequest
        where TResponse : IResponse
    {
    }
}
