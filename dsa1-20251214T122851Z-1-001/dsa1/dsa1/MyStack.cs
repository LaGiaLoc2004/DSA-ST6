using System;
using System.Collections.Generic;

namespace dsa1
{
    public class MyStack<T>
    {
        private List<T> items;

        public MyStack()
        {
            items = new List<T>();
        }

        public void Push(T item)
        {
            items.Add(item);
        }

        public T Pop()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Stack rỗng!");

            T item = items[items.Count - 1];
            items.RemoveAt(items.Count - 1);
            return item;
        }

        public T Peek()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Stack rỗng!");

            return items[items.Count - 1];
        }

        public bool IsEmpty()
        {
            return items.Count == 0;
        }

        public int Count()
        {
            return items.Count;
        }

        public void Clear()
        {
            items.Clear();
        }

        public void Display()
        {
            if (IsEmpty())
            {
                Console.WriteLine("Stack rỗng");
                return;
            }

            Console.Write("Đáy -> ");
            foreach (var item in items)
            {
                Console.Write($"{item} ");
            }
            Console.WriteLine("<- Đỉnh");
        }

        public T[] ToArray()
        {
            return items.ToArray();
        }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }
    }
}
