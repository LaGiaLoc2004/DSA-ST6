using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics; 
using System.Linq; 

namespace dsa1 // Namespace chuẩn khớp với Program.cs
{
    // REGION 1: CÁC CLASS HỖ TRỢ (DATA STRUCTURES & HELPERS)
    #region DataStructures_Helpers

    // 1. Panel chống nháy
    public class SuperBufferedPanel : Panel {
        public SuperBufferedPanel() {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }

    // 2. Stack tự định nghĩa
    public class AppStack<T> {
        private List<T> _items = new List<T>();
        public int Count => _items.Count;
        public void Push(T item) => _items.Add(item);
        public T Pop() { if(Count==0) return default; T val=_items[Count-1]; _items.RemoveAt(Count-1); return val; }
        public T Peek() => Count>0 ? _items[Count-1] : default;
        public T[] ToArray() => _items.ToArray();
    }

    // 3. Node đồ thị
    public class AppGraphNode { 
        public int Value; 
        public Point Position; 
        public List<int> Neighbors = new List<int>(); 
    }

    // 4. Hỗ trợ Hanoi
    public struct AppMove { public char From, To; public AppMove(char f, char t){From=f; To=t;} }
    public class AppHanoiSolver {
        public List<AppMove> Moves = new List<AppMove>();
        public List<string> Steps = new List<string>();
        public void Solve(int n, char a, char b, char c) {
            if(n==1) { Record(a, c, 1); return; }
            Solve(n-1, a, c, b); Record(a, c, n); Solve(n-1, b, a, c);
        }
        void Record(char f, char t, int d) { Moves.Add(new AppMove(f, t)); Steps.Add($"Chuyển đĩa {d} từ {f} sang {t}"); }
    }

    #endregion
    // REGION 2: FORM CHÍNH & KHAI BÁO BIẾN
    public partial class Form1 : Form
    {
        #region UI_Components
        private Panel pnlTop; private ComboBox cboAlgo;
        private Panel pnlLeft; private GroupBox grpInput;
        private Label lblN, lblOrder, lblTarget, lblRuns;
        private NumericUpDown nudN, nudTarget, nudRuns;
        private ComboBox cboOrder; 
        private Button btnInit, btnRun, btnPause, btnSpeed;
        private GroupBox grpOutput; private ListBox lstLog;
        private SuperBufferedPanel pnlCenter; 
        private Panel pnlStats; private Label lblStatsContent;
        #endregion

        #region Core_Data
        private AppStack<int> mainStack; 
        private int[] masterData; 
        private List<AppGraphNode> graphNodes; 
        private List<int> finalPath;
        #endregion

        #region Animation_State
        private System.Windows.Forms.Timer animTimer;
        private string currentMode = "HANOI";
        private bool isPaused = false;
        private bool isFastMode = false;
        
        // Cờ kiểm soát việc vẽ (Nếu dữ liệu quá lớn sẽ tắt vẽ)
        private const int LIMIT_DRAW_SORT = 200; 
        private const int LIMIT_DRAW_SEARCH = 100; 
        private bool shouldDraw = true;

        // Hanoi Vars
        private List<int>[] hanoiPegs; private List<AppMove> hanoiMoves; private int hanoiMoveIndex;
        private int movingDiskVal = -1; private float moveProgress = 0f;
        private PointF startPos, endPos, currentPos; private int hanoiState = 0;

        // Search Vars
        private int searchIndex = 0; private bool searchFound = false;

        // Graph Vars
        private struct GraphStep { public int CurrentNode; public HashSet<int> Visited; public List<int> Stack1; public List<int> Stack2; public string Msg; }
        private List<GraphStep> graphSteps; private int graphStepIndex = 0;

        // Sort Vars (Dùng chung cho cả Merge Sort)
        private struct SortStep { 
            public int[] Arr; 
            public int YellowLimit; // Vùng đã xếp / Hoặc vùng biên
            public int RedIndex;    // Vị trí đang xét / Hoặc vị trí Merge
        }
        private List<SortStep> sortSteps; private int sortStepIndex = 0;
        private bool isSortRunning = false;
        #endregion

