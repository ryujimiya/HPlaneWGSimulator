using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Numerics; // Complex
using System.Drawing;

namespace HPlaneWGSimulator
{
    /// <summary>
    /// Femポストプロセッサロジック
    /// </summary>
    class FemPostProLogic : IDisposable
    {
        /////////////////////////////////////////////////////////////////
        // 定数
        /////////////////////////////////////////////////////////////////
        /// <summary>
        /// 凡例の色レベル数
        /// </summary>
        private const int LegendColorCnt = 10;
        /// <summary>
        /// 表示するモードの数
        /// </summary>
        private const int ShowMaxMode = 1;

        /////////////////////////////////////////////////////////////////
        // 入力データ
        /////////////////////////////////////////////////////////////////
        /// <summary>
        /// 節点リスト
        /// </summary>
        private FemNode[] Nodes;
        /// <summary>
        /// 要素リスト
        /// </summary>
        private FemElement[] Elements;
        private IList<int[]> Ports;
        private int[] ForceNodes;
        private int IncidentPortNo = 1;

        /////////////////////////////////////////////////////////////////
        // 出力データ
        /////////////////////////////////////////////////////////////////
        /// <summary>
        /// 波長
        /// </summary>
        private double WaveLength = 0.0;
        /// <summary>
        /// 考慮モード数
        /// </summary>
        private int MaxMode = 0;
        /// <summary>
        /// ポートの節点リスト
        /// </summary>
        private IList<int[]> NodesBoundaryList = new List<int[]>();
        /// <summary>
        /// ポートの固有値リスト
        /// </summary>
        private IList<Complex[]> EigenValuesList = new List<Complex[]>();
        /// <summary>
        /// ポートの固有ベクトルリスト
        /// </summary>
        private IList<Complex[,]> EigenVecsList = new List<Complex[,]>();
        /// <summary>
        /// 領域内節点リスト
        /// </summary>
        private int[] NodesRegion;
        /// <summary>
        /// 領域内節点の節点番号→インデックスマップ
        /// </summary>
        private Dictionary<int, int> NodesRegionToIndex = new Dictionary<int,int>();
        /// <summary>
        /// フィールド値リスト
        /// </summary>
        private Complex[] ValuesAll = null;
        /// <summary>
        /// 散乱行列
        /// </summary>
        private Complex[] ScatterMat = null;
        /// <summary>
        /// 導波管幅
        /// </summary>
        private double WaveguideWidth = 0.0;
        /// <summary>
        /// フィールド値カラーパレット
        /// </summary>
        private ColorMap FValueColorMap = new ColorMap();
        /// <summary>
        /// フィールド値凡例の色パネル
        /// </summary>
        Panel FValueLegendColorPanel = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FemPostProLogic()
        {
            initInput();
            initOutput();
        }

        /// <summary>
        /// 初期化処理(入力)
        /// </summary>
        private void initInput()
        {
            Nodes = null;
            Elements = null;
            Ports = null;
            ForceNodes = null;
            WaveguideWidth = 0.0;
            IncidentPortNo = 1;
        }

        /// <summary>
        /// 初期化処理(出力)
        /// </summary>
        private void initOutput()
        {
            WaveLength = 0;
            MaxMode = 0;
            NodesBoundaryList.Clear();
            EigenValuesList.Clear();
            EigenVecsList.Clear();
            NodesRegion = null;
            NodesRegionToIndex.Clear();
            ValuesAll = null;
            ScatterMat = null;
        }

