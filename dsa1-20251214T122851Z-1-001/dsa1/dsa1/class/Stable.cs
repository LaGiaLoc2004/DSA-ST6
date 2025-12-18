using System;
using System.Collections.Generic;

public class StableItem : IComparable<StableItem>
{
    public int Value { get; set; }
    public int OriginalIndex { get; set; }

    public int CompareTo(StableItem other)
    {
        return Value.CompareTo(other.Value);
    }
}


public class StabilityEvaluation
{
    // Tạo stack test (500 phần tử)
    private static MyStack<StableItem> CreateTestStack()
    {
        int total = 500;
        int sameValueCount = 200;

        var stack = new MyStack<StableItem>(total);
        int index = 0;

        // 200 phần tử có Value = 1
        for (int i = 0; i < sameValueCount; i++)
        {
            stack.Push(new StableItem
            {
                Value = 1,
                OriginalIndex = index++
            });
        }

        // 50 phần tử khác 1
        Random rnd = new Random();

        for (int i = sameValueCount; i < 50; i++)
        {
            stack.Push(new StableItem
            {
                Value = rnd.Next(2, 10),
                OriginalIndex = index++
            });
        }

        // 200 phần tử có Value = 1
        for (int i = 0; i < sameValueCount; i++)
        {
            stack.Push(new StableItem
            {
                Value = 1,
                OriginalIndex = index++
            });
        }

        // 50 phần tử khác 1
        for (int i = sameValueCount; i < 50; i++)
        {
            stack.Push(new StableItem
            {
                Value = rnd.Next(2, 10),
                OriginalIndex = index++
            });
        }

        return stack;
    }

    // Kiểm tra ổn định 1 lần
    private static bool IsStable(MyStack<StableItem> stack)
    {
        var list = new List<StableItem>();

        while (!stack.IsEmpty())
            list.Add(stack.Pop());

        //list.Reverse();

        for (int i = 0; i < list.Count; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                if (list[i].Value == list[j].Value &&
                    list[i].OriginalIndex > list[j].OriginalIndex)
                    return false;
            }
        }

        return true;
    }

    // Chạy n lần cho từng thuật toán
    private static bool RunInsertionSort(int n)
    {
        for (int i = 0; i < n; i++)
        {
            var stack = CreateTestStack();
            ThuanToanSapXep.InsertionSort<StableItem>(stack);

            if (!IsStable(stack))
                return false;
        }
        return true;
    }

    private static bool RunSelectionSort(int n)
    {
        for (int i = 0; i < n; i++)
        {
            var stack = CreateTestStack();
            ThuanToanSapXep.SelectionSort<StableItem>(stack);

            if (!IsStable(stack))
                return false;
        }
        return true;
    }

    private static bool RunMergeSort(int n)
    {
        for (int i = 0; i < n; i++)
        {
            var stack = CreateTestStack();
            var sorted = ThuanToanSapXep.MergeSort<StableItem>(stack);

            if (!IsStable(sorted))
                return false;
        }
        return true;
    }


    public static void Evaluate(int n)
    {
        bool ins = RunInsertionSort(n);
        bool sel = RunSelectionSort(n);
        bool mer = RunMergeSort(n);

        Console.WriteLine("===== ĐÁNH GIÁ ĐỘ ỔN ĐỊNH (CHẠY N LẦN) =====");
        Console.WriteLine($"Số lần kiểm tra: {n}");
        Console.WriteLine();
        Console.WriteLine($"Insertion Sort : {(ins ? "Ổn định" : "Không ổn định")}");
        Console.WriteLine($"Selection Sort : {(sel ? "Ổn định" : "Không ổn định")}");
        Console.WriteLine($"Merge Sort     : {(mer ? "Ổn định" : "Không ổn định")}");
    }
}

