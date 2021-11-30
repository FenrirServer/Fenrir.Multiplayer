using LiteNetLib.Utils;
using System.Collections.Concurrent;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Object pool of NetDataWriters
    /// NetDataWriters are temporary buffers used to send messages
    /// </summary>
    class NetDataWriterPool
    {
        /// <summary>
        /// Concurrent collection of NetDataWriters
        /// </summary>
        private readonly ConcurrentBag<NetDataWriter> _objects;

        /// <summary>
        /// Default constructor
        /// </summary>
        public NetDataWriterPool() : this(0)
        {
        }

        /// <summary>
        /// Constructs object pool with a given initial size
        /// </summary>
        /// <param name="initialSize">Initial size of the object pool</param>
        public NetDataWriterPool(int initialSize)
        {
            _objects = new ConcurrentBag<NetDataWriter>();

            for (int i = 0; i < initialSize; i++)
            {
                _objects.Add(new NetDataWriter());
            }
        }

        /// <summary>
        /// Removes NetDataWriter from the pool and returns it
        /// If no NetDataWriters are left in the object pool, new one is created
        /// </summary>
        /// <returns>NetDataWriter</returns>
        public NetDataWriter Get()
        {
            if (!_objects.TryTake(out NetDataWriter netDataWRiter))
            {
                netDataWRiter = new NetDataWriter();
            }

            return netDataWRiter;
        }

        /// <summary>
        /// Returns NetDataWriter back to the pool
        /// </summary>
        /// <param name="netDataWriter">NetDataWriter</param>
        public void Return(NetDataWriter netDataWriter)
        {
            netDataWriter.Reset();
            _objects.Add(netDataWriter);
        }
    }
}