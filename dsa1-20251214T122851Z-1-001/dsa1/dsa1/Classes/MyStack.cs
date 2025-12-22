using System;

namespace dsa1
{
    public class Node<T>
    {
        public Node<T> Next;
        public T Data;

        public Node(T data)
        {
            this.Data = data;
            this.Next = null;
        }
    }

    public class MyStack<T>
    {
        private Node<T> top;
        private int count;

        public int Capacity { get; private set; }

        public MyStack(int capacity = 1000)
        {
            Capacity = capacity;
            top = null;
            count = 0;
        }
        public void Push(T item)
        {
            Node<T> n = new Node<T>(item);
            n.Next = top;
            top = n;
            count++;
        }

        public T Pop()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Stack rỗng!");

            Node<T> d = top;     
            T value = d.Data;    
            top = top.Next;      
            
            count--;
            return value;
        }

        public bool IsEmpty()
        {
            return top == null;
        }
        public T Peek()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Stack rỗng!");
            return top.Data;
        }

        public bool IsFull()
        {
            return false; 
        }

        public int Count()
        {
            return count;
        }

        public void Clear()
        {
            top = null;
            count = 0;
        }

        public T[] ToArray()
        {
            T[] arr = new T[count];
            Node<T> current = top;
            int i = 0;
            while (current != null)
            {
                arr[i] = current.Data;
                current = current.Next;
                i++;
            }
            return arr;
        }

        public bool Contains(T item)
        {
            Node<T> current = top;
            while (current != null)
            {
                if (Equals(current.Data, item)) return true;
                current = current.Next;
            }
            return false;
        }

        public void Display()
        {
            if (IsEmpty())
            {
                Console.WriteLine("Stack rỗng");
                return;
            }

            Console.Write("Đỉnh -> ");
            Node<T> current = top;
            while (current != null)
            {
                Console.Write($"{current.Data} ");
                current = current.Next;
            }
            Console.WriteLine("-> Đáy");
        }
    }
}
