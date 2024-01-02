using System;
using System.Collections.Generic;

namespace CommandUndoRedo
{
	public class DropoutStack<T> : LinkedList<T>
	{
		int _maxLength = int.MaxValue;
		public int maxLength {get {return _maxLength;} set {SetMaxLength(value);}}

		public DropoutStack() {}
		public DropoutStack(int maxLength)
		{
			this.maxLength = maxLength;
		}

		public void Push(T item)
		{
			if(this.Count > 0 && this.Count + 1 > maxLength)
			{
				this.RemoveLast();
			}

			if(this.Count + 1 <= maxLength)
			{
				this.AddFirst(item);
			}
		}

		public T Pop()
		{
			T item = this.First.Value;
			this.RemoveFirst();
			return item;
		}

		void SetMaxLength(int max)
		{
			_maxLength = max;

			if(this.Count > _maxLength)
			{
				int leftover = this.Count - _maxLength;
				for(int i = 0; i < leftover; i++)
				{
					this.RemoveLast();
				}
			}
		}
	}
}
