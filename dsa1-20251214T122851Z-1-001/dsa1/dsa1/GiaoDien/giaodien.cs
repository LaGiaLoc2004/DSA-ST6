using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics; 
using System.Linq; 

namespace dsa1 
{
    // Class Panel chống nháy (Double Buffering)
    public class SuperBufferedPanel : Panel {
        public SuperBufferedPanel() {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }

    // Class Node để vẽ đồ thị
    public class UiGraphNode { 
        public int Value; 
        public Point Position; 
        public List<int> Neighbors = new List<int>(); 
    }

    public partial class Form1 : Form
    {
        // Các control giao diện
        private Panel pnlTop, pnlLeft, pnlStats;
        private SuperBufferedPanel pnlCenter;
        private ComboBox cboAlgo, cboOrder;
        private Label lblN, lblOrder, lblTarget, lblRuns, lblStatsContent;
        private NumericUpDown nudN, nudTarget, nudRuns;
        private Button btnInit, btnRun, btnPause, btnSpeed;
        private GroupBox grpInput, grpOutput; 
        private ListBox lstLog;

        // Dữ liệu lõi
        private MyStack<int> mainStack; // Stack lấy từ file MyStack.cs
        private int[] masterData; 
        private List<UiGraphNode> graphNodes; 
        private List<int> finalPath; // Lưu đường đi tìm được

        // Biến điều khiển hoạt ảnh (Animation)
        private System.Windows.Forms.Timer animTimer;
        private string currentMode = "HANOI";
        private bool isPaused = false;
        private bool isFastMode = false;
        private bool shouldDraw = true;

        // Biến cho thuật toán
        private List<int>[] hanoiPegs; 
        private List<Move> hanoiMoves; 
        private int hanoiMoveIndex, movingDiskVal = -1;
        private PointF startPos, endPos, currentPos; 
        private int hanoiState = 0;
        private float moveProgress = 0f;

        private int searchIndex = 0; 
        private bool searchFound = false;
        
        // Struct lưu bước vẽ đồ thị
        private struct GraphStep { public int CurrentNode; public HashSet<int> Visited; public List<int> Stack1; public List<int> Stack2; public string Msg; }
        private List<GraphStep> graphSteps; private int graphStepIndex = 0;

        // Struct lưu bước vẽ sắp xếp
        private struct SortStep { public int[] Arr; public int YellowLimit; public int RedIndex; }
        private List<SortStep> sortSteps; private int sortStepIndex = 0;

        public Form1() {
            InitCustomGUI();
            animTimer = new System.Windows.Forms.Timer();
            animTimer.Interval = 42; 
            animTimer.Tick += AnimTimer_Tick;
            cboAlgo.SelectedIndex = 0;
        }

