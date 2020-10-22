using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    public interface IResponseMap
    {
        Task<MessageWrapper> OnSendRequest(MessageWrapper messageWrapper);
    }
}