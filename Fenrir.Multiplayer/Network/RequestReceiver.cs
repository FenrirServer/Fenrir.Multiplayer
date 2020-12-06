using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    class RequestReceiver : IRequestReceiver
    {
        public Task<MessageWrapper> OnReceiveRequest(IClientPeer peer, MessageWrapper requestWrapper)
        {

        }
    }
}
