using System;

public class ThuanToanSapXep
{
	public static void InsertionSort(MyStack<int> stack)
	{
	    MyStack<int> sorted = new MyStack<int>(stack.Capacity);
	
	    while (!stack.IsEmpty())
	    {
	        int temp = stack.Pop();
	
	        // Dời các phần tử lớn hơn temp về stack gốc
	        while (!sorted.IsEmpty() && sorted.Peek() > temp)
	        {
	            stack.Push(sorted.Pop());
	        }
	
	        sorted.Push(temp);
	    }
	
	    // Đưa lại kết quả về stack ban đầu
	    while (!sorted.IsEmpty())
	        stack.Push(sorted.Pop());
	}

	public static void SelectionSort(MyStack<int> stack)
	{
	    MyStack<int> result = new MyStack<int>(stack.Capacity);
	    MyStack<int> tempStack = new MyStack<int>(stack.Capacity);
	
	    while (!stack.IsEmpty())
	    {
	        int min = stack.Pop();
	        tempStack.Push(min);
	
	        // Tìm min
	        while (!stack.IsEmpty())
	        {
	            int x = stack.Pop();
	            tempStack.Push(x);
	            if (x < min) min = x;
	        }
	
	        bool removed = false;
	
	        // Loại 1 lần min
	        while (!tempStack.IsEmpty())
	        {
	            int x = tempStack.Pop();
	            if (x == min && !removed)
	            {
	                removed = true;
	                continue;
	            }
	            stack.Push(x);
	        }
	
	        result.Push(min);
	    }
	
	    // Đưa lại về stack ban đầu
	    while (!result.IsEmpty())
	        stack.Push(result.Pop());
	}

	private static MyStack<int> Merge(MyStack<int> left, MyStack<int> right)
	{
	    MyStack<int> result = new MyStack<int>(left.Capacity + right.Capacity);
	    MyStack<int> temp = new MyStack<int>(left.Capacity + right.Capacity);
	
	    while (!left.IsEmpty() && !right.IsEmpty())
	    {
	        if (left.Peek() <= right.Peek())
	            temp.Push(left.Pop());
	        else
	            temp.Push(right.Pop());
	    }
	
	    while (!left.IsEmpty()) temp.Push(left.Pop());
	    while (!right.IsEmpty()) temp.Push(right.Pop());
	
	    // đảo lại để đúng thứ tự
	    while (!temp.IsEmpty())
	        result.Push(temp.Pop());
	
	    return result;
	}
	
	public static MyStack<int> MergeSort(MyStack<int> stack)
	{
	    if (stack.Count() <= 1)
	        return stack;
	
	    MyStack<int> left = new MyStack<int>(stack.Capacity);
	    MyStack<int> right = new MyStack<int>(stack.Capacity);
	
	    int mid = stack.Count() / 2;
	
	    for (int i = 0; i < mid; i++)
	        left.Push(stack.Pop());
	
	    while (!stack.IsEmpty())
	        right.Push(stack.Pop());
	
	    left = MergeSort(left);
	    right = MergeSort(right);
	
	    return Merge(left, right);
	}
}
