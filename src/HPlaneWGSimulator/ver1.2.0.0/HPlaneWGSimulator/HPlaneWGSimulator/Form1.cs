using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Threading;

namespace HPlaneWGSimulator
{
    public partial class Form1 : Form
    {
        ////////////////////////////////////////////////////////////////////////
        // 型
        ////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// フォームコントロール同期用デリゲート
        /// </summary>
        delegate void InvokeDelegate();
        delegate void ParameterizedInvokeDelegate(params Object[] parameter);
        ////////////////////////////////////////////////////////////////////////
        // 定数
        ////////////////////////////////////////////////////////////////////////
        
        ////////////////////////////////////////////////////////////////////////
        // フィールド
        ////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// フォームのタイトル
        /// </summary>
        private string TitleBaseName = "";
        /// <summary>
        /// データファイルパス拡張子抜き
        /// </summary>
        //private string FilePathWithoutExt = "";
        /// <summary>
        /// CADデータファイルパス
        /// </summary>
        private string CadDatFilePath = "";
        /// <summary>
        /// Fem入力データファイルパス
        /// </summary>
        private string FemInputDatFilePath = "";
        /// <summary>
        /// Fem出力データファイルパス
        /// </summary>
        private string FemOutputDatFilePath = "";
        /// <summary>
        /// Cadロジック
        /// </summary>
        private CadLogic CadLgc = null;
        /// <summary>
        /// 解析機
        /// </summary>
        private FemSolver Solver = null;
        /// <summary>
        /// ポストプロセッサ
        /// </summary>
        private FemPostProLogic PostPro = null;
        /// <summary>
        /// Cadモード選択ラジオボタンリスト
        /// </summary>
        private RadioButton[] CadModeRadioButtons = null;
        /// <summary>
        /// 媒質選択ラジオボタンリスト
        /// </summary>
        private RadioButton[] MediaRadioButtons = null;
        /// <summary>
        /// 媒質の比誘電率入力テキストボックスリスト
        /// </summary>
        private TextBox[] EpsTextBoxes = null;
        /// <summary>
        /// 周波数に対応するインデックス(1..FemSolver.CalcFreqCnt - 1)
        /// </summary>
        private int FreqNo = -1;
        /// <summary>
        /// 計算スレッド
        /// </summary>
        private Thread SolverThread = null;
        /// <summary>
        /// フォームの初期サイズ
        /// </summary>
        private Size FrmBaseSize;
        private Point CadPanelBaseLocation;
        private Size CadPanelBaseSize;
        private Point GroupBoxCadModeBaseLocation;
        private Size GroupBoxCadModeBaseSize;
        private Point BtnMediaSelectBaseLocation;
        private Size BtnMediaSelectBaseSize;
        private Point FValuePanelBaseLocation;
        private Size FValuePanelBaseSize;
        private Point FValueLegendPanelBaseLocation;
        private Size FValueLegendPanelBaseSize;
        private Point SMatChartBaseLocation;
        private Size SMatChartBaseSize;
        private Point LinkLblEigenShowBaseLocation;
        private Size LinkLblEigenShowBaseSize;
        private Point BetaChartBaseLocation;
        private Size BetaChartBaseSize;
        private Point EigenVecChartBaseLocation;
        private Size EigenVecChartBaseSize;
        private Control MaximizedControl = null;
        /// <summary>
        /// 固有モード表示フラグ
        /// </summary>
        private bool EigenShowFlg = false;
        /// <summary>
        /// フォームの通常時のサイズ(ほぼRestoreBounds.Sizeと同じだが、最大化時[固有モードを見る]を実行すると異なってくる
        /// </summary>
        private Size FrmNormalSize;
        private FormWindowState PrevWindowState = FormWindowState.Normal;


