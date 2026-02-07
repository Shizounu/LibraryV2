using System;
using System.Collections.Generic;

namespace Shizounu.Library.Utility
{
    
	public class PriorityQueue<T> where T : IComparable<T>
	{
		private List<T> _heap = new List<T>();

		public int Count => _heap.Count;

		public void Enqueue(T item)
		{
			_heap.Add(item);
			HeapifyUp(_heap.Count - 1);
		}

		public T Dequeue()
		{
			if (_heap.Count == 0)
				throw new InvalidOperationException("Queue is empty");

			T result = _heap[0];
			int lastIndex = _heap.Count - 1;
			_heap[0] = _heap[lastIndex];
			_heap.RemoveAt(lastIndex);

			if (_heap.Count > 0)
				HeapifyDown(0);

			return result;
		}

		private void HeapifyUp(int index)
		{
			while (index > 0)
			{
				int parentIndex = (index - 1) / 2;
				if (_heap[index].CompareTo(_heap[parentIndex]) >= 0)
					break;

				Swap(index, parentIndex);
				index = parentIndex;
			}
		}

		private void HeapifyDown(int index)
		{
			while (true)
			{
				int leftChild = 2 * index + 1;
				int rightChild = 2 * index + 2;
				int smallest = index;

				if (leftChild < _heap.Count && _heap[leftChild].CompareTo(_heap[smallest]) < 0)
					smallest = leftChild;

				if (rightChild < _heap.Count && _heap[rightChild].CompareTo(_heap[smallest]) < 0)
					smallest = rightChild;

				if (smallest == index)
					break;

				Swap(index, smallest);
				index = smallest;
			}
		}

		private void Swap(int i, int j)
		{
            (_heap[j], _heap[i]) = (_heap[i], _heap[j]);
        }
    }
}