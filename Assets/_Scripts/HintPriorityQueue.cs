using System.Collections.Generic;
using System.Linq;
using System;

public class HintPriorityQueue<T> where T : IPriority {
    private SortedDictionary<int, Queue<T>> dict = new();
    public int Count { get; private set; } = 0;

    public void Enqueue(T item) {
        if(!dict.ContainsKey(item.Priority)) {
            dict[item.Priority] = new Queue<T>();
        }
        Count++;
        dict[item.Priority].Enqueue(item);
    }

    public T Dequeue() {
        if (dict.Count == 0) throw new InvalidOperationException("Queue is empty");

        int highestPriority = dict.First().Key;
        Queue<T> queue = dict[highestPriority];

        T item = queue.Dequeue();
        if(queue.Count == 0) {
            dict.Remove(highestPriority);
        }
        Count--;
        return item;
    }

    public T Peek() {
        if (dict.Count == 0) throw new InvalidOperationException("Queue is empty");

        int highestPriority = dict.First().Key;
        Queue<T> queue = dict[highestPriority];

        T item = queue.Peek();
        return item;
    }

    public void Clear() {
        dict.Clear();
        Count = 0;
    }

    public int PriorityCount(int Priority) {
        return dict.ContainsKey(Priority) ? dict[Priority].Count : 0;
    }
}