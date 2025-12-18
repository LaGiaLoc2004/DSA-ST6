using System;

public static class DataGenerator
{
    public static MyStack<int> TaoStackNgauNhien(int size)
    {
        MyStack<int> stack = new MyStack<int>(size);
        System.Random rand = new System.Random();
        int maxValue = 10000;

        for (int i = 0; i < size; i++)
        {
            stack.Push(rand.Next(0, maxValue + 1));
        }

        return stack;
    }

   
    public static MyStack<int> TaoStackTangDan(int size)
    {
        MyStack<int> stack = new MyStack<int>(size);

        // push ngược để đáy -> nhỏ, đỉnh -> lớn
        for (int i = size - 1; i >= 0; i--)
        {
            stack.Push(i);
        }

        return stack;
    }

    public static MyStack<int> TaoStackGiamDan(int size)
    {
        MyStack<int> stack = new MyStack<int>(size);

        // push xuôi để đỉnh là nhỏ nhất
        for (int i = 0; i < size; i++)
        {
            stack.Push(i);
        }

        return stack;
    }
}

