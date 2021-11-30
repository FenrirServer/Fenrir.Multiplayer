using Fenrir.Multiplayer.Network;

namespace Fenrir.Multiplayer.Client
{
    /// <summary>
    /// Client event listener. 
    /// Defines method for receiving client events
    /// </summary>
    interface IClientEventListener
    {
        /// <summary>
        /// Invoked when client receives an event from the server
        /// </summary>
        /// <param name="messageWrapper">Event message wrapper. Contains event information</param>
        void OnReceiveEvent(MessageWrapper messageWrapper);
    }
}
