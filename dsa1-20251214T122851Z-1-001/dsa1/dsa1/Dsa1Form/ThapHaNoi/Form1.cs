using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics; 
using System.Linq; 

namespace dsa1
{
    public partial class Form1 : Form
    {
        // --- COMPONENTS ---
        private Panel pnlTop; private ComboBox cboAlgo;
        private Panel pnlLeft; private GroupBox grpInput;
        private Label lblN; private NumericUpDown nudN;
        private Label lblOrder; private ComboBox cboOrder; 
        private Label lblTarget; private NumericUpDown nudTarget;
        private Label lblRuns; private NumericUpDown nudRuns;
        private Button btnInit; private Button btnRun;
        private GroupBox grpOutput; private ListBox lstLog;
        private Panel pnlCenter; private Panel pnlStats;  
        private Label lblStatsTitle; private Label lblStatsContent;

        // --- DATA ---
        private MyStack<int> mainStack; 
        private int[] masterData; 
        
        // --- GRAPH DATA ---
        private class GraphNode {
            public int Value;
            public Point Position;
            public List<int> Neighbors = new List<int>(); 
        }
        private List<GraphNode> graphNodes; 
        private List<int> finalPath; // Lưu đường đi tìm được để vẽ
        
        // --- ANIMATION STATES ---
        private System.Windows.Forms.Timer animTimer;
        private string currentMode = "HANOI";

        // 1. Hanoi Vars
        private List<int>[] hanoiPegs; private List<Move> hanoiMoves; private int hanoiMoveIndex;
        
        // 2. Linear Search Vars
        private int searchCurrentIndex = -1; private bool isSearchFound = false;

        // 3. Graph Vars
        private struct GraphStep {
            public int CurrentNode;
            public HashSet<int> Visited;
            public List<int> Stack1;      
            public List<int> Stack2;
            public string LogMsg;
        }
        private List<GraphStep> graphSteps;
        private int graphStepIndex = 0;

        public Form1()
        {
            InitCustomGUI();
            animTimer = new System.Windows.Forms.Timer();
            animTimer.Interval = 200; // Tăng tốc độ theo yêu cầu (Nhanh hơn)
            animTimer.Tick += AnimTimer_Tick;
            cboAlgo.SelectedIndex = 0; 
        }