        /// <summary>
        /// ウィンドウコンストラクタ
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            init();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void init()
        {
            CadModeRadioButtons = new RadioButton[]
            {
                radioBtnNone,
                radioBtnArea,
                radioBtnPort,
                radioBtnErase,
                radioBtnIncidentPort,
                radioBtnPortNumbering
            };
            MediaRadioButtons = new RadioButton[]
            {
                radioBtnMedia0,
                radioBtnMedia1,
                radioBtnMedia2
            };
            EpsTextBoxes = new TextBox[]
            {
                textBoxEps0,
                textBoxEps1,
                textBoxEps2
            };
            System.Diagnostics.Debug.Assert(MediaRadioButtons.Length == Constants.MaxMediaCount);
            System.Diagnostics.Debug.Assert(EpsTextBoxes.Length == Constants.MaxMediaCount);
            panelMedia.Visible = false;

            Variables.Reset();
            CadLgc = new CadLogic(CadPanel);
            Solver = new FemSolver();
            PostPro = new FemPostProLogic();

            // パネルサイズを記憶する
            savePanelSize();

            //this.DoubleBuffered = true;
            // ダブルバッファ制御用のプロパティを強制的に取得する
            System.Reflection.PropertyInfo p;
            p = typeof(System.Windows.Forms.Control).GetProperty(
                         "DoubleBuffered",
                          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // ダブルバッファを有効にする
            p.SetValue(CadPanel, true, null);
            p.SetValue(FValuePanel, true, null);

            // フォームのタイトルを退避
            TitleBaseName = this.Text + " " + MyUtilLib.MyUtil.getAppVersion();

            // ファイル名付きフォームタイトルを設定
            setFrmTitle();

            // GUI初期化
            resetGUI();
            
            // フィールド値凡例パネルの初期化
            PostPro.InitFValueLegend(FValueLegendPanel, labelFreqValue);
        }

        /// <summary>
        /// 自動スクロールを設定する
        /// </summary>
        /// <param name="autoScroll"></param>
        private void setAutoScroll(bool autoScroll)
        {
            this.AutoScroll = autoScroll;
            this.AutoScrollOffset = new Point(0, 0);
            this.AutoScrollPosition = new Point(0, 0);
        }

        /// <summary>
        /// パネルの初期値、サイズを記憶する
        /// </summary>
        private void savePanelSize()
        {
            // 初期の位置、サイズを記憶する
            FrmBaseSize = this.ClientSize;
            CadPanelBaseLocation = CadPanel.Location;
            CadPanelBaseSize = CadPanel.Size;
            GroupBoxCadModeBaseLocation = GroupBoxCadMode.Location;
            GroupBoxCadModeBaseSize = GroupBoxCadMode.Size;
            BtnMediaSelectBaseLocation = btnMediaSelect.Location;
            BtnMediaSelectBaseSize = btnMediaSelect.Size;
            LinkLblEigenShowBaseLocation = linkLblEigenShow.Location;
            LinkLblEigenShowBaseSize = linkLblEigenShow.Size;
            FValuePanelBaseLocation = FValuePanel.Location;
            FValuePanelBaseSize = FValuePanel.Size;
            FValueLegendPanelBaseLocation = FValueLegendPanel.Location;
            FValueLegendPanelBaseSize = FValueLegendPanel.Size;
            SMatChartBaseLocation = SMatChart.Location;
            SMatChartBaseSize = SMatChart.Size;
            BetaChartBaseLocation = BetaChart.Location;
            BetaChartBaseSize = BetaChart.Size;
            EigenVecChartBaseLocation = EigenVecChart.Location;
            EigenVecChartBaseSize = EigenVecChart.Size;
        }

        /// <summary>
        /// パネルの初期値、サイズに戻す
        /// </summary>
        private void restorePanelSize()
        {
            this.ClientSize = FrmBaseSize;
            CadPanel.Location = CadPanelBaseLocation;
            CadPanel.Size = CadPanelBaseSize;
            GroupBoxCadMode.Location = GroupBoxCadModeBaseLocation;
            GroupBoxCadMode.Size = GroupBoxCadModeBaseSize;
            btnMediaSelect.Location = BtnMediaSelectBaseLocation;
            btnMediaSelect.Size = BtnMediaSelectBaseSize;
            linkLblEigenShow.Location = LinkLblEigenShowBaseLocation;
            linkLblEigenShow.Size = LinkLblEigenShowBaseSize;
            FValuePanel.Location = FValuePanelBaseLocation;
            FValuePanel.Size = FValuePanelBaseSize;
            FValueLegendPanel.Location = FValueLegendPanelBaseLocation;
            FValueLegendPanel.Size = FValueLegendPanelBaseSize;
            SMatChart.Location = SMatChartBaseLocation;
            SMatChart.Size = SMatChartBaseSize;
            BetaChart.Location = BetaChartBaseLocation;
            BetaChart.Size = BetaChartBaseSize;
            EigenVecChart.Location = EigenVecChartBaseLocation;
            EigenVecChart.Size = EigenVecChartBaseSize;

            CadLgc.SetupRegionSize();
            CadPanel.Invalidate();
            FValuePanel.Invalidate();
        }

        /// <summary>
        /// パネルのサイズをフォームのサイズに合わせる
        /// </summary>
        private void fitPanelSizeToFrmSize()
        {
            Control[] ctrlList = { SMatChart, BetaChart, EigenVecChart };
            Point[] ctrlBaseLocationList = { SMatChartBaseLocation, BetaChartBaseLocation, EigenVecChartBaseLocation };
            Size[] ctrlBaseSizeList = { SMatChartBaseSize, BetaChartBaseSize, EigenVecChartBaseSize };

            // 個別パネルの最大化処理
            if (MaximizedControl == CadPanel)
            {
                doCadPanelMaximize(ctrlList);
                return;
            }
            else if (MaximizedControl == FValuePanel)
            {
                doFValuePanelMaximize(ctrlList);
                return;
            }
            else if (MaximizedControl != null && ctrlList.Contains(MaximizedControl))
            {
                doControlMaximize(MaximizedControl, ctrlList, ctrlBaseLocationList, ctrlBaseSizeList);
                return;
            }
            else if (MaximizedControl != null)
            {
                // 対応コントロールでない
                // 通常モードに戻す
                MaximizedControl = null; // fail safe
            }

            // 通常の場合
            if (!CadPanel.Visible)
            {
                CadPanel.Visible = true;
            }
            if (!GroupBoxCadMode.Visible)
            {
                GroupBoxCadMode.Visible = true;
            }
            if (!FValuePanel.Visible)
            {
                FValuePanel.Visible = true;
            }
            if (!FValueLegendPanel.Visible)
            {
                FValueLegendPanel.Visible = true;
            }
            if (!SMatChart.Visible)
            {
                SMatChart.Visible = true;
            }
            if (!btnMediaSelect.Visible)
            {
                btnMediaSelect.Visible = true;
            }
            panelMedia.Visible = false;
            if (!linkLblEigenShow.Visible)
            {
                linkLblEigenShow.Visible = true;
            }
            BetaChart.Visible = EigenShowFlg;
            EigenVecChart.Visible = EigenShowFlg;

            // 自動スクロールを一旦やめる
            setAutoScroll(false);

            this.SuspendLayout();
            // パネル配置変更
            double r = (this.ClientSize.Width - SystemInformation.VerticalScrollBarWidth) / (double)(FrmBaseSize.Width - SystemInformation.VerticalScrollBarWidth);
            //            double r = this.ClientSize.Width / (double)FrmBaseSize.Width;
            if (r <= 1.0)
            {
                r = 1.0;
            }
            CadPanel.Location = CadPanelBaseLocation;
            CadPanel.Size = new Size((int)((double)CadPanelBaseSize.Width * r), (int)((double)CadPanelBaseSize.Height * r));
            GroupBoxCadMode.Location = new Point(CadPanel.Left, CadPanel.Bottom);
            GroupBoxCadMode.Size = GroupBoxCadModeBaseSize;
            btnMediaSelect.Location = new Point(BtnMediaSelectBaseLocation.X, GroupBoxCadMode.Top + radioBtnNone.Top);
            btnMediaSelect.Size = BtnMediaSelectBaseSize;
            panelMedia.Location = new Point(btnMediaSelect.Location.X, btnMediaSelect.Location.Y - panelMedia.Height);
            linkLblEigenShow.Location = new Point(LinkLblEigenShowBaseLocation.X, btnMediaSelect.Bottom - linkLblEigenShow.Height);
            FValuePanel.Location = new Point((ClientSize.Width - SystemInformation.VerticalScrollBarWidth - FValueLegendPanelBaseSize.Width - (int)((double)FValuePanelBaseSize.Width * r)), FValuePanelBaseLocation.Y);
            FValuePanel.Size = new Size((int)((double)FValuePanelBaseSize.Width * r), (int)((double)FValuePanelBaseSize.Height * r));
            FValueLegendPanel.Location = new Point(ClientSize.Width - SystemInformation.VerticalScrollBarWidth - FValueLegendPanelBaseSize.Width, FValueLegendPanelBaseLocation.Y);
            FValueLegendPanel.Size = FValueLegendPanelBaseSize;
            SMatChart.Location = new Point(ClientSize.Width - SystemInformation.VerticalScrollBarWidth - (int)((double)SMatChartBaseSize.Width * r), (FValuePanel.Bottom > FValueLegendPanel.Bottom) ? FValuePanel.Bottom : FValueLegendPanel.Bottom);
            SMatChart.Size = new Size((int)((double)SMatChartBaseSize.Width * r), (int)((double)SMatChartBaseSize.Height * r));
            int betaChartYPos = (GroupBoxCadMode.Bottom > SMatChart.Bottom)? GroupBoxCadMode.Bottom : SMatChart.Bottom;
            BetaChart.Location = new Point(CadPanel.Left, betaChartYPos);
            BetaChart.Size = new Size((int)((double)BetaChartBaseSize.Width * r), (int)((double)BetaChartBaseSize.Height * r));
            EigenVecChart.Location = new Point(SMatChart.Right - (int)((double)EigenVecChartBaseSize.Width * r), betaChartYPos);
            EigenVecChart.Size = new Size((int)((double)EigenVecChartBaseSize.Width * r), (int)((double)EigenVecChartBaseSize.Height * r));
            this.ResumeLayout();

            CadLgc.SetupRegionSize();
            CadPanel.Invalidate();
            FValuePanel.Invalidate();

            // 自動スクロールの情報を更新するため明示的に再設定しています。
            setAutoScroll(true);
        }

        /// <summary>
        /// Cadパネルを最大化する
        /// </summary>
        private void doCadPanelMaximize(Control[] ctrlList)
        {
            // 自動スクロールを一旦やめる
            setAutoScroll(false);

            this.SuspendLayout();
            // パネル配置変更
            double r = (this.ClientSize.Width - SystemInformation.VerticalScrollBarWidth) / (double)CadPanelBaseSize.Width;
            CadPanel.Location = new Point(0, btnNew.Bottom); // ファイル操作ボタンの高さ分ずらす
            CadPanel.Size = new Size((int)((double)CadPanelBaseSize.Width * r), (int)((double)CadPanelBaseSize.Height * r));
            GroupBoxCadMode.Location = CadPanel.Location + new Size(0, CadPanel.Height - GroupBoxCadModeBaseSize.Height);
            GroupBoxCadMode.Size = GroupBoxCadModeBaseSize;
            btnMediaSelect.Location = new Point(BtnMediaSelectBaseLocation.X, GroupBoxCadMode.Top + radioBtnNone.Top);
            btnMediaSelect.Size = BtnMediaSelectBaseSize;
            panelMedia.Location = new Point(btnMediaSelect.Location.X, btnMediaSelect.Location.Y - panelMedia.Height);
            this.ResumeLayout();

            linkLblEigenShow.Visible = false;
            FValuePanel.Visible = false;
            FValueLegendPanel.Visible = false;
            foreach (Control workCtrl in ctrlList)
            {
                if (CadPanel != workCtrl)
                {
                    workCtrl.Visible = false;
                }
            }
            CadLgc.SetupRegionSize();
            CadPanel.Invalidate();

            // 自動スクロールの情報を更新するため明示的に再設定しています。
            setAutoScroll(true);
        }

        /// <summary>
        /// フィールド値パネルを最大化する
        /// </summary>
        private void doFValuePanelMaximize(Control[] ctrlList)
        {
            // 自動スクロールを一旦やめる
            setAutoScroll(false);

            this.SuspendLayout();
            // パネル配置変更
            //double r = (this.ClientSize.Width - SystemInformation.VerticalScrollBarWidth) / (double)FValuePanelBaseSize.Width;
            double r = (this.ClientSize.Width - FValueLegendPanelBaseSize.Width - SystemInformation.VerticalScrollBarWidth) / (double)FValuePanelBaseSize.Width; // 凡例分幅を縮める
            FValuePanel.Location = new Point(0, btnNew.Bottom); // ファイル操作ボタンの高さ分ずらす
            FValuePanel.Size = new Size((int)((double)FValuePanelBaseSize.Width * r), (int)((double)FValuePanelBaseSize.Height * r));
            //FValueLegendPanel.Location = FValuePanel.Location + new Size(FValuePanel.Width - FValueLegendPanelBaseSize.Width, 0);
            FValueLegendPanel.Location = FValuePanel.Location + new Size(FValuePanel.Width, 0);
            FValueLegendPanel.Size = FValueLegendPanelBaseSize;
            this.ResumeLayout();

            CadPanel.Visible = false;
            GroupBoxCadMode.Visible = false;
            btnMediaSelect.Visible = false;
            panelMedia.Visible = false;
            linkLblEigenShow.Visible = false;
            foreach (Control workCtrl in ctrlList)
            {
                if (FValuePanel != workCtrl)
                {
                    workCtrl.Visible = false;
                }
            }
            FValuePanel.Invalidate();

            // 自動スクロールの情報を更新するため明示的に再設定しています。
            setAutoScroll(true);
        }

        /// <summary>
        /// コントロールを最大化する
        /// </summary>
        private void doControlMaximize(Control tagtCtrl, Control[] ctrlList, Point[] ctrlBaseLocationList, Size[] ctrlBaseSizeList)
        {
            // 自動スクロールを一旦やめる
            setAutoScroll(false);

            Point tagtBaseLocation = new Point(0, 0);
            Size tagtBaseSize = new Size(0, 0);
            for (int i = 0; i < ctrlList.Length; i++)
            {
                if (tagtCtrl == ctrlList[i])
                {
                    tagtBaseLocation = ctrlBaseLocationList[i];
                    tagtBaseSize = ctrlBaseSizeList[i];
                    break;
                }
            }

            this.SuspendLayout();
            // パネル配置変更
            double r = (this.ClientSize.Width - SystemInformation.VerticalScrollBarWidth) / (double)tagtBaseSize.Width;
            tagtCtrl.Location = new Point(0, btnNew.Bottom); // ファイル操作ボタンの高さ分ずらす
            tagtCtrl.Size = new Size((int)((double)tagtBaseSize.Width * r), (int)((double)tagtBaseSize.Height * r));
            this.ResumeLayout();

            CadPanel.Visible = false;
            GroupBoxCadMode.Visible = false;
            btnMediaSelect.Visible = false;
            panelMedia.Visible = false;
            linkLblEigenShow.Visible = false;
            FValuePanel.Visible = false;
            FValueLegendPanel.Visible = false;
            foreach (Control workCtrl in ctrlList)
            {
                if (tagtCtrl != workCtrl)
                {
                    workCtrl.Visible = false;
                }
            }

            // 自動スクロールの情報を更新するため明示的に再設定しています。
            setAutoScroll(true);
        }

        /// <summary>
        /// フォームが閉じられる前のイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SolverThread != null && SolverThread.IsAlive)
            {
                MessageBox.Show("計算中は終了できません", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // イベントをキャンセルする
                e.Cancel = true;
                return;
            }
            // 変更保存確認ダイアログを表示する
            DialogResult result = confirmSave();
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                // Yes
                // 変更保存した、または変更箇所なし
            }
            else if (result == System.Windows.Forms.DialogResult.No)
            {
                // No
                // 変更を保存しなかった(破棄扱い)
            }
            else if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                // キャンセル
                e.Cancel = true;
                return;
            }
        }

