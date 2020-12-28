using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Server.Events;
using System;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Server
{
    /// <summary>
    /// Fenrir Server Host
    /// </summary>
    public interface IFenrirServer : IFenrirServerInfoProvider, IDisposable
    {
        /// <summary>
        /// Invoked when server status changes
        /// </summary>
        event EventHandler<ServerStatusChangedEventArgs> StatusChanged;
        
        /// <summary>
        /// Invoked protocol is added
        /// </summary>
        event EventHandler<ProtocolAddedEventArgs> ProtocolAdded;

        /// <summary>
        /// Starts the server
        /// </summary>
        /// <returns>Task that completes when server has started</returns>
        Task Start();

        /// <summary>
        /// Stops the server
        /// </summary>
        /// <returns>Task that completes when server has stopped</returns>
        Task Stop();

        /// <summary>
        /// Sets contract serializer. 
        /// If not set, IByteStreamSerializable is the only supported way of serialization.
        /// If set, any data contract will be serialized using that contract serializer,
        /// with IByteStreamSerializable used as a fall back.
        /// </summary>
        void SetContractSerializer(IContractSerializer contractSerializer);

        /// <summary>
        /// Sets Fenrir Logger. If not set, EventBasedLogger is used
        /// </summary>
        /// <param name="logger">Fenrir Logger</param>
        void SetLogger(IFenrirLogger logger);

        /// <summary>
        /// Adds server protocol
        /// </summary>
        /// <param name="protocolListener">Protocol listener to add</param>
        void AddProtocol(IProtocolListener protocolListener);

        /// <summary>
        /// Adds Service
        /// </summary>
        /// <param name="service">Fenrir Service to add</param>
        void AddService(IFenrirService service);

        /// <summary>
        /// Sets custom connection request handler on all installed protocols
        /// </summary>
        /// <typeparam name="TConnectionRequestData">Type of connection request</typeparam>
        /// <param name="handler">Connection request handler</param>
        void SetConnectionRequestHandler<TConnectionRequestData>(Func<IServerConnectionRequest<TConnectionRequestData>, Task<ConnectionResponse>> handler)
            where TConnectionRequestData : class, new();

        /// <summary>
        /// Adds request handler of a given request type, to all installed protocols
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <param name="requestHandler">Request handler</param>
        void AddRequestHandler<TRequest>(IRequestHandler<TRequest> requestHandler)
            where TRequest : IRequest;

        /// <summary>
        /// Adds request handler for a given request and response type, to all installed protocols
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <typeparam name="TResponse">Type of response</typeparam>
        /// <param name="requestHandler">Request handler</param>
        void AddRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse;


        /// <summary>
        /// Adds asynchronous request handler for a given request and response type, to all installed protocols
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <typeparam name="TResponse">Type of response</typeparam>
        /// <param name="requestHandler">Request handler</param>
        void AddRequestHandlerAsync<TRequest, TResponse>(IRequestHandlerAsync<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse;
    }
}