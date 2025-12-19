using System;
using dsa1;

public class TimeSort : TimeAnalyzer
{
    private int size;

    public TimeSort(int n, int size) : base(n)
    {
        this.size = size;
    }

    public void RunAll()
    {
        RunCase("NGẪU NHIÊN", DataGenerator.TaoStackNgauNhien);
        RunCase("TĂNG DẦN", DataGenerator.TaoStackTangDan);
        RunCase("GIẢM DẦN", DataGenerator.TaoStackGiamDan);
    }

    private void RunCase(string title, Func<int, MyStack<int>> generator)
    {
        Console.WriteLine($"\nTRẠNG THÁI: {title}");
        Console.WriteLine("Thuật toán\tSố lần\tTB (ms)\t\tPhương sai\tĐộ lệch chuẩn");

        PrintResult(
            "Insert",
            Measure(() =>
            {
                var s = generator(size);
                ThuanToanSapXep.InsertionSort(s);
            })
        );

        PrintResult(
            "Select",
            Measure(() =>
            {
                var s = generator(size);
                ThuanToanSapXep.SelectionSort(s);
            })
        );

        PrintResult(
            "Merge",
            Measure(() =>
            {
                var s = generator(size);
                ThuanToanSapXep.MergeSort(s);
            })
        );
    }
}
