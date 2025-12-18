using System;
using dsa1;

public class Time
{
    private int n;      // số lần đo
    private int size;   // số phần tử

    public Time(int n, int size)
    {
        this.n = n;
        this.size = size;
    }

    // HÀM CHẠY TỔNG
    public void RunAll()
    {
        RunCase("NGẪU NHIÊN",DataGenerator.TaoStackNgauNhien);
        RunCase("TĂNG DẦN", DataGenerator.TaoStackTangDan);
        RunCase("GIẢM DẦN", DataGenerator.TaoStackGiamDan);
    }

    private void RunCase(string title, Func<int, MyStack<int>> generator)
    {
        Console.WriteLine($"\nTRẠNG THÁI: {title}");
        Console.WriteLine("Thuật toán\tSố lần\tTB (ms)\t\tPhương sai\tĐộ lệch chuẩn");

        PrintResult(
            "Insert",
            Measure(n, () =>
            {
                var s = generator(size);
                ThuanToanSapXep.InsertionSort(s);
            })
        );

        PrintResult(
            "Select",
            Measure(n, () =>
            {
                var s = generator(size);
                ThuanToanSapXep.SelectionSort(s);
            })
        );

        PrintResult(
            "Merge",
            Measure(n, () =>
            {
                var s = generator(size);
                s = ThuanToanSapXep.MergeSort(s);
            })
        );
    }


    private double[] Measure(int n, Action action)
    {
        double[] times = new double[n];
        Timing timer = new Timing();

        for (int i = 0; i < n; i++)
        {
            timer.startTime();
            action();
            timer.StopTime();

            times[i] = timer.Result().TotalMilliseconds;
        }

        return times;
    }

    private void PrintResult(string name, double[] times)
    {
        double avg = Mean(times);
        double var = Variance(times, avg);
        double std = Math.Sqrt(var);

        Console.WriteLine(
            $"{name}\t\t{times.Length}\t{avg:F4}\t\t{var:F4}\t\t{std:F4}"
        );
    }

    private double Mean(double[] data)
    {
        double sum = 0;
        foreach (double x in data) sum += x;
        return sum / data.Length;
    }

    private double Variance(double[] data, double mean)
    {
        double sum = 0;
        foreach (double x in data)
            sum += Math.Pow(x - mean, 2);

        return sum / data.Length;
    }
}

