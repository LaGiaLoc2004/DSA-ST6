using System;

public abstract class TimeAnalyzer
{
    protected int n;

    protected TimeAnalyzer(int n)
    {
        this.n = n;
    }

    // ĐO THỜI GIAN CHUNG
    protected double[] Measure(Action action)
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

    // THỐNG KÊ 
    protected double Mean(double[] data)
    {
        double sum = 0;
        foreach (double x in data) sum += x;
        return sum / data.Length;
    }

    protected double Variance(double[] data, double mean)
    {
        double sum = 0;
        foreach (double x in data)
            sum += Math.Pow(x - mean, 2);

        return sum / data.Length;
    }

    protected void PrintResult(string name, double[] times)
    {
        double avg = Mean(times);
        double var = Variance(times, avg);
        double std = Math.Sqrt(var);

        Console.WriteLine(
            $"{name}\t\t{times.Length}\t{avg:F4}\t\t{var:F4}\t\t{std:F4}"
        );
    }

}

