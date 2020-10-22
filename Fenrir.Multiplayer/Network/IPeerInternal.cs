using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.Network
{
    internal interface IPeerInternal
    {
        void Send(MessageWrapper messageWrapper);
    }
}