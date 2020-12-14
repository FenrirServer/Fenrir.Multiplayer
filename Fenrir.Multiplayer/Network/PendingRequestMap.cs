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
        /// Default constructor
        /// </summary>
        public PendingRequestMap()
        {
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
            TaskCompletionSource<MessageWrapper> tcs = null;
            lock (_syncRoot)
            {
                if(!_requestTcsMap.TryGetValue(requestId, out tcs))
                {
                    return;
                }
                _requestTcsMap.Remove(requestId);
            }

            tcs.SetResult(responseWrapper);
        }
    }
}
