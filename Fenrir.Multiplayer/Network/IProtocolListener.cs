﻿using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Server;
using System;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Represents a protocol server listener
    /// </summary>
    public interface IProtocolListener : IDisposable
    {
        /// <summary>
        /// Indicates if protocol is listening
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Type of the protocol
        /// </summary>
        ProtocolType ProtocolType { get; }

        /// <summary>
        /// Time after which client is disconnected if no keep alive packets are received
        /// </summary>
        int DisconnectTimeout { get; set; }

        /// <summary>
        /// Delay between network ticks
        /// </summary>
        int UpdateTime { get; set; }

        /// <summary>
        /// Interval between KeepAlive packets.
        /// Must be smaller than <seealso cref="DisconnectTimeout"/>
        /// </summary>
        int PingInterval { get; set; }

        /// <summary>
        /// Starts protocol listener
        /// </summary>
        /// <returns>Task that completes when protocol listener is runnign</returns>
        Task Start();

        /// <summary>
        /// Stops the protocol listener
        /// </summary>
        /// <returns>Task that completes when protocol listener is stopped</returns>
        Task Stop();

        /// <summary>
        /// Sets contract serializer. 
        /// If not set, IByteStreamSerializable is the only supported way of serialization.
        /// If set, any data contract will be serialized using that contract serializer,
        /// with IByteStreamSerializable used as a fall back.
        /// </summary>
        void SetContractSerializer(ITypeSerializer contractSerializer);

        /// <summary>
        /// Sets custom connection request handler.
        /// Custom connection request handler allows to dispatch server connections and check custom data
        /// </summary>
        /// <typeparam name="TConnectionRequestData">Type of a custom data contract</typeparam>
        /// <param name="handler">Connection request handler</param>
        void SetConnectionRequestHandler<TConnectionRequestData>(Func<IServerConnectionRequest<TConnectionRequestData>, Task<ConnectionResponse>> handler)
            where TConnectionRequestData : class, new();

        /// <summary>
        /// Sets Fenrir Logger. If not set, EventBasedLogger is used
        /// </summary>
        /// <param name="logger">Fenrir Logger</param>
        void SetLogger(IFenrirLogger logger);

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

        /// <summary>
        /// Removes request handler, from all installed protocols
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <param name="requestHandler">Reqeuest handler</param>
        void RemoveRequestHandler<TRequest>(IRequestHandler<TRequest> requestHandler)
           where TRequest : IRequest;

        /// <summary>
        /// Removes request handler, from all installed protocols
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <typeparam name="TResponse">Type of response</typeparam>
        /// <param name="requestHandler">Request handler</param>
        void RemoveRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse;

        /// <summary>
        /// Removes asynchronous request handler, from all installed protocols
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <typeparam name="TResponse">Type of response</typeparam>
        /// <param name="requestHandler">Request handler</param>
        void RemoveRequestHandlerAsync<TRequest, TResponse>(IRequestHandlerAsync<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse;

        /// <summary>
        /// Returns protocol connection data, required to pass by the client when connecting using this protocol
        /// </summary>
        IProtocolConnectionData GetConnectionData();
    }
}