        // Khởi tạo giao diện bằng code
        private void InitCustomGUI() {
            this.Size = new Size(1350, 850);
            this.Text = "Mô phỏng Thuật toán & Cấu trúc dữ liệu";
            this.StartPosition = FormStartPosition.CenterScreen;

            // Panel trên cùng
            pnlTop = new Panel() { Dock=DockStyle.Top, Height=60, BackColor=Color.MidnightBlue };
            Label t = new Label(){Text="THUẬT TOÁN:", Location=new Point(20,18), AutoSize=true, Font=new Font("Segoe UI",12,FontStyle.Bold), ForeColor=Color.White};
            cboAlgo = new ComboBox(){Location=new Point(150,15), Width=300, Font=new Font("Segoe UI",11), DropDownStyle=ComboBoxStyle.DropDownList};
            cboAlgo.Items.AddRange(new string[]{"Bài toán: Tháp Hà Nội", "Sắp xếp: Insertion Sort", "Sắp xếp: Selection Sort", "Sắp xếp: Merge Sort", "Tìm kiếm: Linear Search", "Tìm kiếm: BFS (2 Stack)", "Tìm kiếm: DFS"});
            cboAlgo.SelectedIndexChanged += (s,e) => ChangeMode();
            pnlTop.Controls.AddRange(new Control[]{t, cboAlgo});

            // Panel bên trái (Điều khiển)
            pnlLeft = new Panel(){Dock=DockStyle.Left, Width=290, BackColor=Color.WhiteSmoke, Padding=new Padding(10)};
            grpInput = new GroupBox(){Text="ĐIỀU KHIỂN", Dock=DockStyle.Top, Height=420, Font=new Font("Segoe UI",10,FontStyle.Bold)};
            
            lblN=new Label(){Text="Số lượng (N):", Location=new Point(15,30), AutoSize=true, Font=new Font("Segoe UI",9)};
            nudN=new NumericUpDown(){Location=new Point(15,55), Width=120, Minimum=1, Maximum=100000, Value=20}; 
            lblOrder=new Label(){Text="Thứ tự:", Location=new Point(140,30), AutoSize=true, Font=new Font("Segoe UI",9)};
            cboOrder=new ComboBox(){Location=new Point(140,55), Width=120, DropDownStyle=ComboBoxStyle.DropDownList};
            cboOrder.Items.AddRange(new object[]{"Tăng dần","Giảm dần", "Ngẫu nhiên"}); cboOrder.SelectedIndex=2;
            lblTarget=new Label(){Text="Tìm số:", Location=new Point(15,90), AutoSize=true, Font=new Font("Segoe UI",9), Visible=false};
            nudTarget=new NumericUpDown(){Location=new Point(15,115), Width=120, Maximum=100000, Visible=false};
            lblRuns=new Label(){Text="Số lần đo:", Location=new Point(140,90), AutoSize=true, Font=new Font("Segoe UI",9)};
            nudRuns=new NumericUpDown(){Location=new Point(140,115), Width=120, Minimum=1, Maximum=100000, Value=1};

            btnInit=new Button(){Text="1. TẠO DỮ LIỆU", Location=new Point(15,170), Width=245, Height=40, BackColor=Color.PowderBlue, FlatStyle=FlatStyle.Flat};
            btnRun=new Button(){Text="2. CHẠY MÔ PHỎNG", Location=new Point(15,220), Width=245, Height=40, BackColor=Color.LightGreen, FlatStyle=FlatStyle.Flat};
            btnPause=new Button(){Text="TẠM DỪNG", Location=new Point(15,270), Width=115, Height=40, BackColor=Color.LightYellow, FlatStyle=FlatStyle.Flat, Enabled=false};
            btnSpeed=new Button(){Text="TỐC ĐỘ: 1x", Location=new Point(145,270), Width=115, Height=40, BackColor=Color.LightSalmon, FlatStyle=FlatStyle.Flat, Enabled=false};

            btnInit.Click += (s,e) => InitData(); btnRun.Click += (s,e) => RunAlgo();
            btnPause.Click += (s,e) => TogglePause(); btnSpeed.Click += (s,e) => ToggleSpeed();

            grpInput.Controls.AddRange(new Control[]{lblN, nudN, lblOrder, cboOrder, lblTarget, nudTarget, lblRuns, nudRuns, btnInit, btnRun, btnPause, btnSpeed});
            grpOutput = new GroupBox(){Text="LOG", Dock=DockStyle.Fill, Font=new Font("Segoe UI",10,FontStyle.Bold)};
            lstLog = new ListBox(){Dock=DockStyle.Fill, Font=new Font("Consolas",9), BorderStyle=BorderStyle.None};
            grpOutput.Controls.Add(lstLog);
            pnlLeft.Controls.Add(grpOutput); pnlLeft.Controls.Add(grpInput);

            // Panel trung tâm (Vẽ hình)
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

        // Thay đổi chế độ khi chọn ComboBox
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
            sortSteps=null; graphSteps=null; finalPath=null; 
            isPaused = false; btnPause.Text = "TẠM DỪNG";
            pnlCenter.Invalidate();
        }

        private void TogglePause() { if (!animTimer.Enabled && !isPaused) return; isPaused = !isPaused; if (isPaused) { animTimer.Stop(); btnPause.Text = "TIẾP TỤC"; } else { animTimer.Start(); btnPause.Text = "TẠM DỪNG"; } }
        private void ToggleSpeed() { isFastMode = !isFastMode; if (isFastMode) { animTimer.Interval = 5; btnSpeed.Text = "TỐC ĐỘ: MAX"; } else { animTimer.Interval = 42; btnSpeed.Text = "TỐC ĐỘ: 1x"; } }

