using System;

namespace Cutt
{
    public class DropOutStack<T> where T : class
    {
        private T[] Items;

        private int Top;

        public int Count;

        private int Capacity;

        public DropOutStack(int capacity)
        {
            Items = new T[capacity];
            Capacity = capacity;
        }

        public void Push(T item)
        {
            Items[Top] = item;
            Top = (Top + 1) % Capacity;
            Count = Math.Min(Count + 1, Capacity);
        }

        public T Pop()
        {
            Top = (Capacity + Top - 1) % Capacity;
            Count = Math.Max(Count - 1, 0);
            T result = Items[Top];
            Items[Top] = (T)null;
            return result;
        }

        public T At(int index)
        {
            if (index < Count)
            {
                int num = (Capacity + Top - 1) % Capacity;
                index = ((index <= num) ? (num - index) : (Capacity + (num - index)));
                return Items[index];
            }
            return (T)null;
        }

        public void Clear()
        {
            for (int i = 0; i < Capacity; i++)
            {
                Items[i] = (T)null;
            }
            Top = 0;
            Count = 0;
        }
    }
}
