using Fenrir.Multiplayer.Serialization;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Raw Byte Stream Message handler
    /// Invoked when message with a given code is received
    /// </summary>
    public interface IRawMessageHandlerAsync
    {
        /// <summary>
        /// Invoked when raw message with a given code is received
        /// </summary>
        /// <param name="code">Unique message code</param>
        /// <param name="reader">Byte Stream Reader - contains message buffer</param>
        /// <param name="peer">Peer that sent the message</param>
        /// <returns>Task that completes when handler is done reading the message and the buffer can be released</returns>
        Task OnReceiveMessageAsync(ushort code, IByteStreamReader reader, IPeer peer);
    }
}
