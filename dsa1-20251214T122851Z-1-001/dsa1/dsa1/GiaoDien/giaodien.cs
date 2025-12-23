using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics; 
using System.Linq; 

namespace dsa1 
{
    // 1. CÁC CLASS HỖ TRỢ GIAO DIỆN & CẤU TRÚC DỮ LIỆU

    // Panel chống nháy (Double Buffered) + Hỗ trợ Focus để cuộn chuột
    public class SuperBufferedPanel : Panel {
        public SuperBufferedPanel() {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
            this.UpdateStyles();
        }
        protected override void OnMouseDown(MouseEventArgs e) { this.Focus(); base.OnMouseDown(e); }
        protected override bool IsInputKey(Keys keyData) { return true; }
    }

    // Node đồ thị
    public class UiGraphNode { 
        public int Value; 
        public Point Position; 
        public List<int> Neighbors = new List<int>(); 
    }

    // Interface và Class hỗ trợ duyệt (Khớp báo cáo)
    public interface ITraversable<T> { bool HasNext(); T Next(); void Reset(); }

    public class ArrayTraversable<T> : ITraversable<T> {
        private T[] data; private int index = 0;
        public ArrayTraversable(T[] data) { this.data = data; }
        public bool HasNext() => index < data.Length;
        public T Next() => data[index++];
        public void Reset() => index = 0;
    }

    // Bộ đo thời gian
    public abstract class TimeAnalyzer {
        protected Stopwatch _stopwatch = new Stopwatch();
        public double Measure(Action action) { GC.Collect(); _stopwatch.Restart(); action(); _stopwatch.Stop(); return _stopwatch.Elapsed.TotalMilliseconds; }
        public double Mean(List<double> data) => data.Count > 0 ? data.Average() : 0;
        public double Variance(List<double> data, double mean) { if (data.Count <= 1) return 0; return data.Sum(x => Math.Pow(x - mean, 2)) / (data.Count - 1); }
    }

    public class TimeSort : TimeAnalyzer { public List<double> RunBenchmark(int runs, Action sortAction) { List<double> times = new List<double>(); for(int i=0; i<runs; i++) times.Add(Measure(sortAction)); return times; } }
    public class TimeSearch : TimeAnalyzer { public List<double> RunBenchmark(int runs, Action searchAction) { List<double> times = new List<double>(); for(int i=0; i<runs; i++) times.Add(Measure(searchAction)); return times; } }
    
    // Thuật toán tìm kiếm tuyến tính (Viết tại chỗ để khớp Interface)
    public class LinearSearchFinder<T> { public bool Search(ITraversable<T> origin, T target) { origin.Reset(); while (origin.HasNext()) { if (Equals(origin.Next(), target)) return true; } return false; } }

    // 2. FORM CHÍNH (FORM1)
    public partial class Form1 : Form
    {
        // --- MÀU SẮC GIAO DIỆN ---
        private readonly Color clrPrimary = Color.FromArgb(25, 118, 210);
        private readonly Color clrSuccess = Color.FromArgb(56, 142, 60);
        private readonly Color clrWarning = Color.FromArgb(255, 160, 0);
        private readonly Color clrDanger  = Color.FromArgb(211, 47, 47);
        private readonly Color clrBg      = Color.FromArgb(245, 245, 245);
        private readonly Color clrText    = Color.FromArgb(33, 33, 33);
        
        // --- CÁC CONTROL ---
        private Panel pnlTop, pnlLeft, pnlStats;
        private SuperBufferedPanel pnlCenter;
        private ComboBox cboAlgo, cboOrder;
        private Label lblN, lblOrder, lblTarget, lblRuns, lblStatsContent;
        private NumericUpDown nudN, nudTarget, nudRuns;
        private Button btnInit, btnRun, btnPause, btnSpeed;
        private GroupBox grpInput, grpOutput; 
        private ListBox lstLog;
        private Label lblStatus; // Thanh trạng thái mô tả thuật toán

        // --- CAMERA CONTROLS (ZOOM & PAN) ---
        private Panel pnlZoom;
        private Button btnZoomIn, btnZoomOut;
        private Label lblZoomValue;
        
        private float currentZoom = 1.0f;     
        private float viewOffsetX = 0f;       
        private float viewOffsetY = 0f;       
        private bool isDragging = false;      
        private Point lastMousePos;           

        // --- DỮ LIỆU & LOGIC ---
        private MyStack<int> mainStack; 
        private int[] masterData; 
        private List<UiGraphNode> graphNodes; 
        private List<int> finalPath; 
        private System.Windows.Forms.Timer animTimer;
        private string currentMode = "HANOI";
        private bool isPaused = false;
        private bool shouldDraw = true;
        private int speedMode = 0; 
        private int frameCounter = 0;
        private int speedDivisor = 5; 

        // --- BIẾN TRẠNG THÁI ANIMATION ---
        private List<int>[] hanoiPegs; private List<Move> hanoiMoves; private int hanoiMoveIndex, movingDiskVal = -1;
        private PointF startPos, endPos, currentPos; private int hanoiState = 0; private float moveProgress = 0f;
        private int searchIndex = 0; private bool searchFound = false;
        private struct GraphStep { public int CurrentNode; public HashSet<int> Visited; public List<int> Stack1; public List<int> Stack2; public string Msg; }
        private List<GraphStep> graphSteps; private int graphStepIndex = 0;
        private struct SortStep { public int[] Arr; public int YellowLimit; public int RedIndex; }
        private List<SortStep> sortSteps; private int sortStepIndex = 0;

