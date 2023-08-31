using System.Collections.Generic;

namespace LibMMD.Util
{
    public class SynchronizedQueue<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();

        public void Enqueue(T val)
        {
            lock (_queue)
            {
                _queue.Enqueue(val);
            }
        }

        public T Take()
        {
            lock (_queue)
            {
                return _queue.Count <= 0 ? default(T) : _queue.Dequeue();
            }
        }

        public int Count()
        {
            lock (_queue)
            {
                return _queue.Count;
            }
        }
    }
}