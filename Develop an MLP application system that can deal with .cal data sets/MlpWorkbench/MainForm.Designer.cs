namespace MlpWorkbench
{
    partial class MainForm
    {
        /// <summary>Required designer variable.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Clean up any resources being used.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadIrisSampleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.separatorToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lblLayerCount = new System.Windows.Forms.Label();
            this.nudLayerCount = new System.Windows.Forms.NumericUpDown();
            this.lblHiddenList = new System.Windows.Forms.Label();
            this.lbHidden = new System.Windows.Forms.ListBox();
            this.lblNodes = new System.Windows.Forms.Label();
            this.nudNodes = new System.Windows.Forms.NumericUpDown();
            this.lblHint = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnTrainEpoch = new System.Windows.Forms.Button();
            this.btnTrainToEnd = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnClassification = new System.Windows.Forms.Button();
            this.lblEpochInfo = new System.Windows.Forms.Label();
            this.lblRmseInfo = new System.Windows.Forms.Label();
            this.lblAccuracyInfo = new System.Windows.Forms.Label();
            this.rtbShow = new System.Windows.Forms.RichTextBox();
            this.ppgShow = new System.Windows.Forms.PropertyGrid();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.chtShow = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLayerCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNodes)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chtShow)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1320, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.loadIrisSampleToolStripMenuItem,
            this.separatorToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(246, 26);
            this.openToolStripMenuItem.Text = "&Open .cal data...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // loadIrisSampleToolStripMenuItem
            // 
            this.loadIrisSampleToolStripMenuItem.Name = "loadIrisSampleToolStripMenuItem";
            this.loadIrisSampleToolStripMenuItem.Size = new System.Drawing.Size(246, 26);
            this.loadIrisSampleToolStripMenuItem.Text = "Load bundled &iris sample";
            this.loadIrisSampleToolStripMenuItem.Click += new System.EventHandler(this.loadIrisSampleToolStripMenuItem_Click);
            // 
            // separatorToolStripMenuItem
            // 
            this.separatorToolStripMenuItem.Name = "separatorToolStripMenuItem";
            this.separatorToolStripMenuItem.Size = new System.Drawing.Size(243, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(246, 26);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(133, 26);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 812);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1320, 26);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(1305, 20);
            this.lblStatus.Spring = true;
            this.lblStatus.Text = "Open a .cal data file to begin.";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 28);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
            this.splitContainer1.Panel1MinSize = 480;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel2MinSize = 520;
            this.splitContainer1.Size = new System.Drawing.Size(1320, 784);
            this.splitContainer1.SplitterDistance = 574;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.tabControl1);
            this.splitContainer3.Panel1MinSize = 330;
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.ppgShow);
            this.splitContainer3.Panel2MinSize = 150;
            this.splitContainer3.Size = new System.Drawing.Size(574, 784);
            this.splitContainer3.SplitterDistance = 470;
            this.splitContainer3.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(574, 470);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(245)))), ((int)(((byte)(251)))));
            this.tabPage1.Controls.Add(this.lblLayerCount);
            this.tabPage1.Controls.Add(this.nudLayerCount);
            this.tabPage1.Controls.Add(this.lblHiddenList);
            this.tabPage1.Controls.Add(this.lbHidden);
            this.tabPage1.Controls.Add(this.lblNodes);
            this.tabPage1.Controls.Add(this.nudNodes);
            this.tabPage1.Controls.Add(this.lblHint);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(566, 437);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Network setup";
            // 
            // lblLayerCount
            // 
            this.lblLayerCount.AutoSize = true;
            this.lblLayerCount.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLayerCount.Location = new System.Drawing.Point(18, 22);
            this.lblLayerCount.Name = "lblLayerCount";
            this.lblLayerCount.Size = new System.Drawing.Size(174, 23);
            this.lblLayerCount.TabIndex = 0;
            this.lblLayerCount.Text = "Number of hidden layers:";
            // 
            // nudLayerCount
            // 
            this.nudLayerCount.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.nudLayerCount.Location = new System.Drawing.Point(232, 19);
            this.nudLayerCount.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            this.nudLayerCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudLayerCount.Name = "nudLayerCount";
            this.nudLayerCount.Size = new System.Drawing.Size(96, 30);
            this.nudLayerCount.TabIndex = 1;
            this.nudLayerCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nudLayerCount.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudLayerCount.ValueChanged += new System.EventHandler(this.nudLayerCount_ValueChanged);
            // 
            // lblHiddenList
            // 
            this.lblHiddenList.AutoSize = true;
            this.lblHiddenList.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblHiddenList.Location = new System.Drawing.Point(18, 68);
            this.lblHiddenList.Name = "lblHiddenList";
            this.lblHiddenList.Size = new System.Drawing.Size(300, 23);
            this.lblHiddenList.TabIndex = 2;
            this.lblHiddenList.Text = "Hidden layers (nodes per layer, top to bottom):";
            // 
            // lbHidden
            // 
            this.lbHidden.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbHidden.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbHidden.FormattingEnabled = true;
            this.lbHidden.ItemHeight = 22;
            this.lbHidden.Location = new System.Drawing.Point(22, 96);
            this.lbHidden.Name = "lbHidden";
            this.lbHidden.Size = new System.Drawing.Size(520, 158);
            this.lbHidden.TabIndex = 3;
            this.lbHidden.SelectedIndexChanged += new System.EventHandler(this.lbHidden_SelectedIndexChanged);
            // 
            // lblNodes
            // 
            this.lblNodes.AutoSize = true;
            this.lblNodes.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblNodes.Location = new System.Drawing.Point(18, 272);
            this.lblNodes.Name = "lblNodes";
            this.lblNodes.Size = new System.Drawing.Size(190, 23);
            this.lblNodes.TabIndex = 4;
            this.lblNodes.Text = "Nodes in selected layer:";
            // 
            // nudNodes
            // 
            this.nudNodes.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.nudNodes.Location = new System.Drawing.Point(232, 269);
            this.nudNodes.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
            this.nudNodes.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudNodes.Name = "nudNodes";
            this.nudNodes.Size = new System.Drawing.Size(96, 30);
            this.nudNodes.TabIndex = 5;
            this.nudNodes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nudNodes.Value = new decimal(new int[] { 8, 0, 0, 0 });
            this.nudNodes.ValueChanged += new System.EventHandler(this.nudNodes_ValueChanged);
            // 
            // lblHint
            // 
            this.lblHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblHint.Location = new System.Drawing.Point(22, 318);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(520, 96);
            this.lblHint.TabIndex = 6;
            this.lblHint.Text = "The layout is applied only when you press Reset Network on the Train && Evaluate tab. Hyperparameters (eta, eta decay, momentum, epoch limit, split ratio, seed) are edited in the property grid below.";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(248)))), ((int)(((byte)(240)))));
            this.tabPage2.Controls.Add(this.progressBar1);
            this.tabPage2.Controls.Add(this.btnReset);
            this.tabPage2.Controls.Add(this.btnTrainEpoch);
            this.tabPage2.Controls.Add(this.btnTrainToEnd);
            this.tabPage2.Controls.Add(this.btnCancel);
            this.tabPage2.Controls.Add(this.btnClassification);
            this.tabPage2.Controls.Add(this.lblEpochInfo);
            this.tabPage2.Controls.Add(this.lblRmseInfo);
            this.tabPage2.Controls.Add(this.lblAccuracyInfo);
            this.tabPage2.Controls.Add(this.rtbShow);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(566, 437);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Train && Evaluate";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(18, 8);
            this.progressBar1.MarqueeAnimationSpeed = 25;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(526, 8);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 0;
            this.progressBar1.Visible = false;
            // 
            // btnReset
            // 
            this.btnReset.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnReset.Location = new System.Drawing.Point(18, 24);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(252, 44);
            this.btnReset.TabIndex = 1;
            this.btnReset.Text = "Reset Network";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnTrainEpoch
            // 
            this.btnTrainEpoch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTrainEpoch.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnTrainEpoch.Location = new System.Drawing.Point(292, 24);
            this.btnTrainEpoch.Name = "btnTrainEpoch";
            this.btnTrainEpoch.Size = new System.Drawing.Size(252, 44);
            this.btnTrainEpoch.TabIndex = 2;
            this.btnTrainEpoch.Text = "Train 1 Epoch";
            this.btnTrainEpoch.UseVisualStyleBackColor = true;
            this.btnTrainEpoch.Click += new System.EventHandler(this.btnTrainEpoch_Click);
            // 
            // btnTrainToEnd
            // 
            this.btnTrainToEnd.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnTrainToEnd.Location = new System.Drawing.Point(18, 76);
            this.btnTrainToEnd.Name = "btnTrainToEnd";
            this.btnTrainToEnd.Size = new System.Drawing.Size(252, 44);
            this.btnTrainToEnd.TabIndex = 3;
            this.btnTrainToEnd.Text = "Train To End";
            this.btnTrainToEnd.UseVisualStyleBackColor = true;
            this.btnTrainToEnd.Click += new System.EventHandler(this.btnTrainToEnd_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Enabled = false;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnCancel.Location = new System.Drawing.Point(292, 76);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(252, 44);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Stop Training";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnClassification
            // 
            this.btnClassification.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClassification.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnClassification.Location = new System.Drawing.Point(18, 128);
            this.btnClassification.Name = "btnClassification";
            this.btnClassification.Size = new System.Drawing.Size(526, 40);
            this.btnClassification.TabIndex = 5;
            this.btnClassification.Text = "Classification Test (confusion matrix + accuracy)";
            this.btnClassification.UseVisualStyleBackColor = true;
            this.btnClassification.Click += new System.EventHandler(this.btnClassification_Click);
            // 
            // lblEpochInfo
            // 
            this.lblEpochInfo.AutoSize = true;
            this.lblEpochInfo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblEpochInfo.Location = new System.Drawing.Point(18, 184);
            this.lblEpochInfo.Name = "lblEpochInfo";
            this.lblEpochInfo.Size = new System.Drawing.Size(115, 21);
            this.lblEpochInfo.TabIndex = 6;
            this.lblEpochInfo.Text = "Epoch: - / -";
            // 
            // lblRmseInfo
            // 
            this.lblRmseInfo.AutoSize = true;
            this.lblRmseInfo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblRmseInfo.Location = new System.Drawing.Point(200, 184);
            this.lblRmseInfo.Name = "lblRmseInfo";
            this.lblRmseInfo.Size = new System.Drawing.Size(133, 21);
            this.lblRmseInfo.TabIndex = 7;
            this.lblRmseInfo.Text = "Test RMSE: -";
            // 
            // lblAccuracyInfo
            // 
            this.lblAccuracyInfo.AutoSize = true;
            this.lblAccuracyInfo.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblAccuracyInfo.Location = new System.Drawing.Point(380, 184);
            this.lblAccuracyInfo.Name = "lblAccuracyInfo";
            this.lblAccuracyInfo.Size = new System.Drawing.Size(154, 21);
            this.lblAccuracyInfo.TabIndex = 8;
            this.lblAccuracyInfo.Text = "Test accuracy: -";
            // 
            // rtbShow
            // 
            this.rtbShow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbShow.BackColor = System.Drawing.Color.White;
            this.rtbShow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbShow.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbShow.Location = new System.Drawing.Point(18, 216);
            this.rtbShow.Name = "rtbShow";
            this.rtbShow.ReadOnly = true;
            this.rtbShow.Size = new System.Drawing.Size(526, 200);
            this.rtbShow.TabIndex = 9;
            this.rtbShow.Text = "";
            // 
            // ppgShow
            // 
            this.ppgShow.BackColor = System.Drawing.SystemColors.Control;
            this.ppgShow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ppgShow.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ppgShow.HelpVisible = true;
            this.ppgShow.Location = new System.Drawing.Point(0, 0);
            this.ppgShow.Name = "ppgShow";
            this.ppgShow.PropertySort = System.Windows.Forms.PropertySort.CategorizedAlphabetical;
            this.ppgShow.Size = new System.Drawing.Size(574, 310);
            this.ppgShow.TabIndex = 0;
            this.ppgShow.ToolbarVisible = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.BackColor = System.Drawing.Color.White;
            this.splitContainer2.Panel1.Controls.Add(this.chtShow);
            this.splitContainer2.Panel1MinSize = 220;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(245)))));
            this.splitContainer2.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer2_Panel2_Paint);
            this.splitContainer2.Panel2MinSize = 240;
            this.splitContainer2.Size = new System.Drawing.Size(742, 784);
            this.splitContainer2.SplitterDistance = 400;
            this.splitContainer2.TabIndex = 0;
            // 
            // chtShow
            // 
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.Gainsboro;
            chartArea1.AxisX.Title = "Epoch";
            chartArea1.AxisX.TitleFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            chartArea1.AxisY.IsStartedFromZero = true;
            chartArea1.AxisY.LabelStyle.Format = "0.000";
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.Gainsboro;
            chartArea1.AxisY.Title = "Test RMSE";
            chartArea1.AxisY.TitleFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            chartArea1.BackColor = System.Drawing.Color.White;
            chartArea1.Name = "ChartArea1";
            this.chtShow.ChartAreas.Add(chartArea1);
            this.chtShow.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend1.Name = "Legend1";
            this.chtShow.Legends.Add(legend1);
            this.chtShow.Location = new System.Drawing.Point(0, 0);
            this.chtShow.Name = "chtShow";
            this.chtShow.Size = new System.Drawing.Size(742, 400);
            this.chtShow.TabIndex = 0;
            this.chtShow.Text = "convergence";
            title1.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            title1.Name = "Title1";
            title1.Text = "Test RMSE convergence";
            this.chtShow.Titles.Add(title1);
            // 
            // dlgOpen
            // 
            this.dlgOpen.FileName = "openFileDialog1";
            this.dlgOpen.Filter = "CAL data files (*.cal)|*.cal|All files (*.*)|*.*";
            this.dlgOpen.Title = "Open .cal data set";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1320, 838);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(1100, 720);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MLP Workbench";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudLayerCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNodes)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chtShow)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadIrisSampleToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator separatorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label lblLayerCount;
        private System.Windows.Forms.NumericUpDown nudLayerCount;
        private System.Windows.Forms.Label lblHiddenList;
        private System.Windows.Forms.ListBox lbHidden;
        private System.Windows.Forms.Label lblNodes;
        private System.Windows.Forms.NumericUpDown nudNodes;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnTrainEpoch;
        private System.Windows.Forms.Button btnTrainToEnd;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnClassification;
        private System.Windows.Forms.Label lblEpochInfo;
        private System.Windows.Forms.Label lblRmseInfo;
        private System.Windows.Forms.Label lblAccuracyInfo;
        private System.Windows.Forms.RichTextBox rtbShow;
        private System.Windows.Forms.PropertyGrid ppgShow;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chtShow;
        private System.Windows.Forms.OpenFileDialog dlgOpen;
    }
}


