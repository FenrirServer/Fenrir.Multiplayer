using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    interface IRequestReceiver
    {
        Task<MessageWrapper> OnReceiveRequest(IClientPeer peer, MessageWrapper requestWrapper);
    }
}