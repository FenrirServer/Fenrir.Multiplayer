namespace Fenrir.Multiplayer.Network
{
    interface IEventReceiver
    {
        void OnReceiveEvent(MessageWrapper eventWrapper);
    }
}
