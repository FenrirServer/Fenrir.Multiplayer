namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Internal peer
    /// Contains methods not exposed via public api
    /// </summary>
    internal interface IPeerInternal : IPeer
    {
        /// <summary>
        /// Sends raw message
        /// </summary>
        /// <param name="messageWrapper">Message wrapper</param>
        void Send(MessageWrapper messageWrapper);
    }
}