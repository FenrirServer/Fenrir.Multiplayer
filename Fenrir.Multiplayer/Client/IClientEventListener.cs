using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.Client
{
    interface IClientEventListener
    {
        void OnReceiveEvent(MessageWrapper messageWrapper);
    }
}