        // =================================================================================
        // PHẦN 1: GIAO DIỆN (UI)
        // =================================================================================
        private void InitCustomGUI()
        {
            this.Size = new Size(1350, 850);
            this.Text = "DSA Dashboard: Final Ultimate Version";
            this.StartPosition = FormStartPosition.CenterScreen;

            // TOP
            pnlTop = new Panel() { Dock = DockStyle.Top, Height = 60, BackColor = Color.LightBlue };
            Label lblTitle = new Label() { Text = "THUẬT TOÁN:", Location = new Point(20, 18), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            cboAlgo = new ComboBox() { Location = new Point(150, 15), Width = 300, Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cboAlgo.Items.AddRange(new string[] { "Bài toán: Tháp Hà Nội", "Sắp xếp: Insertion Sort", "Sắp xếp: Selection Sort", "Sắp xếp: Merge Sort", "Tìm kiếm: Linear Search", "Tìm kiếm: BFS (2 Stack)", "Tìm kiếm: DFS" });
            cboAlgo.SelectedIndexChanged += CboAlgo_SelectedIndexChanged;
            pnlTop.Controls.AddRange(new Control[] { lblTitle, cboAlgo });

            // LEFT
            pnlLeft = new Panel() { Dock = DockStyle.Left, Width = 290, BackColor = Color.AliceBlue, Padding = new Padding(10) };
            grpInput = new GroupBox() { Text = "CẤU HÌNH", Dock = DockStyle.Top, Height = 360, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            
            lblN = new Label() { Text = "Số lượng (N):", Location = new Point(15, 30), AutoSize = true, Font = new Font("Segoe UI", 9) };
            nudN = new NumericUpDown() { Location = new Point(15, 55), Width = 100, Minimum = 1, Maximum = 5000, Value = 10 };
            
            lblOrder = new Label() { Text = "Thứ tự sắp xếp:", Location = new Point(15, 90), AutoSize = true, Font = new Font("Segoe UI", 9) };
            cboOrder = new ComboBox() { Location = new Point(15, 115), Width = 120, Font = new Font("Segoe UI", 9), DropDownStyle = ComboBoxStyle.DropDownList };
            cboOrder.Items.Add("Tăng dần"); cboOrder.Items.Add("Giảm dần"); cboOrder.SelectedIndex = 0;
            
            lblTarget = new Label() { Text = "Tìm số:", Location = new Point(140, 30), AutoSize = true, Font = new Font("Segoe UI", 9), Visible = false };
            nudTarget = new NumericUpDown() { Location = new Point(140, 55), Width = 80, Minimum = 0, Maximum = 10000, Visible = false };
            
            lblRuns = new Label() { Text = "Số lần Test:", Location = new Point(15, 150), AutoSize = true, Font = new Font("Segoe UI", 9) };
            nudRuns = new NumericUpDown() { Location = new Point(15, 175), Width = 100, Minimum = 1, Maximum = 100000, Value = 1 };

            btnInit = new Button() { Text = "1. TẠO DỮ LIỆU", Location = new Point(15, 230), Width = 250, Height = 40, BackColor = Color.LemonChiffon, Cursor = Cursors.Hand };
            btnRun = new Button() { Text = "2. CHẠY THUẬT TOÁN", Location = new Point(15, 280), Width = 250, Height = 40, BackColor = Color.LightGreen, Cursor = Cursors.Hand };
            
            btnInit.Click += BtnInit_Click; btnRun.Click += BtnRun_Click;
            grpInput.Controls.AddRange(new Control[] { lblN, nudN, lblOrder, cboOrder, lblTarget, nudTarget, lblRuns, nudRuns, btnInit, btnRun });

            grpOutput = new GroupBox() { Text = "NHẬT KÝ (LOG)", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            lstLog = new ListBox() { Dock = DockStyle.Fill, Font = new Font("Consolas", 10, FontStyle.Regular), BorderStyle = BorderStyle.None, HorizontalScrollbar = true };
            grpOutput.Controls.Add(lstLog);
            pnlLeft.Controls.Add(grpOutput); pnlLeft.Controls.Add(grpInput); 

            // CENTER & STATS
            pnlCenter = new Panel() { Dock = DockStyle.Fill, BackColor = Color.White };
            pnlCenter.Paint += PnlCenter_Paint; pnlCenter.Resize += (s, e) => { pnlCenter.Invalidate(); }; 
            pnlStats = new Panel() { Size = new Size(320, 220), BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.FixedSingle, Visible = false };
            lblStatsTitle = new Label() { Text = "KẾT QUẢ ĐO LƯỜNG", Dock = DockStyle.Top, Height = 35, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.Firebrick, ForeColor = Color.White };
            lblStatsContent = new Label() { Text = "...", Dock = DockStyle.Fill, Padding = new Padding(15), Font = new Font("Consolas", 10, FontStyle.Regular) };
            pnlStats.Controls.Add(lblStatsContent); pnlStats.Controls.Add(lblStatsTitle);
            pnlCenter.Controls.Add(pnlStats);
            this.Controls.Add(pnlCenter); this.Controls.Add(pnlLeft); this.Controls.Add(pnlTop);
        }

        // =================================================================================
        // PHẦN 2: XỬ LÝ LOGIC
        // =================================================================================
        private void CboAlgo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = cboAlgo.SelectedItem.ToString();
            animTimer.Stop(); pnlStats.Visible = false; lstLog.Items.Clear(); pnlCenter.Invalidate();
            
            lblOrder.Visible = false; cboOrder.Visible = false; 
            lblTarget.Visible = false; nudTarget.Visible = false;
            lblRuns.Visible = true; nudRuns.Visible = true;

            if (selected.Contains("Tháp Hà Nội")) {
                currentMode = "HANOI"; lblN.Text = "Số đĩa (Max 8):"; nudN.Maximum = 8; nudN.Value = 3; 
                lblRuns.Visible = false; nudRuns.Visible = false;
            } else if (selected.Contains("Sắp xếp")) {
                currentMode = "SORT"; lblN.Text = "Số phần tử (Max 5000):"; nudN.Maximum = 5000; nudN.Value = 50; 
                lblOrder.Visible = true; cboOrder.Visible = true;
            } else if (selected.Contains("Linear Search")) {
                currentMode = "LINEAR"; lblN.Text = "Số phần tử (Max 30):"; nudN.Maximum = 30; nudN.Value = 10; 
                lblTarget.Visible = true; nudTarget.Visible = true;
            } else { // GRAPH
                currentMode = "GRAPH"; lblN.Text = "Số đỉnh (Max 25):"; nudN.Maximum = 25; nudN.Value = 15; 
                lblTarget.Visible = true; nudTarget.Visible = true;
            }
        }

        private void BtnInit_Click(object sender, EventArgs e)
        {
            int n = (int)nudN.Value; pnlStats.Visible = false; finalPath = null;
            
            if (currentMode == "HANOI") {
                hanoiPegs = new List<int>[3]; for(int i=0; i<3; i++) hanoiPegs[i]=new List<int>();
                for(int i=n; i>=1; i--) hanoiPegs[0].Add(i); hanoiMoves=new List<Move>(); 
                Log($"[HANOI] Đã đặt {n} đĩa.");
            }
            else if (currentMode == "GRAPH") {
                graphNodes = new List<GraphNode>(); if (n == 0) return; Random rnd = new Random();
                for (int i = 1; i <= n; i++) graphNodes.Add(new GraphNode { Value = i });
                
                // Kết nối cây
                for (int i = 1; i < n; i++) {
                    int parentIdx = rnd.Next(0, i);
                    graphNodes[parentIdx].Neighbors.Add(graphNodes[i].Value);
                    graphNodes[i].Neighbors.Add(graphNodes[parentIdx].Value);
                }
                
                // Layout phân tầng
                int w = pnlCenter.Width - 350; int startY = 60; int layerH = 90;
                var layers = new Dictionary<int, List<GraphNode>>();
                var q = new Queue<GraphNode>(); var visited = new HashSet<int>(); var depth = new Dictionary<int, int>();
                
                GraphNode root = graphNodes[0]; q.Enqueue(root); visited.Add(root.Value); depth[root.Value] = 0;
                while(q.Count > 0) {
                    var u = q.Dequeue(); int d = depth[u.Value];
                    if (!layers.ContainsKey(d)) layers[d] = new List<GraphNode>(); layers[d].Add(u);
                    foreach(var vId in u.Neighbors) {
                        if (!visited.Contains(vId)) { 
                            visited.Add(vId); depth[vId] = d+1; q.Enqueue(graphNodes.First(x=>x.Value==vId)); 
                        }
                    }
                }
                foreach(var d in layers.Keys) {
                    var nodes = layers[d];
                    int sectionW = w / (nodes.Count + 1);
                    for(int i=0; i<nodes.Count; i++) nodes[i].Position = new Point((i+1)*sectionW, startY + d*layerH);
                }
                Log($"[GRAPH] Đã tạo sơ đồ cây {n} đỉnh.");
            }
            else { 
                mainStack = DataGenerator.TaoStackNgauNhien(n); masterData = mainStack.ToArray();
                Log($"[DATA] Đã sinh {n} số ngẫu nhiên.");
            }
            pnlCenter.Invalidate();
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            // Đo lường trước (Benchmark)
            if (currentMode != "HANOI") RunBenchmark();

            // Sau đó chạy Animation
            if (currentMode == "GRAPH") {
                if (graphNodes == null) { MessageBox.Show("Tạo dữ liệu trước!"); return; }
                if (cboAlgo.SelectedItem.ToString().Contains("DFS")) StartDFS();
                else StartBFS_2Stacks(); 
            }
            else if (currentMode == "HANOI") StartHanoi();
            else if (currentMode == "LINEAR") StartLinearAnimation();
        }

        // =================================================================================
        // PHẦN 3: LOGIC & ANIMATION
        // =================================================================================
        
        // --- 3.1 DFS (1 Stack) ---
        private void StartDFS() {
            graphSteps = new List<GraphStep>(); graphStepIndex = 0; finalPath = null; int target = (int)nudTarget.Value;
            Stack<int> s = new Stack<int>(); HashSet<int> visited = new HashSet<int>();
            Dictionary<int, int> parent = new Dictionary<int, int>(); // Để truy vết đường đi

            int start = graphNodes[0].Value; s.Push(start);
            RecordGraphStep(-1, visited, s.ToList(), null, $"Khởi tạo: Push {start}");
            
            bool found = false;
            while(s.Count > 0) {
                int u = s.Pop();
                RecordGraphStep(u, visited, s.ToList(), null, $"Pop {u} xét");
                if(!visited.Contains(u)) {
                    visited.Add(u);
                    RecordGraphStep(u, visited, s.ToList(), null, $"Duyệt {u}" + (u==target?" -> TÌM THẤY!":""));
                    
                    if(u==target) { 
                        found=true; 
                        // Truy vết đường đi
                        finalPath = new List<int>(); int curr = target;
                        while(curr != start && parent.ContainsKey(curr)) { finalPath.Add(curr); curr = parent[curr]; }
                        finalPath.Add(start); finalPath.Reverse();
                        break; 
                    }
                    var neighbors = graphNodes.First(n=>n.Value==u).Neighbors.OrderByDescending(x=>x).ToList();
                    foreach(var v in neighbors) if(!visited.Contains(v)) { 
                        s.Push(v); 
                        if(!parent.ContainsKey(v)) parent[v] = u; // Ghi nhận cha để vẽ đường
                        RecordGraphStep(u, visited, s.ToList(), null, $"Push {v}"); 
                    }
                }
            }
            if(!found) RecordGraphStep(-1, visited, s.ToList(), null, "Không tìm thấy.");
            animTimer.Start();
        }

        // --- 3.2 BFS (2 STACKS) ---
        private void StartBFS_2Stacks() {
            graphSteps = new List<GraphStep>(); graphStepIndex = 0; finalPath = null; int target = (int)nudTarget.Value;
            
            Stack<int> sIn = new Stack<int>(); Stack<int> sOut = new Stack<int>();
            HashSet<int> visited = new HashSet<int>();
            Dictionary<int, int> parent = new Dictionary<int, int>();

            int start = graphNodes[0].Value; sIn.Push(start); visited.Add(start);
            RecordGraphStep(-1, visited, sIn.ToList(), sOut.ToList(), $"BFS Start: Push {start} vào IN");

            bool found = false;
            while(sIn.Count > 0 || sOut.Count > 0) {
                if (sOut.Count == 0) {
                    RecordGraphStep(-1, visited, sIn.ToList(), sOut.ToList(), "OUT rỗng -> Đổ IN sang OUT...");
                    while(sIn.Count > 0) {
                        int val = sIn.Pop(); sOut.Push(val);
                        RecordGraphStep(-1, visited, sIn.ToList(), sOut.ToList(), $"Chuyển {val} sang OUT");
                    }
                }
                int u = sOut.Pop();
                RecordGraphStep(u, visited, sIn.ToList(), sOut.ToList(), $"Pop {u} từ OUT (Dequeue)");
                
                if (u == target) {
                    RecordGraphStep(u, visited, sIn.ToList(), sOut.ToList(), $"-> TÌM THẤY {u}!");
                    found = true;
                    finalPath = new List<int>(); int curr = target;
                    while(curr != start && parent.ContainsKey(curr)) { finalPath.Add(curr); curr = parent[curr]; }
                    finalPath.Add(start); finalPath.Reverse();
                    break;
                }

                var neighbors = graphNodes.First(n=>n.Value==u).Neighbors.OrderBy(x=>x).ToList();
                foreach(var v in neighbors) {
                    if (!visited.Contains(v)) {
                        visited.Add(v); sIn.Push(v); parent[v] = u;
                        RecordGraphStep(u, visited, sIn.ToList(), sOut.ToList(), $"Push {v} vào IN");
                    }
                }
            }
            if(!found) RecordGraphStep(-1, visited, sIn.ToList(), sOut.ToList(), "Không tìm thấy.");
            animTimer.Start();
        }

        private void RecordGraphStep(int cur, HashSet<int> vis, List<int> s1, List<int> s2, string msg) {
            var step = new GraphStep { CurrentNode = cur, Visited = new HashSet<int>(vis), Stack1 = s1, LogMsg = msg };
            if (s2 != null) step.Stack2 = s2;
            graphSteps.Add(step);
        }

        private void StartHanoi() {
            int n = (int)nudN.Value;
            HanoiUsingStack s = new HanoiUsingStack(); s.SolveIterative(n);
            hanoiMoves = s.Moves; foreach(var st in s.Steps) Log(st);
            hanoiMoveIndex = 0; animTimer.Start();
        }
        private void StartLinearAnimation() { searchCurrentIndex = 0; isSearchFound = false; animTimer.Start(); }

        private void RunBenchmark() {
            int runs = (int)nudRuns.Value; string algo = cboAlgo.SelectedItem.ToString(); int target = (int)nudTarget.Value;
            List<double> times = new List<double>();
            
            // Logic đo lường (Chạy ngầm N lần)
            for(int i=0; i<runs; i++) {
                Stopwatch sw = Stopwatch.StartNew();
                if (currentMode == "SORT") {
                     MyStack<int> tmp = new MyStack<int>(masterData.Length); foreach(var x in masterData) tmp.Push(x);
                     if (algo.Contains("Insertion")) ThuanToanSapXep.InsertionSort(tmp);
                     else if (algo.Contains("Selection")) ThuanToanSapXep.SelectionSort(tmp);
                     else if (algo.Contains("Merge")) tmp = ThuanToanSapXep.MergeSort(tmp);
                }
                else if (currentMode == "LINEAR") {
                     var v = new ArrayTraversable<int>(masterData);
                     new ThuatToanTimKiem.LinearSearchFinder<int>().Search(v, target);
                }
                else if (currentMode == "GRAPH") {
                     // Đo DFS/BFS thuần túy (không animation)
                     if (graphNodes == null || graphNodes.Count == 0) break;
                     int start = graphNodes[0].Value;
                     if (algo.Contains("DFS")) SimpleDFS(start, target);
                     else SimpleBFS(start, target);
                }
                sw.Stop(); times.Add(sw.ElapsedTicks / 10.0);
            }

            double avg = times.Count > 0 ? times.Average() : 0;
            double var = times.Count > 1 ? times.Sum(t=>Math.Pow(t-avg,2))/(times.Count-1) : 0;
            lblStatsContent.Text = $"Số lần thử: {runs}\nTB: {avg:F2} µs\nMin: {(times.Count>0?times.Min():0):F2} µs\nMax: {(times.Count>0?times.Max():0):F2} µs\nSD: {Math.Sqrt(var):F2}";
            pnlStats.Location = new Point(pnlCenter.Width - pnlStats.Width - 10, 10); pnlStats.Visible = true; pnlStats.BringToFront();

            // Sắp xếp lại mainStack nếu cần
            if (currentMode == "SORT") {
                mainStack = new MyStack<int>(masterData.Length); foreach(var x in masterData) mainStack.Push(x);
                if (algo.Contains("Insertion")) ThuanToanSapXep.InsertionSort(mainStack);
                else if (algo.Contains("Selection")) ThuanToanSapXep.SelectionSort(mainStack);
                else if (algo.Contains("Merge")) mainStack = ThuanToanSapXep.MergeSort(mainStack);
                bool isDesc = cboOrder.Visible && cboOrder.SelectedIndex == 1;
                if (isDesc) { int[] arr = mainStack.ToArray(); Array.Reverse(arr); mainStack = new MyStack<int>(arr.Length); foreach(var x in arr) mainStack.Push(x); }
                pnlCenter.Invalidate();
            }
        }

        // Helper cho Benchmark Graph (Không animation)
        private void SimpleDFS(int start, int target) {
            Stack<int> s = new Stack<int>(); HashSet<int> v = new HashSet<int>(); s.Push(start);
            while(s.Count > 0) {
                int u = s.Pop();
                if(!v.Contains(u)) {
                    v.Add(u); if(u == target) return;
                    var nb = graphNodes.First(n=>n.Value==u).Neighbors;
                    foreach(var k in nb) if(!v.Contains(k)) s.Push(k);
                }
            }
        }
        private void SimpleBFS(int start, int target) {
             // BFS 2 Stack logic thuần
             Stack<int> sIn = new Stack<int>(); Stack<int> sOut = new Stack<int>(); HashSet<int> v = new HashSet<int>();
             sIn.Push(start); v.Add(start);
             while(sIn.Count > 0 || sOut.Count > 0) {
                 if(sOut.Count == 0) while(sIn.Count > 0) sOut.Push(sIn.Pop());
                 int u = sOut.Pop(); if(u == target) return;
                 var nb = graphNodes.First(n=>n.Value==u).Neighbors;
                 foreach(var k in nb) if(!v.Contains(k)) { v.Add(k); sIn.Push(k); }
             }
        }

        private void AnimTimer_Tick(object sender, EventArgs e) {
            if (currentMode == "GRAPH") {
                if(graphSteps!=null && graphStepIndex < graphSteps.Count) { Log(graphSteps[graphStepIndex].LogMsg); graphStepIndex++; pnlCenter.Invalidate(); }
                else animTimer.Stop();
            } else if (currentMode == "HANOI") {
                if(hanoiMoves!=null && hanoiMoveIndex < hanoiMoves.Count) { 
                    Move m = hanoiMoves[hanoiMoveIndex++]; 
                    int f=(m.From=='A')?0:(m.From=='B')?1:2; int t=(m.To=='A')?0:(m.To=='B')?1:2; 
                    hanoiPegs[t].Add(hanoiPegs[f][hanoiPegs[f].Count-1]); hanoiPegs[f].RemoveAt(hanoiPegs[f].Count-1); 
                    pnlCenter.Invalidate(); 
                } else animTimer.Stop();
            } else if (currentMode == "LINEAR") {
                int[] arr = mainStack.ToArray(); int t = (int)nudTarget.Value;
                if(searchCurrentIndex >= arr.Length) { Log("Ko thấy"); animTimer.Stop(); return; }
                if(arr[searchCurrentIndex] == t) { isSearchFound = true; Log($"Thấy tại {searchCurrentIndex}"); animTimer.Stop(); }
                else searchCurrentIndex++;
                pnlCenter.Invalidate();
            }
        }

        // =================================================================================
        // PHẦN 4: VẼ ĐỒ HỌA (RENDER)
        // =================================================================================
        private void PnlCenter_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; int w = pnlCenter.Width; int h = pnlCenter.Height;
            if (currentMode == "GRAPH") DrawGraph(g, w, h);
            else if (currentMode == "HANOI") DrawHanoi(g, w, h);
            else if (currentMode == "LINEAR") DrawLinear(g, w, h);
            else DrawSort(g, w, h);
        }

        private void DrawGraph(Graphics g, int w, int h) {
            if (graphNodes == null) return;
            GraphStep step = (graphSteps != null && graphStepIndex > 0) ? graphSteps[graphStepIndex - 1] : new GraphStep();
            
            // 1. Vẽ Dây
            using (Pen p = new Pen(Color.Gray, 2)) {
                var drawn = new HashSet<string>();
                foreach(var n in graphNodes) foreach(var nb in n.Neighbors) {
                    string k = n.Value < nb ? $"{n.Value}-{nb}" : $"{nb}-{n.Value}";
                    if(drawn.Add(k)) g.DrawLine(p, n.Position, graphNodes.First(x=>x.Value==nb).Position);
                }
            }
            
            // 2. VẼ ĐƯỜNG ĐI (PATH) NẾU HOÀN THÀNH
            if (!animTimer.Enabled && finalPath != null && finalPath.Count > 1) {
                using (Pen pPath = new Pen(Color.Blue, 4)) {
                    for(int i=0; i<finalPath.Count-1; i++) {
                         Point p1 = graphNodes.First(n=>n.Value==finalPath[i]).Position;
                         Point p2 = graphNodes.First(n=>n.Value==finalPath[i+1]).Position;
                         g.DrawLine(pPath, p1, p2);
                    }
                }
            }

            // 3. Vẽ Node
            foreach(var n in graphNodes) {
                Brush b = Brushes.White;
                if (step.Visited!=null && step.Visited.Contains(n.Value)) b = Brushes.LightGreen;
                if (n.Value == step.CurrentNode) b = Brushes.OrangeRed;
                // Nếu node đang nằm trong Stack nào đó thì tô vàng
                bool inS1 = step.Stack1!=null && step.Stack1.Contains(n.Value);
                bool inS2 = step.Stack2!=null && step.Stack2.Contains(n.Value);
                if (inS1 || inS2) b = Brushes.Gold;

                Rectangle r = new Rectangle(n.Position.X-20, n.Position.Y-20, 40, 40);
                g.FillEllipse(b, r); g.DrawEllipse(Pens.Black, r);
                var sz = g.MeasureString(n.Value.ToString(), this.Font);
                g.DrawString(n.Value.ToString(), new Font("Arial", 11, FontStyle.Bold), Brushes.Black, n.Position.X-sz.Width/2, n.Position.Y-sz.Height/2);
            }

            // 4. VẼ STACK VISUALIZATION
            bool isBFS = cboAlgo.SelectedItem.ToString().Contains("BFS");
            DrawStackBucket(g, w - 240, h - 50, "STACK IN", step.Stack1);
            if (isBFS) DrawStackBucket(g, w - 120, h - 50, "STACK OUT", step.Stack2);
        }

        private void DrawStackBucket(Graphics g, int x, int baseY, string title, List<int> items) {
            int w = 80; int h = 30;
            g.DrawString(title, new Font("Segoe UI", 10, FontStyle.Bold), Brushes.DarkBlue, x - w/2, 40);
            using(Pen p = new Pen(Color.Black, 3)) {
                g.DrawLine(p, x-w/2, 60, x-w/2, baseY); g.DrawLine(p, x+w/2, 60, x+w/2, baseY); 
                g.DrawLine(p, x-w/2, baseY, x+w/2, baseY); 
            }
            if(items == null) return;
            for(int i=0; i<items.Count; i++) {
                int val = items[items.Count - 1 - i]; // Vẽ ngược để đúng chất stack
                int y = baseY - (i + 1) * (h + 5);
                Rectangle r = new Rectangle(x - w/2 + 5, y, w - 10, h);
                g.FillRectangle(Brushes.Gold, r); g.DrawRectangle(Pens.Black, r);
                g.DrawString(val.ToString(), this.Font, Brushes.Black, x - 10, y + 5);
            }
        }

        private void DrawLinear(Graphics g, int w, int h) {
            if(mainStack==null) return; int[] arr = mainStack.ToArray();
            int sz = 50; int startX = (w - (arr.Length*sz + (arr.Length-1)*10))/2; if(startX<20) startX=20;
            for(int i=0; i<arr.Length; i++) {
                int x = startX + i*(sz+10); int y = h/2;
                Brush b = Brushes.White; if(i==searchCurrentIndex) b=Brushes.Yellow; 
                if(isSearchFound && i==searchCurrentIndex) b=Brushes.LightGreen;
                g.FillRectangle(b, x, y, sz, sz); g.DrawRectangle(Pens.Black, x, y, sz, sz);
                g.DrawString(arr[i].ToString(), new Font("Arial", 12), Brushes.Black, x+10, y+15);
            }
        }
        
        private void DrawSort(Graphics g, int w, int h) {
            if(mainStack==null) return; int[] arr = mainStack.ToArray(); int max = arr.Length>0?arr.Max():1;
            int dw = w - (pnlStats.Visible?350:50); float bw = (float)dw/arr.Length; float mh = h*0.85f;
            for(int i=0; i<arr.Length; i++) {
                float bh = ((float)arr[i]/max)*mh; if(bh<1) bh=1;
                float x = 20 + i*bw; float y = h-10-bh;
                if(bw>=2) { 
                    g.FillRectangle(Brushes.SteelBlue, x, y, bw, bh); 
                    if(bw>5) g.DrawRectangle(Pens.White, x, y, bw, bh); 
                    // HIỆN SỐ TRÊN ĐẦU CỘT
                    if(bw > 25) {
                         string t = arr[i].ToString(); var ms = g.MeasureString(t, this.Font);
                         g.DrawString(t, new Font("Arial", 8), Brushes.Black, x + (bw-ms.Width)/2, y - 15);
                    }
                }
                else g.DrawLine(Pens.SteelBlue, x, h-10, x, y);
            }
        }
        
        private void DrawHanoi(Graphics g, int w, int h) {
            if (hanoiPegs == null) return; int by = h - 50; int ph = h/2; int[] px = { w/6, w/2, 5*w/6 };
            for(int p=0;p<3;p++) {
                g.FillRectangle(Brushes.SaddleBrown, px[p]-5, by-ph, 10, ph); g.FillRectangle(Brushes.SaddleBrown, px[p]-60, by, 120, 10);
                for(int i=0; i<hanoiPegs[p].Count; i++) {
                    int d = hanoiPegs[p][i]; int dw = 40 + d*20;
                    int dx = px[p]-dw/2; int dy = by-(i+1)*25;
                    g.FillRectangle(Brushes.Orange, dx, dy, dw, 20); g.DrawRectangle(Pens.Black, dx, dy, dw, 20);
                    // HIỆN SỐ TRONG ĐĨA
                    g.DrawString(d.ToString(), this.Font, Brushes.Black, dx + dw/2 - 5, dy + 2);
                }
            }
        }
        private void Log(string s) { lstLog.Items.Insert(0, s); }
    }
}