        /// <summary>
        /// 入出力データの初期化
        /// </summary>
        public void InitData()
        {
            initInput();
            initOutput();
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~FemPostProLogic()
        {
            Dispose(false);
        }

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        /// <summary>
        /// リソース破棄
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 入力データのセット
        /// </summary>
        /// <param name="solver"></param>
        public void SetInputData(FemSolver solver)
        {
            // 入力データの初期化
            initInput();
            // 入力データの取得
            solver.GetFemInputInfo(out Nodes, out Elements, out Ports, out ForceNodes, out IncidentPortNo, out WaveguideWidth);
            if (Nodes == null || Elements == null || Ports == null || ForceNodes == null || Math.Abs(WaveguideWidth) < 1.0e-12 )
            {
                MessageBox.Show("入力データを取得できません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 各要素に節点情報を補完する
            foreach(FemElement element in Elements)
            {
                element.SetNodesFromAllNodes(Nodes);
            }
            // ポート1の導波管幅
            /*導波管幅は、FemSolverで算出済みの値を取得する
            int port1NodeNumber1 = Ports[0][0];
            int port1NodeNumber2 = Ports[0][Ports[0].Length - 1];
            WaveguideWidth = getDistance(Nodes[port1NodeNumber1 - 1].Coord, Nodes[port1NodeNumber2 - 1].Coord);
            */

            initOutput();
        }

        /// <summary>
        /// 2点間距離の計算
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p0"></param>
        /// <returns></returns>
        private double getDistance(double[] p, double[] p0)
        {
            return Math.Sqrt((p[0] - p0[0]) * (p[0] - p0[0]) + (p[1] - p0[1]) * (p[1] - p0[1]));
        }

        /// <summary>
        /// 出力ファイルから計算済み周波数の数を取得する
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public int GetCalculatedFreqCnt(string filename, out int firstFreqNo, out int lastFreqNo)
        {
            return FemOutputDatFile.GetCalculatedFreqCnt(filename, out firstFreqNo, out lastFreqNo);
        }

        /// <summary>
        /// 出力結果ファイル読み込み
        /// </summary>
        /// <param name="filename"></param>
        public bool LoadOutput(string filename, int freqNo)
        {

            initOutput();
            if (!isInputDataReady())
            {
                //MessageBox.Show("入力データがセットされていません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            int incidentPortNo = 0;
            bool ret = FemOutputDatFile.LoadFromFile(
                filename, freqNo,
                out WaveLength, out MaxMode, out incidentPortNo,
                out NodesBoundaryList, out EigenValuesList, out EigenVecsList,
                out NodesRegion, out ValuesAll,
                out ScatterMat);

            if (ret)
            {
                //System.Diagnostics.Debug.Assert(maxMode == MaxMode);
                System.Diagnostics.Debug.Assert(incidentPortNo == IncidentPortNo);

                // 追加処理
                // 節点番号→インデックスのマップ作成
                for (int ino = 0; ino < NodesRegion.Length; ino++)
                {
                    int nodeNumber = NodesRegion[ino];
                    if (!NodesRegionToIndex.ContainsKey(nodeNumber))
                    {
                        NodesRegionToIndex.Add(nodeNumber, ino);
                    }
                }
                // 要素リストにフィールド値を格納
                foreach (FemElement element in Elements)
                {
                    element.SetFieldValueFromAllValues(ValuesAll, NodesRegionToIndex);
                }
            }
            return ret;
        }

        /// <summary>
        /// 入力データ準備済み？
        /// </summary>
        /// <returns></returns>
        private bool isInputDataReady()
        {
            bool isReady = false;

            if (Nodes == null)
            {
                return isReady;
            }
            if (Elements == null)
            {
                return isReady;
            }
            if (Ports == null)
            {
                return isReady;
            }
            if (ForceNodes == null)
            {
                return isReady;
            }
            if (Math.Abs(WaveguideWidth) < 1.0e-12)
            {
                return isReady;
            }

            isReady = true;
            return isReady;
        }
        /// <summary>
        /// 出力データ準備済み？
        /// </summary>
        /// <returns></returns>
        private bool isOutputDataReady()
        {
            bool isReady = false;
            if (WaveLength == 0)
            {
                return isReady;
            }
            if (MaxMode == 0)
            {
                return isReady;
            }
            if (NodesBoundaryList.Count == 0)
            {
                return isReady;
            }
            if (EigenValuesList.Count == 0)
            {
                return isReady;
            }
            if (EigenVecsList.Count == 0)
            {
                return isReady;
            }
            if (NodesRegion == null)
            {
                return isReady;
            }
            if (NodesRegionToIndex.Count == 0)
            {
                return isReady;
            }
            if (ValuesAll == null)
            {
                return isReady;
            }
            if (ScatterMat == null)
            {
                return isReady;
            }

            isReady = true;
            return isReady;
        }

        /// <summary>
        /// メッシュ描画
        /// </summary>
        /// <param name="g"></param>
        /// <param name="panel"></param>
        public void DrawMesh(Graphics g, Panel panel, bool fitFlg = false)
        {
            if (!isInputDataReady())
            {
                return;
            }
            Size ofs;
            Size delta;
            Size regionSize;
            if (!fitFlg)
            {
                getDrawRegion(panel, out delta, out ofs, out regionSize);
            }
            else
            {
                getFitDrawRegion(panel, out delta, out ofs, out regionSize);
            }

            foreach (FemElement element in Elements)
            {
                element.Draw(g, ofs, delta, regionSize);
            }
        }

        /// <summary>
        /// 描画領域を取得
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="delta"></param>
        /// <param name="ofs"></param>
        /// <param name="regionSize"></param>
        private void getDrawRegion(Panel panel, out Size delta, out Size ofs, out Size regionSize)
        {
            // 描画領域の方眼桝目の寸法を決定
            double deltaxx = panel.Width / (double)(Constants.MaxDiv.Width + 2);
            int deltax = (int)deltaxx;
            double deltayy = panel.Height / (double)(Constants.MaxDiv.Height + 2);
            int deltay = (int)deltayy;
            ofs = new Size(deltax, deltay);
            delta = new Size(deltax, deltay);
           regionSize = new Size(delta.Width * Constants.MaxDiv.Width, delta.Height * Constants.MaxDiv.Height);
        }

        /// <summary>
        /// パネルに合わせて領域を拡縮する
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="delta"></param>
        /// <param name="ofs"></param>
        /// <param name="regionSize"></param>
        private void getFitDrawRegion(Panel panel, out Size delta, out Size ofs, out Size regionSize)
        {
            const int ndim = 2;

            // 節点座標の最小、最大
            double[] minPt = new double[] { double.MaxValue, double.MaxValue };
            double[] maxPt = new double[] { double.MinValue, double.MinValue };
            foreach (FemNode node in Nodes)
            {
                for (int i = 0; i < ndim; i++)
                {
                    if (node.Coord[i] > maxPt[i])
                    {
                        maxPt[i] = node.Coord[i];
                    }
                    if (node.Coord[i] < minPt[i])
                    {
                        minPt[i] = node.Coord[i];
                    }
                }
            }
            double[] midPt = new double[] { (minPt[0] + maxPt[0]) * 0.5, (minPt[1] + maxPt[1]) * 0.5 };

            // 描画領域の方眼桝目の寸法を決定
            // 図形をパネルのサイズにあわせて拡縮する
            int w = (int)(maxPt[0] - minPt[0]);
            int h = (int)(maxPt[1] - minPt[1]);
            int boxWidth = w > h ? w : h;
            System.Diagnostics.Debug.Assert(boxWidth > 0);
            double marginxx = panel.Width / (double)(Constants.MaxDiv.Width + 2);
            int marginx = (int)marginxx;
            double marginyy = panel.Height / (double)(Constants.MaxDiv.Height + 2);
            int marginy = (int)marginyy;
            double deltaxx = (panel.Width - marginx * 2) / (double)boxWidth;
            int deltax = (int)deltaxx;
            double deltayy = (panel.Height - marginy * 2) / (double)boxWidth;
            int deltay = (int)deltayy;
            // 図形の左下がパネルの左下にくるようにする
            int ofsx = marginx - (int)(deltaxx * (minPt[0] - 0));
            int ofsy = marginy - (int)(deltayy * ((Constants.MaxDiv.Height - minPt[1]) - Constants.MaxDiv.Height));
            // 図形の中央がパネルの中央に来るようにする
            ofsx += (int)(deltaxx * (boxWidth - w) * 0.5);
            ofsy -= (int)(deltayy * (boxWidth - h) * 0.5);
            
            delta = new Size(deltax, deltay);
            ofs = new Size(ofsx, ofsy);
            regionSize = new Size(delta.Width * boxWidth, delta.Height * boxWidth);
            //Console.WriteLine("{0},{1}", ofs.Width, ofs.Height);
            //Console.WriteLine("{0},{1}", regionSize.Width, regionSize.Height);
        }

        /// <summary>
        /// フィールド値等高線図描画
        /// </summary>
        /// <param name="g"></param>
        /// <param name="panel"></param>
        public void DrawField(Graphics g, Panel panel)
        {
            if (!isInputDataReady())
            {
                return;
            }
            if (!isOutputDataReady())
            {
                return;
            }
            Size delta;
            Size ofs;
            Size regionSize;
            getFitDrawRegion(panel, out delta, out ofs, out regionSize);

            // フィールド値の絶対値の最小、最大
            double minfValue = double.MaxValue;
            double maxfValue = double.MinValue;
            foreach (Complex fValue in ValuesAll)
            {
                double v = Complex.Abs(fValue);
                if (v > maxfValue)
                {
                    maxfValue = v;
                }
                if (v < minfValue)
                {
                    minfValue = v;
                }
            }
            // カラーマップに最小、最大を設定
            //FValueColorMap.Min = minValue;
            FValueColorMap.Min = 0.0;
            FValueColorMap.Max = maxfValue;

            // 等高線描画
            foreach (FemElement element in Elements)
            {
                element.DrawField(g, ofs, delta, regionSize, FValueColorMap);
            }
        }

        /// <summary>
        /// フィールド値凡例の初期化
        /// </summary>
        /// <param name="legendPanel"></param>
        public void InitFValueLegend(Panel legendPanel, Label labelFreqValue)
        {
            if (FValueLegendColorPanel != null)
            {
                // 初期化済み
                return;
            }
            const int cnt = LegendColorCnt;

            FValueLegendColorPanel = new Panel();
            FValueLegendColorPanel.Location = new Point(0, 15);
            FValueLegendColorPanel.Size = new Size(50, 5 + 20 * cnt + 5);
            FValueLegendColorPanel.Paint += new PaintEventHandler(FValueLegendColorPanel_Paint);
            legendPanel.Controls.Add(FValueLegendColorPanel);

            labelFreqValue.Text = "---";
        }

        /// <summary>
        /// フィールド値凡例内カラーマップパネルのペイントイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FValueLegendColorPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // カラーマップを表示する
            drawFValueLegendColormap(g);
            // カラーマップの目盛を表示する
            drawFValueLegendColorScale(g);
        }

        /// <summary>
        /// 凡例カラーマップの描画
        /// </summary>
        /// <param name="g"></param>
        private void drawFValueLegendColormap(Graphics g)
        {
            const int cnt = LegendColorCnt;
            const int ofsX = 0;
            const int ofsY = 5;
            const int width = 20;         // カラーマップ領域の幅
            const int height = 20 * cnt;  // カラーマップ領域の高さ

            FValueColorMap.Min = 0;
            FValueColorMap.Max = 1.0;
            for (int y = 0; y < height * cnt; y++)
            {
                double value = (height - y) / (double)height;
                Color backColor = FValueColorMap.GetColor(value);
                using (Brush brush = new SolidBrush(backColor))
                {
                    g.FillRectangle(brush, new Rectangle(ofsX, y + ofsY, width, 1));
                }
            }
        }

        /// <summary>
        /// 凡例カラーマップ目盛の描画
        /// </summary>
        /// <param name="g"></param>
        private void drawFValueLegendColorScale(Graphics g)
        {
            const int cnt = LegendColorCnt;
            const int ofsX = 20;
            const int ofsY = 0;
            const int height = 20; // 1目盛の高さ
            double divValue = 1.0 / (double)cnt;

            using (Font font = new Font("MS UI Gothic", 9))
            using (Brush brush = new SolidBrush(FValueLegendColorPanel.ForeColor))
            {
                for (int i = 0; i < cnt + 1; i++)
                {
                    int y = i * height;
                    string text = string.Format("{0}", (cnt - i) * divValue);
                    g.DrawString(text, font, brush, new Point(ofsX, y + ofsY));
                }
            }
        }

        /// <summary>
        /// フィールド値凡例の更新
        /// </summary>
        /// <param name="legendPanel"></param>
        /// <param name="labelFreqValue"></param>
        public void UpdateFValueLegend(Panel legendPanel, Label labelFreqValue)
        {
            if (Math.Abs(WaveLength) < 1.0e-15)
            {
                labelFreqValue.Text = "---";
            }
            else
            {
                labelFreqValue.Text = string.Format("{0:F2}", GetNormalizedFrequency());
            }
        }

        /// <summary>
        /// チャートの色をセットアップ
        /// </summary>
        /// <param name="chart1"></param>
        private void setupChartColor(Chart chart1)
        {
            Color foreColor = chart1.Parent.ForeColor;
            Color backColor = chart1.Parent.BackColor;
            Color lineColor = Color.DarkGray;
            chart1.BackColor = backColor;
            //chart1.ForeColor = foreColor; // 無視される
            chart1.ChartAreas[0].BackColor = backColor;
            chart1.Titles[0].ForeColor = foreColor;
            foreach (Axis axis in chart1.ChartAreas[0].Axes)
            {
                axis.TitleForeColor = foreColor;
                axis.LabelStyle.ForeColor = foreColor;
                axis.LineColor = lineColor;
                axis.MajorGrid.LineColor = lineColor;
                axis.MajorTickMark.LineColor = lineColor;
                //axis.ScaleBreakStyle.LineColor = lineColor;
                //axis.MinorGrid.LineColor = lineColor;
            }
            chart1.Legends[0].BackColor = backColor;
            chart1.Legends[0].ForeColor = foreColor;
        }

        /// <summary>
        /// 反射、透過係数周波数特性グラフの初期化
        /// </summary>
        /// <param name="chart1"></param>
        public void ResetChart(Chart chart1)
        {
            // チャート初期化
            setupChartColor(chart1);
            chart1.Titles[0].Text = "散乱係数周波数特性";
            chart1.ChartAreas[0].Axes[0].Title = "2W/λ";
            chart1.ChartAreas[0].Axes[1].Title = string.Format("|Si{0}|", IncidentPortNo);
            //chart1.ChartAreas[0].Axes[0].Minimum = 1.0;
            //chart1.ChartAreas[0].Axes[0].Maximum = 2.0;
            chart1.ChartAreas[0].Axes[0].Minimum = Constants.DefNormalizedFreqRange[0];
            chart1.ChartAreas[0].Axes[0].Maximum = Constants.DefNormalizedFreqRange[1];
            chart1.ChartAreas[0].Axes[0].Interval = 0.2;
            chart1.ChartAreas[0].Axes[1].Minimum = 0.0;
            chart1.ChartAreas[0].Axes[1].Maximum = 1.0;
            chart1.ChartAreas[0].Axes[1].Interval = 0.2;
            chart1.Series.Clear();
            if (!isInputDataReady())
            {
                //MessageBox.Show("入力データがセットされていません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            for (int portIndex = 0; portIndex < Ports.Count; portIndex++)
            {
                Series series = new Series();
                series.Name = string.Format("|S{0}{1}|", portIndex + 1, IncidentPortNo);
                series.ChartType = SeriesChartType.Line;
                chart1.Series.Add(series);
            }
            // 計算されたグラフのプロパティ値をAutoに設定
            chart1.ResetAutoValues();
        }

        /// <summary>
        /// 反射、透過係数周波数特性グラフに計算結果を追加
        /// </summary>
        /// <param name="chart1"></param>
        public void AddScatterMatrixToChart(Chart chart1)
        {
            if (!isInputDataReady())
            {
                //MessageBox.Show("入力データがセットされていません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!isOutputDataReady())
            {
                //MessageBox.Show("出力データがセットされていません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            for (int portIndex = 0; portIndex < Ports.Count; portIndex++)
            {
                Series series = chart1.Series[portIndex];

                Complex si1 = ScatterMat[portIndex];
                //DataPoint point = series.Points.Add(Complex.Abs(si1));
                //point.AxisLabel = string.Format("{0:G4}", 2.0 * WaveguideWidth/WaveLength);
                //series.Points.AddXY(2.0 * WaveguideWidth / WaveLength, Complex.Abs(si1));
                series.Points.AddXY(GetNormalizedFrequency(), Complex.Abs(si1));
            }
        }

        /// <summary>
        /// 伝搬定数分散特性(グラフの初期化
        /// </summary>
        /// <param name="chart1"></param>
        public void ResetEigenValueChart(Chart chart1)
        {
            // 表示モード数
            int showMaxMode = ShowMaxMode;
            /*
            if (MaxMode > 0)
            {
                showMaxMode = MaxMode;
            }
             */
            // チャート初期化
            setupChartColor(chart1);
            chart1.Titles[0].Text = "規格化伝搬定数周波数特性";
            chart1.ChartAreas[0].Axes[0].Title = "2W/λ";
            chart1.ChartAreas[0].Axes[1].Title = "β/ k0";
            //chart1.ChartAreas[0].Axes[0].Minimum = 1.0;
            //chart1.ChartAreas[0].Axes[0].Maximum = 2.0;
            chart1.ChartAreas[0].Axes[0].Minimum = Constants.DefNormalizedFreqRange[0];
            chart1.ChartAreas[0].Axes[0].Maximum = Constants.DefNormalizedFreqRange[1];
            chart1.ChartAreas[0].Axes[0].Interval = 0.2;
            chart1.ChartAreas[0].Axes[1].Minimum = 0.0;
            //chart1.ChartAreas[0].Axes[1].Maximum = 1.0; // 誘電体比誘電率の最大となるので可変
            chart1.ChartAreas[0].Axes[1].Interval = 0.2;
            chart1.Series.Clear();
            if (!isInputDataReady())
            {
                //MessageBox.Show("入力データがセットされていません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            for (int portIndex = 0; portIndex < Ports.Count; portIndex++)
            {
                for (int modeIndex = 0; modeIndex < showMaxMode; modeIndex++)
                {
                    Series series = new Series();
                    //series.Name = string.Format("mode{0} at {1}", modeIndex + 1, portIndex + 1);
                    series.Name = string.Format("TE{0}0 at {1}", modeIndex + 1, portIndex + 1);
                    series.ChartType = SeriesChartType.Line;
                    chart1.Series.Add(series);
                }
            }
            // 計算されたグラフのプロパティ値をAutoに設定
            chart1.ResetAutoValues();
        }

        /// <summary>
        /// 伝搬定数分散特性グラフに計算結果を追加
        /// </summary>
        /// <param name="chart1"></param>
        public void AddEigenValueToChart(Chart chart1)
        {
            // 波数
            double k0 = 2.0 * Constants.pi / WaveLength;
            // 角周波数
            double omega = k0 * Constants.c0;
            // 表示モード数
            int showMaxMode = ShowMaxMode;
            /*
            if (MaxMode > 0)
            {
                showMaxMode = MaxMode;
            }
             */

            if (!isInputDataReady())
            {
                //MessageBox.Show("入力データがセットされていません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!isOutputDataReady())
            {
                //MessageBox.Show("出力データがセットされていません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            for (int portIndex = 0; portIndex < Ports.Count; portIndex++)
            {
                for (int modeIndex = 0; modeIndex < showMaxMode; modeIndex++)
                {
                    Series series = chart1.Series[portIndex * showMaxMode + modeIndex];

                    Complex normbeta = EigenValuesList[portIndex][modeIndex] / k0;
                    series.Points.AddXY(GetNormalizedFrequency(), normbeta.Real); // 実数部
                }
            }
        }

        public void SetEigenVecToChart(Chart chart1)
        {
            // 波数
            double k0 = 2.0 * Constants.pi / WaveLength;
            // 角周波数
            double omega = k0 * Constants.c0;
            // 表示モード数
            int showMaxMode = ShowMaxMode;
            /*
            if (MaxMode > 0)
            {
                showMaxMode = MaxMode;
            }
             */
            // チャート初期化
            setupChartColor(chart1);
            chart1.Titles[0].Text = "固有モードEz実部、虚部の分布 (2W/λ = " + (isInputDataReady()? string.Format("{0:F2}", GetNormalizedFrequency()) : "---") + ")";
            chart1.ChartAreas[0].Axes[0].Title = "x / W";
            chart1.ChartAreas[0].Axes[1].Title = "Ez";
            chart1.ChartAreas[0].Axes[0].Minimum = 0.0;
            chart1.ChartAreas[0].Axes[0].Maximum = 1.0;
            chart1.ChartAreas[0].Axes[0].Interval = 0.2;
            chart1.ChartAreas[0].Axes[1].Minimum = -1.0;
            chart1.ChartAreas[0].Axes[1].Maximum = 1.0;
            chart1.ChartAreas[0].Axes[1].Interval = 0.2;
            chart1.Series.Clear();
            if (!isInputDataReady())
            {
                //MessageBox.Show("入力データがセットされていません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            for (int portIndex = 0; portIndex < Ports.Count; portIndex++)
            {
                for (int modeIndex = 0; modeIndex < showMaxMode; modeIndex++)
                {
                    string modeDescr = "---モード";
                    if (isOutputDataReady())
                    {
                        Complex beta = EigenValuesList[portIndex][modeIndex];
                        if (Math.Abs(beta.Imaginary / k0) >= 1.0e-12)
                        {
                            modeDescr = "減衰モード";
                        }
                        else if (Math.Abs(beta.Real / k0) >= 1.0e-12)
                        {
                            modeDescr = "伝搬モード";
                        }
                    }

                    Series series;
                    series = new Series();
                    series.Name = string.Format("TE{0}0 実部 at {1}" + Environment.NewLine + modeDescr, modeIndex + 1, portIndex + 1);
                    series.ChartType = SeriesChartType.Line;
                    //series.MarkerStyle = MarkerStyle.Square;
                    series.BorderDashStyle = ChartDashStyle.Solid;
                    chart1.Series.Add(series);
                    series = new Series();
                    series.Name = string.Format("TE{0}0 虚部 at {1}" + Environment.NewLine + modeDescr, modeIndex + 1, portIndex + 1);
                    series.ChartType = SeriesChartType.Line;
                    //series.MarkerStyle = MarkerStyle.Cross;
                    series.BorderDashStyle = ChartDashStyle.Dash;
                    chart1.Series.Add(series);
                }
            }
            // 計算されたグラフのプロパティ値をAutoに設定
            chart1.ResetAutoValues();

            if (!isInputDataReady())
            {
                //MessageBox.Show("入力データがセットされていません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!isOutputDataReady())
            {
                //MessageBox.Show("出力データがセットされていません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            for (int portIndex = 0; portIndex < Ports.Count; portIndex++)
            {
                Complex[,] eigenVecs = EigenVecsList[portIndex];
                int nodeCnt = eigenVecs.GetLength(1);
                for (int modeIndex = 0; modeIndex < showMaxMode; modeIndex++)
                {
                    Complex beta = EigenValuesList[portIndex][modeIndex];
                    /*
                    if (Math.Abs(beta.Imaginary/k0) >= 1.0e-12)
                    {
                        // 減衰モードは除外
                        continue;
                    }
                    */

                    double maxValue = 0.0;
                    for (int ino = 0; ino < nodeCnt; ino++)
                    {
                        double realAbs = Math.Abs(eigenVecs[modeIndex, ino].Real);
                        if (realAbs > maxValue)
                        {
                            maxValue = realAbs;
                        }
                    }
                    Series seriesReal = chart1.Series[(portIndex * showMaxMode + modeIndex) * 2];
                    Series seriesImag = chart1.Series[(portIndex * showMaxMode + modeIndex) * 2 + 1];
                    seriesReal.Points.AddXY(0, 0); // 実数部
                    seriesImag.Points.AddXY(0, 0); // 虚数部
                    for (int ino = 0; ino < nodeCnt; ino++)
                    {
                        // 正確には座標を取ってくる必要があるが等間隔が保障されていれば、下記で規格化された位置は求まる
                        double x0 = (ino + 1) / (double)(nodeCnt + 1);  // 始点と終点は強制境界で除かれている 分割数 = 節点数 - 1 で節点数 = nodeCnt + 2 より 分割数 nodeCnt + 1
                        double real = eigenVecs[modeIndex, ino].Real / maxValue;
                        double imag = eigenVecs[modeIndex, ino].Imaginary / maxValue;
                        seriesReal.Points.AddXY(x0, real); // 実数部
                        seriesImag.Points.AddXY(x0, imag); // 虚数部
                    }
                    seriesReal.Points.AddXY(1.0, 0); // 実数部
                    seriesImag.Points.AddXY(1.0, 0); // 虚数部
                }
            }
        }

        public double GetWaveLength()
        {
            return WaveLength;
        }

        public double GetWaveGuideWidth()
        {
            return WaveguideWidth;
        }

        public double GetNormalizedFrequency()
        {
            if (!isInputDataReady())
            {
                return 0.0;
            }
            if (Math.Abs(WaveLength) < 1.0e-12)
            {
                return 0.0;
            }
            return 2.0 * WaveguideWidth / WaveLength;
        }
    }
}
