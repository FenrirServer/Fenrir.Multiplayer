using LiteNetLib.Utils;
using System.Collections.Concurrent;

namespace Fenrir.Multiplayer.Network
{
    class NetDataWriterPool
    {
        private readonly ConcurrentBag<NetDataWriter> _objects;

        public NetDataWriterPool() : this(0)
        {
        }

        public NetDataWriterPool(int initialSize)
        {
            _objects = new ConcurrentBag<NetDataWriter>();

            for (int i = 0; i < initialSize; i++)
            {
                _objects.Add(new NetDataWriter());
            }
        }

        public NetDataWriter Get()
        {
            if (!_objects.TryTake(out NetDataWriter netDataWRiter))
            {
                netDataWRiter = new NetDataWriter();
            }

            return netDataWRiter;
        }

        public void Return(NetDataWriter netDataWriter)
        {
            netDataWriter.Reset();
            _objects.Add(netDataWriter);
        }
    }
}