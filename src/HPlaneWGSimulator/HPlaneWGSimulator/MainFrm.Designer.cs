﻿namespace HPlaneWGSimulator
{
    partial class MainFrm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFrm));
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Title title2 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Title title3 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.CadPanel = new System.Windows.Forms.Panel();
            this.FValuePanel = new System.Windows.Forms.Panel();
            this.btnNextFValuePanel = new System.Windows.Forms.Button();
            this.btnPrevFValuePanel = new System.Windows.Forms.Button();
            this.btnCalc = new System.Windows.Forms.Button();
            this.GroupBoxCadMode = new System.Windows.Forms.GroupBox();
            this.imageListCadModeErase = new System.Windows.Forms.ImageList(this.components);
            this.imageListCadModeArea = new System.Windows.Forms.ImageList(this.components);
            this.radioBtnLocation = new System.Windows.Forms.RadioButton();
            this.radioBtnNone = new System.Windows.Forms.RadioButton();
            this.radioBtnPortNumbering = new System.Windows.Forms.RadioButton();
            this.radioBtnIncidentPort = new System.Windows.Forms.RadioButton();
            this.radioBtnErase = new System.Windows.Forms.RadioButton();
            this.radioBtnPort = new System.Windows.Forms.RadioButton();
            this.radioBtnArea = new System.Windows.Forms.RadioButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnPrevFreq = new System.Windows.Forms.Button();
            this.btnNextFreq = new System.Windows.Forms.Button();
            this.btnMediaSelect = new System.Windows.Forms.Button();
            this.chkboxAutoCalc = new System.Windows.Forms.CheckBox();
            this.btnRedo = new System.Windows.Forms.Button();
            this.btnUndo = new System.Windows.Forms.Button();
            this.btnSaveAs = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.labelFreq = new System.Windows.Forms.Label();
            this.SMatChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.SMatChartContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMILogGraph = new System.Windows.Forms.ToolStripMenuItem();
            this.FValueLegendPanel = new System.Windows.Forms.Panel();
            this.labelFreqValue = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.GroupBoxMedia = new System.Windows.Forms.GroupBox();
            this.labelEps = new System.Windows.Forms.Label();
            this.textBoxEps2 = new System.Windows.Forms.TextBox();
            this.textBoxEps0 = new System.Windows.Forms.TextBox();
            this.textBoxEps1 = new System.Windows.Forms.TextBox();
            this.radioBtnMedia2 = new System.Windows.Forms.RadioButton();
            this.radioBtnMedia1 = new System.Windows.Forms.RadioButton();
            this.radioBtnMedia0 = new System.Windows.Forms.RadioButton();
            this.panelMedia = new System.Windows.Forms.Panel();
            this.BetaChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.EigenVecChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.linkLblEigenShow = new System.Windows.Forms.LinkLabel();
            this.linkLabelMeshShow = new System.Windows.Forms.LinkLabel();
            this.labelCalcMode = new System.Windows.Forms.Label();
            this.btnLoadCancel = new System.Windows.Forms.Button();
            this.imgcbxCadModeErase = new HPlaneWGSimulator.ImageCombobox();
            this.imgcbxCadModeArea = new HPlaneWGSimulator.ImageCombobox();
            this.FValuePanel.SuspendLayout();
            this.GroupBoxCadMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SMatChart)).BeginInit();
            this.SMatChartContextMenuStrip.SuspendLayout();
            this.FValueLegendPanel.SuspendLayout();
            this.GroupBoxMedia.SuspendLayout();
            this.panelMedia.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BetaChart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EigenVecChart)).BeginInit();
            this.SuspendLayout();
            // 
            // CadPanel
            // 
            this.CadPanel.Location = new System.Drawing.Point(5, 47);
            this.CadPanel.Margin = new System.Windows.Forms.Padding(0);
            this.CadPanel.Name = "CadPanel";
            this.CadPanel.Size = new System.Drawing.Size(448, 448);
            this.CadPanel.TabIndex = 0;
            this.CadPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.CadPanel_Paint);
            this.CadPanel.DoubleClick += new System.EventHandler(this.CadPanel_DoubleClick);
            this.CadPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.CadPanel_MouseClick);
            this.CadPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CadPanel_MouseDown);
            this.CadPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CadPanel_MouseMove);
            this.CadPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CadPanel_MouseUp);
            // 
            // FValuePanel
            // 
            this.FValuePanel.Controls.Add(this.btnNextFValuePanel);
            this.FValuePanel.Controls.Add(this.btnPrevFValuePanel);
            this.FValuePanel.Location = new System.Drawing.Point(514, 4);
            this.FValuePanel.Margin = new System.Windows.Forms.Padding(0);
            this.FValuePanel.Name = "FValuePanel";
            this.FValuePanel.Size = new System.Drawing.Size(320, 320);
            this.FValuePanel.TabIndex = 0;
            this.FValuePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.FValuePanel_Paint);
            this.FValuePanel.DoubleClick += new System.EventHandler(this.FValuePanel_DoubleClick);
            this.FValuePanel.MouseEnter += new System.EventHandler(this.FValuePanel_MouseEnter);
            this.FValuePanel.MouseLeave += new System.EventHandler(this.FValuePanel_MouseLeave);
            this.FValuePanel.MouseHover += new System.EventHandler(this.FValuePanel_MouseHover);
            this.FValuePanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FValuePanel_MouseMove);
            // 
            // btnNextFValuePanel
            // 
            this.btnNextFValuePanel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextFValuePanel.Location = new System.Drawing.Point(3, 219);
            this.btnNextFValuePanel.Name = "btnNextFValuePanel";
            this.btnNextFValuePanel.Size = new System.Drawing.Size(40, 40);
            this.btnNextFValuePanel.TabIndex = 0;
            this.btnNextFValuePanel.Text = ">";
            this.toolTip1.SetToolTip(this.btnNextFValuePanel, "次のパネル");
            this.btnNextFValuePanel.UseVisualStyleBackColor = true;
            this.btnNextFValuePanel.Click += new System.EventHandler(this.btnNextFValuePanel_Click);
            this.btnNextFValuePanel.MouseEnter += new System.EventHandler(this.btnNextFValuePanel_MouseEnter);
            this.btnNextFValuePanel.MouseLeave += new System.EventHandler(this.btnNextFValuePanel_MouseLeave);
            // 
            // btnPrevFValuePanel
            // 
            this.btnPrevFValuePanel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevFValuePanel.Location = new System.Drawing.Point(3, 174);
            this.btnPrevFValuePanel.Name = "btnPrevFValuePanel";
            this.btnPrevFValuePanel.Size = new System.Drawing.Size(40, 40);
            this.btnPrevFValuePanel.TabIndex = 0;
            this.btnPrevFValuePanel.Text = "<";
            this.toolTip1.SetToolTip(this.btnPrevFValuePanel, "前のパネル");
            this.btnPrevFValuePanel.UseVisualStyleBackColor = true;
            this.btnPrevFValuePanel.Click += new System.EventHandler(this.btnPrevFValuePanel_Click);
            this.btnPrevFValuePanel.MouseEnter += new System.EventHandler(this.btnPrevFValuePanel_MouseEnter);
            this.btnPrevFValuePanel.MouseLeave += new System.EventHandler(this.btnPrevFValuePanel_MouseLeave);
            // 
            // btnCalc
            // 
            this.btnCalc.AutoSize = true;
            this.btnCalc.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCalc.ForeColor = System.Drawing.Color.Black;
            this.btnCalc.Location = new System.Drawing.Point(348, 16);
            this.btnCalc.Name = "btnCalc";
            this.btnCalc.Padding = new System.Windows.Forms.Padding(3);
            this.btnCalc.Size = new System.Drawing.Size(69, 28);
            this.btnCalc.TabIndex = 8;
            this.btnCalc.Text = "計算開始";
            this.btnCalc.UseVisualStyleBackColor = true;
            this.btnCalc.Click += new System.EventHandler(this.btnCalc_Click);
            // 
            // GroupBoxCadMode
            // 
            this.GroupBoxCadMode.Controls.Add(this.imgcbxCadModeErase);
            this.GroupBoxCadMode.Controls.Add(this.imgcbxCadModeArea);
            this.GroupBoxCadMode.Controls.Add(this.radioBtnLocation);
            this.GroupBoxCadMode.Controls.Add(this.radioBtnNone);
            this.GroupBoxCadMode.Controls.Add(this.radioBtnPortNumbering);
            this.GroupBoxCadMode.Controls.Add(this.radioBtnIncidentPort);
            this.GroupBoxCadMode.Controls.Add(this.radioBtnErase);
            this.GroupBoxCadMode.Controls.Add(this.radioBtnPort);
            this.GroupBoxCadMode.Controls.Add(this.radioBtnArea);
            this.GroupBoxCadMode.Location = new System.Drawing.Point(5, 495);
            this.GroupBoxCadMode.Margin = new System.Windows.Forms.Padding(0);
            this.GroupBoxCadMode.Name = "GroupBoxCadMode";
            this.GroupBoxCadMode.Padding = new System.Windows.Forms.Padding(0);
            this.GroupBoxCadMode.Size = new System.Drawing.Size(223, 71);
            this.GroupBoxCadMode.TabIndex = 10;
            this.GroupBoxCadMode.TabStop = false;
            // 
            // imageListCadModeErase
            // 
            this.imageListCadModeErase.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListCadModeErase.ImageStream")));
            this.imageListCadModeErase.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListCadModeErase.Images.SetKeyName(0, "消しゴムフリーハンド25x25.png");
            this.imageListCadModeErase.Images.SetKeyName(1, "消しゴム長方形25x25.png");
            this.imageListCadModeErase.Images.SetKeyName(2, "消しゴム直線25x25.png");
            // 
            // imageListCadModeArea
            // 
            this.imageListCadModeArea.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListCadModeArea.ImageStream")));
            this.imageListCadModeArea.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListCadModeArea.Images.SetKeyName(0, "エリア選択フリーハンド25x25 .png");
            this.imageListCadModeArea.Images.SetKeyName(1, "エリア選択25x25.png");
            this.imageListCadModeArea.Images.SetKeyName(2, "エリア選択直線25x25 .png");
            this.imageListCadModeArea.Images.SetKeyName(3, "エリア選択楕円25x25.png");
            // 
            // radioBtnLocation
            // 
            this.radioBtnLocation.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioBtnLocation.Image = ((System.Drawing.Image)(resources.GetObject("radioBtnLocation.Image")));
            this.radioBtnLocation.Location = new System.Drawing.Point(36, 10);
            this.radioBtnLocation.Name = "radioBtnLocation";
            this.radioBtnLocation.Size = new System.Drawing.Size(30, 30);
            this.radioBtnLocation.TabIndex = 1;
            this.radioBtnLocation.TabStop = true;
            this.radioBtnLocation.Text = "　";
            this.toolTip1.SetToolTip(this.radioBtnLocation, "位置移動");
            this.radioBtnLocation.UseVisualStyleBackColor = true;
            this.radioBtnLocation.CheckedChanged += new System.EventHandler(this.radioBtnLocation_CheckedChanged);
            // 
            // radioBtnNone
            // 
            this.radioBtnNone.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioBtnNone.Image = global::HPlaneWGSimulator.Properties.Resources.モードクリア25x25;
            this.radioBtnNone.Location = new System.Drawing.Point(6, 10);
            this.radioBtnNone.Name = "radioBtnNone";
            this.radioBtnNone.Size = new System.Drawing.Size(30, 30);
            this.radioBtnNone.TabIndex = 0;
            this.radioBtnNone.TabStop = true;
            this.radioBtnNone.Text = "　";
            this.toolTip1.SetToolTip(this.radioBtnNone, "描画モード解除");
            this.radioBtnNone.UseVisualStyleBackColor = true;
            this.radioBtnNone.CheckedChanged += new System.EventHandler(this.radioBtnNone_CheckedChanged);
            // 
            // radioBtnPortNumbering
            // 
            this.radioBtnPortNumbering.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioBtnPortNumbering.Image = global::HPlaneWGSimulator.Properties.Resources.入射ポート番号振り25x25;
            this.radioBtnPortNumbering.Location = new System.Drawing.Point(156, 10);
            this.radioBtnPortNumbering.Name = "radioBtnPortNumbering";
            this.radioBtnPortNumbering.Size = new System.Drawing.Size(30, 30);
            this.radioBtnPortNumbering.TabIndex = 6;
            this.radioBtnPortNumbering.TabStop = true;
            this.radioBtnPortNumbering.Text = "　";
            this.toolTip1.SetToolTip(this.radioBtnPortNumbering, "ポート番号振り");
            this.radioBtnPortNumbering.UseVisualStyleBackColor = true;
            this.radioBtnPortNumbering.CheckedChanged += new System.EventHandler(this.radioBtnPortNumbering_CheckedChanged);
            // 
            // radioBtnIncidentPort
            // 
            this.radioBtnIncidentPort.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioBtnIncidentPort.Image = global::HPlaneWGSimulator.Properties.Resources.入射ポート選択25x25;
            this.radioBtnIncidentPort.Location = new System.Drawing.Point(126, 10);
            this.radioBtnIncidentPort.Name = "radioBtnIncidentPort";
            this.radioBtnIncidentPort.Size = new System.Drawing.Size(30, 30);
            this.radioBtnIncidentPort.TabIndex = 5;
            this.radioBtnIncidentPort.TabStop = true;
            this.radioBtnIncidentPort.Text = "　";
            this.toolTip1.SetToolTip(this.radioBtnIncidentPort, "入射ポート選択");
            this.radioBtnIncidentPort.UseVisualStyleBackColor = true;
            this.radioBtnIncidentPort.CheckedChanged += new System.EventHandler(this.radioBtnIncidentPort_CheckedChanged);
            // 
            // radioBtnErase
            // 
            this.radioBtnErase.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioBtnErase.Image = global::HPlaneWGSimulator.Properties.Resources.消しゴム長方形25x25;
            this.radioBtnErase.Location = new System.Drawing.Point(186, 10);
            this.radioBtnErase.Name = "radioBtnErase";
            this.radioBtnErase.Size = new System.Drawing.Size(30, 30);
            this.radioBtnErase.TabIndex = 7;
            this.radioBtnErase.TabStop = true;
            this.radioBtnErase.Text = "　";
            this.toolTip1.SetToolTip(this.radioBtnErase, "消しゴム");
            this.radioBtnErase.UseVisualStyleBackColor = true;
            this.radioBtnErase.CheckedChanged += new System.EventHandler(this.radioBtnErase_CheckedChanged);
            // 
            // radioBtnPort
            // 
            this.radioBtnPort.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioBtnPort.Image = global::HPlaneWGSimulator.Properties.Resources.境界選択25x25;
            this.radioBtnPort.Location = new System.Drawing.Point(96, 10);
            this.radioBtnPort.Name = "radioBtnPort";
            this.radioBtnPort.Size = new System.Drawing.Size(30, 30);
            this.radioBtnPort.TabIndex = 4;
            this.radioBtnPort.TabStop = true;
            this.radioBtnPort.Text = "　";
            this.toolTip1.SetToolTip(this.radioBtnPort, "ポーt境界");
            this.radioBtnPort.UseVisualStyleBackColor = true;
            this.radioBtnPort.CheckedChanged += new System.EventHandler(this.radioBtnPort_CheckedChanged);
            // 
            // radioBtnArea
            // 
            this.radioBtnArea.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioBtnArea.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.radioBtnArea.Image = global::HPlaneWGSimulator.Properties.Resources.エリア選択25x25;
            this.radioBtnArea.Location = new System.Drawing.Point(66, 10);
            this.radioBtnArea.Name = "radioBtnArea";
            this.radioBtnArea.Size = new System.Drawing.Size(30, 30);
            this.radioBtnArea.TabIndex = 2;
            this.radioBtnArea.TabStop = true;
            this.radioBtnArea.Text = "　";
            this.toolTip1.SetToolTip(this.radioBtnArea, "マス目選択");
            this.radioBtnArea.UseVisualStyleBackColor = true;
            this.radioBtnArea.CheckedChanged += new System.EventHandler(this.radioBtnArea_CheckedChanged);
            // 
            // btnPrevFreq
            // 
            this.btnPrevFreq.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnPrevFreq.ForeColor = System.Drawing.Color.Black;
            this.btnPrevFreq.Location = new System.Drawing.Point(0, 288);
            this.btnPrevFreq.Name = "btnPrevFreq";
            this.btnPrevFreq.Size = new System.Drawing.Size(30, 30);
            this.btnPrevFreq.TabIndex = 0;
            this.btnPrevFreq.Text = "◀";
            this.toolTip1.SetToolTip(this.btnPrevFreq, "前の周波数");
            this.btnPrevFreq.UseVisualStyleBackColor = true;
            this.btnPrevFreq.Click += new System.EventHandler(this.btnPrevFreq_Click);
            // 
            // btnNextFreq
            // 
            this.btnNextFreq.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnNextFreq.ForeColor = System.Drawing.Color.Black;
            this.btnNextFreq.Location = new System.Drawing.Point(36, 288);
            this.btnNextFreq.Name = "btnNextFreq";
            this.btnNextFreq.Size = new System.Drawing.Size(30, 30);
            this.btnNextFreq.TabIndex = 1;
            this.btnNextFreq.Text = "▶";
            this.toolTip1.SetToolTip(this.btnNextFreq, "次の周波数");
            this.btnNextFreq.UseVisualStyleBackColor = true;
            this.btnNextFreq.Click += new System.EventHandler(this.btnNextFreq_Click);
            // 
            // btnMediaSelect
            // 
            this.btnMediaSelect.BackColor = System.Drawing.Color.Gray;
            this.btnMediaSelect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMediaSelect.ForeColor = System.Drawing.Color.Black;
            this.btnMediaSelect.Location = new System.Drawing.Point(242, 505);
            this.btnMediaSelect.Name = "btnMediaSelect";
            this.btnMediaSelect.Size = new System.Drawing.Size(44, 40);
            this.btnMediaSelect.TabIndex = 11;
            this.btnMediaSelect.Text = "媒質";
            this.toolTip1.SetToolTip(this.btnMediaSelect, "媒質");
            this.btnMediaSelect.UseVisualStyleBackColor = false;
            this.btnMediaSelect.Click += new System.EventHandler(this.btnMediaSelect_Click);
            // 
            // chkboxAutoCalc
            // 
            this.chkboxAutoCalc.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkboxAutoCalc.Image = ((System.Drawing.Image)(resources.GetObject("chkboxAutoCalc.Image")));
            this.chkboxAutoCalc.Location = new System.Drawing.Point(319, 16);
            this.chkboxAutoCalc.Name = "chkboxAutoCalc";
            this.chkboxAutoCalc.Size = new System.Drawing.Size(28, 28);
            this.chkboxAutoCalc.TabIndex = 7;
            this.chkboxAutoCalc.Text = " ";
            this.toolTip1.SetToolTip(this.chkboxAutoCalc, "自動計算");
            this.chkboxAutoCalc.UseVisualStyleBackColor = true;
            this.chkboxAutoCalc.CheckedChanged += new System.EventHandler(this.chkboxAutoCalc_CheckedChanged);
            // 
            // btnRedo
            // 
            this.btnRedo.Image = global::HPlaneWGSimulator.Properties.Resources.やり直し;
            this.btnRedo.Location = new System.Drawing.Point(205, 4);
            this.btnRedo.Name = "btnRedo";
            this.btnRedo.Size = new System.Drawing.Size(40, 40);
            this.btnRedo.TabIndex = 5;
            this.toolTip1.SetToolTip(this.btnRedo, "やり直し Ctrl+Y");
            this.btnRedo.UseVisualStyleBackColor = true;
            this.btnRedo.Click += new System.EventHandler(this.btnRedo_Click);
            // 
            // btnUndo
            // 
            this.btnUndo.Image = global::HPlaneWGSimulator.Properties.Resources.元に戻す;
            this.btnUndo.Location = new System.Drawing.Point(165, 4);
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Size = new System.Drawing.Size(40, 40);
            this.btnUndo.TabIndex = 4;
            this.toolTip1.SetToolTip(this.btnUndo, "元に戻す Ctrl+Z");
            this.btnUndo.UseVisualStyleBackColor = true;
            this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.Image = global::HPlaneWGSimulator.Properties.Resources.名前を付けて保存;
            this.btnSaveAs.Location = new System.Drawing.Point(125, 4);
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(40, 40);
            this.btnSaveAs.TabIndex = 3;
            this.toolTip1.SetToolTip(this.btnSaveAs, "名前を付けて保存");
            this.btnSaveAs.UseVisualStyleBackColor = true;
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            // 
            // btnSave
            // 
            this.btnSave.Image = global::HPlaneWGSimulator.Properties.Resources.上書き保存;
            this.btnSave.Location = new System.Drawing.Point(85, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(40, 40);
            this.btnSave.TabIndex = 2;
            this.toolTip1.SetToolTip(this.btnSave, "上書き保存 Ctrl+S");
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Image = global::HPlaneWGSimulator.Properties.Resources.開く;
            this.btnOpen.Location = new System.Drawing.Point(45, 4);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(40, 40);
            this.btnOpen.TabIndex = 1;
            this.toolTip1.SetToolTip(this.btnOpen, "開く Ctrl+O");
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnNew
            // 
            this.btnNew.Image = global::HPlaneWGSimulator.Properties.Resources.新規;
            this.btnNew.Location = new System.Drawing.Point(5, 4);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(40, 40);
            this.btnNew.TabIndex = 0;
            this.toolTip1.SetToolTip(this.btnNew, "新規作成");
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // labelFreq
            // 
            this.labelFreq.AutoSize = true;
            this.labelFreq.Location = new System.Drawing.Point(0, 249);
            this.labelFreq.Name = "labelFreq";
            this.labelFreq.Size = new System.Drawing.Size(48, 12);
            this.labelFreq.TabIndex = 0;
            this.labelFreq.Text = "2W/λ =";
            // 
            // SMatChart
            // 
            chartArea1.Name = "ChartArea1";
            this.SMatChart.ChartAreas.Add(chartArea1);
            this.SMatChart.ContextMenuStrip = this.SMatChartContextMenuStrip;
            legend1.Name = "Legend1";
            this.SMatChart.Legends.Add(legend1);
            this.SMatChart.Location = new System.Drawing.Point(453, 326);
            this.SMatChart.Name = "SMatChart";
            this.SMatChart.Size = new System.Drawing.Size(448, 240);
            this.SMatChart.TabIndex = 0;
            this.SMatChart.TabStop = false;
            this.SMatChart.Text = "chart1";
            title1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            title1.Name = "Title1";
            this.SMatChart.Titles.Add(title1);
            this.SMatChart.DoubleClick += new System.EventHandler(this.SMatChart_DoubleClick);
            // 
            // SMatChartContextMenuStrip
            // 
            this.SMatChartContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMILogGraph});
            this.SMatChartContextMenuStrip.Name = "SMatChartContextMenuStrip";
            this.SMatChartContextMenuStrip.Size = new System.Drawing.Size(125, 26);
            // 
            // toolStripMILogGraph
            // 
            this.toolStripMILogGraph.Name = "toolStripMILogGraph";
            this.toolStripMILogGraph.Size = new System.Drawing.Size(124, 22);
            this.toolStripMILogGraph.Text = "対数表示";
            this.toolStripMILogGraph.Click += new System.EventHandler(this.toolStripMILogGraph_Click);
            // 
            // FValueLegendPanel
            // 
            this.FValueLegendPanel.Controls.Add(this.labelFreqValue);
            this.FValueLegendPanel.Controls.Add(this.btnNextFreq);
            this.FValueLegendPanel.Controls.Add(this.btnPrevFreq);
            this.FValueLegendPanel.Controls.Add(this.labelFreq);
            this.FValueLegendPanel.Location = new System.Drawing.Point(833, 4);
            this.FValueLegendPanel.Margin = new System.Windows.Forms.Padding(0);
            this.FValueLegendPanel.Name = "FValueLegendPanel";
            this.FValueLegendPanel.Size = new System.Drawing.Size(97, 320);
            this.FValueLegendPanel.TabIndex = 14;
            // 
            // labelFreqValue
            // 
            this.labelFreqValue.AutoSize = true;
            this.labelFreqValue.Font = new System.Drawing.Font("MS UI Gothic", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.labelFreqValue.Location = new System.Drawing.Point(5, 265);
            this.labelFreqValue.Name = "labelFreqValue";
            this.labelFreqValue.Size = new System.Drawing.Size(42, 19);
            this.labelFreqValue.TabIndex = 3;
            this.labelFreqValue.Text = "---";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "cad";
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "CADデータ(*.cad)|*.cad";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "CADデータ(*.cad)|*.cad";
            // 
            // GroupBoxMedia
            // 
            this.GroupBoxMedia.Controls.Add(this.labelEps);
            this.GroupBoxMedia.Controls.Add(this.textBoxEps2);
            this.GroupBoxMedia.Controls.Add(this.textBoxEps0);
            this.GroupBoxMedia.Controls.Add(this.textBoxEps1);
            this.GroupBoxMedia.Controls.Add(this.radioBtnMedia2);
            this.GroupBoxMedia.Controls.Add(this.radioBtnMedia1);
            this.GroupBoxMedia.Controls.Add(this.radioBtnMedia0);
            this.GroupBoxMedia.ForeColor = System.Drawing.SystemColors.ControlText;
            this.GroupBoxMedia.Location = new System.Drawing.Point(18, 7);
            this.GroupBoxMedia.Name = "GroupBoxMedia";
            this.GroupBoxMedia.Size = new System.Drawing.Size(207, 125);
            this.GroupBoxMedia.TabIndex = 0;
            this.GroupBoxMedia.TabStop = false;
            this.GroupBoxMedia.Text = "媒質";
            // 
            // labelEps
            // 
            this.labelEps.AutoSize = true;
            this.labelEps.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelEps.Location = new System.Drawing.Point(111, 12);
            this.labelEps.Name = "labelEps";
            this.labelEps.Size = new System.Drawing.Size(69, 12);
            this.labelEps.TabIndex = 20;
            this.labelEps.Text = "比誘電率εr";
            // 
            // textBoxEps2
            // 
            this.textBoxEps2.Location = new System.Drawing.Point(108, 85);
            this.textBoxEps2.Name = "textBoxEps2";
            this.textBoxEps2.Size = new System.Drawing.Size(80, 19);
            this.textBoxEps2.TabIndex = 5;
            this.textBoxEps2.TextChanged += new System.EventHandler(this.textBoxEps2_TextChanged);
            // 
            // textBoxEps0
            // 
            this.textBoxEps0.Location = new System.Drawing.Point(108, 25);
            this.textBoxEps0.Name = "textBoxEps0";
            this.textBoxEps0.ReadOnly = true;
            this.textBoxEps0.Size = new System.Drawing.Size(80, 19);
            this.textBoxEps0.TabIndex = 3;
            this.textBoxEps0.Text = "1.0";
            this.textBoxEps0.TextChanged += new System.EventHandler(this.textBoxEps0_TextChanged);
            // 
            // textBoxEps1
            // 
            this.textBoxEps1.Location = new System.Drawing.Point(108, 55);
            this.textBoxEps1.Name = "textBoxEps1";
            this.textBoxEps1.Size = new System.Drawing.Size(80, 19);
            this.textBoxEps1.TabIndex = 4;
            this.textBoxEps1.TextChanged += new System.EventHandler(this.textBoxEps1_TextChanged);
            // 
            // radioBtnMedia2
            // 
            this.radioBtnMedia2.BackColor = System.Drawing.Color.LightGreen;
            this.radioBtnMedia2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioBtnMedia2.ForeColor = System.Drawing.Color.Black;
            this.radioBtnMedia2.Location = new System.Drawing.Point(12, 85);
            this.radioBtnMedia2.Name = "radioBtnMedia2";
            this.radioBtnMedia2.Size = new System.Drawing.Size(80, 24);
            this.radioBtnMedia2.TabIndex = 2;
            this.radioBtnMedia2.TabStop = true;
            this.radioBtnMedia2.Text = "誘電体2";
            this.radioBtnMedia2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radioBtnMedia2.UseVisualStyleBackColor = false;
            this.radioBtnMedia2.CheckedChanged += new System.EventHandler(this.radioBtnMedia2_CheckedChanged);
            // 
            // radioBtnMedia1
            // 
            this.radioBtnMedia1.BackColor = System.Drawing.Color.MistyRose;
            this.radioBtnMedia1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioBtnMedia1.ForeColor = System.Drawing.Color.Black;
            this.radioBtnMedia1.Location = new System.Drawing.Point(12, 55);
            this.radioBtnMedia1.Name = "radioBtnMedia1";
            this.radioBtnMedia1.Size = new System.Drawing.Size(80, 24);
            this.radioBtnMedia1.TabIndex = 1;
            this.radioBtnMedia1.TabStop = true;
            this.radioBtnMedia1.Text = "誘電体1";
            this.radioBtnMedia1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radioBtnMedia1.UseVisualStyleBackColor = false;
            this.radioBtnMedia1.CheckedChanged += new System.EventHandler(this.radioBtnMedia1_CheckedChanged);
            // 
            // radioBtnMedia0
            // 
            this.radioBtnMedia0.BackColor = System.Drawing.Color.Gray;
            this.radioBtnMedia0.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioBtnMedia0.ForeColor = System.Drawing.Color.Black;
            this.radioBtnMedia0.Location = new System.Drawing.Point(12, 25);
            this.radioBtnMedia0.Name = "radioBtnMedia0";
            this.radioBtnMedia0.Size = new System.Drawing.Size(80, 24);
            this.radioBtnMedia0.TabIndex = 0;
            this.radioBtnMedia0.TabStop = true;
            this.radioBtnMedia0.Text = "真空";
            this.radioBtnMedia0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radioBtnMedia0.UseVisualStyleBackColor = false;
            this.radioBtnMedia0.CheckedChanged += new System.EventHandler(this.radioBtnMedia0_CheckedChanged);
            // 
            // panelMedia
            // 
            this.panelMedia.BackColor = System.Drawing.SystemColors.Control;
            this.panelMedia.Controls.Add(this.GroupBoxMedia);
            this.panelMedia.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panelMedia.Location = new System.Drawing.Point(456, 51);
            this.panelMedia.Name = "panelMedia";
            this.panelMedia.Size = new System.Drawing.Size(242, 147);
            this.panelMedia.TabIndex = 12;
            // 
            // BetaChart
            // 
            chartArea2.Name = "ChartArea1";
            this.BetaChart.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.BetaChart.Legends.Add(legend2);
            this.BetaChart.Location = new System.Drawing.Point(5, 568);
            this.BetaChart.Name = "BetaChart";
            this.BetaChart.Size = new System.Drawing.Size(448, 240);
            this.BetaChart.TabIndex = 0;
            this.BetaChart.TabStop = false;
            this.BetaChart.Text = "chart1";
            title2.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            title2.Name = "Title1";
            this.BetaChart.Titles.Add(title2);
            this.BetaChart.DoubleClick += new System.EventHandler(this.BetaChart_DoubleClick);
            // 
            // EigenVecChart
            // 
            chartArea3.Name = "ChartArea1";
            this.EigenVecChart.ChartAreas.Add(chartArea3);
            legend3.Name = "Legend1";
            this.EigenVecChart.Legends.Add(legend3);
            this.EigenVecChart.Location = new System.Drawing.Point(453, 568);
            this.EigenVecChart.Name = "EigenVecChart";
            this.EigenVecChart.Size = new System.Drawing.Size(448, 240);
            this.EigenVecChart.TabIndex = 0;
            this.EigenVecChart.TabStop = false;
            this.EigenVecChart.Text = "chart1";
            title3.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            title3.Name = "Title1";
            this.EigenVecChart.Titles.Add(title3);
            this.EigenVecChart.DoubleClick += new System.EventHandler(this.EigenVecChart_DoubleClick);
            // 
            // linkLblEigenShow
            // 
            this.linkLblEigenShow.AutoSize = true;
            this.linkLblEigenShow.Location = new System.Drawing.Point(292, 533);
            this.linkLblEigenShow.Name = "linkLblEigenShow";
            this.linkLblEigenShow.Size = new System.Drawing.Size(87, 12);
            this.linkLblEigenShow.TabIndex = 13;
            this.linkLblEigenShow.TabStop = true;
            this.linkLblEigenShow.Text = "固有モードを見る";
            this.linkLblEigenShow.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLblEigenShow_LinkClicked);
            // 
            // linkLabelMeshShow
            // 
            this.linkLabelMeshShow.AutoSize = true;
            this.linkLabelMeshShow.Location = new System.Drawing.Point(251, 32);
            this.linkLabelMeshShow.Name = "linkLabelMeshShow";
            this.linkLabelMeshShow.Size = new System.Drawing.Size(68, 12);
            this.linkLabelMeshShow.TabIndex = 6;
            this.linkLabelMeshShow.TabStop = true;
            this.linkLabelMeshShow.Text = "メッシュを見る";
            this.linkLabelMeshShow.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelMeshShow_LinkClicked);
            // 
            // labelCalcMode
            // 
            this.labelCalcMode.AutoSize = true;
            this.labelCalcMode.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.labelCalcMode.ForeColor = System.Drawing.Color.Navy;
            this.labelCalcMode.Location = new System.Drawing.Point(251, 10);
            this.labelCalcMode.Name = "labelCalcMode";
            this.labelCalcMode.Size = new System.Drawing.Size(48, 12);
            this.labelCalcMode.TabIndex = 0;
            this.labelCalcMode.Text = "H面 TE";
            // 
            // btnLoadCancel
            // 
            this.btnLoadCancel.AutoSize = true;
            this.btnLoadCancel.ForeColor = System.Drawing.Color.Black;
            this.btnLoadCancel.Location = new System.Drawing.Point(420, 10);
            this.btnLoadCancel.Name = "btnLoadCancel";
            this.btnLoadCancel.Size = new System.Drawing.Size(75, 34);
            this.btnLoadCancel.TabIndex = 9;
            this.btnLoadCancel.Text = "読み込み\r\nキャンセル";
            this.btnLoadCancel.UseVisualStyleBackColor = true;
            this.btnLoadCancel.Click += new System.EventHandler(this.btnLoadCancel_Click);
            // 
            // imgcbxCadModeErase
            // 
            this.imgcbxCadModeErase.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.imgcbxCadModeErase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imgcbxCadModeErase.FormattingEnabled = true;
            this.imgcbxCadModeErase.ImageList = this.imageListCadModeErase;
            this.imgcbxCadModeErase.Location = new System.Drawing.Point(95, 46);
            this.imgcbxCadModeErase.Name = "imgcbxCadModeErase";
            this.imgcbxCadModeErase.Size = new System.Drawing.Size(121, 20);
            this.imgcbxCadModeErase.TabIndex = 10;
            this.imgcbxCadModeErase.SelectedIndexChanged += new System.EventHandler(this.imgcbxCadModeErase_SelectedIndexChanged);
            // 
            // imgcbxCadModeArea
            // 
            this.imgcbxCadModeArea.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.imgcbxCadModeArea.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imgcbxCadModeArea.FormattingEnabled = true;
            this.imgcbxCadModeArea.ImageList = this.imageListCadModeArea;
            this.imgcbxCadModeArea.Location = new System.Drawing.Point(66, 46);
            this.imgcbxCadModeArea.Name = "imgcbxCadModeArea";
            this.imgcbxCadModeArea.Size = new System.Drawing.Size(121, 20);
            this.imgcbxCadModeArea.TabIndex = 9;
            this.imgcbxCadModeArea.SelectedIndexChanged += new System.EventHandler(this.imgcbxCadModeArea_SelectedIndexChanged);
            // 
            // MainFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(952, 567);
            this.Controls.Add(this.chkboxAutoCalc);
            this.Controls.Add(this.btnLoadCancel);
            this.Controls.Add(this.linkLabelMeshShow);
            this.Controls.Add(this.labelCalcMode);
            this.Controls.Add(this.FValueLegendPanel);
            this.Controls.Add(this.panelMedia);
            this.Controls.Add(this.linkLblEigenShow);
            this.Controls.Add(this.btnMediaSelect);
            this.Controls.Add(this.GroupBoxCadMode);
            this.Controls.Add(this.btnCalc);
            this.Controls.Add(this.btnRedo);
            this.Controls.Add(this.btnUndo);
            this.Controls.Add(this.btnSaveAs);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.btnNew);
            this.Controls.Add(this.EigenVecChart);
            this.Controls.Add(this.BetaChart);
            this.Controls.Add(this.SMatChart);
            this.Controls.Add(this.FValuePanel);
            this.Controls.Add(this.CadPanel);
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "MainFrm";
            this.Text = "H面導波管シミュレータ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.FValuePanel.ResumeLayout(false);
            this.GroupBoxCadMode.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SMatChart)).EndInit();
            this.SMatChartContextMenuStrip.ResumeLayout(false);
            this.FValueLegendPanel.ResumeLayout(false);
            this.FValueLegendPanel.PerformLayout();
            this.GroupBoxMedia.ResumeLayout(false);
            this.GroupBoxMedia.PerformLayout();
            this.panelMedia.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.BetaChart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EigenVecChart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel CadPanel;
        private System.Windows.Forms.Panel FValuePanel;
        private System.Windows.Forms.Button btnCalc;
        private System.Windows.Forms.RadioButton radioBtnArea;
        private System.Windows.Forms.RadioButton radioBtnPort;
        private System.Windows.Forms.RadioButton radioBtnErase;
        private System.Windows.Forms.GroupBox GroupBoxCadMode;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.DataVisualization.Charting.Chart SMatChart;
        private System.Windows.Forms.Panel FValueLegendPanel;
        private System.Windows.Forms.Button btnNextFreq;
        private System.Windows.Forms.Button btnPrevFreq;
        private System.Windows.Forms.Label labelFreq;
        private System.Windows.Forms.Label labelFreqValue;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.RadioButton radioBtnIncidentPort;
        private System.Windows.Forms.Button btnSaveAs;
        private System.Windows.Forms.RadioButton radioBtnNone;
        private System.Windows.Forms.RadioButton radioBtnPortNumbering;
        private System.Windows.Forms.Button btnMediaSelect;
        private System.Windows.Forms.GroupBox GroupBoxMedia;
        private System.Windows.Forms.Label labelEps;
        private System.Windows.Forms.TextBox textBoxEps2;
        private System.Windows.Forms.TextBox textBoxEps0;
        private System.Windows.Forms.TextBox textBoxEps1;
        private System.Windows.Forms.RadioButton radioBtnMedia2;
        private System.Windows.Forms.RadioButton radioBtnMedia1;
        private System.Windows.Forms.RadioButton radioBtnMedia0;
        private System.Windows.Forms.Panel panelMedia;
        private System.Windows.Forms.DataVisualization.Charting.Chart BetaChart;
        private System.Windows.Forms.DataVisualization.Charting.Chart EigenVecChart;
        private System.Windows.Forms.Button btnUndo;
        private System.Windows.Forms.Button btnRedo;
        private System.Windows.Forms.LinkLabel linkLblEigenShow;
        private System.Windows.Forms.Label labelCalcMode;
        private System.Windows.Forms.LinkLabel linkLabelMeshShow;
        private System.Windows.Forms.Button btnLoadCancel;
        private System.Windows.Forms.Button btnPrevFValuePanel;
        private System.Windows.Forms.Button btnNextFValuePanel;
        private System.Windows.Forms.RadioButton radioBtnLocation;
        private ImageCombobox imgcbxCadModeArea;
        private System.Windows.Forms.ImageList imageListCadModeArea;
        private ImageCombobox imgcbxCadModeErase;
        private System.Windows.Forms.ImageList imageListCadModeErase;
        private System.Windows.Forms.CheckBox chkboxAutoCalc;
        private System.Windows.Forms.ContextMenuStrip SMatChartContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMILogGraph;
    }
}

