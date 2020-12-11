namespace Fenrir.Multiplayer.Network
{
    internal interface IPeerInternal : IPeer
    {
        void Send(MessageWrapper messageWrapper);
    }
}