        public Form1() {
            InitCustomGUI();
            animTimer = new System.Windows.Forms.Timer();
            animTimer.Interval = 42; 
            animTimer.Tick += AnimTimer_Tick;
            cboAlgo.SelectedIndex = 0;
        }
        // REGION 3: THIẾT KẾ GIAO DIỆN (GUI LAYOUT)
        #region GUI_Setup
        private void InitCustomGUI() {
            this.Size = new Size(1350, 850);
            this.Text = "DSA FINAL PROMAX 8 (Base 5 Structure + Merge Sort)";
            this.StartPosition = FormStartPosition.CenterScreen;

            // TOP PANEL
            pnlTop = new Panel() { Dock=DockStyle.Top, Height=60, BackColor=Color.MidnightBlue };
            Label t = new Label(){Text="THUẬT TOÁN:", Location=new Point(20,18), AutoSize=true, Font=new Font("Segoe UI",12,FontStyle.Bold), ForeColor=Color.White};
            cboAlgo = new ComboBox(){Location=new Point(150,15), Width=300, Font=new Font("Segoe UI",11), DropDownStyle=ComboBoxStyle.DropDownList};
            // Đã thay Bubble Sort bằng Merge Sort
            cboAlgo.Items.AddRange(new string[]{"Bài toán: Tháp Hà Nội", "Sắp xếp: Insertion Sort", "Sắp xếp: Selection Sort", "Sắp xếp: Merge Sort", "Tìm kiếm: Linear Search", "Tìm kiếm: BFS (2 Stack)", "Tìm kiếm: DFS"});
            cboAlgo.SelectedIndexChanged += (s,e) => ChangeMode();
            pnlTop.Controls.AddRange(new Control[]{t, cboAlgo});

            // LEFT PANEL
            pnlLeft = new Panel(){Dock=DockStyle.Left, Width=290, BackColor=Color.WhiteSmoke, Padding=new Padding(10)};
            grpInput = new GroupBox(){Text="ĐIỀU KHIỂN", Dock=DockStyle.Top, Height=420, Font=new Font("Segoe UI",10,FontStyle.Bold)};
            
            lblN=new Label(){Text="Số lượng (N):", Location=new Point(15,30), AutoSize=true, Font=new Font("Segoe UI",9)};
            // Cho phép nhập số lớn (100,000)
            nudN=new NumericUpDown(){Location=new Point(15,55), Width=120, Minimum=1, Maximum=100000, Value=20};
            
            lblOrder=new Label(){Text="Thứ tự:", Location=new Point(140,30), AutoSize=true, Font=new Font("Segoe UI",9)};
            cboOrder=new ComboBox(){Location=new Point(140,55), Width=120, DropDownStyle=ComboBoxStyle.DropDownList};
            cboOrder.Items.AddRange(new object[]{"Tăng dần","Giảm dần"}); cboOrder.SelectedIndex=0;

            lblTarget=new Label(){Text="Tìm số:", Location=new Point(15,90), AutoSize=true, Font=new Font("Segoe UI",9), Visible=false};
            nudTarget=new NumericUpDown(){Location=new Point(15,115), Width=120, Maximum=100000, Visible=false};

            lblRuns=new Label(){Text="Số lần Test:", Location=new Point(140,90), AutoSize=true, Font=new Font("Segoe UI",9)};
            nudRuns=new NumericUpDown(){Location=new Point(140,115), Width=120, Minimum=1, Maximum=100000, Value=1};

            btnInit=new Button(){Text="1. TẠO DỮ LIỆU", Location=new Point(15,170), Width=245, Height=35, BackColor=Color.PowderBlue, FlatStyle=FlatStyle.Flat};
            btnRun=new Button(){Text="2. CHẠY MÔ PHỎNG", Location=new Point(15,215), Width=245, Height=35, BackColor=Color.LightGreen, FlatStyle=FlatStyle.Flat};
            
            btnPause=new Button(){Text="TẠM DỪNG", Location=new Point(15,260), Width=115, Height=35, BackColor=Color.LightYellow, FlatStyle=FlatStyle.Flat, Enabled=false};
            btnSpeed=new Button(){Text="TỐC ĐỘ: 1x", Location=new Point(145,260), Width=115, Height=35, BackColor=Color.LightSalmon, FlatStyle=FlatStyle.Flat, Enabled=false};

            // Gán sự kiện
            btnInit.Click += (s,e) => InitData();
            btnRun.Click += (s,e) => RunAlgo();
            btnPause.Click += (s,e) => TogglePause();
            btnSpeed.Click += (s,e) => ToggleSpeed();

            // QUAN TRỌNG: Thêm đủ các nút vào GroupBox
            grpInput.Controls.AddRange(new Control[]{lblN, nudN, lblOrder, cboOrder, lblTarget, nudTarget, lblRuns, nudRuns, btnInit, btnRun, btnPause, btnSpeed});
            
            grpOutput = new GroupBox(){Text="LOG HỆ THỐNG", Dock=DockStyle.Fill, Font=new Font("Segoe UI",10,FontStyle.Bold)};
            lstLog = new ListBox(){Dock=DockStyle.Fill, Font=new Font("Consolas",9), BorderStyle=BorderStyle.None};
            grpOutput.Controls.Add(lstLog);
            pnlLeft.Controls.Add(grpOutput); pnlLeft.Controls.Add(grpInput);

            // CENTER PANEL
            pnlCenter = new SuperBufferedPanel(){Dock=DockStyle.Fill, BackColor=Color.White};
            pnlCenter.Paint += PnlCenter_Paint;
            pnlCenter.Resize += (s,e) => pnlCenter.Invalidate();

            pnlStats = new Panel(){Size=new Size(300,220), BackColor=Color.AliceBlue, BorderStyle=BorderStyle.FixedSingle, Visible=false};
            Label stTitle = new Label(){Text="KẾT QUẢ ĐO LƯỜNG", Dock=DockStyle.Top, Height=30, TextAlign=ContentAlignment.MiddleCenter, BackColor=Color.Navy, ForeColor=Color.White, Font=new Font("Arial",10,FontStyle.Bold)};
            lblStatsContent = new Label(){Dock=DockStyle.Fill, Padding=new Padding(10), Font=new Font("Consolas",9)};
            pnlStats.Controls.AddRange(new Control[]{lblStatsContent, stTitle});
            pnlCenter.Controls.Add(pnlStats);

            this.Controls.AddRange(new Control[]{pnlCenter, pnlLeft, pnlTop});
        }
        #endregion
        // REGION 4: XỬ LÝ SỰ KIỆN & CONTROL LOGIC
        #region Controllers
        private void ChangeMode() {
            string s = cboAlgo.SelectedItem.ToString();
            animTimer.Stop(); pnlStats.Visible=false; lstLog.Items.Clear(); ResetData();
            
            lblOrder.Visible=false; cboOrder.Visible=false; lblTarget.Visible=false; nudTarget.Visible=false; lblRuns.Visible=true; nudRuns.Visible=true;
            btnPause.Enabled = false; btnSpeed.Enabled = false;

            if(s.Contains("Hà Nội")) { currentMode="HANOI"; lblN.Text="Số đĩa (Max 8):"; nudN.Value=3; lblRuns.Visible=false; nudRuns.Visible=false; }
            else if(s.Contains("Sắp xếp")) { currentMode="SORT"; lblN.Text="Số phần tử:"; nudN.Value=20; lblOrder.Visible=true; cboOrder.Visible=true; }
            else if(s.Contains("Linear")) { currentMode="LINEAR"; lblN.Text="Số phần tử:"; nudN.Value=15; lblTarget.Visible=true; nudTarget.Visible=true; }
            else { currentMode="GRAPH"; lblN.Text="Số đỉnh:"; nudN.Value=10; lblTarget.Visible=true; nudTarget.Visible=true; }
        }

