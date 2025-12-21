using System;
using dsa1;

public class ThuatToanTimKiem
{
    public class BFSFinder<T>
    {
        private MyStack<T> chieuVao = new MyStack<T>();
        private MyStack<T> chieuRa = new MyStack<T>();

        public bool Search(ITraversable<T> origin, T target)
        {
            // Đưa dữ liệu vào chieuVao
            while (origin.HasNext())
            {
                chieuVao.Push(origin.Next());
            }

            // BFS mô phỏng FIFO bằng 2 Stack
            while (!chieuVao.IsEmpty() || !chieuRa.IsEmpty())
            {
                if (chieuRa.IsEmpty())
                {
                    while (!chieuVao.IsEmpty())
                    {
                        chieuRa.Push(chieuVao.Pop());
                    }
                }

                T current = chieuRa.Pop();

                // Logic xác nhận target
                if (Equals(current, target))
                    return true;
            }
            return false;
        }
    }

    public class DFSFinder<T>
    {
        public bool Search(ITraversable<T> origin, T target)
        {
            MyStack<T> tempStack = new MyStack<T>();

            // Đưa dữ liệu vào stack tạm thời
            while (origin.HasNext())
            {
                tempStack.Push(origin.Next());
            }

            // Duyệt theo cơ chế LIFO (DFS)
            while (!tempStack.IsEmpty())
            {
                T current = tempStack.Pop();

                // Logic xác nhận target
                if (Equals(current, target))
                    return true;
            }
            return false;
        }
    }

    public class LinearSearchFinder<T>
    {
        public bool Search(ITraversable<T> origin, T target)
        {
            MyStack<T> tempStack = new MyStack<T>();

            // Đưa dữ liệu vào stack
            while (origin.HasNext())
            {
                tempStack.Push(origin.Next());
            }

            // Tìm kiếm
            while (!tempStack.IsEmpty())
            {
                if (Equals(tempStack.Pop(), target))
                    return true;
            }

            return false;
        }
    }
}