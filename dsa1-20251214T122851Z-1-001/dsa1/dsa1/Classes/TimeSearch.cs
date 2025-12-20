using System;
using dsa1;

public class TimeSearch : TimeAnalyzer
{
    private int size;

    public TimeSearch(int n, int size) : base(n) { this.size = size; }

    public void Run()
    {
        Console.WriteLine("\nTRẠNG THÁI: TÌM KIẾM NGẪU NHIÊN");
        Console.WriteLine("Thuật toán\tSố lần\tTB (ms)\t\tPhương sai\tĐộ lệch chuẩn");

        // 1. LINEAR SEARCH
        PrintResult("Linear", Measure(() => {
            var stack = DataGenerator.TaoStackNgauNhien(size);
            int target = -1; // Giả lập tìm giá trị không có để đo worst-case
            var finder = new ThuatToanTimKiem.LinearSearchFinder<int>();
            var trav = new ArrayTraversable<int>(stack.ToArray());
            finder.Search(trav, target);
        }));

        // 2. DFS
        PrintResult("DFS", Measure(() => {
            var stack = DataGenerator.TaoStackNgauNhien(size);
            int target = -1;
            var finder = new ThuatToanTimKiem.DFSFinder<int>();
            var trav = new ArrayTraversable<int>(stack.ToArray());
            finder.Search(trav, target);
        }));

        // 3. BFS 
        PrintResult("BFS", Measure(() => {
            var stack = DataGenerator.TaoStackNgauNhien(size);
            int target = -1;
            var finder = new ThuatToanTimKiem.BFSFinder<int>();
            var trav = new ArrayTraversable<int>(stack.ToArray());
            finder.Search(trav, target);
        }));
    }
}