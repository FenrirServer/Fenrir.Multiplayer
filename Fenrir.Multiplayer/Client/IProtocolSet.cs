using Fenrir.Multiplayer.Network;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Client
{
    interface IProtocolSet : IDictionary<ProtocolType, IProtocol>
    {
    }
}