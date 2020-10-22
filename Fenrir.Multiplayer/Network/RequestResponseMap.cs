using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    class RequestResponseMap : IResponseMap, IResponseReceiver
    {
        private Dictionary<int, TaskCompletionSource<MessageWrapper>> _requests = new Dictionary<int, TaskCompletionSource<MessageWrapper>>();

        private object _syncRoot;

        public RequestResponseMap()
        {
        }

        public Task<MessageWrapper> OnSendRequest(MessageWrapper messageWrapper)
        {
            TaskCompletionSource<MessageWrapper> tcs = new TaskCompletionSource<MessageWrapper>();
            lock (_syncRoot)
            {
                _requests[messageWrapper.RequestId] = tcs;
            }
            return tcs.Task;
        }

        public void OnReceiveResponse(int requestId, MessageWrapper responseWrapper)
        {
            TaskCompletionSource<MessageWrapper> tcs = null;
            lock (_syncRoot)
            {
                if(!_requests.TryGetValue(requestId, out tcs))
                {
                    return;
                }
                _requests.Remove(requestId);
            }

            tcs.SetResult(responseWrapper);
        }
    }
}
