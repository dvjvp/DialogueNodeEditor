

namespace System.Collections.Generic
{
	class LimitedSizeStack <T> : LinkedList<T>
	{
		private readonly int _maxSize;
		public LimitedSizeStack(int maxSize)
		{
			_maxSize = maxSize;
		}

		public void Push(T item)
		{
			AddFirst(item);

			if (Count > _maxSize)
			{
				RemoveLast();
			}
		}

		public T Pop()
		{
			var item = First.Value;
			RemoveFirst();
			return item;
		}

	}
}
