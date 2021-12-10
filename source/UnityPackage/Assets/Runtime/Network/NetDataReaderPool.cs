using LiteNetLib.Utils;
using System.Collections.Concurrent;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Object pool of NetDataReaders LiteNet
    /// NetDataReaders are temporary LiteNet buffers used to receive messages
    /// </summary>
    class NetDataReaderPool
    {
        /// <summary>
        /// Concurrent collection of NetDataReaders
        /// </summary>
        private readonly ConcurrentBag<NetDataReader> _objects;

        /// <summary>
        /// Default constructor
        /// </summary>
        public NetDataReaderPool() : this(0)
        {
        }

        /// <summary>
        /// Constructs object pool with a given initial size
        /// </summary>
        /// <param name="initialSize">Initial size of the object pool</param>
        public NetDataReaderPool(int initialSize)
        {
            _objects = new ConcurrentBag<NetDataReader>();

            for (int i = 0; i < initialSize; i++)
            {
                _objects.Add(new NetDataReader());
            }
        }

        /// <summary>
        /// Removes NetDataReader from the pool and returns it
        /// If no NetDataReaders are left in the object pool, new one is created
        /// </summary>
        /// <returns>NetDataReader</returns>
        public NetDataReader Get()
        {
            if (!_objects.TryTake(out NetDataReader netDataReader))
            {
                netDataReader = new NetDataReader();
            }

            return netDataReader;
        }

        /// <summary>
        /// Returns NetDataReader back to the pool
        /// </summary>
        /// <param name="netDataReader">NetDataReader</param>
        public void Return(NetDataReader netDataReader)
        {
            netDataReader.Clear();
            _objects.Add(netDataReader);
        }
    }
}