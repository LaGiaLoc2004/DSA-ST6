namespace dsa1
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.grpConfig = new System.Windows.Forms.GroupBox();
            this.lblDisks = new System.Windows.Forms.Label();
            this.nudDisks = new System.Windows.Forms.NumericUpDown();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.grpResult = new System.Windows.Forms.GroupBox();
            this.lstSteps = new System.Windows.Forms.ListBox();
            this.pnlDraw = new System.Windows.Forms.Panel();
            this.grpConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDisks)).BeginInit();
            this.grpResult.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(878, 50);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "THÁP HÀ NỘI - STACK (ANIMATION)";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grpConfig
            // 
            this.grpConfig.Controls.Add(this.btnStart);
            this.grpConfig.Controls.Add(this.btnStop);
            this.grpConfig.Controls.Add(this.nudDisks);
            this.grpConfig.Controls.Add(this.lblDisks);
            this.grpConfig.Location = new System.Drawing.Point(10, 60);
            this.grpConfig.Name = "grpConfig";
            this.grpConfig.Size = new System.Drawing.Size(260, 140);
            this.grpConfig.TabIndex = 2;
            this.grpConfig.TabStop = false;
            this.grpConfig.Text = "Cấu hình";
            // 
            // lblDisks
            // 
            this.lblDisks.AutoSize = true;
            this.lblDisks.Location = new System.Drawing.Point(15, 30);
            this.lblDisks.Name = "lblDisks";
            this.lblDisks.Size = new System.Drawing.Size(71, 28);
            this.lblDisks.TabIndex = 0;
            this.lblDisks.Text = "Số đĩa:";
            // 
            // nudDisks
            // 
            this.nudDisks.Location = new System.Drawing.Point(90, 25);
            this.nudDisks.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudDisks.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudDisks.Name = "nudDisks";
            this.nudDisks.Size = new System.Drawing.Size(120, 34);
            this.nudDisks.TabIndex = 1;
            this.nudDisks.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(120, 70);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 64);
            this.btnStop.TabIndex = 3;
            this.btnStop.Text = "Dừng";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(15, 70);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 64);
            this.btnStart.TabIndex = 4;
            this.btnStart.Text = "Bắt đầu";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // grpResult
            // 
            this.grpResult.Controls.Add(this.lstSteps);
            this.grpResult.Location = new System.Drawing.Point(10, 210);
            this.grpResult.Name = "grpResult";
            this.grpResult.Size = new System.Drawing.Size(260, 340);
            this.grpResult.TabIndex = 3;
            this.grpResult.TabStop = false;
            this.grpResult.Text = "Các bước di chuyển";
            // 
            // lstSteps
            // 
            this.lstSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstSteps.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstSteps.FormattingEnabled = true;
            this.lstSteps.ItemHeight = 23;
            this.lstSteps.Location = new System.Drawing.Point(3, 30);
            this.lstSteps.Name = "lstSteps";
            this.lstSteps.Size = new System.Drawing.Size(254, 307);
            this.lstSteps.TabIndex = 0;
            // 
            // pnlDraw
            // 
            this.pnlDraw.Location = new System.Drawing.Point(280, 60);
            this.pnlDraw.Name = "pnlDraw";
            this.pnlDraw.Size = new System.Drawing.Size(590, 490);
            this.pnlDraw.TabIndex = 4;
            this.pnlDraw.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlDraw_Paint);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(878, 544);
            this.Controls.Add(this.pnlDraw);
            this.Controls.Add(this.grpResult);
            this.Controls.Add(this.grpConfig);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tower of Hanoi - Stack Animation";
            this.grpConfig.ResumeLayout(false);
            this.grpConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDisks)).EndInit();
            this.grpResult.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.NumericUpDown nudDisks;
        private System.Windows.Forms.Label lblDisks;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.GroupBox grpResult;
        private System.Windows.Forms.ListBox lstSteps;
        private System.Windows.Forms.Panel pnlDraw;
    }
}

