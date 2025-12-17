using System;

namespace dsa1
{
    public class Move
    {
        public char From { get; set; }
        public char To { get; set; }
        public int Disk { get; set; }

        public Move(char from, char to, int disk)
        {
            From = from;
            To = to;
            Disk = disk;
        }

        public override string ToString()
        {
            return $"Di chuyển đĩa {Disk} từ {From} sang {To}";
        }
    }
}
