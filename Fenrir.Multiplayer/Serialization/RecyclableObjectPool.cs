using System;
using System.Collections.Concurrent;

namespace Fenrir.Multiplayer.Serialization
{
    /// <summary>
    /// Recyclable object pool
    /// Object pool of objects that implement <seealso cref="IRecyclable"/>
    /// When object is requested from a pool, it looks at the current size of the pool.
    /// If no objects in the pool left, creates a new one.
    /// When object is returned to the pool, <seealso cref="IRecyclable.Recycle"/> is called and object is reset to it's original state
    /// 
    /// This class is thread-safe.
    /// </summary>
    /// <typeparam name="T">Type of the object to pool</typeparam>
    class RecyclableObjectPool<T>
        where T : IRecyclable
    {
        /// <summary>
        /// Collection of object currently available in the pool
        /// </summary>
        private readonly ConcurrentBag<T> _objects;
        
        /// <summary>
        /// Factory method used to create a new object
        /// </summary>
        private readonly Func<T> _factoryMethod;

        /// <summary>
        /// Default constructor
        /// </summary>
        public RecyclableObjectPool(Func<T> factoryMethod) : this(0)
        {
            if(factoryMethod == null)
            {
                throw new ArgumentNullException(nameof(factoryMethod));
            }

            _factoryMethod = factoryMethod;
        }

        /// <summary>
        /// Creates object pool of a given size
        /// </summary>
        /// <param name="initialSize">Initial size</param>
        public RecyclableObjectPool(int initialSize)
        {
            _objects = new ConcurrentBag<T>();

            for(int i=0; i < initialSize; i++)
            {
                T obj = _factoryMethod.Invoke();
                _objects.Add(obj);
            }
        }

        /// <summary>
        /// Returns object from the pool
        /// </summary>
        /// <returns>Object from the pool</returns>
        public T Get()
        {
            if(!_objects.TryTake(out T obj))
            {
                obj = _factoryMethod.Invoke();
            }

            return obj;
        }

        /// <summary>
        /// Returns object to the given pool. Object is recycled before it's made available to other consumers.
        /// </summary>
        /// <param name="obj"></param>
        public void Return(T obj)
        {
            obj.Recycle();

            _objects.Add(obj);
        }
    }
}
