using System;
using System.Collections;

namespace LibMMD.Util
{
    using System.Threading;
    using System.Collections.Generic;

    public class BlockingQueue<T>
    {
        private int _count;

        private readonly Queue<T> _queue = new Queue<T>();

        private readonly int _maxSize;

        public BlockingQueue(int maxSize = int.MaxValue)
        {
            _maxSize = maxSize;
        }

        public int MaxSize
        {
            get { return _maxSize; }
        }

        public int Count
        {
            get
            {
                lock (_queue)
                {
                    return _count;
                }
            }
        }

        public T DequeueNonBlocking()
        {
            lock (_queue)
            {
                if (_count <= 0)
                {
                    return default(T);
                }

                _count--;

                var ret = _queue.Dequeue();
                Monitor.PulseAll(_queue);
                return ret;
            }
        }

        public T Dequeue()
        {
            lock (_queue)
            {
                // If we have items remaining in the queue, skip over this. 
                while (_count <= 0)
                {
                    Monitor.Wait(_queue);
                }

                _count--;

                var ret = _queue.Dequeue();
                Monitor.PulseAll(_queue);
                return ret;
            }
        }

        public int Enqueue(T data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data is null");
            }

            lock (_queue)
            {
                while (_count >= _maxSize)
                {
                    Monitor.Wait(_queue);
                }

                _queue.Enqueue(data);

                _count++;

                Monitor.PulseAll(_queue);
                return _count;
            }
        }
    }
}