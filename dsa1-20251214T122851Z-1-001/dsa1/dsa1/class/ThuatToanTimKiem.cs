using System;

public class ThuatToanTimKiem
{
	public class  BFSFinder<T>
	{
        private MyStack<T> chieuVao = new MyStack<T>();
        private MyStack<T> chieuRa = new MyStack<T>();

        public void Search(ITraversable<T> origin, Action<T> visit)
        {
            // Đưa dữ liệu vào chieuVao
            while (origin.HasNext())
            {
                chieuVao.Push(origin.Next());
            }

            // BFS mô phỏng FIFO
            while (!chieuVao.IsEmpty() || !chieuRa.IsEmpty())
            {
                if (chieuRa.IsEmpty())
                {
                    while (!chieuVao.IsEmpty())
                    {
                        chieuRa.Push(chieuVao.Pop());
                    }
                }

                visit(chieuRa.Pop());
            }
        }
    }

    public class DFSFinder<T>
    {
         public bool Search(ITraversable<T> origin, Action<T> visit)
         {
            MyStack<T> tempStack = new MyStack<T>();

            // Đưa dữ liệu vào stack tạm thời
            while (origin.HasNext())
            {
                tempStack.Push(origin.Next());
            }

            while (!tempStack.IsEmpty())
            {
                visit(tempStack.Pop());
            }
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
