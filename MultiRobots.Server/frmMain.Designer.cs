namespace MultiRobots.Server
{
    partial class frmMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.label1 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.listLog = new System.Windows.Forms.ListView();
            this.Date = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Message = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnMotor = new System.Windows.Forms.Button();
            this.btnRestart = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnAlarmReset = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxShow = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxExit = new System.Windows.Forms.ToolStripMenuItem();
            this.btnJigReset = new System.Windows.Forms.Button();
            this.cboWorkCount = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Port";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(44, 15);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(66, 23);
            this.txtPort.TabIndex = 1;
            this.txtPort.Text = "9999";
            this.txtPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(504, 9);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 33);
            this.btnStop.TabIndex = 3;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Visible = false;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(423, 9);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 33);
            this.btnStart.TabIndex = 4;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Visible = false;
            // 
            // listLog
            // 
            this.listLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Date,
            this.Message});
            this.listLog.HideSelection = false;
            this.listLog.Location = new System.Drawing.Point(12, 50);
            this.listLog.Name = "listLog";
            this.listLog.Size = new System.Drawing.Size(567, 273);
            this.listLog.TabIndex = 0;
            this.listLog.UseCompatibleStateImageBehavior = false;
            this.listLog.View = System.Windows.Forms.View.Details;
            // 
            // Date
            // 
            this.Date.Text = "Date";
            this.Date.Width = 140;
            // 
            // Message
            // 
            this.Message.Text = "Message";
            this.Message.Width = 341;
            // 
            // btnMotor
            // 
            this.btnMotor.Location = new System.Drawing.Point(12, 329);
            this.btnMotor.Name = "btnMotor";
            this.btnMotor.Size = new System.Drawing.Size(75, 24);
            this.btnMotor.TabIndex = 4;
            this.btnMotor.Text = "Motor on";
            this.btnMotor.UseVisualStyleBackColor = true;
            // 
            // btnRestart
            // 
            this.btnRestart.Location = new System.Drawing.Point(423, 329);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(75, 24);
            this.btnRestart.TabIndex = 5;
            this.btnRestart.Text = "재기동";
            this.btnRestart.UseVisualStyleBackColor = true;
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(504, 329);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 24);
            this.btnPause.TabIndex = 4;
            this.btnPause.Text = "정지";
            this.btnPause.UseVisualStyleBackColor = true;
            // 
            // btnAlarmReset
            // 
            this.btnAlarmReset.Location = new System.Drawing.Point(93, 329);
            this.btnAlarmReset.Name = "btnAlarmReset";
            this.btnAlarmReset.Size = new System.Drawing.Size(75, 24);
            this.btnAlarmReset.TabIndex = 5;
            this.btnAlarmReset.Text = "알람 리셋";
            this.btnAlarmReset.UseVisualStyleBackColor = true;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxShow,
            this.ctxExit});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(105, 48);
            // 
            // ctxShow
            // 
            this.ctxShow.Name = "ctxShow";
            this.ctxShow.Size = new System.Drawing.Size(104, 22);
            this.ctxShow.Text = "Show";
            this.ctxShow.Click += new System.EventHandler(this.ctxShow_Click);
            // 
            // ctxExit
            // 
            this.ctxExit.Name = "ctxExit";
            this.ctxExit.Size = new System.Drawing.Size(104, 22);
            this.ctxExit.Text = "Exit";
            this.ctxExit.Click += new System.EventHandler(this.ctxExit_Click);
            // 
            // btnJigReset
            // 
            this.btnJigReset.Location = new System.Drawing.Point(174, 329);
            this.btnJigReset.Name = "btnJigReset";
            this.btnJigReset.Size = new System.Drawing.Size(75, 24);
            this.btnJigReset.TabIndex = 5;
            this.btnJigReset.Text = "초기화";
            this.btnJigReset.UseVisualStyleBackColor = true;
            // 
            // cboWorkCount
            // 
            this.cboWorkCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboWorkCount.FormattingEnabled = true;
            this.cboWorkCount.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5"});
            this.cboWorkCount.Location = new System.Drawing.Point(209, 15);
            this.cboWorkCount.Name = "cboWorkCount";
            this.cboWorkCount.Size = new System.Drawing.Size(121, 23);
            this.cboWorkCount.TabIndex = 6;
            this.cboWorkCount.SelectedIndexChanged += new System.EventHandler(this.CboWorkCount_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(145, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "작업 횟수";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(591, 364);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboWorkCount);
            this.Controls.Add(this.btnJigReset);
            this.Controls.Add(this.btnAlarmReset);
            this.Controls.Add(this.btnRestart);
            this.Controls.Add(this.listLog);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.btnMotor);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.ListView listLog;
        private System.Windows.Forms.ColumnHeader Date;
        private System.Windows.Forms.ColumnHeader Message;
        private System.Windows.Forms.Button btnMotor;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnAlarmReset;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ctxShow;
        private System.Windows.Forms.ToolStripMenuItem ctxExit;
        private System.Windows.Forms.Button btnJigReset;
        private System.Windows.Forms.ComboBox cboWorkCount;
        private System.Windows.Forms.Label label2;
    }
}