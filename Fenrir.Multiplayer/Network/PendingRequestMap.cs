using Fenrir.Multiplayer.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Map of pending requests.
    /// Stores request tasks until response arrives
    /// </summary>
    class PendingRequestMap
    {
        /// <summary>
        /// Stores pending request tasks by request id
        /// </summary>
        private Dictionary<int, TaskCompletionSource<MessageWrapper>> _requestTcsMap = new Dictionary<int, TaskCompletionSource<MessageWrapper>>();

        /// <summary>
        /// Sync root
        /// </summary>
        private object _syncRoot = new object();

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Creates pending request map
        /// </summary>
        /// <param name="logger">Logger</param>
        public PendingRequestMap(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Invoked when request is sent
        /// Adds request id to the request tcs map
        /// </summary>
        /// <param name="messageWrapper">Message wrapper of the request</param>
        /// <returns>Task that completes when response wrapper is received</returns>
        public Task<MessageWrapper> OnSendRequest(MessageWrapper messageWrapper)
        {
            TaskCompletionSource<MessageWrapper> tcs = new TaskCompletionSource<MessageWrapper>();
            lock (_syncRoot)
            {
                _requestTcsMap[messageWrapper.RequestId] = tcs;
            }
            return tcs.Task;
        }

        /// <summary>
        /// Invoked when response wrapper is received
        /// </summary>
        /// <param name="requestId">Id of the request</param>
        /// <param name="responseWrapper">Response wrapper</param>
        public void OnReceiveResponse(int requestId, MessageWrapper responseWrapper)
        {
            bool foundResponseTcs = false;
            TaskCompletionSource<MessageWrapper> tcs = null;

            lock (_syncRoot)
            {
                foundResponseTcs = _requestTcsMap.TryGetValue(requestId, out tcs);
                if (foundResponseTcs) 
                {
                    _requestTcsMap.Remove(requestId);
                }
            }

            if(!foundResponseTcs)
            {
                _logger.Warning("Received response but no pending request is found with id {0}", requestId);
                return;
            }

            try
            {
                tcs.SetResult(responseWrapper);
            }
            catch(Exception e)
            {
                _logger.Error("Uncaught exception in a continuation for request: {0}", e);
            }
        }
    }
}
