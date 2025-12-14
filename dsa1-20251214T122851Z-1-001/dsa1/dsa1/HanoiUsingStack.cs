using System;
using System.Collections.Generic;

namespace dsa1
{
    public class HanoiUsingStack
    {
        private MyStack<int> src;   // Cọc A
        private MyStack<int> aux;   // Cọc B
        private MyStack<int> dest;  // Cọc C

        // Danh sách bước mô tả dạng text (dùng để hiển thị)
        public List<string> Steps { get; private set; } = new List<string>();

        // Danh sách bước chi tiết (từ cọc nào sang cọc nào, đĩa nào) dùng cho animation
        public List<Move> Moves { get; private set; } = new List<Move>();

        public HanoiUsingStack()
        {
            src = new MyStack<int>();
            aux = new MyStack<int>();
            dest = new MyStack<int>();
        }

        private void Init(int n)
        {
            src.Clear();
            aux.Clear();
            dest.Clear();
            Steps.Clear();
            Moves.Clear();

            // Đĩa lớn ở dưới, nhỏ ở trên (n, n-1, ..., 1)
            for (int i = n; i >= 1; i--)
            {
                src.Push(i);
            }
        }

        private void Log(string message)
        {
            Steps.Add(message);
        }

        private void AddMove(char from, char to, int disk)
        {
            Moves.Add(new Move(from, to, disk));
        }

        private void MoveDisk(MyStack<int> from, MyStack<int> to, char fromName, char toName)
        {
            if (from.IsEmpty() && to.IsEmpty())
                return;

            if (from.IsEmpty())
            {
                int disk = to.Pop();
                from.Push(disk);
                string msg = $"Di chuyển đĩa {disk} từ {toName} sang {fromName}";
                Log(msg);
                AddMove(toName, fromName, disk);
            }
            else if (to.IsEmpty())
            {
                int disk = from.Pop();
                to.Push(disk);
                string msg = $"Di chuyển đĩa {disk} từ {fromName} sang {toName}";
                Log(msg);
                AddMove(fromName, toName, disk);
            }
            else
            {
                int topFrom = from.Peek();
                int topTo = to.Peek();

                if (topFrom < topTo)
                {
                    int disk = from.Pop();
                    to.Push(disk);
                    string msg = $"Di chuyển đĩa {disk} từ {fromName} sang {toName}";
                    Log(msg);
                    AddMove(fromName, toName, disk);
                }
                else
                {
                    int disk = to.Pop();
                    from.Push(disk);
                    string msg = $"Di chuyển đĩa {disk} từ {toName} sang {fromName}";
                    Log(msg);
                    AddMove(toName, fromName, disk);
                }
            }
        }

        public void SolveIterative(int n)
        {
            if (n <= 0)
                throw new ArgumentException("Số đĩa phải > 0");

            Init(n);

            int totalMoves = (int)Math.Pow(2, n) - 1;
            Log($"Tổng số bước cần thực hiện: {totalMoves}");

            MyStack<int> s = src;   // source
            MyStack<int> a = aux;   // auxiliary
            MyStack<int> d = dest;  // destination

            char sName = 'A';
            char aName = 'B';
            char dName = 'C';

            // Nếu n chẵn, đổi vai trò B và C
            if (n % 2 == 0)
            {
                var tmpStack = d;
                d = a;
                a = tmpStack;

                char tmpName = dName;
                dName = aName;
                aName = tmpName;
            }

            for (int i = 1; i <= totalMoves; i++)
            {
                if (i % 3 == 1)
                {
                    MoveDisk(s, d, sName, dName);
                }
                else if (i % 3 == 2)
                {
                    MoveDisk(s, a, sName, aName);
                }
                else
                {
                    MoveDisk(a, d, aName, dName);
                }
            }

            Log("Hoàn thành.");
        }
    }
}
