namespace Fenrir.Multiplayer.Network
{
    public interface IResponseReceiver
    {
        void OnReceiveResponse(int requestId, MessageWrapper responseWrapper);
    }
}
