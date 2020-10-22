using System.Collections.Concurrent;

namespace Fenrir.Multiplayer.Serialization
{
    class RecyclableObjectPool<T>
        where T : IRecyclable, new()
    {

        private readonly ConcurrentBag<T> _objects;

        public RecyclableObjectPool() : this(0)
        {
        }

        public RecyclableObjectPool(int initialSize)
        {
            _objects = new ConcurrentBag<T>();

            for(int i=0; i < initialSize; i++)
            {
                _objects.Add(new T());
            }
        }

        public T Get()
        {
            if(!_objects.TryTake(out T byteStream))
            {
                byteStream = new T();
            }

            return byteStream;
        }

        public void Return(T byteStream)
        {
            byteStream.Recycle();

            _objects.Add(byteStream);
        }
    }
}
