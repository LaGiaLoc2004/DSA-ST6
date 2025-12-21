using System;
using System.Collections.Generic;
using dsa1;


public class StableItem : IComparable<StableItem>
{
    public int Value { get; set; }
    public int OriginalIndex { get; set; }

    // Thêm dấu ? sau StableItem để cho phép null
    public int CompareTo(StableItem other)
    {
        if (other == null) return 1; // Mặc định đối tượng hiện tại lớn hơn null
        return Value.CompareTo(other.Value);
    }
}

public class StabilityEvaluation
{
    private static Random rnd = new Random();

    // Tạo stack test (500 phần tử)
    private static MyStack<StableItem> CreateTestStack()
    {
        int total = 500;
        int sameValueCount = 400;
        int otherCount = 100;

        List<StableItem> list = new List<StableItem>(total);
        int index = 0;

        // 400 phần tử có Value = 1
        for (int i = 0; i < sameValueCount; i++)
        {
            list.Add(new StableItem
            {
                Value = 1,
                OriginalIndex = index++
            });
        }

        // 100 phần tử còn lại (khác 1)
        for (int i = 0; i < otherCount; i++)
        {
            int v;
            do
            {
                v = rnd.Next(2, 10); // đảm bảo != 1
            } while (v == 1);

            list.Add(new StableItem
            {
                Value = v,
                OriginalIndex = index++
            });
        }

        // SHUFFLE – Fisher–Yates
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        // Đưa vào stack
        MyStack<StableItem> stack = new MyStack<StableItem>(total);
        foreach (var item in list)
            stack.Push(item);

        return stack;
    }


    // Kiểm tra ổn định 1 lần
    private static bool IsStable(MyStack<StableItem> stack)
    {
        var list = new List<StableItem>();

        while (!stack.IsEmpty())
            list.Add(stack.Pop());

        list.Reverse();

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

        Console.WriteLine("ĐÁNH GIÁ ĐỘ ỔN ĐỊNH (CHẠY N LẦN)");
        Console.WriteLine($"Số lần kiểm tra: {n}");
        Console.WriteLine();
        Console.WriteLine($"Insertion Sort : {(ins ? "Ổn định" : "Không ổn định")}");
        Console.WriteLine($"Selection Sort : {(sel ? "Ổn định" : "Không ổn định")}");
        Console.WriteLine($"Merge Sort     : {(mer ? "Ổn định" : "Không ổn định")}");
    }
}