        private void ResetData() {
            graphNodes=null; masterData=null; mainStack=null; hanoiPegs=null; 
            sortSteps=null; graphSteps=null; finalPath=null; isSortRunning=false;
            isPaused = false; btnPause.Text = "TẠM DỪNG";
            pnlCenter.Invalidate();
        }

        private void TogglePause() {
            if (!animTimer.Enabled && !isPaused) return; 
            isPaused = !isPaused;
            if (isPaused) { animTimer.Stop(); btnPause.Text = "TIẾP TỤC"; }
            else { animTimer.Start(); btnPause.Text = "TẠM DỪNG"; }
        }

        private void ToggleSpeed() {
            isFastMode = !isFastMode;
            if (isFastMode) { animTimer.Interval = 5; btnSpeed.Text = "TỐC ĐỘ: MAX"; }
            else { animTimer.Interval = 42; btnSpeed.Text = "TỐC ĐỘ: 1x"; }
        }

        private void InitData() {
            int n = (int)nudN.Value; ResetData(); pnlStats.Visible=false;
            Random rnd = new Random();

            // LOGIC KIỂM TRA: VẼ hay CHẠY NGẦM
            shouldDraw = true;
            if(currentMode == "SORT" && n > LIMIT_DRAW_SORT) shouldDraw = false;
            if(currentMode == "LINEAR" && n > LIMIT_DRAW_SEARCH) shouldDraw = false;
            
            // Riêng Hanoi phải giới hạn cứng vì 2^n quá lớn
            if(currentMode == "HANOI" && n > 8) { n=8; nudN.Value=8; Log("! Tự chỉnh N=8 để tránh treo."); }

            if (!shouldDraw) Log($"⚠ Dữ liệu lớn ({n} phần tử). Chế độ: CHẠY NGẦM (Không vẽ).");

            if(currentMode=="HANOI") {
                hanoiPegs=new List<int>[3]; for(int i=0;i<3;i++) hanoiPegs[i]=new List<int>();
                for(int i=n; i>=1; i--) hanoiPegs[0].Add(i);
                Log($"Đã đặt {n} đĩa.");
            }
            else if(currentMode=="GRAPH") {
                graphNodes=new List<AppGraphNode>();
                for(int i=1;i<=n;i++) graphNodes.Add(new AppGraphNode{Value=i});
                for(int i=1;i<n;i++) { 
                    int p = rnd.Next(0, i);
                    graphNodes[p].Neighbors.Add(graphNodes[i].Value);
                    graphNodes[i].Neighbors.Add(graphNodes[p].Value);
                }
                // Chỉ tính tọa độ nếu cần vẽ
                if(shouldDraw) {
                    int w=pnlCenter.Width-350, startY=60, levelH=90;
                    var levels=new Dictionary<int,List<AppGraphNode>>();
                    var q=new Queue<AppGraphNode>(); var vis=new HashSet<int>(); var dep=new Dictionary<int,int>();
                    q.Enqueue(graphNodes[0]); vis.Add(graphNodes[0].Value); dep[graphNodes[0].Value]=0;
                    while(q.Count>0) {
                        var u=q.Dequeue(); int d=dep[u.Value];
                        if(!levels.ContainsKey(d)) levels[d]=new List<AppGraphNode>(); levels[d].Add(u);
                        foreach(var nid in u.Neighbors) {
                            var node=graphNodes.FirstOrDefault(x=>x.Value==nid);
                            if(node!=null && !vis.Contains(nid)) { vis.Add(nid); dep[nid]=d+1; q.Enqueue(node); }
                        }
                    }
                    foreach(var kv in levels) {
                        int section = w / (kv.Value.Count+1);
                        for(int i=0; i<kv.Value.Count; i++) kv.Value[i].Position = new Point((i+1)*section, startY + kv.Key*levelH);
                    }
                }
                Log($"Tạo cây {n} đỉnh.");
            }
            else { 
                masterData = new int[n];
                for(int i=0;i<n;i++) masterData[i]=rnd.Next(1, 1000);
                mainStack = new AppStack<int>(); foreach(var x in masterData) mainStack.Push(x);
                Log($"Sinh {n} số ngẫu nhiên.");
            }
            pnlCenter.Invalidate();
        }

