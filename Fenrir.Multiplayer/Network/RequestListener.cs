using Fenrir.Multiplayer.Exceptions;
using System;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Network
{
    class RequestListener
    {
        private readonly object _syncRoot = new object();

        private Dictionary<Type, Action<MessageWrapper>> _requestHandlers = new Dictionary<Type, Action<MessageWrapper>>();

        public void AddRequestHandler<TRequest>(IRequestHandler<TRequest> requestHandler)
            where TRequest : IRequest
        {
            lock (_syncRoot)
            {
                if (_requestHandlers.ContainsKey(typeof(TRequest)))
                {
                    throw new RequestListenerException($"Failed to add request handler {requestHandler.GetType()}, handler for request type {typeof(TRequest).Name} is already registered");
                }

                _requestHandlers.Add(typeof(TRequest), requestWrapper =>
                {
                    requestHandler.HandleRequest((TRequest)requestWrapper.MessageData, requestWrapper.Peer);
                });
            }
        }

        public void AddRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            lock (_syncRoot)
            {
                if (_requestHandlers.ContainsKey(typeof(TRequest)))
                {
                    throw new RequestListenerException($"Failed to add request handler {requestHandler.GetType()}, handler for request type {typeof(TRequest).Name} is already registered");
                }

                _requestHandlers.Add(typeof(TRequest), async requestWrapper =>
                {
                    TResponse response = await requestHandler.HandleRequest((TRequest)requestWrapper.MessageData, requestWrapper.Peer);

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

        public void RemoveRequestHandler<TRequest>(IRequestHandler<TRequest> requestHandler)
            where TRequest : IRequest
        {
            lock (_syncRoot)
            {
                if (!_requestHandlers.ContainsKey(typeof(TRequest)))
                {
                    throw new RequestListenerException($"Failed to remove request handler {requestHandler.GetType()}, handler for request type {typeof(TRequest).Name} is not registered");
                }

                _requestHandlers.Remove(typeof(TRequest));
            }
        }

        public void RemoveRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> requestHandler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            lock (_syncRoot)
            {
                if (!_requestHandlers.ContainsKey(typeof(TRequest)))
                {
                    throw new RequestListenerException($"Failed to remove request handler {requestHandler.GetType()}, handler for request type {typeof(TRequest).Name} is not registered");
                }

                _requestHandlers.Remove(typeof(TRequest));
            }
        }


        public void OnReceiveRequest(MessageWrapper requestWrapper)
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
                throw new EventListenerException($"Failed to dispatch request of type {requestType}, handler for request type is not registered");
            }

            requestHandler.Invoke(requestWrapper);
        }
    }
}
