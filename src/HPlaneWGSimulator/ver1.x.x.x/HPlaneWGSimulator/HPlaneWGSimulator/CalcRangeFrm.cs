using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HPlaneWGSimulator
{
    /// <summary>
    /// 計算範囲入力フォーム
    /// </summary>
    public partial class CalcRangeFrm : Form
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CalcRangeFrm()
        {
            InitializeComponent();

            DialogResult = DialogResult.None;
            textBoxMinFreq.Text = string.Format("{0:F2}", Variables.NormalizedFreqRange[0]);
            textBoxMaxFreq.Text = string.Format("{0:F2}", Variables.NormalizedFreqRange[1]);
            double delta = (Variables.NormalizedFreqRange[1] - Variables.NormalizedFreqRange[0])/Variables.CalcFreqencyPointCount;
            textBoxDeltaFreq.Text = string.Format("{0:F2}", delta);
        }

        /// <summary>
        /// フォームの閉じられる前のイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalcRangeFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.None)
            {
                DialogResult = DialogResult.Abort;
            }
        }

        /// <summary>
        /// [実行]ボタンクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_Click(object sender, EventArgs e)
        {
            double minFreq = double.Parse(textBoxMinFreq.Text);
            double maxFreq = double.Parse(textBoxMaxFreq.Text);
            double deltaFreq = double.Parse(textBoxDeltaFreq.Text);
            /* 規格化周波数の制限は外す
            if (minFreq < Constants.DefNormalizedFreqRange[0] - 1.0e-12 || minFreq > Constants.DefNormalizedFreqRange[1] + 1.0e-12)
            {
                MessageBox.Show(string.Format("開始規格化周波数は{0:F2}～{0:F2}で指定してください", Constants.DefNormalizedFreqRange[0], Constants.DefNormalizedFreqRange[1]));
                return;
            }
            if (maxFreq < Constants.DefNormalizedFreqRange[0] - 1.0e-12 || maxFreq > Constants.DefNormalizedFreqRange[1] + 1.0e-12)
            {
                MessageBox.Show(string.Format("終了規格化周波数は{0:F2}～{0:F2}で指定してください", Constants.DefNormalizedFreqRange[0], Constants.DefNormalizedFreqRange[1]));
                return;
            }
             */
            if (maxFreq - minFreq < 0.1 - 1.0e-12)
            {
                MessageBox.Show("開始と終了は0.1以上離してください");
                return;
            }
            if (deltaFreq < 0.01 - 1.0e-12 || deltaFreq > 0.5 + 1.0e-12)
            {
                MessageBox.Show("計算間隔は0.01～0.5を指定してください");
                return;
            }
            int cnt = (int)((double)(maxFreq - minFreq) / deltaFreq);
            if (cnt < 2)
            {
                //return;
                cnt = 1; // 1箇所で計算
            }
            Variables.CalcFreqencyPointCount = cnt;
            Variables.NormalizedFreqRange[0] = minFreq;
            Variables.NormalizedFreqRange[1] = maxFreq;
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// [中止]ボタンクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAbort_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
        }
    }
}
