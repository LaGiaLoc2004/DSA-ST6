using dsa1;

public class ThuanToanSapXep
{
    // INSERTION SORT
    public static void InsertionSort<T>(MyStack<T> stack)
        where T : IComparable<T>
    {
        MyStack<T> sorted = new MyStack<T>(stack.Capacity);

        while (!stack.IsEmpty())
        {
            T temp = stack.Pop();

            while (!sorted.IsEmpty() &&
                   sorted.Peek().CompareTo(temp) > 0)
            {
                stack.Push(sorted.Pop());
            }

            sorted.Push(temp);
        }

        while (!sorted.IsEmpty())
            stack.Push(sorted.Pop());
    }

    //SELECTION SORT
    public static void SelectionSort<T>(MyStack<T> stack)
        where T : IComparable<T>
    {
        MyStack<T> result = new MyStack<T>(stack.Capacity);
        MyStack<T> tempStack = new MyStack<T>(stack.Capacity);

        while (!stack.IsEmpty())
        {
            T min = stack.Pop();
            tempStack.Push(min);

            while (!stack.IsEmpty())
            {
                T x = stack.Pop();
                tempStack.Push(x);

                if (x.CompareTo(min) < 0)
                    min = x;
            }

            bool removed = false;

            while (!tempStack.IsEmpty())
            {
                T x = tempStack.Pop();

                if (!removed && x.CompareTo(min) == 0)
                {
                    removed = true;
                    continue;
                }

                stack.Push(x);
            }

            result.Push(min);
        }

        while (!result.IsEmpty())
            stack.Push(result.Pop());
    }

    // MERGE SORT
    public static MyStack<T> MergeSort<T>(MyStack<T> stack)
        where T : IComparable<T>
    {
        if (stack.Count() <= 1)
            return stack;

        int mid = stack.Count() / 2;

        MyStack<T> left = new MyStack<T>(stack.Capacity);
        MyStack<T> right = new MyStack<T>(stack.Capacity);

        for (int i = 0; i < mid; i++)
            left.Push(stack.Pop());

        while (!stack.IsEmpty())
            right.Push(stack.Pop());

        left = MergeSort(left);
        right = MergeSort(right);

        return Merge(left, right);
    }

    private static MyStack<T> Merge<T>(MyStack<T> left, MyStack<T> right)
        where T : IComparable<T>
    {
        MyStack<T> temp = new MyStack<T>(left.Capacity + right.Capacity);
        MyStack<T> result = new MyStack<T>(left.Capacity + right.Capacity);

        while (!left.IsEmpty() && !right.IsEmpty())
        {
            if (left.Peek().CompareTo(right.Peek()) <= 0)
                temp.Push(left.Pop());
            else
                temp.Push(right.Pop());
        }

        while (!left.IsEmpty()) temp.Push(left.Pop());
        while (!right.IsEmpty()) temp.Push(right.Pop());

        while (!temp.IsEmpty())
            result.Push(temp.Pop());

        return result;
    }
}
