using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace dsa1
{
    public partial class Form1 : Form
    {
        // 3 cọc: 0 = A, 1 = B, 2 = C
        private List<int>[] pegs;
        // Danh sách bước di chuyển để animation
        private List<Move> moves;
        private int currentMoveIndex;
        private int numberOfDisks;
        private Timer animationTimer;

        public Form1()
        {
            InitializeComponent();

            // Khởi tạo mảng 3 cọc
            pegs = new List<int>[3];
            for (int i = 0; i < 3; i++)
                pegs[i] = new List<int>();

            moves = new List<Move>();
            currentMoveIndex = 0;
            numberOfDisks = 3;

            // Timer cho animation
            animationTimer = new Timer();
            animationTimer.Interval = 500;   // 500ms / bước
            animationTimer.Tick += AnimationTimer_Tick;

            // Gắn event
            this.Load += Form1_Load;
            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            // pnlDraw_Paint đã được gắn sẵn trong Designer
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Giá trị mặc định
            nudDisks.Value = 3;
            InitPegs(3);
        }

        // Khởi tạo trạng thái 3 cọc với n đĩa trên cọc A
        private void InitPegs(int n)
        {
            numberOfDisks = n;

            for (int i = 0; i < 3; i++)
                pegs[i].Clear();

            // Đĩa lớn ở dưới, nhỏ ở trên: n, n-1, ..., 1
            for (int i = n; i >= 1; i--)
            {
                pegs[0].Add(i);
            }

            currentMoveIndex = 0;
            pnlDraw.Invalidate();
        }

        // Nút "Bắt đầu"
        private void BtnStart_Click(object sender, EventArgs e)
        {
            int n = (int)nudDisks.Value;

            InitPegs(n);

            // Gọi thuật toán tháp Hà Nội dùng stack
            HanoiUsingStack hanoi = new HanoiUsingStack();
            hanoi.SolveIterative(n);

            moves = hanoi.Moves;

            // Hiển thị log bước di chuyển
            lstSteps.Items.Clear();
            foreach (var s in hanoi.Steps)
            {
                lstSteps.Items.Add(s);
            }

            currentMoveIndex = 0;

            if (moves.Count == 0)
            {
                MessageBox.Show("Không có bước di chuyển nào.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            animationTimer.Start();
        }

        // Nút "Dừng"
        private void BtnStop_Click(object sender, EventArgs e)
        {
            animationTimer.Stop();
        }

        // Timer tick: thực hiện từng bước di chuyển
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (currentMoveIndex >= moves.Count)
            {
                animationTimer.Stop();
                return;
            }

            Move m = moves[currentMoveIndex];

            int fromIndex = PegNameToIndex(m.From);
            int toIndex = PegNameToIndex(m.To);

            if (fromIndex < 0 || toIndex < 0)
            {
                currentMoveIndex++;
                return;
            }

            // Thực hiện di chuyển trên dữ liệu pegs
            if (pegs[fromIndex].Count > 0)
            {
                int disk = pegs[fromIndex][pegs[fromIndex].Count - 1];
                pegs[fromIndex].RemoveAt(pegs[fromIndex].Count - 1);
                pegs[toIndex].Add(disk);
            }

            currentMoveIndex++;
            pnlDraw.Invalidate();
        }

        private int PegNameToIndex(char name)
        {
            switch (name)
            {
                case 'A': return 0;
                case 'B': return 1;
                case 'C': return 2;
                default: return -1;
            }
        }

        // Vẽ 3 cọc + các đĩa trên mỗi cọc
        private void pnlDraw_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = pnlDraw.ClientSize.Width;
            int h = pnlDraw.ClientSize.Height;

            // Nền
            g.Clear(Color.White);

            int baseY = h - 50;      // mặt "đất"
            int pegHeight = h - 120; // chiều cao cọc
            int pegWidth = 10;
            int diskHeight = 20;
            int marginBottom = 10;

            // Vị trí 3 cọc A, B, C theo chiều ngang
            int pegAX = w / 6;
            int pegBX = w / 2;
            int pegCX = 5 * w / 6;

            // Vẽ cọc
            DrawPeg(g, pegAX, baseY, pegWidth, pegHeight, "A");
            DrawPeg(g, pegBX, baseY, pegWidth, pegHeight, "B");
            DrawPeg(g, pegCX, baseY, pegWidth, pegHeight, "C");

            if (numberOfDisks <= 0) return;

            int maxDiskWidth = w / 4;
            int minDiskWidth = 40;

            for (int pegIndex = 0; pegIndex < 3; pegIndex++)
            {
                List<int> peg = pegs[pegIndex];
                int centerX = (pegIndex == 0) ? pegAX : (pegIndex == 1 ? pegBX : pegCX);

                for (int i = 0; i < peg.Count; i++)
                {
                    int disk = peg[i];

                    int diskWidth;
                    if (numberOfDisks == 1)
                    {
                        diskWidth = maxDiskWidth;
                    }
                    else
                    {
                        diskWidth = minDiskWidth +
                                    (disk - 1) * (maxDiskWidth - minDiskWidth) / (numberOfDisks - 1);
                    }

                    int diskX = centerX - diskWidth / 2;
                    int diskY = baseY - (i + 1) * diskHeight - marginBottom;

                    Color diskColor = GetDiskColor(disk);

                    using (Brush br = new SolidBrush(diskColor))
                    using (Pen pen = new Pen(Color.Black, 1))
                    {
                        g.FillRectangle(br, diskX, diskY, diskWidth, diskHeight);
                        g.DrawRectangle(pen, diskX, diskY, diskWidth, diskHeight);
                    }

                    // Vẽ số đĩa trên mặt
                    string text = disk.ToString();
                    SizeF textSize = g.MeasureString(text, this.Font);
                    float textX = diskX + (diskWidth - textSize.Width) / 2;
                    float textY = diskY + (diskHeight - textSize.Height) / 2;
                    g.DrawString(text, this.Font, Brushes.White, textX, textY);
                }
            }
        }

        private void DrawPeg(Graphics g, int centerX, int baseY, int pegWidth, int pegHeight, string label)
        {
            int x = centerX - pegWidth / 2;
            int y = baseY - pegHeight;

            using (Brush br = new SolidBrush(Color.SaddleBrown))
            {
                g.FillRectangle(br, x, y, pegWidth, pegHeight);
            }

            // Chữ A/B/C dưới cọc
            SizeF textSize = g.MeasureString(label, this.Font);
            float textX = centerX - textSize.Width / 2;
            float textY = baseY + 5;
            g.DrawString(label, this.Font, Brushes.Black, textX, textY);
        }

        private Color GetDiskColor(int disk)
        {
            Color[] colors = new Color[]
            {
                Color.SteelBlue,
                Color.CadetBlue,
                Color.Coral,
                Color.IndianRed,
                Color.MediumSeaGreen,
                Color.MediumPurple,
                Color.DarkOrange,
                Color.Teal
            };

            return colors[(disk - 1) % colors.Length];
        }
    }
}