        public Form1() {
            InitCustomGUI();
            InitZoomControls();
            animTimer = new System.Windows.Forms.Timer(); animTimer.Interval = 16; animTimer.Tick += AnimTimer_Tick;
            cboAlgo.SelectedIndex = 0; // Mặc định chọn Tháp Hà Nội
            ApplySpeed(); 
        }

        private GraphicsPath RoundedRect(RectangleF rect, float radius) {
            GraphicsPath path = new GraphicsPath(); if (radius <= 0) radius = 1;
            path.StartFigure(); path.AddArc(rect.X, rect.Y, radius, radius, 180, 90); path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90); path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90); path.CloseFigure(); return path;
        }

        private void InitCustomGUI() {
            this.Size = new Size(1350, 850);
            this.Text = "ỨNG DỤNG STACK ĐỂ GIẢI BÀI TOÁN THÁP HÀ NỘI, BÀI TOÁN SẮP XẾP VÀ BÀI TOÁN TÌM KIẾM ";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            pnlTop = new Panel() { Dock=DockStyle.Top, Height=60, BackColor=clrPrimary };
            
            Label t = new Label(){Text="CHỨC NĂNG:", Location=new Point(20,18), AutoSize=true, Font=new Font("Segoe UI",11,FontStyle.Bold), ForeColor=Color.White};
            cboAlgo = new ComboBox(){Location=new Point(120,15), Width=320, Font=new Font("Segoe UI",11), DropDownStyle=ComboBoxStyle.DropDownList, FlatStyle=FlatStyle.Flat};
            
            cboAlgo.Items.Add("Bài toán Tháp Hà Nội"); // Mục chính
            cboAlgo.Items.Add("────────────────────"); // Dòng kẻ
            cboAlgo.Items.Add("Mở rộng: Thuật toán Sắp xếp");
            cboAlgo.Items.Add("   Insertion Sort"); // Thụt đầu dòng
            cboAlgo.Items.Add("   Selection Sort");
            cboAlgo.Items.Add("   Merge Sort");
            cboAlgo.Items.Add("Mở rộng: Thuật toán Tìm kiếm");
            cboAlgo.Items.Add("   Linear Search");
            cboAlgo.Items.Add("   BFS ( 2 STACK)");
            cboAlgo.Items.Add("   DFS ");
            
            cboAlgo.SelectedIndexChanged += (s,e) => ChangeMode();

            // THANH TRẠNG THÁI (Mô tả kỹ thuật)
            lblStatus = new Label() {
                Location = new Point(460, 18),
                AutoSize = true,
                ForeColor = Color.WhiteSmoke,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                Text = ""
            };

            pnlTop.Controls.AddRange(new Control[]{t, cboAlgo, lblStatus});

            pnlLeft = new Panel(){Dock=DockStyle.Left, Width=290, BackColor=clrBg, Padding=new Padding(10)};
            grpInput = new GroupBox(){Text="THAM SỐ ĐẦU VÀO", Dock=DockStyle.Top, Height=420, Font=new Font("Segoe UI",10,FontStyle.Bold), ForeColor=clrPrimary};
            
            lblN=new Label(){Text="Số lượng (N):", Location=new Point(15,30), AutoSize=true, Font=new Font("Segoe UI",9), ForeColor=Color.Black};
            nudN=new NumericUpDown(){Location=new Point(15,55), Width=120, Minimum=1, Maximum=100000, Value=20}; 
            lblOrder=new Label(){Text="Thứ tự:", Location=new Point(140,30), AutoSize=true, Font=new Font("Segoe UI",9), ForeColor=Color.Black};
            cboOrder=new ComboBox(){Location=new Point(140,55), Width=120, DropDownStyle=ComboBoxStyle.DropDownList};
            cboOrder.Items.AddRange(new object[]{"Tăng dần","Giảm dần", "Ngẫu nhiên"}); cboOrder.SelectedIndex=2;
            lblTarget=new Label(){Text="Tìm số:", Location=new Point(15,90), AutoSize=true, Font=new Font("Segoe UI",9), Visible=false, ForeColor=Color.Black};
            nudTarget=new NumericUpDown(){Location=new Point(15,115), Width=120, Maximum=100000, Visible=false};
            lblRuns=new Label(){Text="Số lần đo:", Location=new Point(140,90), AutoSize=true, Font=new Font("Segoe UI",9), ForeColor=Color.Black};
            nudRuns=new NumericUpDown(){Location=new Point(140,115), Width=120, Minimum=1, Maximum=100000, Value=1};

            btnInit=new Button(){Text="KHỞI TẠO DỮ LIỆU", Location=new Point(15,170), Width=245, Height=40, BackColor=clrPrimary, FlatStyle=FlatStyle.Flat, ForeColor=Color.White, Font=new Font("Segoe UI", 10, FontStyle.Bold)};
            btnRun=new Button(){Text="BẮT ĐẦU MÔ PHỎNG", Location=new Point(15,220), Width=245, Height=40, BackColor=clrSuccess, FlatStyle=FlatStyle.Flat, ForeColor=Color.White, Font=new Font("Segoe UI", 10, FontStyle.Bold)};
            btnPause=new Button(){Text="TẠM DỪNG", Location=new Point(15,270), Width=115, Height=40, BackColor=clrWarning, FlatStyle=FlatStyle.Flat, ForeColor=Color.Black, Enabled=false};
            btnSpeed=new Button(){Text="TỐC ĐỘ: 1x", Location=new Point(145,270), Width=115, Height=40, BackColor=clrDanger, FlatStyle=FlatStyle.Flat, ForeColor=Color.White, Enabled=true};

            btnInit.Click += (s, e) => InitData();
            btnRun.Click += (s,e) => RunAlgo();
            btnPause.Click += (s,e) => TogglePause(); 
            btnSpeed.Click += (s,e) => ToggleSpeed();

            Action<Button> setRound = (b) => { using (GraphicsPath path = RoundedRect(new RectangleF(0, 0, b.Width, b.Height), 12)) b.Region = new Region(path); };
            setRound(btnInit); setRound(btnRun); setRound(btnPause); setRound(btnSpeed);

            grpInput.Controls.AddRange(new Control[]{lblN, nudN, lblOrder, cboOrder, lblTarget, nudTarget, lblRuns, nudRuns, btnInit, btnRun, btnPause, btnSpeed});
            grpOutput = new GroupBox(){Text="NHẬT KÝ (LOG)", Dock=DockStyle.Fill, Font=new Font("Segoe UI",10,FontStyle.Bold), ForeColor=clrPrimary};
            lstLog = new ListBox(){Dock=DockStyle.Fill, Font=new Font("Consolas",9), BorderStyle=BorderStyle.None, ForeColor=Color.Black};
            grpOutput.Controls.Add(lstLog);
            pnlLeft.Controls.Add(grpOutput); pnlLeft.Controls.Add(grpInput);

            pnlCenter = new SuperBufferedPanel(){Dock=DockStyle.Fill, BackColor=Color.White};
            pnlCenter.Paint += PnlCenter_Paint;
            pnlCenter.Resize += (s,e) => {
                pnlCenter.Invalidate();
                // Giữ Zoom Panel ở góc phải dưới
                if(pnlZoom != null) pnlZoom.Location = new Point(pnlCenter.Width - pnlZoom.Width - 20, pnlCenter.Height - pnlZoom.Height - 20);
            };
            
            // LOGIC KÉO CHUỘT (PAN)
            pnlCenter.MouseDown += (s, e) => { if (e.Button == MouseButtons.Right) { isDragging = true; lastMousePos = e.Location; Cursor = Cursors.SizeAll; } };
            pnlCenter.MouseMove += (s, e) => { if (isDragging) { viewOffsetX += e.X - lastMousePos.X; viewOffsetY += e.Y - lastMousePos.Y; lastMousePos = e.Location; pnlCenter.Invalidate(); } };
            pnlCenter.MouseUp += (s, e) => { if (e.Button == MouseButtons.Right) { isDragging = false; Cursor = Cursors.Default; } };
            // LOGIC CUỘN CHUỘT (SCROLL)
            pnlCenter.MouseWheel += (s, e) => { viewOffsetY += e.Delta; pnlCenter.Invalidate(); };

            pnlStats = new Panel(){Size=new Size(300,220), BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle, Visible=false};
            Label stTitle = new Label(){Text="KẾT QUẢ ĐO LƯỜNG", Dock=DockStyle.Top, Height=35, TextAlign=ContentAlignment.MiddleCenter, BackColor=clrPrimary, ForeColor=Color.White, Font=new Font("Segoe UI",10,FontStyle.Bold)};
            lblStatsContent = new Label(){Dock=DockStyle.Fill, Padding=new Padding(10), Font=new Font("Consolas",10), ForeColor=clrText};
            pnlStats.Controls.AddRange(new Control[]{lblStatsContent, stTitle});
            pnlCenter.Controls.Add(pnlStats);
            this.Controls.AddRange(new Control[]{pnlCenter, pnlLeft, pnlTop});
        }

        private void InitZoomControls() {
            pnlZoom = new Panel() { Size = new Size(160, 40), BackColor = Color.WhiteSmoke, Visible = true }; 
            pnlZoom.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = RoundedRect(new RectangleF(0, 0, pnlZoom.Width, pnlZoom.Height), 15)) 
                using (Pen pen = new Pen(Color.Silver, 1)) { e.Graphics.FillPath(Brushes.White, path); e.Graphics.DrawPath(pen, path); }
            };
            btnZoomOut = new Button() { Text = "-", Width = 40, Height = 30, Location = new Point(5, 5), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.Black, Font = new Font("Arial", 12, FontStyle.Bold) };
            btnZoomOut.FlatAppearance.BorderSize = 0;
            btnZoomOut.Click += (s, e) => { if (currentZoom > 0.25f) { currentZoom -= 0.1f; UpdateZoomLabel(); pnlCenter.Invalidate(); } };
            btnZoomIn = new Button() { Text = "+", Width = 40, Height = 30, Location = new Point(115, 5), FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.Black, Font = new Font("Arial", 12, FontStyle.Bold) };
            btnZoomIn.FlatAppearance.BorderSize = 0;
            btnZoomIn.Click += (s, e) => { if (currentZoom < 3.0f) { currentZoom += 0.1f; UpdateZoomLabel(); pnlCenter.Invalidate(); } };
            lblZoomValue = new Label() { Text = "100%", AutoSize = false, Width = 60, Height = 30, Location = new Point(50, 5), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.Transparent };
            pnlZoom.Controls.AddRange(new Control[] { btnZoomOut, lblZoomValue, btnZoomIn });
            pnlCenter.Controls.Add(pnlZoom);
            pnlZoom.Location = new Point(pnlCenter.Width - pnlZoom.Width - 20, pnlCenter.Height - pnlZoom.Height - 20);
        }
        private void UpdateZoomLabel() => lblZoomValue.Text = $"{(int)(currentZoom * 100)}%";

        private void ChangeMode() {
            string s = cboAlgo.SelectedItem.ToString();
            // Nếu chọn dòng kẻ hoặc tiêu đề nhóm -> Reset
            if(s.Contains("───") || s.StartsWith("Mở rộng:")) { cboAlgo.SelectedIndex = 0; return; }

            animTimer.Stop(); pnlStats.Visible=false; lstLog.Items.Clear(); ResetData();
            lblOrder.Visible=false; cboOrder.Visible=false; lblTarget.Visible=false; nudTarget.Visible=false; lblRuns.Visible=true; nudRuns.Visible=true;
            btnPause.Enabled = false; btnSpeed.Enabled = true; 
            
            // LOGIC CHỌN MODE & CẬP NHẬT STATUS
            if(s.Contains("Tháp Hà Nội")) { 
                currentMode="HANOI"; lblN.Text="Số đĩa:"; nudN.Value=3; lblRuns.Visible=false; nudRuns.Visible=false;
                lblStatus.Text = "Nguyên lý: Sử dụng STACK để khử đệ quy, mô phỏng ngăn xếp lời gọi hàm.";
            }
            else {
                if(s.Contains("Sort")) { 
                    currentMode="SORT"; lblN.Text="Số phần tử:"; nudN.Value=20; lblOrder.Visible=true; cboOrder.Visible=true; 
                    lblStatus.Text = "Ứng dụng mở rộng: Dùng Stack/Queue trong các thuật toán Sắp xếp.";
                }
                else if(s.Contains("Linear")) { 
                    currentMode="LINEAR"; lblN.Text="Số phần tử:"; nudN.Value=15; lblTarget.Visible=true; nudTarget.Visible=true; 
                    lblStatus.Text = "So sánh hiệu năng tìm kiếm.";
                }
                else { 
                    currentMode="GRAPH"; lblN.Text="Số đỉnh:"; nudN.Value=10; lblTarget.Visible=true; nudTarget.Visible=true; 
                    lblStatus.Text = "Ứng dụng mở rộng: Dùng Stack (DFS) và Queue (BFS) duyệt đồ thị.";
                }
            }
        }

        private void ResetData() {
            graphNodes=null; masterData=null; mainStack=null; hanoiPegs=null; sortSteps=null; graphSteps=null; finalPath=null; 
            isPaused = false; btnPause.Text = "TẠM DỪNG";
            currentZoom = 1.0f; viewOffsetX = 0f; viewOffsetY = 0f; // Reset Camera
            UpdateZoomLabel(); pnlCenter.Invalidate();
        }

        private void TogglePause() { if (!animTimer.Enabled && !isPaused) return; isPaused = !isPaused; if (isPaused) { animTimer.Stop(); btnPause.Text = "TIẾP TỤC"; } else { animTimer.Start(); btnPause.Text = "TẠM DỪNG"; } }
        private void ToggleSpeed() { speedMode = (speedMode + 1) % 3; ApplySpeed(); }
        private void ApplySpeed() {
            if (speedMode == 0) { animTimer.Interval = 16; speedDivisor = 5; btnSpeed.Text = "TỐC ĐỘ: 1x"; } 
            else if (speedMode == 1) { animTimer.Interval = 1; speedDivisor = 1; btnSpeed.Text = "TỐC ĐỘ: 5x"; } 
            else { animTimer.Interval = 1; speedDivisor = 1; btnSpeed.Text = "TỐC ĐỘ: MAX"; }
        }

        private void InitData() {
            int n = (int)nudN.Value; ResetData(); pnlStats.Visible=false; Random rnd = new Random(); shouldDraw = true; 
            if(currentMode=="HANOI") { hanoiPegs=new List<int>[3]; for(int i=0;i<3;i++) hanoiPegs[i]=new List<int>(); for(int i=n; i>=1; i--) hanoiPegs[0].Add(i); Log($"Đã đặt {n} đĩa."); }
            else if(currentMode=="GRAPH") {
                graphNodes=new List<UiGraphNode>(); for(int i=1;i<=n;i++) graphNodes.Add(new UiGraphNode{Value=i});
                for(int i=1;i<n;i++) { int p = rnd.Next(0, i); graphNodes[p].Neighbors.Add(graphNodes[i].Value); graphNodes[i].Neighbors.Add(graphNodes[p].Value); }
                int w=pnlCenter.Width-350, startY=60, levelH=90; var levels=new Dictionary<int,List<UiGraphNode>>(); var q=new Queue<UiGraphNode>(); var vis=new HashSet<int>(); var dep=new Dictionary<int,int>();
                q.Enqueue(graphNodes[0]); vis.Add(graphNodes[0].Value); dep[graphNodes[0].Value]=0;
                while(q.Count>0) { var u=q.Dequeue(); int d=dep[u.Value]; if(!levels.ContainsKey(d)) levels[d]=new List<UiGraphNode>(); levels[d].Add(u); foreach(var nid in u.Neighbors) { var node=graphNodes.FirstOrDefault(x=>x.Value==nid); if(node!=null && !vis.Contains(nid)) { vis.Add(nid); dep[nid]=d+1; q.Enqueue(node); } } }
                foreach(var kv in levels) { int section = w / (kv.Value.Count+1); for(int i=0; i<kv.Value.Count; i++) kv.Value[i].Position = new Point((i+1)*section, startY + kv.Key*levelH); }
                Log($"Tạo cây {n} đỉnh.");
            }
            else { if (cboOrder.SelectedIndex == 0) mainStack = DataGenerator.TaoStackTangDan(n); else if (cboOrder.SelectedIndex == 1) mainStack = DataGenerator.TaoStackGiamDan(n); else mainStack = DataGenerator.TaoStackNgauNhien(n); masterData = mainStack.ToArray(); Log($"Đã sinh {n} số."); }
            pnlCenter.Invalidate();
        }

        private void RunAlgo() {
            if(currentMode=="HANOI") {
                if(hanoiPegs==null) return;
                try { HanoiUsingStack solver = new HanoiUsingStack(); solver.SolveIterative((int)nudN.Value); hanoiMoves = solver.Moves; hanoiMoveIndex = 0; hanoiState = 0; if (solver.Steps.Count > 1000) Log($"... và {solver.Steps.Count - 1000} bước nữa."); for(int i = Math.Min(solver.Steps.Count, 1000) - 1; i >= 0; i--) Log(solver.Steps[i]); StartAnim(); } catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); } return;
            }
            if((currentMode=="GRAPH" && graphNodes==null) || (currentMode!="GRAPH" && masterData==null)) { MessageBox.Show("Cần tạo dữ liệu!"); return; }

            int runs=(int)nudRuns.Value; string algo=cboAlgo.SelectedItem.ToString(); int t=(int)nudTarget.Value;
            List<double> times = new List<double>();
            
            if(currentMode == "SORT") { 
                var timeSort = new TimeSort(); 
                times = timeSort.RunBenchmark(runs, () => { 
                    MyStack<int> tempStack = new MyStack<int>(masterData.Length); 
                    foreach(var x in masterData) tempStack.Push(x); 
                    // GỌI ĐÚNG CLASS 'ThuanToanSapXep' NHƯ BẠN YÊU CẦU
                    if(algo.Contains("Insertion")) ThuanToanSapXep.InsertionSort(tempStack); 
                    else if(algo.Contains("Selection")) ThuanToanSapXep.SelectionSort(tempStack); 
                    else if(algo.Contains("Merge")) ThuanToanSapXep.MergeSort(tempStack); 
                }); 
            } 
            else if(currentMode == "LINEAR") { var timeSearch = new TimeSearch(); var traversable = new ArrayTraversable<int>(masterData); var finder = new LinearSearchFinder<int>(); times = timeSearch.RunBenchmark(runs, () => { finder.Search(traversable, t); }); }
            else if(currentMode == "GRAPH") { var timeSearch = new TimeSearch(); times = timeSearch.RunBenchmark(runs, () => { if(graphNodes.Count>0) { if (algo.Contains("DFS")) { MyStack<int> s = new MyStack<int>(); s.Push(graphNodes[0].Value); while(s.Count() > 0) s.Pop(); } else { MyStack<int> s1 = new MyStack<int>(); MyStack<int> s2 = new MyStack<int>(); s1.Push(graphNodes[0].Value); while(s1.Count() > 0 || s2.Count() > 0) { if(s2.Count() == 0) while(s1.Count() > 0) s2.Push(s1.Pop()); s2.Pop(); } } } }); }

            var analyzer = new TimeSort(); double avg = analyzer.Mean(times); double variance = analyzer.Variance(times, avg); double sd = Math.Sqrt(variance);
            lblStatsContent.Text = $"Số lần thử: {runs}\n------------------\nTrung Bình : {avg:F4} ms\nPhương Sai : {variance:F4}\nĐộ Lệch  : {sd:F4} ms";
            pnlStats.Location=new Point(pnlCenter.Width-pnlStats.Width-20, 20); pnlStats.Visible=true; pnlStats.BringToFront();
            if (!shouldDraw) { Log("Đã hoàn tất chạy ngầm."); return; }

            if(currentMode=="SORT") { sortSteps=new List<SortStep>(); int[] arr=(int[])masterData.Clone(); bool desc=cboOrder.SelectedIndex==1; sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=0, RedIndex=-1}); if(algo.Contains("Insertion")) { for(int i=1; i<arr.Length; i++) { int key=arr[i], j=i-1; sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i, RedIndex=i}); while(j>=0 && (desc ? arr[j]<key : arr[j]>key)) { arr[j+1]=arr[j]; j--; sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i, RedIndex=j+1}); } arr[j+1]=key; sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i+1, RedIndex=j+1}); } } else if(algo.Contains("Selection")) { for(int i=0; i<arr.Length-1; i++) { int m=i; for(int j=i+1; j<arr.Length; j++) { sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i, RedIndex=j}); if(desc ? arr[j]>arr[m] : arr[j]<arr[m]) m=j; } int tmp=arr[m]; arr[m]=arr[i]; arr[i]=tmp; sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i+1, RedIndex=-1}); } } else if(algo.Contains("Merge")) MergeSortAnim(arr, 0, arr.Length - 1, desc); sortSteps.Add(new SortStep{Arr=arr, YellowLimit=arr.Length, RedIndex=-1}); sortStepIndex=0; StartAnim(); }
            else if(currentMode=="LINEAR") { searchIndex=0; searchFound=false; StartAnim(); }
            else if(currentMode=="GRAPH") { graphSteps=new List<GraphStep>(); graphStepIndex=0; finalPath=null; if(algo.Contains("DFS")) RunDFS(t); else RunBFS(t); StartAnim(); }
        }

        private void StartAnim() { btnPause.Enabled = true; btnSpeed.Enabled = true; isPaused = false; btnPause.Text = "TẠM DỪNG"; ApplySpeed(); animTimer.Start(); }
        
        // VÒNG LẶP ANIMATION (CÓ CƠ CHẾ CHỐNG TREO MÁY)
        private void AnimTimer_Tick(object sender, EventArgs e) {
            bool isInstant = (speedMode == 2); int loopLimit = isInstant ? 5000 : 1; int loopCount = 0;
            do {
                bool finished = false;
                if(currentMode=="HANOI") { if(hanoiState==0) { if(hanoiMoveIndex>=hanoiMoves.Count){ finished=true; animTimer.Stop(); btnPause.Enabled=false; break; } Move m=hanoiMoves[hanoiMoveIndex]; int f = (m.From=='A')?0:(m.From=='B')?1:2; int t = (m.To=='A')?0:(m.To=='B')?1:2; if(hanoiPegs[f].Count==0)return; movingDiskVal=hanoiPegs[f].Last(); hanoiPegs[f].RemoveAt(hanoiPegs[f].Count-1); int w=pnlCenter.Width, h=pnlCenter.Height; startPos=new PointF(w*(f==0?1:f==1?3:5)/6, h-100-(hanoiPegs[f].Count+1)*25); endPos=new PointF(w*(t==0?1:t==1?3:5)/6, h-100-(hanoiPegs[t].Count+1)*25); currentPos=startPos; moveProgress=0; hanoiState=1; } else { moveProgress += (isInstant ? 1.0f : (speedMode == 1 ? 0.2f : 0.05f)); if(moveProgress>=1f){ Move m=hanoiMoves[hanoiMoveIndex]; int t = (m.To=='A')?0:(m.To=='B')?1:2; hanoiPegs[t].Add(movingDiskVal); movingDiskVal=-1; hanoiState=0; hanoiMoveIndex++; } else { float safeY=pnlCenter.Height/3; if(moveProgress<0.3f) currentPos.Y=startPos.Y+(safeY-startPos.Y)*(moveProgress/0.3f); else if(moveProgress<0.7f) { currentPos.Y=safeY; currentPos.X=startPos.X+(endPos.X-startPos.X)*((moveProgress-0.3f)/0.4f); } else currentPos.Y=safeY+(endPos.Y-safeY)*((moveProgress-0.7f)/0.3f); } } } 
                else { if (!isInstant) { frameCounter++; if (frameCounter % speedDivisor != 0) return; } if(currentMode=="SORT") { if(sortSteps!=null && sortStepIndex<sortSteps.Count) { sortStepIndex++; } else { finished=true; animTimer.Stop(); btnPause.Enabled=false; } } else if(currentMode=="LINEAR") { if(searchIndex>=masterData.Length){ finished=true; animTimer.Stop(); Log("Không tìm thấy."); } else if(masterData[searchIndex]==(int)nudTarget.Value){ finished=true; searchFound=true; animTimer.Stop(); Log($"Thấy tại {searchIndex}"); } else searchIndex++; } else if(currentMode=="GRAPH") { if(graphSteps!=null && graphStepIndex<graphSteps.Count) { if(!isInstant) Log(graphSteps[graphStepIndex].Msg); graphStepIndex++; } else { finished=true; animTimer.Stop(); if(finalPath != null && finalPath.Count > 0) Log("ĐƯỜNG ĐI: " + string.Join(" -> ", finalPath)); } } }
                if (finished) break; if (!isInstant) break; loopCount++; if (loopCount > loopLimit) break;
            } while (true); pnlCenter.Invalidate();
        }

        private void PnlCenter_Paint(object sender, PaintEventArgs e) {
            Graphics g=e.Graphics; g.SmoothingMode=SmoothingMode.HighQuality; int w=pnlCenter.Width, h=pnlCenter.Height;
            if(!shouldDraw) { g.DrawString("CHẾ ĐỘ CHẠY NGẦM", new Font("Segoe UI", 20, FontStyle.Bold), Brushes.Gray, w/2-140, h/2); return; }

            GraphicsState originalState = g.Save();
            
            // --- ÁP DỤNG CAMERA (Di chuyển & Zoom) ---
            g.TranslateTransform(w/2 + viewOffsetX, h/2 + viewOffsetY); 
            g.ScaleTransform(currentZoom, currentZoom);                 
            g.TranslateTransform(-w/2, -h/2);                           

            if(currentMode=="HANOI" && hanoiPegs!=null) { int by=h-80; int[] px={w/6, w/2, 5*w/6}; for(int p=0;p<3;p++) { using(SolidBrush bPost = new SolidBrush(Color.DimGray)) { g.FillRectangle(bPost, px[p]-4, by-250, 8, 250); g.FillRectangle(bPost, px[p]-60, by, 120, 8); g.DrawString($"Cọc {(char)('A'+p)}", new Font("Segoe UI",12,FontStyle.Bold), Brushes.Black, px[p]-25, by+15); } for(int i=0;i<hanoiPegs[p].Count;i++) DrawDisk(g, px[p], by-(i+1)*25, hanoiPegs[p][i]); } if(movingDiskVal!=-1) DrawDisk(g, (int)currentPos.X, (int)currentPos.Y, movingDiskVal); }
            else if(currentMode=="SORT") { if(sortSteps!=null) { var s=sortSteps[Math.Min(sortStepIndex,sortSteps.Count-1)]; DrawBars(g,w,h,s.Arr,s.YellowLimit,s.RedIndex); } else if(masterData!=null) DrawBars(g,w,h,masterData,-1,-1); }
            else if(currentMode=="LINEAR" && masterData!=null) { float bw=Math.Min(60, (w-40f)/masterData.Length-5); for(int i=0;i<masterData.Length;i++) { float x=30+i*(bw+5); Brush b = Brushes.White; Pen pen = Pens.Gray; if(animTimer.Enabled||searchFound){ if(i==searchIndex) { b=new SolidBrush(clrWarning); pen=new Pen(Color.OrangeRed,2); } if(searchFound&&i==searchIndex) { b=new SolidBrush(clrSuccess); pen=new Pen(Color.DarkGreen,2); } } using(GraphicsPath p = RoundedRect(new RectangleF(x,h/2-30,bw,bw), 8)) { g.FillPath(b, p); g.DrawPath(pen, p); } if(bw>20)g.DrawString(masterData[i].ToString(),new Font("Segoe UI",10, FontStyle.Bold),Brushes.Black,x+5,h/2-10); } }
            else if(currentMode=="GRAPH" && graphNodes!=null) { 
                var st=(graphSteps!=null&&graphStepIndex>0)?graphSteps[graphStepIndex-1]:new GraphStep(); 
                int minX = graphNodes.Min(n => n.Position.X); int maxX = graphNodes.Max(n => n.Position.X); int maxY = graphNodes.Max(n => n.Position.Y);
                int padding = 60; float requiredW = (maxX - minX) + padding * 2; float requiredH = maxY + padding * 2;
                float scaleX = (float)w / requiredW; float scaleY = (float)h / requiredH; float autoScale = Math.Min(scaleX, scaleY); if (autoScale > 1.0f) autoScale = 1.0f; 
                g.TranslateTransform(w/2, 0); g.ScaleTransform(autoScale, autoScale); g.TranslateTransform(-w/2, 0);
                using(Pen p=new Pen(Color.LightGray,2)) foreach(var n in graphNodes)foreach(var nb in n.Neighbors){var t=graphNodes.First(x=>x.Value==nb); if(n.Value<nb)g.DrawLine(p,n.Position,t.Position);} 
                if(finalPath!=null && !animTimer.Enabled) using(Pen p=new Pen(clrPrimary, 5)) for(int i=0;i<finalPath.Count-1;i++){var p1=graphNodes.First(x=>x.Value==finalPath[i]).Position;var p2=graphNodes.First(x=>x.Value==finalPath[i+1]).Position;g.DrawLine(p,p1,p2);} 
                foreach(var n in graphNodes){ SolidBrush b = new SolidBrush(Color.White); Pen pen = new Pen(Color.Gray, 1); if(st.Visited!=null&&st.Visited.Contains(n.Value)) { b = new SolidBrush(clrSuccess); pen = new Pen(Color.DarkGreen, 2); } if(st.CurrentNode==n.Value) { b = new SolidBrush(clrDanger); pen = new Pen(Color.Maroon, 2); } if(st.Stack1!=null&&st.Stack1.Contains(n.Value)) { b = new SolidBrush(clrWarning); pen = new Pen(Color.OrangeRed, 2); } g.FillEllipse(b,n.Position.X-20,n.Position.Y-20,40,40); g.DrawEllipse(pen,n.Position.X-20,n.Position.Y-20,40,40); if(autoScale * currentZoom > 0.5f) g.DrawString(n.Value.ToString(),new Font("Segoe UI",10,FontStyle.Bold),(b.Color==Color.White||b.Color==clrWarning)?Brushes.Black:Brushes.White,n.Position.X-10,n.Position.Y-9); } 
            }
            g.Restore(originalState);
            if(currentMode == "GRAPH") { var st=(graphSteps!=null&&graphStepIndex>0)?graphSteps[graphStepIndex-1]:new GraphStep(); DrawBucket(g,w-240,h-50,"STACK 1",st.Stack1); if(cboAlgo.SelectedItem.ToString().Contains("BFS")) DrawBucket(g,w-120,h-50,"STACK 2",st.Stack2); }
        }
        
        void DrawDisk(Graphics g, int x, int y, int v) { int totalN = (int)nudN.Value; float maxW = pnlCenter.Width / 3.5f; float diskStep = maxW / totalN; float w = 20 + v * diskStep; float h = 24; using(GraphicsPath path = RoundedRect(new RectangleF(x-w/2, y, w, h), 6)) using(SolidBrush b = new SolidBrush(clrDanger)) { g.FillPath(b, path); g.DrawPath(Pens.Maroon, path); } if(totalN <= 20) g.DrawString(v.ToString(), new Font("Segoe UI", 10, FontStyle.Bold), Brushes.White, x-7, y+3); }
        void DrawBars(Graphics g, int w, int h, int[] a, int yl, int ri) { 
            float mx=a.Length>0?a.Max():1, bw=(w-40f)/a.Length; 
            // LOGIC HIỆN SỐ THÔNG MINH KHI ZOOM
            float actualWidth = bw * currentZoom;
            using(SolidBrush bNormal = new SolidBrush(clrPrimary)) using(SolidBrush bDone = new SolidBrush(clrWarning)) using(SolidBrush bActive = new SolidBrush(clrDanger)) { 
                for(int i=0;i<a.Length;i++) { 
                    float bh=(a[i]/mx)*(h*0.8f); float x=20+i*bw, y=h-20-bh; 
                    Brush b = bNormal; if (yl != -1 && i < yl) b = bDone; if (i==ri) b = bActive; 
                    GraphicsPath path = new GraphicsPath(); path.StartFigure(); path.AddArc(x, y, bw, bw>10?10:bw, 180, 90); path.AddArc(x+bw-(bw>10?10:bw), y, bw>10?10:bw, bw>10?10:bw, 270, 90); path.AddLine(x+bw, y, x+bw, y+bh); path.AddLine(x+bw, y+bh, x, y+bh); path.CloseFigure(); g.FillPath(b, path); 
                    if(actualWidth > 15) g.DrawString(a[i].ToString(),new Font("Segoe UI",8), Brushes.Black, x, y-15); 
                } 
            } 
        }
        void DrawBucket(Graphics g, int x, int by, string t, List<int> l) { using(Pen p = new Pen(clrPrimary, 3)) { g.DrawString(t,new Font("Segoe UI",10,FontStyle.Bold),Brushes.Black,x-30,40); g.DrawLine(p,x-40,60,x-40,by); g.DrawLine(p,x+40,60,x+40,by); g.DrawLine(p,x-40,by,x+40,by); } if(l!=null && l.Count > 0) { float availableH = by - 65; float itemH = 30; if (l.Count * 35 > availableH) { itemH = availableH / l.Count; } for(int i=0;i<l.Count;i++) { float yPos = by - (i+1)*(itemH + (itemH<5?0:5)); g.FillRectangle(new SolidBrush(clrWarning),x-35, yPos, 70, itemH); if(itemH > 5) g.DrawRectangle(Pens.Black,x-35, yPos, 70, itemH); if(itemH > 15) g.DrawString(l[l.Count-1-i].ToString(),new Font("Segoe UI", itemH/2 + 2, FontStyle.Bold),Brushes.Black,x-10, yPos + itemH/2 - 7); } } }
        void Log(string s) => lstLog.Items.Insert(0, s);
        void MergeSortAnim(int[] arr, int l, int r, bool desc) { if(l<r){int m=l+(r-l)/2;MergeSortAnim(arr,l,m,desc);MergeSortAnim(arr,m+1,r,desc);MergeAnim(arr,l,m,r,desc);} }
        void MergeAnim(int[] arr, int l, int m, int r, bool desc) { int n1=m-l+1,n2=r-m; int[] L=new int[n1],R=new int[n2]; Array.Copy(arr,l,L,0,n1); Array.Copy(arr,m+1,R,0,n2); int i=0,j=0,k=l; while(i<n1&&j<n2) { bool c=desc?L[i]>=R[j]:L[i]<=R[j]; sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(),YellowLimit=-1,RedIndex=k}); if(c)arr[k++]=L[i++];else arr[k++]=R[j++]; } while(i<n1){sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(),YellowLimit=-1,RedIndex=k});arr[k++]=L[i++];} while(j<n2){sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(),YellowLimit=-1,RedIndex=k});arr[k++]=R[j++];} }
        void RunDFS(int t) { MyStack<int> s = new MyStack<int>(); HashSet<int> v = new HashSet<int>(); Dictionary<int,int> p = new Dictionary<int,int>(); s.Push(graphNodes[0].Value); bool f = false; RecordGraph(graphNodes[0].Value, v, s.ToArray().ToList(), null, $"Start: Push {graphNodes[0].Value}"); while(s.Count() > 0){ int u = s.Pop(); RecordGraph(u,v,s.ToArray().ToList(),null,$"Pop {u}"); if(!v.Contains(u)){ v.Add(u); if(u==t){ f=true; BuildPath(p,graphNodes[0].Value,t); RecordGraph(u,v,s.ToArray().ToList(),null,"TÌM THẤY!"); break; } var nb = graphNodes.First(x=>x.Value==u).Neighbors.OrderByDescending(x=>x).ToList(); foreach(var n in nb) if(!v.Contains(n)) { s.Push(n); if(!p.ContainsKey(n)) p[n]=u; RecordGraph(u, v, s.ToArray().ToList(), null, $"Push {n}"); } } } if(!f) RecordGraph(-1,v,s.ToArray().ToList(),null,"Không thấy đích!"); }
        void RunBFS(int t) { MyStack<int> sIn = new MyStack<int>(); MyStack<int> sOut = new MyStack<int>(); HashSet<int> v = new HashSet<int>(); Dictionary<int,int> p = new Dictionary<int,int>(); int start = graphNodes[0].Value; sIn.Push(start); v.Add(start); RecordGraph(start, v, sIn.ToArray().ToList(), sOut.ToArray().ToList(), $"Start: Push {start}"); bool f = false; while(sIn.Count() > 0 || sOut.Count() > 0){ if(sOut.Count() == 0) while(sIn.Count() > 0) sOut.Push(sIn.Pop()); int u = sOut.Pop(); RecordGraph(u,v,sIn.ToArray().ToList(),sOut.ToArray().ToList(),$"Pop {u}"); if(u==t){ f=true; BuildPath(p,start,t); RecordGraph(u,v,sIn.ToArray().ToList(),sOut.ToArray().ToList(),"TÌM THẤY!"); break; } var nb = graphNodes.First(x=>x.Value==u).Neighbors.OrderBy(x=>x).ToList(); foreach(var n in nb) if(!v.Contains(n)){ v.Add(n); sIn.Push(n); p[n]=u; RecordGraph(u, v, sIn.ToArray().ToList(), sOut.ToArray().ToList(), $"Push {n}"); } } if(!f) RecordGraph(-1,v,sIn.ToArray().ToList(),sOut.ToArray().ToList(),"Không thấy đích!"); }
        void BuildPath(Dictionary<int,int> p, int s, int e) { finalPath=new List<int>(); int c=e; while(c!=s && p.ContainsKey(c)) { finalPath.Add(c); c=p[c]; } finalPath.Add(s); finalPath.Reverse(); }
        void RecordGraph(int c, HashSet<int> v, List<int> s1, List<int> s2, string m) { var st=new GraphStep{CurrentNode=c, Visited=new HashSet<int>(v), Stack1=s1, Msg=m}; if(s2!=null) st.Stack2=s2; graphSteps.Add(st); }
    }
}
