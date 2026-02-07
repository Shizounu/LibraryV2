using System;
using System.Collections.Generic;

namespace Shizounu.Library.Utility
{
    
	public class PriorityQueue<T> where T : IComparable<T>
	{
		private List<T> heap = new List<T>();

		public int Count => heap.Count;

		public void Enqueue(T item)
		{
			heap.Add(item);
			HeapifyUp(heap.Count - 1);
		}

		public T Dequeue()
		{
			if (heap.Count == 0)
				throw new InvalidOperationException("Queue is empty");

			T result = heap[0];
			int lastIndex = heap.Count - 1;
			heap[0] = heap[lastIndex];
			heap.RemoveAt(lastIndex);

			if (heap.Count > 0)
				HeapifyDown(0);

			return result;
		}

		private void HeapifyUp(int index)
		{
			while (index > 0)
			{
				int parentIndex = (index - 1) / 2;
				if (heap[index].CompareTo(heap[parentIndex]) >= 0)
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

				if (leftChild < heap.Count && heap[leftChild].CompareTo(heap[smallest]) < 0)
					smallest = leftChild;

				if (rightChild < heap.Count && heap[rightChild].CompareTo(heap[smallest]) < 0)
					smallest = rightChild;

				if (smallest == index)
					break;

				Swap(index, smallest);
				index = smallest;
			}
		}

		private void Swap(int i, int j)
		{
            (heap[j], heap[i]) = (heap[i], heap[j]);
        }
    }
}