        // Khởi tạo dữ liệu
        private void InitData() {
            int n = (int)nudN.Value; ResetData(); pnlStats.Visible=false;
            Random rnd = new Random();

            shouldDraw = (currentMode != "SORT" || n <= 200) && (currentMode != "LINEAR" || n <= 100);
            if(currentMode == "HANOI" && n > 8) { n=8; nudN.Value=8; Log("! Tự chỉnh N=8."); }
            if (!shouldDraw) Log($"⚠ Dữ liệu lớn ({n}). Chế độ: CHẠY NGẦM.");

            if(currentMode=="HANOI") {
                hanoiPegs=new List<int>[3]; for(int i=0;i<3;i++) hanoiPegs[i]=new List<int>();
                for(int i=n; i>=1; i--) hanoiPegs[0].Add(i);
                Log($"Đã đặt {n} đĩa.");
            }
            else if(currentMode=="GRAPH") {
                graphNodes=new List<UiGraphNode>();
                for(int i=1;i<=n;i++) graphNodes.Add(new UiGraphNode{Value=i});
                for(int i=1;i<n;i++) { int p = rnd.Next(0, i); graphNodes[p].Neighbors.Add(graphNodes[i].Value); graphNodes[i].Neighbors.Add(graphNodes[p].Value); }
                
                // Thuật toán Layout cây để vẽ đẹp
                if (shouldDraw) { 
                    int w=pnlCenter.Width-350, startY=60, levelH=90;
                    var levels=new Dictionary<int,List<UiGraphNode>>(); var q=new Queue<UiGraphNode>(); var vis=new HashSet<int>(); var dep=new Dictionary<int,int>();
                    q.Enqueue(graphNodes[0]); vis.Add(graphNodes[0].Value); dep[graphNodes[0].Value]=0;
                    while(q.Count>0) {
                        var u=q.Dequeue(); int d=dep[u.Value]; if(!levels.ContainsKey(d)) levels[d]=new List<UiGraphNode>(); levels[d].Add(u);
                        foreach(var nid in u.Neighbors) { var node=graphNodes.FirstOrDefault(x=>x.Value==nid); if(node!=null && !vis.Contains(nid)) { vis.Add(nid); dep[nid]=d+1; q.Enqueue(node); } }
                    }
                    foreach(var kv in levels) { int section = w / (kv.Value.Count+1); for(int i=0; i<kv.Value.Count; i++) kv.Value[i].Position = new Point((i+1)*section, startY + kv.Key*levelH); }
                }
                Log($"Tạo cây {n} đỉnh.");
            }
            else { 
                // Gọi lớp DataGenerator
                if (cboOrder.SelectedIndex == 0) mainStack = DataGenerator.TaoStackTangDan(n);
                else if (cboOrder.SelectedIndex == 1) mainStack = DataGenerator.TaoStackGiamDan(n);
                else mainStack = DataGenerator.TaoStackNgauNhien(n);

                masterData = mainStack.ToArray();
                Log($"Đã sinh {n} số.");
            }
            pnlCenter.Invalidate();
        }

