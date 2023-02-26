using System;

public class RandomTable<T>
{
    private const int MAX_COUNT = 16;

    private const int OFFSET_RANDOM_BUCKET = 1;

    private bool _bInitialized;

    private Func<int, int, T> _fnGenerator;

    private int _bucketCount;

    private int _stackSize;

    private T[,] _arrRandomValue = new T[0, 0];

    private int _curStackPtr;

    public bool isInitialized
    {
        get
        {
            return this._bInitialized;
        }
    }

    public int bucketCount
    {
        get
        {
            return this._bucketCount;
        }
    }

    public int stackSize
    {
        get
        {
            return this._stackSize;
        }
    }

    public T this[int idxBucket]
    {
        get
        {
            if (this._bInitialized)
            {
                return (idxBucket > 0 && idxBucket <= this._bucketCount) ? this._arrRandomValue[idxBucket - 1, this._curStackPtr] : default(T);
            }
            return default(T);
        }
    }

    public void Build(int bucketCount, int stackSize, Func<int, int, T> fnGenerator)
    {
        this._bucketCount = bucketCount - 1;
        this._stackSize = stackSize;
        this._fnGenerator = fnGenerator;
        this._arrRandomValue = new T[this._bucketCount, this._stackSize];
        for (int i = 0; i < this._bucketCount; i++)
        {
            for (int j = 0; j < this._stackSize; j++)
            {
                this._arrRandomValue[i, j] = this._fnGenerator(i + 1, j);
            }
        }
        this._bInitialized = true;
    }

    public void Update()
    {
        if (!this._bInitialized)
        {
            return;
        }
        this._curStackPtr++;
        this._curStackPtr %= this._stackSize;
    }
}
