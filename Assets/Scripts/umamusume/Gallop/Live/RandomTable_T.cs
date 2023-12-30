using System;

namespace Gallop.Live
{
    public class RandomTable<T>
    {
        private const int MAX_COUNT = 16;
        private const int OFFSET_RANDOM_BUCKET = 1;
        private bool _bInitialized;
        private Func<int, int, T> _fnGenerator;
        private int _bucketCount;
        private int _stackSize;
        private T[,] _arrRandomValue;
        private int _curStackPtr;

        public bool isInitialized { get; }
        public int bucketCount { get; }
        public int stackSize { get; }
        public T Item { get; }
    }
}
