using Fenrir.Multiplayer.Network;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Client
{
    class ProtocolSet : Dictionary<ProtocolType, IProtocol>, IProtocolSet
    {
    }
}
