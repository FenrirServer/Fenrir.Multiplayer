using System;

namespace Fenrir.Multiplayer.Network
{
    [Flags]
    enum MessageFlags
    {
        None = 0,
        Encrypted = 1,
    }
}
