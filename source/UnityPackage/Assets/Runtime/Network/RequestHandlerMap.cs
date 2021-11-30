using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.Logging;
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
        /// Logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Request handlers bound to a request type
        /// </summary>
        private Dictionary<Type, Action<MessageWrapper, IServerPeer>> _requestHandlers = new Dictionary<Type, Action<MessageWrapper, IServerPeer>>();

        /// <summary>
        /// Creates RequestHandlerMap
        /// </summary>
        /// <param name="logger">Logger</param>
        public RequestHandlerMap(ILogger logger)
        {
            _logger = logger;
        }

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

            Action<MessageWrapper, IServerPeer> handlerAction = (requestWrapper, serverPeer) =>
            {
                try
                {
                    requestHandler.HandleRequest((TRequest)requestWrapper.MessageData, serverPeer);
                }
                catch(Exception e)
                {
                    _logger.Error("Uncaught exception in request {0} handler {1}: {2}", typeof(TRequest).Name, requestHandler, e.ToString());
                }
            };

            lock (_syncRoot)
            {
                if (_requestHandlers.ContainsKey(typeof(TRequest)))
                {
                    throw new RequestHandlerException($"Failed to add request handler {requestHandler.GetType()}, handler for request type {typeof(TRequest).Name} is already registered");
                }

                _requestHandlers.Add(typeof(TRequest), handlerAction);
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

            Action<MessageWrapper, IServerPeer> handlerAction = (requestWrapper, serverPeer) =>
            {
                short requestId = requestWrapper.RequestId;
                bool isEncrypted = requestWrapper.Flags.HasFlag(MessageFlags.IsEncrypted);
                bool isOrdered = requestWrapper.Flags.HasFlag(MessageFlags.IsOrdered);
                byte channel = requestWrapper.Channel;

                TResponse response = default;
                    
                try
                {
                    response = requestHandler.HandleRequest((TRequest)requestWrapper.MessageData, serverPeer);
                }
                catch (Exception e)
                {
                    _logger.Error("Uncaught exception in request {0} handler {1}: {2}", typeof(TRequest).Name, requestHandler, e.ToString());
                }

                if (response == null)
                {
                    serverPeer.SendResponse<ErrorResponse>(new ErrorResponse(), requestId, isEncrypted, channel, isOrdered);
                    return;
                }

                serverPeer.SendResponse<TResponse>(response, requestId, isEncrypted, channel, isOrdered);

            };

            lock (_syncRoot)
            {
                if (_requestHandlers.ContainsKey(typeof(TRequest)))
                {
                    throw new RequestHandlerException($"Failed to add request handler {requestHandler.GetType()}, handler for request type {typeof(TRequest).Name} is already registered");
                }

                _requestHandlers.Add(typeof(TRequest), handlerAction);
            }
        }

        /// <summary>
        /// Adds asynchronous request handler for a given request and response type
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <typeparam name="TResponse">Type of response</typeparam>
        /// <param name="requestHandler">Request handler</param>
        public void AddRequestHandlerAsync<TRequest, TResponse>(IRequestHandlerAsync<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException(nameof(requestHandler));
            }

            Action<MessageWrapper, IServerPeer> handlerAction = async (requestWrapper, serverPeer) =>
            {
                short requestId = requestWrapper.RequestId;
                bool isEncrypted = requestWrapper.Flags.HasFlag(MessageFlags.IsEncrypted);
                bool isOrdered = requestWrapper.Flags.HasFlag(MessageFlags.IsOrdered);
                byte channel = requestWrapper.Channel;

                TResponse response = default;

                try
                {
                    response = await requestHandler.HandleRequestAsync((TRequest)requestWrapper.MessageData, serverPeer);
                }
                catch (Exception e)
                {
                    _logger.Error("Uncaught exception in request {0} handler {1}: {2}", typeof(TRequest).Name, requestHandler, e.ToString());
                }

                if (response == null)
                {
                    serverPeer.SendResponse<ErrorResponse>(new ErrorResponse(), requestId, isEncrypted, channel, isOrdered);
                    return;
                }

                serverPeer.SendResponse<TResponse>(response, requestId, isEncrypted, channel, isOrdered);

            };

            lock (_syncRoot)
            {
                if (_requestHandlers.ContainsKey(typeof(TRequest)))
                {
                    throw new RequestHandlerException($"Failed to add request handler {requestHandler.GetType()}, handler for request type {typeof(TRequest).Name} is already registered");
                }

                _requestHandlers.Add(typeof(TRequest), handlerAction);
            }
        }
        /// <summary>
        /// Removes request handler
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        public void RemoveRequestHandler<TRequest>()
            where TRequest : IRequest
        {
            lock (_syncRoot)
            {
                if (!_requestHandlers.ContainsKey(typeof(TRequest)))
                {
                    throw new RequestHandlerException($"Failed to remove request handler for request type {typeof(TRequest).Name}, handler for the given type is not registered");
                }

                _requestHandlers.Remove(typeof(TRequest));
            }
        }

        /// <summary>
        /// Removes request handler
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <typeparam name="TResponse">Type of response</typeparam>
        public void RemoveRequestHandler<TRequest, TResponse>()
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            lock (_syncRoot)
            {
                if (!_requestHandlers.ContainsKey(typeof(TRequest)))
                {
                    throw new RequestHandlerException($"Failed to remove request handler for request type {typeof(TRequest).Name} and response type {typeof(TResponse).Name}, handler for the given type is not registered");
                }

                _requestHandlers.Remove(typeof(TRequest));
            }
        }

        /// <summary>
        /// Removes request handler
        /// </summary>
        /// <typeparam name="TRequest">Type of request</typeparam>
        /// <typeparam name="TResponse">Type of response</typeparam>
        public void RemoveRequestHandlerAsync<TRequest, TResponse>()
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            lock (_syncRoot)
            {
                if (!_requestHandlers.ContainsKey(typeof(TRequest)))
                {
                    throw new RequestHandlerException($"Failed to remove request handler for request type {typeof(TRequest).Name} and response type {typeof(TResponse).Name}, handler for the given type is not registered");
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
            bool hasRequestHandler = false;
            Action<MessageWrapper, IServerPeer> requestHandler = null;

            lock (_syncRoot)
            {
                hasRequestHandler = _requestHandlers.TryGetValue(requestType, out requestHandler);
            }

            // If found, invoke
            if (!hasRequestHandler)
            {
                _logger.Warning($"Failed to dispatch request of type {0}, handler for request type is not registered", requestType);
                return;
            }

            requestHandler.Invoke(requestWrapper, serverPeer);
        }
    }
}