        // Chạy thuật toán
        private void RunAlgo() {
            if(currentMode=="HANOI") {
                if(hanoiPegs==null) return;
                try {
                    HanoiUsingStack solver = new HanoiUsingStack();
                    solver.SolveIterative((int)nudN.Value);
                    hanoiMoves = solver.Moves;
                    hanoiMoveIndex = 0; hanoiState = 0;
                    foreach(var s in solver.Steps) Log(s);
                    StartAnim();
                } catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                return;
            }

            if((currentMode=="GRAPH" && graphNodes==null) || (currentMode!="GRAPH" && masterData==null)) { MessageBox.Show("Cần tạo dữ liệu!"); return; }

            int runs=(int)nudRuns.Value; string algo=cboAlgo.SelectedItem.ToString(); int t=(int)nudTarget.Value;
            
            // BENCHMARK (Đo thời gian)
            List<double> times=new List<double>();
            for(int i=0;i<runs;i++) {
                Stopwatch sw=Stopwatch.StartNew();
                if(currentMode=="SORT") {
                    MyStack<int> tempStack = new MyStack<int>(masterData.Length);
                    foreach(var x in masterData) tempStack.Push(x);
                    if(algo.Contains("Insertion")) ThuanToanSapXep.InsertionSort(tempStack);
                    else if(algo.Contains("Selection")) ThuanToanSapXep.SelectionSort(tempStack);
                    else if(algo.Contains("Merge")) ThuanToanSapXep.MergeSort(tempStack);
                } 
                else if(currentMode=="LINEAR") {
                    var finder = new ThuatToanTimKiem.LinearSearchFinder<int>();
                    var traversable = new ArrayTraversable<int>(masterData);
                    finder.Search(traversable, t);
                } 
                else if(currentMode=="GRAPH") {
                    if(graphNodes.Count>0) { if (algo.Contains("DFS")) { Stack<int> s=new Stack<int>(); s.Push(graphNodes[0].Value); while(s.Count>0) s.Pop(); } else { Queue<int> q=new Queue<int>(); q.Enqueue(graphNodes[0].Value); while(q.Count>0) q.Dequeue(); } }
                }
                sw.Stop(); times.Add(sw.Elapsed.TotalMilliseconds);
            }
            
            // Tính toán và hiển thị thống kê
            double avg = times.Count > 0 ? times.Average() : 0;
            double variance = times.Count > 1 ? times.Sum(v => Math.Pow(v - avg, 2)) / (times.Count - 1) : 0;
            double sd = Math.Sqrt(variance);
            
            lblStatsContent.Text = $"Số lần thử: {runs}\n------------------\nTrung Bình : {avg:F4} ms\nPhương Sai : {variance:F4}\nĐộ Lệch  : {sd:F4} ms";
            pnlStats.Location=new Point(pnlCenter.Width-pnlStats.Width-10, 10); pnlStats.Visible=true; pnlStats.BringToFront();

            if (!shouldDraw) { Log("Đã hoàn tất chạy ngầm."); return; }

            // Chuẩn bị dữ liệu để vẽ hoạt ảnh
            if(currentMode=="SORT") {
                sortSteps=new List<SortStep>();
                int[] arr=(int[])masterData.Clone(); bool desc=cboOrder.SelectedIndex==1;
                sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=0, RedIndex=-1});
                
