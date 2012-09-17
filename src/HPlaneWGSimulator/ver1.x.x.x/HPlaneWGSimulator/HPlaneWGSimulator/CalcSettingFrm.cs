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
    /*public*/ partial class CalcSettingFrm : Form
    {
        /////////////////////////////////////////////////////////////////////////////
        // 型
        /////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 要素形状構造体
        /// </summary>
        struct ElemShapeStruct
        {
            /// <summary>
            /// 要素形状区分
            /// </summary>
            public Constants.FemElementShapeDV ElemShapeDv;
            /// <summary>
            /// 補間次数
            /// </summary>
            public int Order;
            /// <summary>
            /// 表示テキスト
            /// </summary>
            public string Text;
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="elemShapeDv">要素形状区分</param>
            /// <param name="order">補間次数</param>
            /// <param name="text">表示テキスト</param>
            public ElemShapeStruct(Constants.FemElementShapeDV elemShapeDv, int order, string text)
            {
                ElemShapeDv = elemShapeDv;
                Order = order;
                Text = text;
            }
            /// <summary>
            /// 文字列に変換する
            ///    コンボボックスの表示用テキストを返却する
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                //return base.ToString();
                return Text;
            }
        }
        /////////////////////////////////////////////////////////////////////////////
        // フィールド
        /////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 規格化周波数(開始)
        /// </summary>
        public double NormalizedFreq1
        {
            get;
            private set;
        }
        /// <summary>
        /// 規格化周波数(終了)
        /// </summary>
        public double NormalizedFreq2
        {
            get;
            private set;
        }
        /// <summary>
        /// 計算ポイント数
        /// </summary>
        public int CalcFreqCnt
        {
            get;
            private set;
        }
        /// <summary>
        /// 要素形状区分
        /// </summary>
        public Constants.FemElementShapeDV ElemShapeDv
        {
            get;
            private set;
        }
        /// <summary>
        /// 要素補間次数
        /// </summary>
        public int ElemOrder
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="normalizedFreq1">計算開始規格化周波数</param>
        /// <param name="normalizedFreq2">計算終了規格化周波数</param>
        /// <param name="calcFreqCnt">計算点数</param>
        /// <param name="elemShapeDv">要素形状区分</param>
        /// <param name="elemOrder">要素次数</param>
        public CalcSettingFrm(double normalizedFreq1, double normalizedFreq2, int calcFreqCnt,
            Constants.FemElementShapeDV elemShapeDv, int elemOrder)
        {
            InitializeComponent();

            DialogResult = DialogResult.None;

            // フィールドに格納
            NormalizedFreq1 = normalizedFreq1;
            NormalizedFreq2 = normalizedFreq2;
            CalcFreqCnt = calcFreqCnt;
            ElemShapeDv = elemShapeDv;
            ElemOrder = elemOrder;
            if (CalcFreqCnt == 0)
            {
                // 既定値を設定
                NormalizedFreq1 = Constants.DefNormalizedFreqRange[0];
                NormalizedFreq2 = Constants.DefNormalizedFreqRange[1];
                CalcFreqCnt = Constants.DefCalcFreqencyPointCount;
                ElemShapeDv = Constants.DefElemShapeDv;
                ElemOrder = Constants.DefElementOrder;
            }

            // GUIにセット
            // 計算範囲
            textBoxMinFreq.Text = string.Format("{0:F2}", NormalizedFreq1);
            textBoxMaxFreq.Text = string.Format("{0:F2}", NormalizedFreq2);
            double delta = (NormalizedFreq2 - NormalizedFreq1) / CalcFreqCnt;
            textBoxDeltaFreq.Text = string.Format("{0:F2}", delta);
            // 要素形状・次数
            ElemShapeStruct[] esList =
            {
                new ElemShapeStruct(Constants.FemElementShapeDV.Triangle, Constants.SecondOrder, "２次三角形要素"),
                new ElemShapeStruct(Constants.FemElementShapeDV.QuadType2, Constants.SecondOrder, "２次四角形要素"),
                new ElemShapeStruct(Constants.FemElementShapeDV.Triangle, Constants.FirstOrder, "１次三角形要素"),
                new ElemShapeStruct(Constants.FemElementShapeDV.QuadType2, Constants.FirstOrder, "１次四角形要素"),
            };
            foreach (ElemShapeStruct es in esList)
            {
                cboxElemShapeDv.Items.Add(es);
                if (es.ElemShapeDv == ElemShapeDv && es.Order == ElemOrder)
                {
                    cboxElemShapeDv.SelectedItem = es;
                }
            }
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
            // GUIからデータを取得
            // 計算範囲
            double minFreq = double.Parse(textBoxMinFreq.Text);
            double maxFreq = double.Parse(textBoxMaxFreq.Text);
            double deltaFreq = double.Parse(textBoxDeltaFreq.Text);
            // 要素形状・次数
            ElemShapeStruct selectedEs = (ElemShapeStruct)cboxElemShapeDv.SelectedItem;

            /* 規格化周波数の制限は外す
            if (minFreq < Constants.DefNormalizedFreqRange[0] - Constants.PrecisionLowerLimit || minFreq > Constants.DefNormalizedFreqRange[1] + Constants.PrecisionLowerLimit)
            {
                MessageBox.Show(string.Format("開始規格化周波数は{0:F2}～{0:F2}で指定してください", Constants.DefNormalizedFreqRange[0], Constants.DefNormalizedFreqRange[1]));
                return;
            }
            if (maxFreq < Constants.DefNormalizedFreqRange[0] - Constants.PrecisionLowerLimit || maxFreq > Constants.DefNormalizedFreqRange[1] + Constants.PrecisionLowerLimit)
            {
                MessageBox.Show(string.Format("終了規格化周波数は{0:F2}～{0:F2}で指定してください", Constants.DefNormalizedFreqRange[0], Constants.DefNormalizedFreqRange[1]));
                return;
            }
             */
            if (maxFreq - minFreq < 0.1 - Constants.PrecisionLowerLimit)
            {
                MessageBox.Show("開始と終了は0.1以上離してください");
                return;
            }
            if (deltaFreq < 0.01 - Constants.PrecisionLowerLimit || deltaFreq > 0.5 + Constants.PrecisionLowerLimit)
            {
                MessageBox.Show("計算間隔は0.01～0.5を指定してください");
                return;
            }
            //int cnt = (int)((double)(maxFreq - minFreq) / deltaFreq);
            int cnt = (int)Math.Ceiling((double)(maxFreq - minFreq) / deltaFreq);
            if (cnt < 2)
            {
                //return;
                cnt = 1; // 1箇所で計算
            }
            
            // 設定された計算範囲を格納
            CalcFreqCnt = cnt;
            NormalizedFreq1 = minFreq;
            NormalizedFreq2 = maxFreq;
            ElemShapeDv = selectedEs.ElemShapeDv;
            ElemOrder = selectedEs.Order;
            
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
