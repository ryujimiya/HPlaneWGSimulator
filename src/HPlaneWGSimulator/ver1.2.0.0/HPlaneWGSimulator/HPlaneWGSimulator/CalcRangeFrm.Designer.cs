namespace HPlaneWGSimulator
{
    partial class CalcRangeFrm
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
            this.labelCalcRangeNote = new System.Windows.Forms.Label();
            this.labelDeltaNote = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelCalcRange
            // 
            this.labelCalcRange.AutoSize = true;
            this.labelCalcRange.Location = new System.Drawing.Point(12, 9);
            this.labelCalcRange.Name = "labelCalcRange";
            this.labelCalcRange.Size = new System.Drawing.Size(100, 12);
            this.labelCalcRange.TabIndex = 0;
            this.labelCalcRange.Text = "計算範囲 2W/λ =";
            // 
            // textBoxMinFreq
            // 
            this.textBoxMinFreq.Location = new System.Drawing.Point(118, 6);
            this.textBoxMinFreq.Name = "textBoxMinFreq";
            this.textBoxMinFreq.Size = new System.Drawing.Size(53, 19);
            this.textBoxMinFreq.TabIndex = 2;
            // 
            // labelCalcRangeTo
            // 
            this.labelCalcRangeTo.AutoSize = true;
            this.labelCalcRangeTo.Location = new System.Drawing.Point(177, 9);
            this.labelCalcRangeTo.Name = "labelCalcRangeTo";
            this.labelCalcRangeTo.Size = new System.Drawing.Size(17, 12);
            this.labelCalcRangeTo.TabIndex = 0;
            this.labelCalcRangeTo.Text = "～";
            // 
            // textBoxMaxFreq
            // 
            this.textBoxMaxFreq.Location = new System.Drawing.Point(200, 6);
            this.textBoxMaxFreq.Name = "textBoxMaxFreq";
            this.textBoxMaxFreq.Size = new System.Drawing.Size(53, 19);
            this.textBoxMaxFreq.TabIndex = 3;
            // 
            // labelCalcDelta
            // 
            this.labelCalcDelta.AutoSize = true;
            this.labelCalcDelta.Location = new System.Drawing.Point(12, 58);
            this.labelCalcDelta.Name = "labelCalcDelta";
            this.labelCalcDelta.Size = new System.Drawing.Size(53, 12);
            this.labelCalcDelta.TabIndex = 0;
            this.labelCalcDelta.Text = "計算間隔";
            // 
            // textBoxDeltaFreq
            // 
            this.textBoxDeltaFreq.Location = new System.Drawing.Point(118, 55);
            this.textBoxDeltaFreq.Name = "textBoxDeltaFreq";
            this.textBoxDeltaFreq.Size = new System.Drawing.Size(53, 19);
            this.textBoxDeltaFreq.TabIndex = 4;
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(62, 104);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "実行";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnAbort
            // 
            this.btnAbort.Location = new System.Drawing.Point(169, 104);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 1;
            this.btnAbort.Text = "中止";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // labelCalcRangeNote
            // 
            this.labelCalcRangeNote.AutoSize = true;
            this.labelCalcRangeNote.Location = new System.Drawing.Point(21, 30);
            this.labelCalcRangeNote.Name = "labelCalcRangeNote";
            this.labelCalcRangeNote.Size = new System.Drawing.Size(91, 12);
            this.labelCalcRangeNote.TabIndex = 5;
            this.labelCalcRangeNote.Text = "※1.0～2.0で指定";
            // 
            // labelDeltaNote
            // 
            this.labelDeltaNote.AutoSize = true;
            this.labelDeltaNote.Location = new System.Drawing.Point(21, 77);
            this.labelDeltaNote.Name = "labelDeltaNote";
            this.labelDeltaNote.Size = new System.Drawing.Size(97, 12);
            this.labelDeltaNote.TabIndex = 6;
            this.labelDeltaNote.Text = "※0.01～0.5で指定";
            // 
            // CalcRangeFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 139);
            this.Controls.Add(this.labelDeltaNote);
            this.Controls.Add(this.labelCalcRangeNote);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.textBoxDeltaFreq);
            this.Controls.Add(this.labelCalcDelta);
            this.Controls.Add(this.textBoxMaxFreq);
            this.Controls.Add(this.labelCalcRangeTo);
            this.Controls.Add(this.textBoxMinFreq);
            this.Controls.Add(this.labelCalcRange);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CalcRangeFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "計算範囲";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CalcRangeFrm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.Label labelCalcRangeNote;
        private System.Windows.Forms.Label labelDeltaNote;

    }
}