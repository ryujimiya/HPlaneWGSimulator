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
        /// 導波路構造区分構造体
        /// </summary>
        struct WGStructureDVStruct
        {
            /// <summary>
            /// 導波路構造区分
            /// </summary>
            public FemSolver.WGStructureDV WGStructureDv;
            /// <summary>
            /// 表示テキスト
            /// </summary>
            public string Text;
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="elemShapeDv">導波路構造区分</param>
            /// <param name="text">表示テキスト</param>
            public WGStructureDVStruct(FemSolver.WGStructureDV wgStructureDv, string text)
            {
                WGStructureDv = wgStructureDv;
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
        /// <summary>
        /// リニアシステムソルバー区分構造体
        /// </summary>
        struct LinearSystemEqnSolverStruct
        {
            /// <summary>
            /// リニアシステムソルバー区分
            /// </summary>
            public FemSolver.LinearSystemEqnSoverDV LsEqnSolverDv;
            /// <summary>
            /// 表示テキスト
            /// </summary>
            public string Text;
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="lsSolverDv">リニアシステムソルバー区分</param>
            /// <param name="text">表示テキスト</param>
            public LinearSystemEqnSolverStruct(FemSolver.LinearSystemEqnSoverDV lsEqnSolverDv, string text)
            {
                LsEqnSolverDv = lsEqnSolverDv;
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
        /// 線形方程式解法区分
        /// </summary>
        public FemSolver.LinearSystemEqnSoverDV LsEqnSolverDv
        {
            get;
            private set;
        }
        /// <summary>
        /// 導波路構造区分
        /// </summary>
        public FemSolver.WGStructureDV WGStructureDv
        {
            get;
            private set;
        }
        /// <summary>
        /// モード区分
        /// </summary>
        public FemSolver.WaveModeDV WaveModeDv
        {
            get;
            private set;
        }
        /// <summary>
        /// 導波路幅(E面解析用)
        /// </summary>
        public double WaveguideWidthForEPlane
        {
            get;
            private set;
        }

        /// <summary>
        /// 計算対象モードラジオボタンリスト
        /// </summary>
        private RadioButton[] RadioBtnModeDvs = null;
    
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="normalizedFreq1">計算開始規格化周波数</param>
        /// <param name="normalizedFreq2">計算終了規格化周波数</param>
        /// <param name="calcFreqCnt">計算点数</param>
        /// <param name="wgStructureDv">導波路構造区分</param>
        /// <param name="waveModeDv">モード区分</param>
        /// <param name="elemShapeDv">要素形状区分</param>
        /// <param name="elemOrder">要素次数</param>
        /// <param name="lsEqnSolverDv">線形方程式解法区分</param>
        /// <param name="waveguideWidthForEPlane">導波路幅(E面解析用)</param>
        public CalcSettingFrm(double normalizedFreq1, double normalizedFreq2, int calcFreqCnt,
            FemSolver.WGStructureDV wgStructureDv,
            FemSolver.WaveModeDV waveModeDv,
            Constants.FemElementShapeDV elemShapeDv, int elemOrder,
            FemSolver.LinearSystemEqnSoverDV lsEqnSolverDv,
            double waveguideWidthForEPlane)
        {
            InitializeComponent();

            DialogResult = DialogResult.None;

            // フィールドに格納
            NormalizedFreq1 = normalizedFreq1;
            NormalizedFreq2 = normalizedFreq2;
            CalcFreqCnt = calcFreqCnt;
            WGStructureDv = wgStructureDv;
            WaveModeDv = waveModeDv;
            ElemShapeDv = elemShapeDv;
            ElemOrder = elemOrder;
            LsEqnSolverDv = lsEqnSolverDv;
            if (CalcFreqCnt == 0)
            {
                // 既定値を設定
                NormalizedFreq1 = Constants.DefNormalizedFreqRange[0];
                NormalizedFreq2 = Constants.DefNormalizedFreqRange[1];
                CalcFreqCnt = Constants.DefCalcFreqencyPointCount;
            }
            // GUIにセット
            // 計算範囲
            textBoxMinFreq.Text = string.Format("{0:F3}", NormalizedFreq1);
            textBoxMaxFreq.Text = string.Format("{0:F3}", NormalizedFreq2);
            double delta = (NormalizedFreq2 - NormalizedFreq1) / CalcFreqCnt;
            textBoxDeltaFreq.Text = string.Format("{0:F3}", delta);
            // 計算モード
            RadioBtnModeDvs = new RadioButton[]{ radioBtnWaveModeDvTE, radioBtnWaveModeDvTM };
            FemSolver.WaveModeDV[] waveModeDvOf_radioBtnModeDvs = { FemSolver.WaveModeDV.TE, FemSolver.WaveModeDV.TM };
            for (int i = 0; i < RadioBtnModeDvs.Length; i++)
            {
                RadioBtnModeDvs[i].Tag = waveModeDvOf_radioBtnModeDvs[i];
                if ((FemSolver.WaveModeDV)RadioBtnModeDvs[i].Tag == WaveModeDv)
                {
                    RadioBtnModeDvs[i].Checked = true;
                }
            }
            // 導波路構造区分
            WGStructureDVStruct[] wgStructureDvStructList = 
            {
                new WGStructureDVStruct(FemSolver.WGStructureDV.HPlane2D, "H面導波管"),
                new WGStructureDVStruct(FemSolver.WGStructureDV.EPlane2D, "E面導波管"),
                new WGStructureDVStruct(FemSolver.WGStructureDV.ParaPlate2D, "平行平板導波路"),
            };
            foreach (WGStructureDVStruct wgStructureDvStruct in wgStructureDvStructList)
            {
                cboxWGStructureDv.Items.Add(wgStructureDvStruct);
                if (wgStructureDvStruct.WGStructureDv == WGStructureDv)
                {
                    cboxWGStructureDv.SelectedItem = wgStructureDvStruct;
                }
            }
            // 導波路幅(E面解析用)
            this.textBoxWaveguideWidthForEPlane.Text = string.Format("{0:F4}", waveguideWidthForEPlane);

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
            // 線形方程式解法
            LinearSystemEqnSolverStruct[] lsList = 
            {
                //new LinearSystemEqnSolverStruct(FemSolver.LinearSystemEqnSoverDV.PCOCG, "PCOCG"),
                new LinearSystemEqnSolverStruct(FemSolver.LinearSystemEqnSoverDV.Zgbsv, "zgbsv(バンド行列)"),
                new LinearSystemEqnSolverStruct(FemSolver.LinearSystemEqnSoverDV.Zgesv, "zgesv(一般行列)"),
            };
            foreach (LinearSystemEqnSolverStruct ls in lsList)
            {
                cboxLsEqnSolverDv.Items.Add(ls);
                if (ls.LsEqnSolverDv == LsEqnSolverDv)
                {
                    cboxLsEqnSolverDv.SelectedItem = ls;
                }
            }
        }

        /// <summary>
        /// 導波管幅(E面解析用)の有効・無効設定
        /// </summary>
        private void setEnableGUI()
        {
            // モード区分
            FemSolver.WaveModeDV waveModeDv = WaveModeDv;
            foreach (RadioButton rbtn in RadioBtnModeDvs)
            {
                if (rbtn.Checked)
                {
                    waveModeDv = (FemSolver.WaveModeDV)rbtn.Tag;
                    break;
                }
            }
            // 導波路構造区分
            if (cboxWGStructureDv.SelectedItem == null)
            {
                return;
            }
            WGStructureDVStruct wgStructureDvStruct = (WGStructureDVStruct)cboxWGStructureDv.SelectedItem;

            // 導波管幅(E面解析用)
            bool enabled;
            enabled = (wgStructureDvStruct.WGStructureDv == FemSolver.WGStructureDV.EPlane2D
                || (wgStructureDvStruct.WGStructureDv == FemSolver.WGStructureDV.HPlane2D && waveModeDv == FemSolver.WaveModeDV.TM)
                );
            this.textBoxWaveguideWidthForEPlane.ReadOnly = !enabled; // 無効の時ReadOnlyはtrue
            //this.textBoxWaveguideWidthForEPlane.Visible = enabled;
            //this.labelWaveguideWidthForEPlane.Visible = enabled;
            //this.labelWaveguideWidthUnit.Visible = enabled;

            // TMモードは平行平板のみ対応
            enabled = (wgStructureDvStruct.WGStructureDv == FemSolver.WGStructureDV.ParaPlate2D);
            foreach (RadioButton rbtn in RadioBtnModeDvs)
            {
                //rbtn.Enabled = enabled;
                rbtn.Visible = enabled;
            }
            labelWaveModeDv.Visible = enabled;
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
            FemSolver.WaveModeDV waveModeDv = WaveModeDv;
            foreach (RadioButton rbtn in RadioBtnModeDvs)
            {
                if (rbtn.Checked)
                {
                    waveModeDv = (FemSolver.WaveModeDV)rbtn.Tag;
                    break;
                }
            }
            // 導波路構造区分
            WGStructureDVStruct wgStructureDvStruct = (WGStructureDVStruct)cboxWGStructureDv.SelectedItem;
            double waveguideWidthForEPlane = double.Parse(textBoxWaveguideWidthForEPlane.Text);
            // 要素形状・次数
            ElemShapeStruct selectedEs = (ElemShapeStruct)cboxElemShapeDv.SelectedItem;
            // 線形方程式解法
            LinearSystemEqnSolverStruct selectedLs = (LinearSystemEqnSolverStruct)cboxLsEqnSolverDv.SelectedItem;

            if (maxFreq - minFreq < Constants.PrecisionLowerLimit)
            {
                MessageBox.Show("開始と終了が同じか逆転しています");
                return;
            }
            if (deltaFreq < Constants.PrecisionLowerLimit)
            {
                MessageBox.Show("計算間隔が設定されていません");
                return;
            }
            if (wgStructureDvStruct.WGStructureDv == FemSolver.WGStructureDV.EPlane2D && Math.Abs(waveguideWidthForEPlane) < Constants.PrecisionLowerLimit)
            {
                MessageBox.Show("導波路幅(E面解析用)が指定されていません");
                return;
            }
            //int cnt = (int)((double)(maxFreq - minFreq) / deltaFreq);
            int cnt = (int)Math.Ceiling((double)(maxFreq - minFreq) / deltaFreq);
            if (cnt < 2)
            {
                //return;
                cnt = 1; // 1箇所で計算
            }
            // TMモードは平行平板以外では計算できない --> TEモードをセットする
            if (wgStructureDvStruct.WGStructureDv != FemSolver.WGStructureDV.ParaPlate2D)
            {
                if (waveModeDv == FemSolver.WaveModeDV.TM)
                {
                    //MessageBox.Show("TMモードでは計算できません。TEモードに変更します。", "", MessageBoxButtons.OK);
                    waveModeDv = FemSolver.WaveModeDV.TE;
                }
            }
            
            // 設定された計算範囲を格納
            CalcFreqCnt = cnt;
            NormalizedFreq1 = minFreq;
            NormalizedFreq2 = maxFreq;
            WGStructureDv = wgStructureDvStruct.WGStructureDv;
            WaveModeDv = waveModeDv;
            ElemShapeDv = selectedEs.ElemShapeDv;
            ElemOrder = selectedEs.Order;
            LsEqnSolverDv = selectedLs.LsEqnSolverDv;
            WaveguideWidthForEPlane = waveguideWidthForEPlane;
            
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

        /// <summary>
        /// 導波路構造コンボボックス選択インデックス変更イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboxWGStructureDv_SelectedIndexChanged(object sender, EventArgs e)
        {
            setEnableGUI();
        }

        /// <summary>
        /// モード区分TEチェック状態変更イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioBtnWaveModeDvTE_CheckedChanged(object sender, EventArgs e)
        {
            setEnableGUI();
        }

        /// <summary>
        /// モード区分TMチェック状態変更イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioBtnWaveModeDvTM_CheckedChanged(object sender, EventArgs e)
        {
            setEnableGUI();
        }

        /// <summary>
        /// フォーム表示時イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalcSettingFrm_Shown(object sender, EventArgs e)
        {
            // GUIの有効無効設定
            setEnableGUI();
        }
    }
}