                if(algo.Contains("Insertion")) {
                    for(int i=1; i<arr.Length; i++) { 
                        int key=arr[i], j=i-1; 
                        sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i, RedIndex=i}); 
                        while(j>=0 && (desc ? arr[j]<key : arr[j]>key)) { arr[j+1]=arr[j]; j--; sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i, RedIndex=j+1}); } 
                        arr[j+1]=key; sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i+1, RedIndex=j+1}); 
                    }
                } else if(algo.Contains("Selection")) {
                    for(int i=0; i<arr.Length-1; i++) { 
                        int m=i; 
                        for(int j=i+1; j<arr.Length; j++) { sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i, RedIndex=j}); if(desc ? arr[j]>arr[m] : arr[j]<arr[m]) m=j; } 
                        int tmp=arr[m]; arr[m]=arr[i]; arr[i]=tmp; sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(), YellowLimit=i+1, RedIndex=-1}); 
                    }
                } else if(algo.Contains("Merge")) MergeSortAnim(arr, 0, arr.Length - 1, desc);
                
                sortSteps.Add(new SortStep{Arr=arr, YellowLimit=arr.Length, RedIndex=-1}); sortStepIndex=0; StartAnim();
            }
            else if(currentMode=="LINEAR") { searchIndex=0; searchFound=false; StartAnim(); }
            else if(currentMode=="GRAPH") { 
                graphSteps=new List<GraphStep>(); graphStepIndex=0; finalPath=null; 
                if(algo.Contains("DFS")) RunDFS(t); else RunBFS(t); 
                StartAnim(); 
            }
        }

        private void StartAnim() { btnPause.Enabled = true; btnSpeed.Enabled = true; isPaused = false; btnPause.Text = "TẠM DỪNG"; animTimer.Start(); }

        // Logic xử lý khi Timer chạy
        private void AnimTimer_Tick(object sender, EventArgs e) {
            if(currentMode=="HANOI") {
                if(hanoiState==0) { 
                    if(hanoiMoveIndex>=hanoiMoves.Count){animTimer.Stop();btnPause.Enabled=false;return;} 
                    Move m=hanoiMoves[hanoiMoveIndex]; 
                    int f = (m.From=='A')?0:(m.From=='B')?1:2; 
                    int t = (m.To=='A')?0:(m.To=='B')?1:2;
                    if(hanoiPegs[f].Count==0)return; 
                    movingDiskVal=hanoiPegs[f].Last(); hanoiPegs[f].RemoveAt(hanoiPegs[f].Count-1); 
                    int w=pnlCenter.Width, h=pnlCenter.Height;
                    startPos=new PointF(w*(f==0?1:f==1?3:5)/6, h-50-(hanoiPegs[f].Count+1)*25); 
                    endPos=new PointF(w*(t==0?1:t==1?3:5)/6, h-50-(hanoiPegs[t].Count+1)*25); 
                    currentPos=startPos; moveProgress=0; hanoiState=1; 
                }
                else { 
                    moveProgress += (isFastMode ? 0.3f : 0.1f); 
                    if(moveProgress>=1f){ 
                        Move m=hanoiMoves[hanoiMoveIndex]; int t = (m.To=='A')?0:(m.To=='B')?1:2;
                        hanoiPegs[t].Add(movingDiskVal); movingDiskVal=-1; hanoiState=0; hanoiMoveIndex++; 
                    } else { 
                        float safeY=pnlCenter.Height/3; 
                        if(moveProgress<0.3f) currentPos.Y=startPos.Y+(safeY-startPos.Y)*(moveProgress/0.3f); 
                        else if(moveProgress<0.7f) { currentPos.Y=safeY; currentPos.X=startPos.X+(endPos.X-startPos.X)*((moveProgress-0.3f)/0.4f); } 
                        else currentPos.Y=safeY+(endPos.Y-safeY)*((moveProgress-0.7f)/0.3f); 
                    } 
                } 
                pnlCenter.Invalidate();
            } else if(currentMode=="SORT") { 
                if(sortSteps!=null && sortStepIndex<sortSteps.Count) { sortStepIndex++; pnlCenter.Invalidate(); } else { animTimer.Stop(); btnPause.Enabled=false; } 
            } else if(currentMode=="LINEAR") { 
                if(searchIndex>=masterData.Length){animTimer.Stop();Log("Không tìm thấy.");} else if(masterData[searchIndex]==(int)nudTarget.Value){searchFound=true;animTimer.Stop();Log($"Thấy tại {searchIndex}");} else searchIndex++; pnlCenter.Invalidate(); 
            } else if(currentMode=="GRAPH") { 
                if(graphSteps!=null && graphStepIndex<graphSteps.Count) { 
                    Log(graphSteps[graphStepIndex].Msg); 
                    graphStepIndex++; 
                    pnlCenter.Invalidate(); 
                } else { 
                    animTimer.Stop(); // Dừng hoạt ảnh
                    // IN ĐƯỜNG ĐI Ở ĐÂY (Sau khi chạy xong)
                    if(finalPath != null && finalPath.Count > 0) {
                        Log("=========================");
                        Log("ĐƯỜNG ĐI: " + string.Join(" -> ", finalPath));
                    }
                }
            }
        }

        // Logic vẽ đồ họa
        private void PnlCenter_Paint(object sender, PaintEventArgs e) {
            Graphics g=e.Graphics; g.SmoothingMode=SmoothingMode.AntiAlias; int w=pnlCenter.Width, h=pnlCenter.Height;
            if(!shouldDraw) { g.DrawString("CHẾ ĐỘ CHẠY NGẦM", new Font("Arial", 18, FontStyle.Bold), Brushes.Gray, w/2-100, h/2); return; }

            if(currentMode=="HANOI" && hanoiPegs!=null) { 
                int by=h-100; int[] px={w/6, w/2, 5*w/6}; 
                for(int p=0;p<3;p++) { 
                    g.FillRectangle(Brushes.SaddleBrown, px[p]-5, by-250, 10, 250); g.FillRectangle(Brushes.Gray, px[p]-60, by, 120, 10); g.DrawString($"Cọc {(char)('A'+p)}", new Font("Arial",12,FontStyle.Bold), Brushes.Black, px[p]-10, by+10); 
                    for(int i=0;i<hanoiPegs[p].Count;i++) DrawDisk(g, px[p], by-(i+1)*25, hanoiPegs[p][i]); 
                } 
                if(movingDiskVal!=-1) DrawDisk(g, (int)currentPos.X, (int)currentPos.Y, movingDiskVal); 
            }
            else if(currentMode=="SORT") { 
                if(sortSteps!=null) { 
                    var s=sortSteps[Math.Min(sortStepIndex,sortSteps.Count-1)]; 
                    DrawBars(g,w,h,s.Arr,s.YellowLimit,s.RedIndex); 
                } else if(masterData!=null) DrawBars(g,w,h,masterData,-1,-1); 
            }
            else if(currentMode=="LINEAR" && masterData!=null) { float bw=Math.Min(50, (w-40f)/masterData.Length-2); for(int i=0;i<masterData.Length;i++) { float x=20+i*(bw+2); Brush b=Brushes.White; if(animTimer.Enabled||searchFound){if(i==searchIndex)b=Brushes.Yellow; if(searchFound&&i==searchIndex)b=Brushes.LightGreen;} g.FillRectangle(b,x,h/2,bw,bw); g.DrawRectangle(Pens.Black,x,h/2,bw,bw); if(bw>15)g.DrawString(masterData[i].ToString(),new Font("Arial",8),Brushes.Black,x+2,h/2+5); } }
            else if(currentMode=="GRAPH" && graphNodes!=null) { var st=(graphSteps!=null&&graphStepIndex>0)?graphSteps[graphStepIndex-1]:new GraphStep(); using(Pen p=new Pen(Color.LightGray,2)) foreach(var n in graphNodes)foreach(var nb in n.Neighbors){var t=graphNodes.First(x=>x.Value==nb); if(n.Value<nb)g.DrawLine(p,n.Position,t.Position);} if(finalPath!=null&&!animTimer.Enabled)using(Pen p=new Pen(Color.Blue,4))for(int i=0;i<finalPath.Count-1;i++){var p1=graphNodes.First(x=>x.Value==finalPath[i]).Position;var p2=graphNodes.First(x=>x.Value==finalPath[i+1]).Position;g.DrawLine(p,p1,p2);} foreach(var n in graphNodes){Brush b=Brushes.White; if(st.Visited!=null&&st.Visited.Contains(n.Value))b=Brushes.LightGreen; if(st.CurrentNode==n.Value)b=Brushes.OrangeRed; bool inS=(st.Stack1!=null&&st.Stack1.Contains(n.Value)); if(inS)b=Brushes.Gold; g.FillEllipse(b,n.Position.X-20,n.Position.Y-20,40,40); g.DrawEllipse(Pens.Black,n.Position.X-20,n.Position.Y-20,40,40); g.DrawString(n.Value.ToString(),new Font("Arial",10),Brushes.Black,n.Position.X-10,n.Position.Y-8); } DrawBucket(g,w-240,h-50,"STACK 1",st.Stack1); if(cboAlgo.SelectedItem.ToString().Contains("BFS"))DrawBucket(g,w-120,h-50,"STACK 2",st.Stack2); }
        }
        
        // Vẽ đĩa Hà Nội (Có số)
        void DrawDisk(Graphics g, int x, int y, int v) { 
            int w=40+v*30; 
            g.FillRectangle(Brushes.OrangeRed,x-w/2,y,w,20); 
            g.DrawRectangle(Pens.Maroon,x-w/2,y,w,20); 
            g.DrawString(v.ToString(), new Font("Arial", 9, FontStyle.Bold), Brushes.White, x-6, y+3);
        }

        // Vẽ cột Sắp xếp (Có tô màu vàng vùng đã xếp)
        void DrawBars(Graphics g, int w, int h, int[] a, int yl, int ri) { 
            float mx=a.Length>0?a.Max():1, bw=(w-40f)/a.Length; 
            for(int i=0;i<a.Length;i++) { 
                float bh=(a[i]/mx)*(h*0.8f); float x=20+i*bw, y=h-20-bh; 
                Brush b=Brushes.SteelBlue; 
                if (yl != -1 && i < yl) b = Brushes.Gold; 
                if (i==ri) b=Brushes.Red; 
                g.FillRectangle(b,x,y,bw,bh); 
                if(bw>20)g.DrawString(a[i].ToString(),new Font("Arial",8),Brushes.Black,x,y-15); 
            } 
        }

        void DrawBucket(Graphics g, int x, int by, string t, List<int> l) { g.DrawString(t,new Font("Arial",10),Brushes.Navy,x-30,40); using(Pen p=new Pen(Color.Navy,3)){g.DrawLine(p,x-40,60,x-40,by);g.DrawLine(p,x+40,60,x+40,by);g.DrawLine(p,x-40,by,x+40,by);} if(l!=null)for(int i=0;i<l.Count;i++) { g.FillRectangle(Brushes.Gold,x-35,by-(i+1)*35,70,30); g.DrawRectangle(Pens.Black,x-35,by-(i+1)*35,70,30); g.DrawString(l[l.Count-1-i].ToString(),new Font("Arial",10),Brushes.Black,x-10,by-(i+1)*35+5); } }
        void Log(string s) => lstLog.Items.Insert(0, s);
        
        // Các hàm hỗ trợ thuật toán & ghi nhận bước vẽ
        void MergeSortAnim(int[] arr, int l, int r, bool desc) { if(l<r){int m=l+(r-l)/2;MergeSortAnim(arr,l,m,desc);MergeSortAnim(arr,m+1,r,desc);MergeAnim(arr,l,m,r,desc);} }
        void MergeAnim(int[] arr, int l, int m, int r, bool desc) {
            int n1=m-l+1,n2=r-m; int[] L=new int[n1],R=new int[n2]; Array.Copy(arr,l,L,0,n1); Array.Copy(arr,m+1,R,0,n2); int i=0,j=0,k=l;
            while(i<n1&&j<n2) { bool c=desc?L[i]>=R[j]:L[i]<=R[j]; sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(),YellowLimit=-1,RedIndex=k}); if(c)arr[k++]=L[i++];else arr[k++]=R[j++]; }
            while(i<n1){sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(),YellowLimit=-1,RedIndex=k});arr[k++]=L[i++];} while(j<n2){sortSteps.Add(new SortStep{Arr=(int[])arr.Clone(),YellowLimit=-1,RedIndex=k});arr[k++]=R[j++];}
        }
        
        void RunDFS(int t) { 
            Stack<int> s=new Stack<int>(); HashSet<int> v=new HashSet<int>(); Dictionary<int,int> p=new Dictionary<int,int>(); 
            s.Push(graphNodes[0].Value); bool f=false; 
            while(s.Count>0){
                int u=s.Pop(); RecordGraph(u,v,s.ToList(),null,$"Pop {u}"); 
                if(!v.Contains(u)){
                    v.Add(u); 
                    if(u==t){ f=true; BuildPath(p,graphNodes[0].Value,t); RecordGraph(u,v,s.ToList(),null,"TÌM THẤY!"); break; } 
                    var nb=graphNodes.First(x=>x.Value==u).Neighbors.OrderByDescending(x=>x).ToList(); 
                    foreach(var n in nb)if(!v.Contains(n)){s.Push(n);if(!p.ContainsKey(n))p[n]=u;}
                }
            } 
            if(!f)RecordGraph(-1,v,s.ToList(),null,"Không thấy đích!"); 
        }
        
        void RunBFS(int t) { 
            Stack<int> sIn=new Stack<int>(), sOut=new Stack<int>(); HashSet<int> v=new HashSet<int>(); Dictionary<int,int> p=new Dictionary<int,int>(); 
            int start=graphNodes[0].Value; sIn.Push(start); v.Add(start); bool f=false; 
            while(sIn.Count>0||sOut.Count>0){ 
                if(sOut.Count==0)while(sIn.Count>0)sOut.Push(sIn.Pop()); 
                int u=sOut.Pop(); RecordGraph(u,v,sIn.ToList(),sOut.ToList(),$"Pop {u}"); 
                if(u==t){ f=true; BuildPath(p,start,t); RecordGraph(u,v,sIn.ToList(),sOut.ToList(),"TÌM THẤY!"); break; } 
                var nb=graphNodes.First(x=>x.Value==u).Neighbors.OrderBy(x=>x).ToList(); 
                foreach(var n in nb)if(!v.Contains(n)){v.Add(n);sIn.Push(n);p[n]=u;} 
            } 
            if(!f)RecordGraph(-1,v,sIn.ToList(),sOut.ToList(),"Không thấy đích!"); 
        }
        
        void BuildPath(Dictionary<int,int> p, int s, int e) { finalPath=new List<int>(); int c=e; while(c!=s && p.ContainsKey(c)) { finalPath.Add(c); c=p[c]; } finalPath.Add(s); finalPath.Reverse(); }
        void RecordGraph(int c, HashSet<int> v, List<int> s1, List<int> s2, string m) { var st=new GraphStep{CurrentNode=c, Visited=new HashSet<int>(v), Stack1=s1, Msg=m}; if(s2!=null) st.Stack2=s2; graphSteps.Add(st); }
    }
}