        private void RunAlgo() {
            if(currentMode=="HANOI") {
                if(hanoiPegs==null) return;
                AppHanoiSolver s=new AppHanoiSolver(); s.Solve((int)nudN.Value, 'A','B','C');
                hanoiMoves=s.Moves; hanoiMoveIndex=0; hanoiState=0;
                foreach(var step in s.Steps) Log(step);
                StartAnim(); return;
            }
            if((currentMode=="GRAPH" && graphNodes==null) || (currentMode!="GRAPH" && masterData==null)) { MessageBox.Show("Cần tạo dữ liệu!"); return; }

            int runs=(int)nudRuns.Value; string algo=cboAlgo.SelectedItem.ToString(); int t=(int)nudTarget.Value;
            
            // --- 1. CHẠY BENCHMARK ---
            List<double> times=new List<double>();
            for(int i=0;i<runs;i++) {
                Stopwatch sw=Stopwatch.StartNew();
                if(currentMode=="SORT") {
                    int[] arr=(int[])masterData.Clone();
                    if(algo.Contains("Insertion")) InsertionSort(arr);
                    else if(algo.Contains("Selection")) SelectionSort(arr);
                    else if(algo.Contains("Merge")) MergeSort(arr, 0, arr.Length - 1);
                } else if(currentMode=="LINEAR") {
                    foreach(var x in masterData) if(x==t) break;
                } else if(currentMode=="GRAPH") {
                    if(graphNodes.Count>0) {
                        if (algo.Contains("DFS")) { Stack<int> s=new Stack<int>(); s.Push(graphNodes[0].Value); while(s.Count>0) s.Pop(); }
                        else { Queue<int> q=new Queue<int>(); q.Enqueue(graphNodes[0].Value); while(q.Count>0) q.Dequeue(); }
                    }
                }
                sw.Stop(); times.Add(sw.Elapsed.TotalMilliseconds);
            }
            
            // --- 2. TÍNH TOÁN & HIỂN THỊ KẾT QUẢ ---
            double avg = times.Count > 0 ? times.Average() : 0;
            double variance = times.Count > 1 ? times.Sum(v => Math.Pow(v - avg, 2)) / (times.Count - 1) : 0;
            double sd = Math.Sqrt(variance);

            lblStatsContent.Text = $"Số lần thử: {runs}\n" +
                                   $"----------------\n" +
                                   $"Trung bình : {avg:F4} ms\n" +
                                   $"Tối thiểu  : {(times.Count>0?times.Min():0):F4} ms\n" +
                                   $"Tối đa     : {(times.Count>0?times.Max():0):F4} ms\n" +
                                   $"Phương sai : {variance:F4}\n" +
                                   $"Độ lệch chuẩn. : {sd:F4}";
            pnlStats.Location=new Point(pnlCenter.Width-pnlStats.Width-10, 10); pnlStats.Visible=true; pnlStats.BringToFront();

            // Nếu chế độ chạy ngầm -> Dừng tại đây, không vẽ
            if (!shouldDraw) {
                Log("Đã hoàn tất chạy ngầm.");
                return;
            }

            // --- 3. CHUẨN BỊ ANIMATION (NẾU CẦN VẼ) ---
            if(currentMode=="SORT") {
                sortSteps=new List<SortStep>();
                int[] arr=(int[])masterData.Clone(); bool desc=cboOrder.SelectedIndex==1;
                sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=-1, RedIndex=-1});
                
