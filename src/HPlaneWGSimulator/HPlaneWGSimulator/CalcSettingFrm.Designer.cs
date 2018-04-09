namespace HPlaneWGSimulator
{
    partial class CalcSettingFrm
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
            this.labelCalcRange = new System.Windows.Forms.Label();
            this.textBoxMinFreq = new System.Windows.Forms.TextBox();
            this.labelCalcRangeTo = new System.Windows.Forms.Label();
            this.textBoxMaxFreq = new System.Windows.Forms.TextBox();
            this.labelCalcDelta = new System.Windows.Forms.Label();
            this.textBoxDeltaFreq = new System.Windows.Forms.TextBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.labelElemSapeDv = new System.Windows.Forms.Label();
            this.cboxElemShapeDv = new System.Windows.Forms.ComboBox();
            this.labelWaveModeDv = new System.Windows.Forms.Label();
            this.radioBtnWaveModeDvTE = new System.Windows.Forms.RadioButton();
            this.radioBtnWaveModeDvTM = new System.Windows.Forms.RadioButton();
            this.cboxLsEqnSolverDv = new System.Windows.Forms.ComboBox();
            this.labelLsEqnSolverDv = new System.Windows.Forms.Label();
            this.cboxWGStructureDv = new System.Windows.Forms.ComboBox();
            this.labelWGStructureDv = new System.Windows.Forms.Label();
            this.textBoxWaveguideWidthForEPlane = new System.Windows.Forms.TextBox();
            this.labelWaveguideWidthForEPlane = new System.Windows.Forms.Label();
            this.labelWaveguideWidthUnit = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelCalcRange
            // 
            this.labelCalcRange.AutoSize = true;
            this.labelCalcRange.Location = new System.Drawing.Point(25, 26);
            this.labelCalcRange.Name = "labelCalcRange";
            this.labelCalcRange.Size = new System.Drawing.Size(100, 12);
            this.labelCalcRange.TabIndex = 0;
            this.labelCalcRange.Text = "計算範囲 2W/λ =";
            // 
            // textBoxMinFreq
            // 
            this.textBoxMinFreq.Location = new System.Drawing.Point(131, 23);
            this.textBoxMinFreq.Name = "textBoxMinFreq";
            this.textBoxMinFreq.Size = new System.Drawing.Size(53, 19);
            this.textBoxMinFreq.TabIndex = 1;
            // 
            // labelCalcRangeTo
            // 
            this.labelCalcRangeTo.AutoSize = true;
            this.labelCalcRangeTo.Location = new System.Drawing.Point(190, 26);
            this.labelCalcRangeTo.Name = "labelCalcRangeTo";
            this.labelCalcRangeTo.Size = new System.Drawing.Size(17, 12);
            this.labelCalcRangeTo.TabIndex = 0;
            this.labelCalcRangeTo.Text = "～";
            // 
            // textBoxMaxFreq
            // 
            this.textBoxMaxFreq.Location = new System.Drawing.Point(213, 23);
            this.textBoxMaxFreq.Name = "textBoxMaxFreq";
            this.textBoxMaxFreq.Size = new System.Drawing.Size(53, 19);
            this.textBoxMaxFreq.TabIndex = 2;
            // 
            // labelCalcDelta
            // 
            this.labelCalcDelta.AutoSize = true;
            this.labelCalcDelta.Location = new System.Drawing.Point(25, 54);
            this.labelCalcDelta.Name = "labelCalcDelta";
            this.labelCalcDelta.Size = new System.Drawing.Size(53, 12);
            this.labelCalcDelta.TabIndex = 0;
            this.labelCalcDelta.Text = "計算間隔";
            // 
            // textBoxDeltaFreq
            // 
            this.textBoxDeltaFreq.Location = new System.Drawing.Point(131, 51);
            this.textBoxDeltaFreq.Name = "textBoxDeltaFreq";
            this.textBoxDeltaFreq.Size = new System.Drawing.Size(53, 19);
            this.textBoxDeltaFreq.TabIndex = 3;
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(39, 313);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "実行";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnAbort
            // 
            this.btnAbort.Location = new System.Drawing.Point(146, 313);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 1;
            this.btnAbort.Text = "中止";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // labelElemSapeDv
            // 
            this.labelElemSapeDv.AutoSize = true;
            this.labelElemSapeDv.Location = new System.Drawing.Point(25, 26);
            this.labelElemSapeDv.Name = "labelElemSapeDv";
            this.labelElemSapeDv.Size = new System.Drawing.Size(83, 12);
            this.labelElemSapeDv.TabIndex = 7;
            this.labelElemSapeDv.Text = "要素形状・次数";
            // 
            // cboxElemShapeDv
            // 
            this.cboxElemShapeDv.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxElemShapeDv.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboxElemShapeDv.FormattingEnabled = true;
            this.cboxElemShapeDv.Location = new System.Drawing.Point(131, 26);
            this.cboxElemShapeDv.Name = "cboxElemShapeDv";
            this.cboxElemShapeDv.Size = new System.Drawing.Size(135, 20);
            this.cboxElemShapeDv.TabIndex = 1;
            // 
            // labelWaveModeDv
            // 
            this.labelWaveModeDv.AutoSize = true;
            this.labelWaveModeDv.Location = new System.Drawing.Point(16, 74);
            this.labelWaveModeDv.Name = "labelWaveModeDv";
            this.labelWaveModeDv.Size = new System.Drawing.Size(57, 12);
            this.labelWaveModeDv.TabIndex = 0;
            this.labelWaveModeDv.Text = "モード区分";
            // 
            // radioBtnWaveModeDvTE
            // 
            this.radioBtnWaveModeDvTE.AutoSize = true;
            this.radioBtnWaveModeDvTE.Location = new System.Drawing.Point(138, 72);
            this.radioBtnWaveModeDvTE.Name = "radioBtnWaveModeDvTE";
            this.radioBtnWaveModeDvTE.Size = new System.Drawing.Size(37, 16);
            this.radioBtnWaveModeDvTE.TabIndex = 3;
            this.radioBtnWaveModeDvTE.TabStop = true;
            this.radioBtnWaveModeDvTE.Text = "TE";
            this.radioBtnWaveModeDvTE.UseVisualStyleBackColor = true;
            this.radioBtnWaveModeDvTE.CheckedChanged += new System.EventHandler(this.radioBtnWaveModeDvTE_CheckedChanged);
            // 
            // radioBtnWaveModeDvTM
            // 
            this.radioBtnWaveModeDvTM.AutoSize = true;
            this.radioBtnWaveModeDvTM.Location = new System.Drawing.Point(199, 72);
            this.radioBtnWaveModeDvTM.Name = "radioBtnWaveModeDvTM";
            this.radioBtnWaveModeDvTM.Size = new System.Drawing.Size(39, 16);
            this.radioBtnWaveModeDvTM.TabIndex = 4;
            this.radioBtnWaveModeDvTM.TabStop = true;
            this.radioBtnWaveModeDvTM.Tag = "";
            this.radioBtnWaveModeDvTM.Text = "TM";
            this.radioBtnWaveModeDvTM.UseVisualStyleBackColor = true;
            this.radioBtnWaveModeDvTM.CheckedChanged += new System.EventHandler(this.radioBtnWaveModeDvTM_CheckedChanged);
            // 
            // cboxLsEqnSolverDv
            // 
            this.cboxLsEqnSolverDv.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxLsEqnSolverDv.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboxLsEqnSolverDv.FormattingEnabled = true;
            this.cboxLsEqnSolverDv.Location = new System.Drawing.Point(131, 62);
            this.cboxLsEqnSolverDv.Name = "cboxLsEqnSolverDv";
            this.cboxLsEqnSolverDv.Size = new System.Drawing.Size(135, 20);
            this.cboxLsEqnSolverDv.TabIndex = 2;
            // 
            // labelLsEqnSolverDv
            // 
            this.labelLsEqnSolverDv.AutoSize = true;
            this.labelLsEqnSolverDv.Location = new System.Drawing.Point(25, 62);
            this.labelLsEqnSolverDv.Name = "labelLsEqnSolverDv";
            this.labelLsEqnSolverDv.Size = new System.Drawing.Size(89, 12);
            this.labelLsEqnSolverDv.TabIndex = 0;
            this.labelLsEqnSolverDv.Text = "線形方程式解法";
            // 
            // cboxWGStructureDv
            // 
            this.cboxWGStructureDv.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxWGStructureDv.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboxWGStructureDv.FormattingEnabled = true;
            this.cboxWGStructureDv.Location = new System.Drawing.Point(138, 18);
            this.cboxWGStructureDv.Name = "cboxWGStructureDv";
            this.cboxWGStructureDv.Size = new System.Drawing.Size(135, 20);
            this.cboxWGStructureDv.TabIndex = 1;
            this.cboxWGStructureDv.SelectedIndexChanged += new System.EventHandler(this.cboxWGStructureDv_SelectedIndexChanged);
            // 
            // labelWGStructureDv
            // 
            this.labelWGStructureDv.AutoSize = true;
            this.labelWGStructureDv.Location = new System.Drawing.Point(16, 21);
            this.labelWGStructureDv.Name = "labelWGStructureDv";
            this.labelWGStructureDv.Size = new System.Drawing.Size(65, 12);
            this.labelWGStructureDv.TabIndex = 10;
            this.labelWGStructureDv.Text = "導波路構造";
            // 
            // textBoxWaveguideWidthForEPlane
            // 
            this.textBoxWaveguideWidthForEPlane.Location = new System.Drawing.Point(138, 44);
            this.textBoxWaveguideWidthForEPlane.Name = "textBoxWaveguideWidthForEPlane";
            this.textBoxWaveguideWidthForEPlane.Size = new System.Drawing.Size(67, 19);
            this.textBoxWaveguideWidthForEPlane.TabIndex = 2;
            // 
            // labelWaveguideWidthForEPlane
            // 
            this.labelWaveguideWidthForEPlane.AutoSize = true;
            this.labelWaveguideWidthForEPlane.Location = new System.Drawing.Point(16, 48);
            this.labelWaveguideWidthForEPlane.Name = "labelWaveguideWidthForEPlane";
            this.labelWaveguideWidthForEPlane.Size = new System.Drawing.Size(104, 12);
            this.labelWaveguideWidthForEPlane.TabIndex = 11;
            this.labelWaveguideWidthForEPlane.Text = "導波管幅(E面解析)";
            // 
            // labelWaveguideWidthUnit
            // 
            this.labelWaveguideWidthUnit.AutoSize = true;
            this.labelWaveguideWidthUnit.Location = new System.Drawing.Point(211, 51);
            this.labelWaveguideWidthUnit.Name = "labelWaveguideWidthUnit";
            this.labelWaveguideWidthUnit.Size = new System.Drawing.Size(29, 12);
            this.labelWaveguideWidthUnit.TabIndex = 13;
            this.labelWaveguideWidthUnit.Text = "目盛";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cboxWGStructureDv);
            this.groupBox1.Controls.Add(this.labelWaveguideWidthUnit);
            this.groupBox1.Controls.Add(this.labelWGStructureDv);
            this.groupBox1.Controls.Add(this.labelWaveguideWidthForEPlane);
            this.groupBox1.Controls.Add(this.textBoxWaveguideWidthForEPlane);
            this.groupBox1.Controls.Add(this.labelWaveModeDv);
            this.groupBox1.Controls.Add(this.radioBtnWaveModeDvTE);
            this.groupBox1.Controls.Add(this.radioBtnWaveModeDvTM);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(289, 100);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "導波路";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.labelElemSapeDv);
            this.groupBox2.Controls.Add(this.cboxElemShapeDv);
            this.groupBox2.Controls.Add(this.cboxLsEqnSolverDv);
            this.groupBox2.Controls.Add(this.labelLsEqnSolverDv);
            this.groupBox2.Location = new System.Drawing.Point(12, 198);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(289, 96);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "有限要素法解法";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.labelCalcRange);
            this.groupBox3.Controls.Add(this.textBoxMinFreq);
            this.groupBox3.Controls.Add(this.labelCalcRangeTo);
            this.groupBox3.Controls.Add(this.textBoxMaxFreq);
            this.groupBox3.Controls.Add(this.labelCalcDelta);
            this.groupBox3.Controls.Add(this.textBoxDeltaFreq);
            this.groupBox3.Location = new System.Drawing.Point(12, 118);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(289, 74);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "計算周波数";
            // 
            // CalcSettingFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(321, 348);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.btnRun);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CalcSettingFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "計算設定";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CalcRangeFrm_FormClosing);
            this.Shown += new System.EventHandler(this.CalcSettingFrm_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelCalcRange;
        private System.Windows.Forms.TextBox textBoxMinFreq;
        private System.Windows.Forms.Label labelCalcRangeTo;
        private System.Windows.Forms.TextBox textBoxMaxFreq;
        private System.Windows.Forms.Label labelCalcDelta;
        private System.Windows.Forms.TextBox textBoxDeltaFreq;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnAbort;
        private System.Windows.Forms.Label labelWaveModeDv;
        private System.Windows.Forms.RadioButton radioBtnWaveModeDvTE;
        private System.Windows.Forms.RadioButton radioBtnWaveModeDvTM;
        private System.Windows.Forms.Label labelElemSapeDv;
        private System.Windows.Forms.ComboBox cboxElemShapeDv;
        private System.Windows.Forms.ComboBox cboxLsEqnSolverDv;
        private System.Windows.Forms.Label labelLsEqnSolverDv;
        private System.Windows.Forms.ComboBox cboxWGStructureDv;
        private System.Windows.Forms.Label labelWGStructureDv;
        private System.Windows.Forms.TextBox textBoxWaveguideWidthForEPlane;
        private System.Windows.Forms.Label labelWaveguideWidthForEPlane;
        private System.Windows.Forms.Label labelWaveguideWidthUnit;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;

    }
}