        /// <summary>
        /// 変更保存確認
        /// </summary>
        /// <returns>DialogResult.Yes/No/Cancel 保存する必要のないときはDialogResult.Yes</returns>
        private DialogResult confirmSave()
        {
            DialogResult result = DialogResult.Cancel;
            // 現在編集中の図面があれば上書きする
            if (CadLgc.IsDirty)
            {
                result = MessageBox.Show("Cadデータが変更されています。Cadデータを保存しますか", "アプリケーションを終了します", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    //上書き保存
                    doSave(true);
                }
                else if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    // キャンセル
                }
            }
            else
            {
                // 変更なしの場合は、[Yes]と同じ動作にする
                result = DialogResult.Yes;
            }
            return result;
        }

        /// <summary>
        /// メインフォームが閉じられた後のイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 破棄処理
            CadLgc.Dispose();
            PostPro.Dispose();
        }

        /// <summary>
        /// フォームのリサイズイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Resize(object sender, EventArgs e)
        {
            fitPanelSizeToFrmSize();
        }

        /// <summary>
        /// Cad画面の描画ハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CadPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (CadLgc != null)
            {
                CadLgc.CadPanelPaint(g);
            }
            if (PostPro != null && btnCalc.Text == "計算キャンセル")
            {
                // 計算実行中はメッシュ表示
                PostPro.DrawMesh(g, CadPanel);
            }
        }

        /// <summary>
        /// マウスクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CadPanel_MouseClick(object sender, MouseEventArgs e)
        {
            CadLgc.CadPanelMouseClick(e);
            // 元に戻す、やり直しボタンの操作可能フラグをセットアップ
            setupUndoRedoEnable();
        }


        /// <summary>
        /// マウス押下イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CadPanel_MouseDown(object sender, MouseEventArgs e)
        {
            CadLgc.CadPanelMouseDown(e);
        }

        /// <summary>
        /// マウス移動イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CadPanel_MouseMove(object sender, MouseEventArgs e)
        {
            CadLgc.CadPanelMouseMove(e);
        }

        /// <summary>
        /// マウスアップイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CadPanel_MouseUp(object sender, MouseEventArgs e)
        {
            CadLgc.CadPanelMouseUp(e);
            // 元に戻す、やり直しボタンの操作可能フラグをセットアップ
            setupUndoRedoEnable();
        }

        /// <summary>
        /// フィールド値パネル描画イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FValuePanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (PostPro != null)
            {
                PostPro.DrawField(g, FValuePanel);
            }
        }

        /// <summary>
        /// [計算開始]ボタン押下イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCalc_Click(object sender, EventArgs e)
        {
            if (SolverThread != null && SolverThread.IsAlive)
            {
                if (!Solver.IsCalcAborted)
                {
                    if (MessageBox.Show("計算をキャンセルしますか", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        // 2重チェック
                        if (SolverThread != null && SolverThread.IsAlive)
                        {
                            new Thread(new ThreadStart(delegate()
                                {
                                    Solver.IsCalcAborted = true;
                                    SolverThread.Join();
                                    SolverThread = null;
                                })).Start();
                        }
                    }
                }
                return;
            }

            // 計算範囲ダイアログを表示する
            CalcRangeFrm frm = new CalcRangeFrm();
            DialogResult result = frm.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            // Cadデータ保存＆Fem入力データ作成保存
            doSave(true);
            if (FemInputDatFilePath == "")
            {
                return;
            }

            // [計算開始]ボタンの無効化
            setCtrlEnable(false);
            btnCalc.Text = "計算キャンセル";

            // Cadモードを操作なしにする
            foreach (RadioButton rb in CadModeRadioButtons)
            {
                rb.Checked = false;
            }
            radioBtnNone.Checked = true;
            CadLgc.CadMode = CadLogic.CadModeType.None;

            // 選択中媒質を真空にする
            radioBtnMedia0.Checked = true;
            CadLgc.SelectedMediaIndex = 0;
            // 媒質選択グループボックスへ読み込み値を反映
            setupGroupBoxMedia();
            // 媒質選択ボタンの背景色とテキストを設定
            btnMediaSelect_SetColorAndText();

            // 解析機へ入力データを読み込む
            Solver.Load(FemInputDatFilePath);

            // ポストプロセッサに入力データをコピー
            PostPro.SetInputData(Solver);

            // メッシュ描画
            //using (Graphics g = CadPanel.CreateGraphics())
            //{
            //    PostPro.DrawMesh(g, CadPanel);
            //}
            CadPanel.Invalidate();

            // 周波数インデックス初期化
            FreqNo = -1;
            // チャート初期化
            PostPro.ResetChart(SMatChart);

            // 等高線図の凡例
            PostPro.UpdateFValueLegend(FValueLegendPanel, labelFreqValue);
            // 等高線図
            FValuePanel.Invalidate();

            // 固有値チャート初期化
            PostPro.ResetEigenValueChart(BetaChart);
            // 固有ベクトル表示(空のデータで初期化)
            PostPro.SetEigenVecToChart(EigenVecChart);

            SolverThread = new Thread(new ThreadStart(delegate()
                {
                    // 各波長の結果出力時に呼ばれるコールバックの定義
                    ParameterizedInvokeDelegate eachDoneCallback = new ParameterizedInvokeDelegate(delegate(Object[] args)
                        {
                            /*
                            bool resetFlg = false;
                            if (args != null && args.Length > 0)
                            {
                                resetFlg = Boolean.Parse((string)args[0]);
                            }
                            if (resetFlg)
                            {
                                this.Invoke(new InvokeDelegate(delegate()
                                {
                                    // チャート初期化
                                    PostPro.ResetChart(SMatChart);

                                    // 等高線図の凡例
                                    PostPro.UpdateFValueLegend(FValueLegendPanel, labelFreqValue);
                                    // 等高線図
                                    FValuePanel.Invalidate();
                                }));
                                return;
                            }
                            */

                            // ポストプロセッサへ結果読み込み(freqNo: -1は最後の結果を読み込み)
                            PostPro.LoadOutput(FemOutputDatFilePath, -1);

                            // 結果をグラフィック表示
                            this.Invoke(new InvokeDelegate(delegate()
                                {
                                    // 等高線図の凡例
                                    PostPro.UpdateFValueLegend(FValueLegendPanel, labelFreqValue);
                                    // 等高線図
                                    FValuePanel.Invalidate();
                                }));

                            this.Invoke(new InvokeDelegate(delegate()
                            {
                                // Sマトリックス周波数特性グラフに計算した点を追加
                                PostPro.AddScatterMatrixToChart(SMatChart);
                            }));

                            this.Invoke(new InvokeDelegate(delegate()
                                {
                                    if (PostPro.GetCalculatedFreqCnt(FemOutputDatFilePath) == 1) 
                                    {
                                        // 固有値チャート初期化(モード数が変わっているので再度初期化する)
                                        PostPro.ResetEigenValueChart(BetaChart);
                                    }
                                    // 固有値(伝搬定数)周波数特性グラフに計算した点を追加
                                    PostPro.AddEigenValueToChart(BetaChart);

                                    // 固有ベクトル表示
                                    PostPro.SetEigenVecToChart(EigenVecChart);
                                }));
                        }
                        );
                    // 解析実行
                    Solver.Run(FemOutputDatFilePath, this, eachDoneCallback);

                    // 解析終了したので[計算開始]ボタンを有効化
                    this.Invoke(new InvokeDelegate(delegate()
                        {
                            //[計算開始]ボタンを有効化
                            setCtrlEnable(true);
                            btnCalc.Text = "計算開始";

                            // 周波数インデックスを最後にセット
                            //BUGFIX
                            //周波数番号は1起点なので、件数 = 最後の番号となる
                            FreqNo = PostPro.GetCalculatedFreqCnt(FemOutputDatFilePath);
                            // 周波数ボタンの有効・無効化
                            setupBtnFreqEnable();

                            // Cadパネル再描画
                            CadPanel.Invalidate();

                        }));
                }));
            SolverThread.Name = "solverThread";
            SolverThread.Start();
        }

        /// <summary>
        /// 計算中のボタン非有効化
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        private void setCtrlEnable(bool enabled)
        {
            //btnCalc.Enabled = enabled;
            // ポストプロセッサ系ボタン
            btnPrevFreq.Enabled = enabled;
            btnNextFreq.Enabled = enabled;
            // 編集系ボタン
            btnNew.Enabled = enabled;
            btnOpen.Enabled = enabled;
            btnSave.Enabled = enabled;
            btnSaveAs.Enabled = enabled;
            btnUndo.Enabled = enabled;
            btnRedo.Enabled = enabled;
            if (enabled)
            {
                setupUndoRedoEnable();
            }
            GroupBoxCadMode.Enabled = enabled;
            if (!enabled)
            {
                panelMedia.Visible = false;
            }
            btnMediaSelect.Enabled = enabled;
        }

        /// <summary>
        /// 「描画モード解除」ラジオボタンチェック状態変更イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioBtnNone_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtnNone.Checked)
            {
                CadLgc.CadMode = CadLogic.CadModeType.None;
            }
            else
            {
                CadLgc.CadMode = CadLogic.CadModeType.None;
            }
            //Console.WriteLine("CadMode:{0}", CadLgc.CadMode);
        }
        /// <summary>
        /// 「マス目」ラジオボタンチェック状態変更イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioBtnArea_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtnArea.Checked)
            {
                CadLgc.CadMode = CadLogic.CadModeType.Area;
            }
            else
            {
                CadLgc.CadMode = CadLogic.CadModeType.None;
            }
            //Console.WriteLine("CadMode:{0}", CadLgc.CadMode);
        }
        /// <summary>
        /// 「ポート境界」ラジオボタンチェック状態変更イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioBtnPort_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtnPort.Checked)
            {
                CadLgc.CadMode = CadLogic.CadModeType.Port;
            }
            else
            {
                CadLgc.CadMode = CadLogic.CadModeType.None;
            }
            //Console.WriteLine("CadMode:{0}", CadLgc.CadMode);
        }
        /// <summary>
        /// 「消しゴム」ラジオボタンチェック状態変更イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioBtnErase_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtnErase.Checked)
            {
                CadLgc.CadMode = CadLogic.CadModeType.Erase;
            }
            else
            {
                CadLgc.CadMode = CadLogic.CadModeType.None;
            }
            //Console.WriteLine("CadMode:{0}", CadLgc.CadMode);
        }
        /// <summary>
        /// 「入射ポート選択」ラジオボタンチェック状態変更イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioBtnIncidentPort_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtnIncidentPort.Checked)
            {
                CadLgc.CadMode = CadLogic.CadModeType.IncidentPort;
            }
            else
            {
                CadLgc.CadMode = CadLogic.CadModeType.None;
            }
            //Console.WriteLine("CadMode:{0}", CadLgc.CadMode);

        }
        /// <summary>
        /// 「ポート番号振り」ラジオボタンチェック状態変更イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioBtnPortNumbering_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBtnPortNumbering.Checked)
            {
                CadLgc.CadMode = CadLogic.CadModeType.PortNumbering;
            }
            else
            {
                CadLgc.CadMode = CadLogic.CadModeType.None;
            }
            //Console.WriteLine("CadMode:{0}", CadLgc.CadMode);
        }

        /// <summary>
        /// [前の周波数]ボタン押下イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrevFreq_Click(object sender, EventArgs e)
        {
            if (FreqNo == -1)
            {
                return;
            }
            if (FreqNo <= 1)
            {
                return;
            }
            // 前の周波数
            FreqNo--;
            // 周波数ボタンの有効・無効化
            setupBtnFreqEnable();

            // ポストプロセッサへ結果読み込み
            bool ret = PostPro.LoadOutput(FemOutputDatFilePath, FreqNo);

            // 結果をグラフィック表示
            // 等高線図表示
            PostPro.UpdateFValueLegend(FValueLegendPanel, labelFreqValue);
            FValuePanel.Invalidate();
            // 固有ベクトル表示
            PostPro.SetEigenVecToChart(EigenVecChart);
        }

        /// <summary>
        /// [次の周波数]ボタン押下イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNextFreq_Click(object sender, EventArgs e)
        {
            if (FreqNo == -1)
            {
                return;
            }
            if (FreqNo >= PostPro.GetCalculatedFreqCnt(FemOutputDatFilePath))
            {
                return;
            }
            // 次の周波数
            FreqNo++;
            // 周波数ボタンの有効・無効化
            setupBtnFreqEnable();

            // ポストプロセッサへ結果読み込み
            bool ret = PostPro.LoadOutput(FemOutputDatFilePath, FreqNo);

            // 結果をグラフィック表示
            // 等高線図表示
            PostPro.UpdateFValueLegend(FValueLegendPanel, labelFreqValue);
            FValuePanel.Invalidate();
            // 固有ベクトル表示
            PostPro.SetEigenVecToChart(EigenVecChart);
        }

        /// <summary>
        /// 周波数ボタンの有効・無効化
        /// </summary>
        private void setupBtnFreqEnable()
        {
            btnPrevFreq.Enabled = (FreqNo > 1 && FreqNo <= PostPro.GetCalculatedFreqCnt(FemOutputDatFilePath));
            btnNextFreq.Enabled = (FreqNo >= 1 && FreqNo < PostPro.GetCalculatedFreqCnt(FemOutputDatFilePath));
        }

        /// <summary>
        /// フォーム初期表示イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Shown(object sender, EventArgs e)
        {
            FrmNormalSize = this.Size;
            //Console.WriteLine("FrmNormalSize:{0},{1}", FrmNormalSize.Width, FrmNormalSize.Height);

            // パネルを再配置
            fitPanelSizeToFrmSize();
        }

        /// <summary>
        /// キー押下イベントハンドラ
        ///   フォームのKeyPreviewをtrueにするとすべてのキーイベントをフォームが受け取れます
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.O)
            {
                // ファイルを開く
                doOpen();
                // 子コントロールへイベントを伝搬させないようにする
                e.Handled = true;
            }
            if (e.Control && e.KeyCode == Keys.S)
            {
                // 上書き保存
                doSave(true);
                // 子コントロールへイベントを伝搬させないようにする
                e.Handled = true;
            }
            if (e.Control && e.KeyCode == Keys.Z)
            {
                // 元に戻す
                bool executed = doUndo();
                if (executed)
                {
                    // 子コントロールへイベントを伝搬させないようにする
                    e.Handled = true;
                }
            }
            if (e.Control && e.KeyCode == Keys.Y)
            {
                // やり直し
                bool executed = doRedo();
                if (executed)
                {
                    // 子コントロールへイベントを伝搬させないようにする
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// [新規作成]ボタンクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNew_Click(object sender, EventArgs e)
        {
            // 変更保存確認ダイアログを表示する
            DialogResult result = confirmSave();
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                // Yes
                // 変更保存した、または変更箇所なし
            }
            else if (result == System.Windows.Forms.DialogResult.No)
            {
                // No
                // 変更を保存しなかった(破棄扱い)
            }
            else if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                // キャンセル
                return;
            }

            //////////////////////////////
            // 新規作成処理
            /////////////////////////////
            setupFilenames("");
            setFrmTitle();
            
            // GUIの初期化
            resetGUI();
        }

        /// <summary>
        /// [ファイルを開く]ボタンクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpen_Click(object sender, EventArgs e)
        {
            // 変更保存確認ダイアログを表示する
            DialogResult result = confirmSave();
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                // Yes
                // 変更保存した、または変更箇所なし
            }
            else if (result == System.Windows.Forms.DialogResult.No)
            {
                // No
                // 変更を保存しなかった(破棄扱い)
            }
            else if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                // キャンセル
                return;
            }

            //////////////////////////////
            // ファイルを開く
            //////////////////////////////
            doOpen();
        }

        /// <summary>
        /// ファイルを開く処理
        /// </summary>
        private void doOpen()
        {
            openFileDialog1.InitialDirectory = Application.UserAppDataPath;
            openFileDialog1.FileName = "";
            if (CadDatFilePath.Length > 0)
            {
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(CadDatFilePath);
                openFileDialog1.FileName = Path.GetFileName(CadDatFilePath);
            }
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                // ファイル名の格納
                string path = openFileDialog1.FileName;
                setupFilenames(path);

                // タイトル変更
                setFrmTitle();
                // 読み込み処理
                loadFromFile();
            }
        }

        /// <summary>
        /// ファイルを開くダイアログのファイル名を元にCad, Fem入出力ファイル名を決定する
        /// </summary>
        /// <param name="path"></param>
        private void setupFilenames(string path)
        {
            if (path == "")
            {
                CadDatFilePath = "";
                FemInputDatFilePath = "";
                FemOutputDatFilePath = "";
            }
            else
            {
                string basename = Form1.GetFilePathWithoutExt(path);
                CadDatFilePath = basename + Constants.CadExt;
                FemInputDatFilePath = basename + Constants.FemInputExt;
                FemOutputDatFilePath = basename + Constants.FemOutputExt;
            }
        }

        /// <summary>
        /// GUI初期化
        /// </summary>
        private void resetGUI()
        {
            // 変数初期化
            Variables.Reset();

            // Cadデータの初期化
            CadLgc.InitData();
            // 元に戻す、やり直しボタンの操作可能フラグをセットアップ
            setupUndoRedoEnable();

            // Cadモードを操作なしにする
            foreach (RadioButton rb in CadModeRadioButtons)
            {
                rb.Checked = false;
            }
            radioBtnNone.Checked = true;
            CadLgc.CadMode = CadLogic.CadModeType.None;
            // 選択中媒質を真空にする
            radioBtnMedia0.Checked = true;
            CadLgc.SelectedMediaIndex = 0;
            // 媒質選択グループボックスへ読み込み値を反映
            setupGroupBoxMedia();
            // 媒質選択ボタンの背景色とテキストを設定
            btnMediaSelect_SetColorAndText();
            // 方眼線描画
            CadPanel.Invalidate();

            // 解析機の入力データ初期化
            Solver.InitData();
            // ポストプロセッサの入力データ初期化
            PostPro.InitData();

            // 周波数インデックス初期化
            FreqNo = -1;
            // 周波数ボタンの有効・無効化
            setupBtnFreqEnable();
            // チャート初期化
            PostPro.ResetChart(SMatChart);
            // 等高線図凡例
            PostPro.UpdateFValueLegend(FValueLegendPanel, labelFreqValue);
            // 等高線図
            FValuePanel.Invalidate();
            // 固有値チャート初期化
            // この段階ではMaxModeの値が0なので、後に計算値ロード後一回だけ初期化する
            PostPro.ResetEigenValueChart(BetaChart);
            // 固有ベクトル表示
            PostPro.SetEigenVecToChart(EigenVecChart);
        }

        /// <summary>
        /// ファイル読み込み処理
        /// </summary>
        private void loadFromFile()
        {
            resetGUI();

            // Cadデータの読み込み
            CadLgc.DeserializeCadData(CadDatFilePath);
            // 媒質選択グループボックスへ読み込み値を反映
            setupGroupBoxMedia();
            // 媒質選択ボタンの背景色とテキストを設定
            btnMediaSelect_SetColorAndText();
            // 元に戻す、やり直しボタンの操作可能フラグをセットアップ
            setupUndoRedoEnable();
            // 方眼線描画
            CadPanel.Invalidate();

            if (File.Exists(FemOutputDatFilePath))
            {
                // 解析機へ入力データを読み込む
                Solver.Load(FemInputDatFilePath);
                // ポストプロセッサに入力データをコピー
                PostPro.SetInputData(Solver);

                // 周波数インデックス初期化
                FreqNo = -1;
                // 周波数ボタンの有効・無効化
                setupBtnFreqEnable();
                // チャート初期化(ポート数が変わっているので再度初期化する)
                PostPro.ResetChart(SMatChart);
                //描画の途中経過を表示
                Application.DoEvents();

                // 周波数特性グラフの表示
                int dataCnt = FemOutputDatFile.GetCalculatedFreqCnt(FemOutputDatFilePath);
                for (int freqIndex = 0; freqIndex < dataCnt; freqIndex++)
                {
                    int freqNo = freqIndex + 1;
                    // ポストプロセッサへ結果読み込み
                    bool ret = PostPro.LoadOutput(FemOutputDatFilePath, freqNo);
                    if (!ret)
                    {
                        break;
                    }
                    // Sマトリックス周波数特性グラフに計算した点を追加
                    PostPro.AddScatterMatrixToChart(SMatChart);
                    if (freqNo == 1)
                    {
                        // チャート初期化(モード数が変わっているので再度初期化する)
                        PostPro.ResetEigenValueChart(BetaChart);
                    }
                    // 伝搬定数周波数特性グラフに計算した点を追加
                    PostPro.AddEigenValueToChart(BetaChart);

                    //描画の途中経過を表示
                    Application.DoEvents();
                }

                // 周波数
                FreqNo = 1;
                // 周波数ボタンの有効・無効化
                setupBtnFreqEnable();

                // ポストプロセッサへ結果読み込み
                PostPro.LoadOutput(FemOutputDatFilePath, FreqNo);

                // 等高線図凡例
                PostPro.UpdateFValueLegend(FValueLegendPanel, labelFreqValue);
                // 等高線図
                FValuePanel.Invalidate();
                // 固有ベクトル表示
                PostPro.SetEigenVecToChart(EigenVecChart);
            }
        }

        /// <summary>
        /// 拡張子を抜いたパスを取得する
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFilePathWithoutExt(string path)
        {
            return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(path);
        }

        /// <summary>
        /// [上書き保存]ボタンクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            doSave(true);
        }

        /// <summary>
        /// [名前を付けて保存]ボタンクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            doSave(false);
        }

        /// <summary>
        /// ファイル保存処理
        /// </summary>
        /// <param name="overwriteFlg">上書きフラグ</param>
        private void doSave(bool overwriteFlg)
        {
            if (CadDatFilePath.Length == 0 || !overwriteFlg)
            {
                // 名前を付けて保存
                saveFileDialog1.InitialDirectory = Application.UserAppDataPath;
                saveFileDialog1.FileName = "";
                if (CadDatFilePath.Length > 0)
                {
                    saveFileDialog1.InitialDirectory = Path.GetDirectoryName(CadDatFilePath);
                    saveFileDialog1.FileName = Path.GetFileName(CadDatFilePath);
                }
                DialogResult result = saveFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // ファイル名の格納
                    string path = saveFileDialog1.FileName;
                    setupFilenames(path);

                    // フォームタイトル変更
                    setFrmTitle();
                }
                else
                {
                    return;
                }
            }
            // ファイル書き込み処理
            saveToFile();

        }
        
        /// <summary>
        /// ファイル書き込み処理
        ///   Cadデータファイル作成、Fem入力データファイル作成
        /// </summary>
        private void saveToFile()
        {
            // Fem入出力データの削除
            removeAllFemDatFile();
            // 解析機の入力データ初期化
            Solver.InitData();
            // ポストプロセッサの入力データ初期化
            PostPro.InitData();

            // 周波数インデックス初期化
            FreqNo = -1;
            // 周波数ボタンの有効・無効化
            setupBtnFreqEnable();
            // チャート初期化
            PostPro.ResetChart(SMatChart);
            // 等高線図凡例
            PostPro.UpdateFValueLegend(FValueLegendPanel, labelFreqValue);
            // 等高線図
            FValuePanel.Invalidate();
            // 固有値(伝搬定数)チャート初期化
            PostPro.ResetEigenValueChart(BetaChart);
            // 固有ベクトル表示
            PostPro.SetEigenVecToChart(EigenVecChart);

            // Cadデータの書き込み
            CadLgc.SerializeCadData(CadDatFilePath);
            // FEM入力データの作成
            CadLgc.MkFemInputData(FemInputDatFilePath);
            // 元に戻す、やり直しボタンの操作可能フラグをセットアップ
            setupUndoRedoEnable();
        }

        /// <summary>
        /// Fem入出力データの削除
        /// </summary>
        private void removeAllFemDatFile()
        {
            if (File.Exists(FemInputDatFilePath))
            {
                File.Delete(FemInputDatFilePath);
            }
            string basename = Form1.GetFilePathWithoutExt(FemOutputDatFilePath);
            string outfilename = basename + Constants.FemOutputExt;
            string indexfilename = outfilename + Constants.FemOutputIndexExt;
            System.Diagnostics.Debug.Assert(outfilename == FemOutputDatFilePath);
            if (File.Exists(outfilename))
            {
                File.Delete(outfilename);
            }
            if (File.Exists(indexfilename))
            {
                File.Delete(indexfilename);
            }
        }

        /// <summary>
        /// フォームのタイトル(ウィンドウテキスト)を設定する
        /// </summary>
        private void setFrmTitle()
        {
            string fn = Path.GetFileName(CadDatFilePath);
            if (fn.Length == 0)
            {
                fn = "(無題)";
            }
            this.Text = fn + " - " + TitleBaseName;
        }

        /// <summary>
        /// [元に戻す]ボタンクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUndo_Click(object sender, EventArgs e)
        {
            doUndo();
        }

        /// <summary>
        /// [やり直し]ボタンクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRedo_Click(object sender, EventArgs e)
        {
            doRedo();
        }

        /// <summary>
        /// 元に戻す、やり直しボタンの操作可能フラグを設定する
        /// </summary>
        private void setupUndoRedoEnable()
        {
            btnUndo.Enabled = CadLgc.CanUndo();
            btnRedo.Enabled = CadLgc.CanRedo();
        }

        /// <summary>
        /// Cadパネルダブルクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CadPanel_DoubleClick(object sender, EventArgs e)
        {
            if (CadLgc.CadMode != CadLogic.CadModeType.None)
            {
                MessageBox.Show("実行するには描画モードを解除してください", MaximizedControl == CadPanel? "元のサイズに戻す" : "最大化");
                return;
            }
            flipMaximizedControl(CadPanel);
        }

        /// <summary>
        /// 最大化コントロールをフリップ(セット済みならクリア、未セットならセット)
        /// </summary>
        /// <param name="tagtCtrl"></param>
        private void flipMaximizedControl(Control tagtCtrl)
        {
            if (MaximizedControl != null)
            {
                if (MaximizedControl == tagtCtrl)
                {
                    MaximizedControl = null;
                }
            }
            else
            {
                MaximizedControl = tagtCtrl;
            }
            fitPanelSizeToFrmSize();
        }

        /// <summary>
        /// フィールド値パネルダブルクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FValuePanel_DoubleClick(object sender, EventArgs e)
        {
            flipMaximizedControl(FValuePanel);
        }

        /// <summary>
        /// S行列チャートダブルクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SMatChart_DoubleClick(object sender, EventArgs e)
        {
            flipMaximizedControl(SMatChart);
        }

        /// <summary>
        /// 伝搬定数チャートダブルクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BetaChart_DoubleClick(object sender, EventArgs e)
        {
            flipMaximizedControl(BetaChart);
        }

        /// <summary>
        /// 固有モードチャートダブルクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EigenVecChart_DoubleClick(object sender, EventArgs e)
        {
            flipMaximizedControl(EigenVecChart);
        }


        /// <summary>
        /// [媒質選択]ボタンクリックイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMediaSelect_Click(object sender, EventArgs e)
        {
            panelMedia.Visible = !panelMedia.Visible;
        }

        /// <summary>
        /// 媒質ラジオボタン0(真空)のチェック状態変更イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioBtnMedia0_CheckedChanged(object sender, EventArgs e)
        {
            mediaRadioButtons_CheckedChangedProc(sender);
        }

        /// <summary>
        /// 媒質ラジオボタン1(誘電体1)のチェック状態変更イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioBtnMedia1_CheckedChanged(object sender, EventArgs e)
        {
            mediaRadioButtons_CheckedChangedProc(sender);
        }

        /// <summary>
        /// 媒質ラジオボタン2(誘電体2)のチェック状態変更イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioBtnMedia2_CheckedChanged(object sender, EventArgs e)
        {
            mediaRadioButtons_CheckedChangedProc(sender);
        }

        /// <summary>
        /// 比誘電率テキストボックス0のテキストが変更されたときのイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxEps0_TextChanged(object sender, EventArgs e)
        {
            textBoxEps_TextChangedProc(sender);
        }

        /// <summary>
        /// 比誘電率テキストボックス1のテキストが変更されたときのイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxEps1_TextChanged(object sender, EventArgs e)
        {
            textBoxEps_TextChangedProc(sender);
        }

        /// <summary>
        /// 比誘電率テキストボックス2のテキストが変更されたときのイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxEps2_TextChanged(object sender, EventArgs e)
        {
            textBoxEps_TextChangedProc(sender);
        }

        /// <summary>
        /// 媒質ラジオボタンのチェック状態が変更されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        private void mediaRadioButtons_CheckedChangedProc(object sender)
        {
            RadioButton tgtRadioBtn = sender as RadioButton;

            CadLgc.SelectedMediaIndex = getSelectedMediaRadioBtnIndex();
            btnMediaSelect_SetColorAndText();
            panelMedia.Visible = false;
        }

        /// <summary>
        /// 選択中の媒質ラジオボタンの媒質インデックスを取得する
        /// </summary>
        /// <returns></returns>
        private int getSelectedMediaRadioBtnIndex()
        {
            int selIndex = 0;
            for (int i = 0; i < MediaRadioButtons.Length; i++)
            {
                if (MediaRadioButtons[i].Checked)
                {
                    selIndex = i;
                    break;
                }
            }
            return selIndex;
        }

        /// <summary>
        /// 比誘電率入力テキストボックスが変更された時の処理
        /// </summary>
        /// <param name="sender"></param>
        private void textBoxEps_TextChangedProc(object sender)
        {
            TextBox tgtTextBox = sender as TextBox;
            try
            {
                if (tgtTextBox.Text.Length != 0)
                {
                    double er = double.Parse(tgtTextBox.Text);
                    if (Math.Abs(er) < 1.0e-6)
                    {
                        // 0は不正値として処理する
                        return;
                    }
                    for (int i = 0; i < EpsTextBoxes.Length; i++)
                    {
                        if (EpsTextBoxes[i] == tgtTextBox)
                        {
                            int mediaIndex = i;
                            MediaInfo media = CadLgc.GetMediaInfo(mediaIndex);
                            media.SetQ(new double[,]
                            {
                                {er, 0.0, 0.0},
                                {0.0, er, 0.0},
                                {0.0, 0.0, er},
                            });
                            CadLgc.SetMediaInfo(mediaIndex, media);
                            btnMediaSelect_SetColorAndText();
                            break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
            }
        }

        /// <summary>
        /// 媒質選択グループボックスに読み込んだCadの値をセットする
        /// </summary>
        private void setupGroupBoxMedia()
        {
            for (int i = 0; i < Constants.MaxMediaCount; i++)
            {
                MediaInfo media = CadLgc.GetMediaInfo(i);
                MediaRadioButtons[i].BackColor = media.BackColor;
                double er = media.Q[2, 2];  // qzz
                EpsTextBoxes[i].Text = string.Format("{0:F6}", er);
            }
        }

        /// <summary>
        /// [媒質選択]ボタンの背景色とテキストを設定する
        /// </summary>
        private void btnMediaSelect_SetColorAndText()
        {
            int selIndex = CadLgc.SelectedMediaIndex;
            MediaInfo media = CadLgc.GetMediaInfo(selIndex);
            //RadioButton selRadioBtn = MediaRadioButtons[selIndex]; 
            btnMediaSelect.BackColor = media.BackColor;
            //double er = media.Q[2, 2]; // qzz
            //btnMediaSelect.Text = string.Format("(εr:{0:F2})", er);
        }

        /// <summary>
        /// 「元に戻す」処理
        /// </summary>
        /// <returns></returns>
        private bool doUndo()
        {
            bool executed = false;
            // 媒質パネルが表示されているときは、テキストボックスのショートカットキーと重複するので処理しない
            if (CadLgc.CanUndo() && (MaximizedControl == null || MaximizedControl == CadPanel) && !panelMedia.Visible && btnCalc.Text != "計算キャンセル")
            {
                // 元に戻す
                CadLgc.Undo();
                //CadPanel.Invalidate(); // CadLogic内で処理される
                executed = true;
            }
            if (executed)
            {
                setupUndoRedoEnable();
            }
            return executed;
        }

        /// <summary>
        /// 「やり直し」処理
        /// </summary>
        /// <returns></returns>
        private bool doRedo()
        {
            bool executed = false;
            // 媒質パネルが表示されているときは、テキストボックスのショートカットキーと重複するので処理しない
            if (CadLgc.CanRedo() && (MaximizedControl == null || MaximizedControl == CadPanel) && !panelMedia.Visible && btnCalc.Text != "計算キャンセル")
            {
                // やり直し
                CadLgc.Redo();
                //CadPanel.Invalidate(); // CadLogic内で処理される
                // 子コントロールへイベントを伝搬させないようにする
                executed = true;
            }
            if (executed)
            {
                setupUndoRedoEnable();
            }
            return executed;
        }

        /// <summary>
        /// [固有モード表示]リンクラベルリッククリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLblEigenShow_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MaximizedControl != null)
            {
                // パネル最大化中は表示切替しない
                return;
            }

            // 固有モード表示フラグを切り替える
            EigenShowFlg = !EigenShowFlg;

            // リンクのテキスト変更
            linkLblEigenShow.Text = EigenShowFlg ? "隠す" : "固有モードを見る";

            setupFrmSizeForEigenShow();
        }

        /// <summary>
        /// 固有モードの表示/非表示にあったフォームのサイズを設定する
        /// </summary>
        private void setupFrmSizeForEigenShow()
        {
            if (MaximizedControl != null)
            {
                // パネル最大化中は配置変更しなくてよい
                return;
            }
            int dY = 0;
            if (EigenShowFlg)
            {
                // ウィンドウを広げる
                // ウィンドウの高さを伝搬定数チャートの右下Y座標の高さにする
                dY = BetaChart.Bottom - this.ClientSize.Height;
                // 微調整
                if (PrevWindowState == FormWindowState.Maximized && this.WindowState == FormWindowState.Normal)
                {
                    // 最大化→最小化のときタイトルバーの高さ分、フォームの高さが設定したい高さより大きい（目視で確認）
                    //dY -= SystemInformation.CaptionHeight;
                    dY -= SystemInformation.CaptionButtonSize.Height - 5; // 少し大きすぎるので調整
                }
            }
            else
            {
                // ウィンドウを折りたたむ
                dY = BetaChart.Top - this.ClientSize.Height;
                // 微調整
                if (PrevWindowState == FormWindowState.Maximized && this.WindowState == FormWindowState.Normal)
                {
                    // 最大化→最小化のときタイトルバーの高さ分、フォームの高さが設定したい高さより大きい（目視で確認）
                    //dY -= SystemInformation.CaptionHeight;
                    dY -= SystemInformation.CaptionButtonSize.Height - 5; // 少し大きすぎるので調整
                }
            }
            //this.Size += new Size(0, dY);
            Size sizeToSet = this.Size + new Size(0, dY);
            //Console.WriteLine("■same? Size:{0},{1}", this.Size.Width, this.Size.Height);
            //Console.WriteLine("■add Size:{0},{1}", 0, dY);
            //Console.WriteLine("■sizeToSet:{0},{1}", sizeToSet.Width, sizeToSet.Height);
            this.Size = sizeToSet;  // ここでセットされない or セットした後書き換わっている
            //Console.WriteLine("■Set! ret Size:{0},{1}", this.Size.Width, this.Size.Height);
            if (!this.Size.Equals(sizeToSet))
            {
                //Console.WriteLine("■Set! replaced? ret Size:{0},{1}", this.Size.Width, this.Size.Height);
                if (this.WindowState == FormWindowState.Normal)
                {
                    // 追記: Form1_SizeChangedの処理を遅延実行することで、ここに来ることはなくなった
                    // Note: 最大化状態では、常にセットに失敗する(retryMaxまで到達)
                    //       最大化状態→元のサイズに戻すの過程では、何度か実行するとセットに成功する(こちらが必要な処理なので、上記ウィンドウステートの条件を追加した)
                    int retryCnt = 0;
                    int retryMax = 5;
                    while (!this.Size.Equals(sizeToSet))
                    {
                        retryCnt++;
                        this.Size = sizeToSet;  // 再度セットする
                        Console.WriteLine("■Set! ret Size:{0},{1}  retry = {2}", this.Size.Width, this.Size.Height, retryCnt);
                        if (retryCnt >= retryMax)
                        {
                            break;
                        }
                    }
                }
            }
            fitPanelSizeToFrmSize();
        }

        /// <summary>
        /// フォームのサイズが変更された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            FormWindowState windowState = this.WindowState;

            //Console.WriteLine("■■■Size:{0},{1}", this.Size.Width, this.Size.Height);
            if (windowState == FormWindowState.Normal)
            {
                if (PrevWindowState == FormWindowState.Maximized)
                {
                    // 最大化状態→元のサイズに戻すの場合

                    // スレッド化により、スレッドが終了するまでPrevWindowStateとFrmNormalSizeが更新されないようにイベントハンドラを削除する
                    this.SizeChanged -= Form1_SizeChanged;
                    this.Visible = false;
                    // スレッド化して処理を遅延実行する
                    int delayMsec = 0;
                    new Thread(new ThreadStart(delegate()
                        {
                            Thread.Sleep(delayMsec);
                            this.Invoke(new InvokeDelegate(delegate()
                                {
                                    // 最大化前に記録したフォームのサイズを復元する
                                    this.Size = FrmNormalSize;
                                    //Console.WriteLine("■■Set! ret Size:{0},{1}", this.Size.Width, this.Size.Height);
                                    // パネル再配置
                                    fitPanelSizeToFrmSize();
                                    //Console.WriteLine("■■same? ret Size:{0},{1}", this.Size.Width, this.Size.Height);
                                    if (MaximizedControl == null)
                                    {
                                        // 固有モードの表示/非表示にあったフォームのサイズを設定する
                                        setupFrmSizeForEigenShow();
                                        //Console.WriteLine("■■changed!!!! ret Size:{0},{1}", this.Size.Width, this.Size.Height);
                                    }
                                    // サイズ変更イベントハンドラを元に戻す
                                    this.SizeChanged += Form1_SizeChanged;
                                    this.Visible = true;
                                }), null);
                        })).Start();

                }
                else
                {
                    // 通常のサイズの場合

                    // サイズを記録する
                    FrmNormalSize = this.Size;
                    //Console.WriteLine("■■FrmNormalSize:{0},{1}", FrmNormalSize.Width, FrmNormalSize.Height);
                }
            }
            if (PrevWindowState != windowState)
            {
                PrevWindowState = windowState;
            }
        }


    }
}