                if(algo.Contains("Insertion")) {
                    for(int i=1; i<arr.Length; i++) {
                        int key=arr[i], j=i-1;
                        sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i, RedIndex=i});
                        while(j>=0 && (desc ? arr[j]<key : arr[j]>key)) {
                            arr[j+1]=arr[j]; j--;
                            sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i, RedIndex=j+1});
                        }
                        arr[j+1]=key;
                        sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i, RedIndex=j+1});
                    }
                } 
                else if(algo.Contains("Selection")) {
                    for(int i=0; i<arr.Length-1; i++) {
                        int m=i;
                        for(int j=i+1; j<arr.Length; j++) {
                            sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i, RedIndex=j});
                            if(desc ? arr[j]>arr[m] : arr[j]<arr[m]) m=j;
                        }
                        int tmp=arr[m]; arr[m]=arr[i]; arr[i]=tmp;
                        sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i+1, RedIndex=-1});
                    }
                } else if(algo.Contains("Merge")) {
                    // Gọi hàm Animation Merge Sort
                    MergeSortAnim(arr, 0, arr.Length - 1, desc);
                }
                sortSteps.Add(new SortStep{Arr=arr, YellowLimit=arr.Length, RedIndex=-1});
                sortStepIndex=0; isSortRunning=true; StartAnim();
            }
            else if(currentMode=="LINEAR") {
                searchIndex=0; searchFound=false; StartAnim();
            }
            else if(currentMode=="GRAPH") {
                graphSteps=new List<GraphStep>(); graphStepIndex=0; finalPath=null;
                if(algo.Contains("DFS")) RunDFS(t); else RunBFS(t);
                StartAnim();
            }
        }

        private void StartAnim() {
            btnPause.Enabled = true; btnSpeed.Enabled = true;
            isPaused = false; btnPause.Text = "TẠM DỪNG";
            animTimer.Start();
        }
        #endregion

        // REGION 5: LOGIC THUẬT TOÁN (ALGORITHMS)
        #region Algorithm_Logic
        // --- 1. Sắp xếp (Core logic để đo thời gian) ---
        void InsertionSort(int[] arr) {
            for(int i=1; i<arr.Length; i++) {
                int key=arr[i], j=i-1;
                while(j>=0 && arr[j]>key) { arr[j+1]=arr[j]; j--; }
                arr[j+1]=key;
            }
        }
        void SelectionSort(int[] arr) {
            for(int i=0; i<arr.Length-1; i++) {
                int m=i;
                for(int j=i+1; j<arr.Length; j++) if(arr[j]<arr[m]) m=j;
                int t=arr[m]; arr[m]=arr[i]; arr[i]=t;
            }
        }
        void MergeSort(int[] arr, int l, int r) {
            if (l < r) {
                int m = l + (r - l) / 2;
                MergeSort(arr, l, m);
                MergeSort(arr, m + 1, r);
                Merge(arr, l, m, r);
            }
        }
        void Merge(int[] arr, int l, int m, int r) {
            int n1 = m - l + 1; int n2 = r - m;
            int[] L = new int[n1]; int[] R = new int[n2];
            Array.Copy(arr, l, L, 0, n1); Array.Copy(arr, m + 1, R, 0, n2);
            int i=0, j=0, k=l;
            while(i<n1 && j<n2) { if(L[i]<=R[j]) arr[k++]=L[i++]; else arr[k++]=R[j++]; }
            while(i<n1) arr[k++]=L[i++];
            while(j<n2) arr[k++]=R[j++];
        }

        // --- 2. Sắp xếp (Animation logic để vẽ) ---
        void MergeSortAnim(int[] arr, int l, int r, bool desc) {
            if (l < r) {
                int m = l + (r - l) / 2;
                MergeSortAnim(arr, l, m, desc);
                MergeSortAnim(arr, m + 1, r, desc);
                MergeAnim(arr, l, m, r, desc);
            }
        }
        void MergeAnim(int[] arr, int l, int m, int r, bool desc) {
            int n1 = m - l + 1; int n2 = r - m;
            int[] L = new int[n1]; int[] R = new int[n2];
            Array.Copy(arr, l, L, 0, n1); Array.Copy(arr, m + 1, R, 0, n2);
            int i=0, j=0, k=l;
            while(i<n1 && j<n2) {
                bool cond = desc ? L[i]>=R[j] : L[i]<=R[j];
                // RedIndex = k (vị trí đang ghi), YellowLimit = -1 (không dùng)
                sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=-1, RedIndex=k}); 
                if(cond) arr[k++]=L[i++]; else arr[k++]=R[j++];
            }
            while(i<n1) {
                sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=-1, RedIndex=k});
                arr[k++]=L[i++];
            }
            while(j<n2) {
                sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=-1, RedIndex=k});
                arr[k++]=R[j++];
            }
        }

        // --- 3. Đồ thị ---
        void RunDFS(int target) {
            Stack<int> s=new Stack<int>(); HashSet<int> v=new HashSet<int>(); Dictionary<int,int> p=new Dictionary<int,int>();
            int start=graphNodes[0].Value; s.Push(start);
            RecordGraph(-1, v, s.ToList(), null, $"Start DFS {start}");
            bool found=false;
            while(s.Count>0) {
                int u=s.Pop(); RecordGraph(u, v, s.ToList(), null, $"Pop {u}");
                if(!v.Contains(u)) {
                    v.Add(u); RecordGraph(u, v, s.ToList(), null, $"Visit {u}");
                    if(u==target) { 
                        found=true; BuildPath(p, start, target); 
                        RecordGraph(u, v, s.ToList(), null, $"TÌM THẤY! Đường đi: {string.Join("->", finalPath)}");
                        break; 
                    }
                    var nb=graphNodes.First(x=>x.Value==u).Neighbors.OrderByDescending(x=>x).ToList();
                    foreach(var n in nb) if(!v.Contains(n)) { s.Push(n); if(!p.ContainsKey(n)) p[n]=u; RecordGraph(u, v, s.ToList(), null, $"Push {n}"); }
                }
            }
            if(!found) RecordGraph(-1, v, s.ToList(), null, "Not found");
        }

        void RunBFS(int target) {
            Stack<int> sIn=new Stack<int>(), sOut=new Stack<int>(); HashSet<int> v=new HashSet<int>(); Dictionary<int,int> p=new Dictionary<int,int>();
            int start=graphNodes[0].Value; sIn.Push(start); v.Add(start);
            RecordGraph(-1, v, sIn.ToList(), sOut.ToList(), $"Start BFS {start}");
            bool found=false;
            while(sIn.Count>0 || sOut.Count>0) {
                if(sOut.Count==0) {
                    RecordGraph(-1, v, sIn.ToList(), sOut.ToList(), "In -> Out");
                    while(sIn.Count>0) sOut.Push(sIn.Pop());
                }
                int u=sOut.Pop(); RecordGraph(u, v, sIn.ToList(), sOut.ToList(), $"Pop Out {u}");
                if(u==target) { 
                    found=true; BuildPath(p, start, target); 
                    RecordGraph(u, v, sIn.ToList(), sOut.ToList(), $"TÌM THẤY! Đường đi: {string.Join("->", finalPath)}");
                    break; 
                }
                var nb=graphNodes.First(x=>x.Value==u).Neighbors.OrderBy(x=>x).ToList();
                foreach(var n in nb) if(!v.Contains(n)) { v.Add(n); sIn.Push(n); p[n]=u; RecordGraph(u, v, sIn.ToList(), sOut.ToList(), $"Push In {n}"); }
            }
            if(!found) RecordGraph(-1, v, sIn.ToList(), sOut.ToList(), "Not found");
        }

        void BuildPath(Dictionary<int,int> p, int s, int e) {
            finalPath=new List<int>(); int c=e;
            while(c!=s && p.ContainsKey(c)) { finalPath.Add(c); c=p[c]; }
            finalPath.Add(s); finalPath.Reverse();
        }
        void RecordGraph(int c, HashSet<int> v, List<int> s1, List<int> s2, string m) {
            var st=new GraphStep{CurrentNode=c, Visited=new HashSet<int>(v), Stack1=s1, Msg=m};
            if(s2!=null) st.Stack2=s2; graphSteps.Add(st);
        }
        void Log(string s) => lstLog.Items.Insert(0, s);
        #endregion
        // REGION 6: ĐỘNG CƠ ANIMATION & VẼ (RENDERING)
        #region Animation_Rendering
        private void AnimTimer_Tick(object sender, EventArgs e) {
            if(currentMode=="HANOI") {
                if(hanoiState==0) { 
                    if(hanoiMoveIndex>=hanoiMoves.Count) { animTimer.Stop(); btnPause.Enabled=false; return; }
                    AppMove m=hanoiMoves[hanoiMoveIndex];
                    int f=(m.From=='A')?0:(m.From=='B')?1:2; int t=(m.To=='A')?0:(m.To=='B')?1:2;
                    if(hanoiPegs[f].Count==0) return;
                    movingDiskVal=hanoiPegs[f].Last(); hanoiPegs[f].RemoveAt(hanoiPegs[f].Count-1);
                    int w=pnlCenter.Width, h=pnlCenter.Height;
                    startPos=new PointF(w*(f==0?1:f==1?3:5)/6, h-50-(hanoiPegs[f].Count+1)*25);
                    endPos=new PointF(w*(t==0?1:t==1?3:5)/6, h-50-(hanoiPegs[t].Count+1)*25);
                    currentPos=startPos; moveProgress=0; hanoiState=1;
                } else { 
                    moveProgress += (isFastMode ? 0.3f : 0.1f);
                    if(moveProgress>=1f) {
                        AppMove m=hanoiMoves[hanoiMoveIndex]; int t=(m.To=='A')?0:(m.To=='B')?1:2;
                        hanoiPegs[t].Add(movingDiskVal); movingDiskVal=-1; hanoiState=0; hanoiMoveIndex++;
                    } else {
                        float safeY=pnlCenter.Height/3;
                        if(moveProgress<0.3f) currentPos.Y=startPos.Y+(safeY-startPos.Y)*(moveProgress/0.3f);
                        else if(moveProgress<0.7f) { currentPos.Y=safeY; currentPos.X=startPos.X+(endPos.X-startPos.X)*((moveProgress-0.3f)/0.4f); }
                        else currentPos.Y=safeY+(endPos.Y-safeY)*((moveProgress-0.7f)/0.3f);
                    }
                }
                pnlCenter.Invalidate();
            }
            else if(currentMode=="SORT") {
                if(sortSteps!=null && sortStepIndex<sortSteps.Count) { sortStepIndex++; pnlCenter.Invalidate(); }
                else { animTimer.Stop(); btnPause.Enabled=false; }
            }
            else if(currentMode=="LINEAR") {
                if(searchIndex>=masterData.Length) { animTimer.Stop(); btnPause.Enabled=false; Log("Không tìm thấy."); }
                else if(masterData[searchIndex]==(int)nudTarget.Value) { 
                    searchFound=true; animTimer.Stop(); btnPause.Enabled=false; 
                    Log($"Tìm thấy số {masterData[searchIndex]} tại vị trí {searchIndex}"); 
                }
                else searchIndex++;
                pnlCenter.Invalidate();
            }
            else if(currentMode=="GRAPH") {
                if(graphSteps!=null && graphStepIndex<graphSteps.Count) { Log(graphSteps[graphStepIndex].Msg); graphStepIndex++; pnlCenter.Invalidate(); }
                else { animTimer.Stop(); btnPause.Enabled=false; }
            }
        }

        private void PnlCenter_Paint(object sender, PaintEventArgs e) {
            Graphics g=e.Graphics; g.SmoothingMode=SmoothingMode.AntiAlias;
            int w=pnlCenter.Width, h=pnlCenter.Height;

            // Nếu đang chế độ chạy ngầm
            if(!shouldDraw) {
                g.DrawString("CHẾ ĐỘ CHẠY NGẦM (Dữ liệu lớn)\nXem kết quả bên cột phải ->", new Font("Arial", 16, FontStyle.Bold), Brushes.Gray, w/2-250, h/2);
                return;
            }

            if(currentMode=="HANOI") {
                if(hanoiPegs==null) return;
                int by=h-100; int[] px={w/6, w/2, 5*w/6};
                for(int p=0;p<3;p++) {
                    g.FillRectangle(Brushes.SaddleBrown, px[p]-5, by-250, 10, 250);
                    g.FillRectangle(Brushes.Gray, px[p]-60, by, 120, 10);
                    g.DrawString($"Cọc {(char)('A'+p)}", new Font("Arial",12,FontStyle.Bold), Brushes.Black, px[p]-15, by+20);
                    for(int i=0;i<hanoiPegs[p].Count;i++) DrawDisk(g, px[p], by-(i+1)*25, hanoiPegs[p][i]);
                }
                if(movingDiskVal!=-1) DrawDisk(g, (int)currentPos.X, (int)currentPos.Y, movingDiskVal);
            }
            else if(currentMode=="SORT") {
                if(sortSteps==null && masterData!=null) DrawBars(g, w, h, masterData, -1, -1);
                else if(sortSteps!=null) {
                    var s = sortSteps[Math.Min(sortStepIndex, sortSteps.Count-1)];
                    DrawBars(g, w, h, s.Arr, s.YellowLimit, s.RedIndex);
                }
            }
            else if(currentMode=="LINEAR") {
                if(masterData==null) return;
                float boxW=50; 
                if(masterData.Length > 0) boxW = Math.Min(50, (w-40)/masterData.Length - 2);
                float sx=(w - (masterData.Length*(boxW+2)))/2; if(sx<20) sx=20;
                
                for(int i=0; i<masterData.Length; i++) {
                    float x=sx+i*(boxW+2); Brush b=Brushes.White;
                    if(animTimer.Enabled || searchFound) { if(i==searchIndex) b=Brushes.Yellow; if(searchFound && i==searchIndex) b=Brushes.LightGreen; }
                    g.FillRectangle(b, x, h/2, boxW, boxW);
                    if(boxW>5) g.DrawRectangle(Pens.Black, x, h/2, boxW, boxW);
                    if(boxW>20) g.DrawString(masterData[i].ToString(), new Font("Arial", boxW>30?10:7), Brushes.Black, x+2, h/2+boxW/3);
                }
            }
            else if(currentMode=="GRAPH") {
                if(graphNodes==null) return;
                var step = (graphSteps!=null && graphStepIndex>0) ? graphSteps[graphStepIndex-1] : new GraphStep();
                
                // Vẽ cạnh
                using(Pen p=new Pen(Color.LightGray, 2))
                foreach(var n in graphNodes) foreach(var nb in n.Neighbors) {
                    var t=graphNodes.FirstOrDefault(x=>x.Value==nb);
                    if(t!=null && n.Value<nb) g.DrawLine(p, n.Position, t.Position);
                }
                // Vẽ đường đi
                if(finalPath!=null && !animTimer.Enabled) {
                    using(Pen p=new Pen(Color.Blue, 4))
                    for(int i=0;i<finalPath.Count-1;i++) {
                        var p1=graphNodes.First(x=>x.Value==finalPath[i]).Position;
                        var p2=graphNodes.First(x=>x.Value==finalPath[i+1]).Position;
                        g.DrawLine(p, p1, p2);
                    }
                }
                // Vẽ node
                foreach(var n in graphNodes) {
                    Brush b=Brushes.White;
                    if(step.Visited!=null && step.Visited.Contains(n.Value)) b=Brushes.LightGreen;
                    if(step.CurrentNode==n.Value) b=Brushes.OrangeRed;
                    bool inS = (step.Stack1!=null && step.Stack1.Contains(n.Value)) || (step.Stack2!=null && step.Stack2.Contains(n.Value));
                    if(inS) b=Brushes.Gold;
                    
                    g.FillEllipse(b, n.Position.X-20, n.Position.Y-20, 40, 40);
                    g.DrawEllipse(Pens.Black, n.Position.X-20, n.Position.Y-20, 40, 40);
                    g.DrawString(n.Value.ToString(), new Font("Arial", 11, FontStyle.Bold), Brushes.Black, n.Position.X-10, n.Position.Y-8);
                }
                bool bfs = cboAlgo.SelectedItem.ToString().Contains("BFS");
                DrawBucket(g, w-240, h-50, "STACK 1", step.Stack1);
                if(bfs) DrawBucket(g, w-120, h-50, "STACK 2", step.Stack2);
            }
        }

        void DrawDisk(Graphics g, int cx, int cy, int val) {
            int dw=40+val*30; Rectangle r=new Rectangle(cx-dw/2, cy, dw, 20);
            g.FillRectangle(Brushes.OrangeRed, r); g.DrawRectangle(Pens.Maroon, r);
            g.DrawString(val.ToString(), new Font("Arial", 10, FontStyle.Bold), Brushes.White, cx-6, cy+2);
        }
        void DrawBars(Graphics g, int w, int h, int[] arr, int yLim, int rIdx) {
            if(arr==null || arr.Length==0) return;
            float max=arr.Max(), bw=(float)(w-40)/arr.Length, mh=h*0.8f;
            for(int i=0; i<arr.Length; i++) {
                float bh=(arr[i]/max)*mh; if(bh<5) bh=5;
                float x=20+i*bw, y=h-20-bh;
                Brush b=Brushes.SteelBlue;
                if(cboAlgo.SelectedItem.ToString().Contains("Bubble") ? (i>=yLim && yLim>0) : (i<yLim)) b=Brushes.Gold;
                if(i==rIdx) b=Brushes.Red;
                if(bw>2) { 
                    g.FillRectangle(b, x, y, bw, bh); 
                    if(bw>5) g.DrawRectangle(Pens.White, x, y, bw, bh); 
                    if(bw>20) g.DrawString(arr[i].ToString(), new Font("Arial", 8), Brushes.Black, x + (bw/2)-7, y-15);
                }
                else g.DrawLine(new Pen(b), x, h-20, x, y);
            }
            if(isSortRunning) g.DrawString("Vàng: Đã xếp | Đỏ: Đang xét", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, 20, 20);
        }
        void DrawBucket(Graphics g, int x, int by, string t, List<int> it) {
            g.DrawString(t, new Font("Arial", 10, FontStyle.Bold), Brushes.Navy, x-30, 40);
            using(Pen p=new Pen(Color.Navy, 3)) { g.DrawLine(p, x-40, 60, x-40, by); g.DrawLine(p, x+40, 60, x+40, by); g.DrawLine(p, x-40, by, x+40, by); }
            if(it!=null) for(int i=0; i<it.Count; i++) {
                int val=it[it.Count-1-i];
                g.FillRectangle(Brushes.Gold, x-35, by-(i+1)*35, 70, 30); g.DrawRectangle(Pens.Black, x-35, by-(i+1)*35, 70, 30);
                g.DrawString(val.ToString(), new Font("Arial", 10), Brushes.Black, x-10, by-(i+1)*35+5);
            }
        }
        #endregion
    }
}
