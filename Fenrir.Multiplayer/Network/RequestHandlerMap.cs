using Fenrir.Multiplayer.Exceptions;
using System;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Request handler map
    /// Stores requests bound to request types
    /// </summary>
    class RequestHandlerMap
    {
        /// <summary>
        /// Sync root
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Request handlers bound to a request type
        /// </summary>
        private Dictionary<Type, Action<MessageWrapper>> _requestHandlers = new Dictionary<Type, Action<MessageWrapper>>();

        /// <summary>
        /// Adds request handler of a given request type
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <param name="requestHandler">Request handler</param>
        public void AddRequestHandler<TRequest>(IRequestHandler<TRequest> requestHandler)
            where TRequest : IRequest
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            lock (_syncRoot)
            {
                if (_requestHandlers.ContainsKey(typeof(TRequest)))
                {
                    throw new RequestHandlerException($"Failed to add request handler {requestHandler.GetType()}, handler for request type {typeof(TRequest).Name} is already registered");
                }

                _requestHandlers.Add(typeof(TRequest), requestWrapper =>
                {
                    requestHandler.HandleRequest((TRequest)requestWrapper.MessageData, (IServerPeer)requestWrapper.Peer);
                });
            }
        }

        /// <summary>
        /// Adds request handler for a given request and response type
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <typeparam name="TResponse">Type of response</typeparam>
        /// <param name="requestHandler">Request handler</param>
        public void AddRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            lock (_syncRoot)
            {
                if (_requestHandlers.ContainsKey(typeof(TRequest)))
                {
                    throw new RequestHandlerException($"Failed to add request handler {requestHandler.GetType()}, handler for request type {typeof(TRequest).Name} is already registered");
                }

                _requestHandlers.Add(typeof(TRequest), async requestWrapper =>
                {
                    TResponse response = await requestHandler.HandleRequest((TRequest)requestWrapper.MessageData, (IServerPeer)requestWrapper.Peer);

                    if(response != null)
                    {
                        var peer = requestWrapper.Peer;
                        peer.Send(new MessageWrapper() 
                        { 
                            MessageType = MessageType.Response,
                            RequestId = requestWrapper.RequestId, 
                            Peer = requestWrapper.Peer, 
                            MessageData = response 
                        });
                    }
                });
            }
        }

        /// <summary>
        /// Removes request handler
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <param name="requestHandler">Reqeuest handler</param>
        public void RemoveRequestHandler<TRequest>(IRequestHandler<TRequest> requestHandler)
            where TRequest : IRequest
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            lock (_syncRoot)
            {
                if (!_requestHandlers.ContainsKey(typeof(TRequest)))
                {
                    throw new RequestHandlerException($"Failed to remove request handler {requestHandler.GetType()}, handler for request type {typeof(TRequest).Name} is not registered");
                }

                _requestHandlers.Remove(typeof(TRequest));
            }
        }

        /// <summary>
        /// Removes request handler
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <typeparam name="TResponse">Type of response</typeparam>
        /// <param name="requestHandler">Request handler</param>
        public void RemoveRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            lock (_syncRoot)
            {
                if (!_requestHandlers.ContainsKey(typeof(TRequest)))
                {
                    throw new RequestHandlerException($"Failed to remove request handler {requestHandler.GetType()}, handler for request type {typeof(TRequest).Name} is not registered");
                }

                _requestHandlers.Remove(typeof(TRequest));
            }
        }

        /// <summary>
        /// Invoked when request is received by the server
        /// </summary>
        /// <param name="serverPeer">Peer from which request is received</param>
        /// <param name="requestWrapper">Wrapped message</param>
        public void OnReceiveRequest(IServerPeer serverPeer, MessageWrapper requestWrapper)
        {
            Type requestType = requestWrapper.MessageData.GetType();

            // Try to get request handler
            Action<MessageWrapper> requestHandler = null;

            lock (_syncRoot)
            {
                if (_requestHandlers.ContainsKey(requestType))
                {
                    requestHandler = _requestHandlers[requestType];
                }
            }

            // If found, invoke
            if (requestHandler == null)
            {
                throw new RequestHandlerException($"Failed to dispatch request of type {requestType}, handler for request type is not registered");
            }

            requestHandler.Invoke(requestWrapper);
        }
    }
}
