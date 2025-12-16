using System;

namespace dsa1
{
    public class MyStack<T>
    {
        private T[] items;
        private int top;

        public int Capacity { get; private set; }

        public MyStack(int capacity = 1000)
        {
            if (capacity <= 0) capacity = 1;
            Capacity = capacity;
            items = new T[Capacity];
            top = -1;
        }

        public void Push(T item)
        {
            if (IsFull())
                throw new InvalidOperationException("Stack đầy!");

            items[++top] = item;
        }

        public T Pop()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Stack rỗng!");

            T value = items[top];
            items[top] = default(T); // dọn tham chiếu
            top--;
            return value;
        }

        public T Peek()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Stack rỗng!");

            return items[top];
        }

        public bool IsEmpty()
        {
            return top == -1;
        }

        public bool IsFull()
        {
            return top == Capacity - 1;
        }

        public int Count()
        {
            return top + 1;
        }

        public void Clear()
        {
            // reset nhanh
            Array.Clear(items, 0, top + 1);
            top = -1;
        }

        // Các hàm phụ (không bắt buộc nhưng giữ để bạn dùng/debug)
        public T[] ToArray()
        {
            T[] arr = new T[Count()];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = items[i];
            return arr;
        }

        public bool Contains(T item)
        {
            for (int i = 0; i <= top; i++)
            {
                if (Equals(items[i], item)) return true;
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

            Console.Write("Đáy -> ");
            for (int i = 0; i <= top; i++)
                Console.Write($"{items[i]} ");
            Console.WriteLine("<- Đỉnh");
        }
    }
}