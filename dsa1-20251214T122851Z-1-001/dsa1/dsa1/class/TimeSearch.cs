using System;
using dsa1;

public class TimeSearch: TimeAnalyzer
{
    private int size;     // số phần tử

    public TimeSearch(int n, int size) : base(n)
    {
        this.size = size;
    }

    public void Run()
    {
        Console.WriteLine("Thuật toán\tSố lần\tTB (ms)\t\tPhương sai\tĐộ lệch chuẩn");

        // LINEAR SEARCH
        PrintResult(
            "Linear",
            Measure(() =>
            {
                MyStack<int> stack = DataGenerator.TaoStackNgauNhien(size);
                int target = stack.Peek(); // worst-case

                var finder = new ThuatToanTimKiem.LinearSearchFinder<int>();
                var trav = new Interface.ArrayTraversable<int>(stack.ToArray());
                finder.Search(trav, target);
            })
        );

        // DFS
        PrintResult(
            "DFS",
            Measure(() =>
            {
                MyStack<int> stack = DataGenerator.TaoStackNgauNhien(size);

                var finder = new ThuatToanTimKiem.DFSFinder<int>();
                var trav = new Interface.ArrayTraversable<int>(stack.ToArray());
                finder.Search(trav, _ => { });
            })
        );

        // BFS 
        PrintResult(
            "BFS",
            Measure(() =>
            {
                MyStack<int> stack = DataGenerator.TaoStackNgauNhien(size);

                var finder = new ThuatToanTimKiem.BFSFinder<int>();
                var trav = new Interface.ArrayTraversable<int>(stack.ToArray());
                finder.Search(trav, _ => { });
            })
        );
    }
}

