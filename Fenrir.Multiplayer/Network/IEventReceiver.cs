namespace Fenrir.Multiplayer.Network
{
    public interface IEventReceiver
    {
        void OnReceiveEvent(MessageWrapper eventWrapper);
    }
}
