using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
//using System.Numerics; // Complex
using KrdLab.clapack; // KrdLab.clapack.Complex
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
        //private const int ShowMaxMode = 2;

        /// <summary>
        /// 1度だけの初期化済み?
        /// </summary>
        private bool IsInitializedOnce = false;
        
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
        /// <summary>
        /// 媒質リスト
        /// </summary>
        private MediaInfo[] Medias;
        /// <summary>
        /// ポートの節点番号リストのリスト
        /// </summary>
        private IList<int[]> Ports;
        /// <summary>
        /// 強制境界の節点番号リスト
        /// </summary>
        private int[] ForceNodes;
        /// <summary>
        /// 入射ポート番号
        /// </summary>
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
        //private IList<int[]> NodesBoundaryList = new List<int[]>(); //メモリ節約の為、削除
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
        //private int[] NodesRegion; //メモリ節約の為、ローカル変数で処理するようにしたため削除
        /// <summary>
        /// フィールド値リスト
        /// </summary>
        //private Complex[] ValuesAll = null; //メモリ節約の為、ローカル変数で処理するようにしたため削除
        /// <summary>
        /// フィールド値の絶対値の最小値
        /// </summary>
        private double MinFValue = 0.0;
        /// <summary>
        /// フィールド値の絶対値の最大値
        /// </summary>
        private double MaxFValue = 1.0;
        /// <summary>
        /// フィールド値の回転の絶対値の最小値
        /// </summary>
        private double MinRotFValue = 0.0;
        /// <summary>
        /// フィールド値の回転の絶対値の最大値
        /// </summary>
        private double MaxRotFValue = 1.0;
        /// <summary>
        /// 複素ポインティングベクトルの絶対値の最小値
        /// </summary>
        private double MinPoyntingFValue = 0.0;
        /// <summary>
        /// 複素ポインティングベクトルの絶対値の最大値
        /// </summary>
        private double MaxPoyntingFValue = 1.0;
        /// <summary>
        /// 散乱行列
        /// </summary>
        private IList<Complex[]> ScatterVecList = new List<Complex[]>();
        /// <summary>
        /// 導波管幅
        /// </summary>
        private double WaveguideWidth = 0.0;
        /// <summary>
        /// 計算開始波長
        /// </summary>
        public double FirstWaveLength
        {
            get;
            private set;
        }
        /// <summary>
        /// 計算終了波長
        /// </summary>
        public double LastWaveLength
        {
            get;
            private set;
        }
        /// <summary>
        /// 計算する周波数の個数
        /// </summary>
        public int CalcFreqCnt
        {
            get;
            private set;
        }

        /// <summary>
        /// 波のモード区分
        /// </summary>
        public FemSolver.WaveModeDV WaveModeDv
        {
            get;
            private set;
        }

        /// <summary>
        /// 要素の数を取得する(表示用)
        /// </summary>
        public int ElementCnt
        {
            get
            {
                if (Elements != null)
                {
                    return Elements.Length;
                }
                return 0;
            }
        }
        /// <summary>
        /// 節点の数を取得する(表示用)
        /// </summary>
        public int NodeCnt
        {
            get
            {
                if (Nodes != null)
                {
                    return Nodes.Length;
                }
                return 0;
            }
        }
        /// <summary>
        /// フィールド値カラーパレット
        /// </summary>
        private ColorMap FValueColorMap = new ColorMap();
        /// <summary>
        /// フィールド値凡例の色パネル
        /// </summary>
        Panel FValueLegendColorPanel = null;
        /// <summary>
        /// 媒質境界の辺のリスト
        ///   辺は"節点番号_節点番号"として格納
        /// </summary>
        private IList<string> MediaBEdgeList = new List<string>();

        /// <summary>
        /// 表示するフィールドのフィールド区分
        /// </summary>
        public FemElement.FieldDV ShowFieldDv
        {
            get;
            set;
        }

        /// <summary>
        /// 表示するフィールドの値区分
        /// </summary>
        public FemElement.ValueDV ShowValueDv
        {
            get;
            set;
        }

        /// <summary>
        /// フィールド値描画を荒くする？
        /// </summary>
        private bool _IsCoarseFieldMesh = false;

        /// <summary>
        /// フィールド値描画を荒くする？
        /// </summary>
        public bool IsCoarseFieldMesh
        {
            get
            {
                return _IsCoarseFieldMesh;
            }
            set
            {
                if (value != _IsCoarseFieldMesh)
                {
                    if (Elements != null)
                    {
                        foreach (FemElement element in Elements)
                        {
                            element.IsCoarseFieldMesh = value;
                        }
                    }
                }
                _IsCoarseFieldMesh = value;
            }
        }

        /// <summary>
        /// 自動計算モード？
        /// </summary>
        public bool IsAutoCalc
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FemPostProLogic()
        {
            IsInitializedOnce = false;
            ShowFieldDv = FemElement.FieldDV.Field;
            ShowValueDv = FemElement.ValueDV.Abs;
            IsAutoCalc = false;
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
            Medias = null;
            Ports = null;
            ForceNodes = null;
            WaveguideWidth = FemSolver.DefWaveguideWidth;
            IncidentPortNo = 1;
            CalcFreqCnt = 0;
            FirstWaveLength = 0.0;
            LastWaveLength = 0.0;
            WaveModeDv = FemSolver.WaveModeDV.TE;
            _IsCoarseFieldMesh = false;
        }

        /// <summary>
        /// 初期化処理(出力)
        /// </summary>
        private void initOutput()
        {
            WaveLength = 0;
            MaxMode = 0;
            //NodesBoundaryList.Clear();
            MediaBEdgeList.Clear();
            EigenValuesList.Clear();
            EigenVecsList.Clear();
            //NodesRegion = null;
            //ValuesAll = null;
            MaxFValue = 1.0;
            MinFValue = 0.0;
            MaxRotFValue = 1.0;
            MinRotFValue = 0.0;
            MaxPoyntingFValue = 1.0;
            MinPoyntingFValue = 0.0;

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
        /// 入出力データの初期化
        /// </summary>
        private void initDataOnce(
            Panel FValueLegendPanel,
            Label labelFreqValue
        )
        {
            if (IsInitializedOnce) return;

            // フィールド値凡例パネルの初期化
            InitFValueLegend(FValueLegendPanel, labelFreqValue);
            
            IsInitializedOnce = true;
        }

        /// <summary>
        /// 入出力データの初期化
        /// </summary>
        public void InitData(
            FemSolver solver,
            Panel CadPanel,
            Panel FValuePanel,
            Panel FValueLegendPanel, Label labelFreqValue,
            Chart SMatChart,
            Chart BetaChart,
            Chart EigenVecChart
            )
        {
            initInput();
            initOutput();

            // 一度だけの初期化処理
            initDataOnce(FValueLegendPanel, labelFreqValue);

            // ポストプロセッサに入力データをコピー
            // 入力データの取得
            solver.GetFemInputInfo(out Nodes, out Elements, out Medias, out Ports, out ForceNodes, out IncidentPortNo, out WaveguideWidth);
            // チャートの設定用に開始終了波長を取得
            FirstWaveLength = solver.FirstWaveLength;
            LastWaveLength = solver.LastWaveLength;
            CalcFreqCnt = solver.CalcFreqCnt;
            // 波のモード区分を取得
            WaveModeDv = solver.WaveModeDv;

            //if (isInputDataReady())
            // ポートが指定されていなくてもメッシュを表示できるように条件を変更
            if (Elements != null && Elements.Length > 0 && Nodes != null && Nodes.Length > 0 && Medias != null && Medias.Length > 0)
            {
                // 各要素に節点情報を補完する
                foreach (FemElement element in Elements)
                {
                    element.SetNodesFromAllNodes(Nodes);
                    element.LineColor = Color.Black;
                    element.BackColor = Medias[element.MediaIndex].BackColor;
                }
            }

            // メッシュ描画
            //using (Graphics g = CadPanel.CreateGraphics())
            //{
            //    DrawMesh(g, CadPanel);
            //}
            //CadPanel.Invalidate();

            if (!IsAutoCalc)
            {
                // チャート初期化
                ResetSMatChart(SMatChart);
                // 等高線図の凡例
                UpdateFValueLegend(FValueLegendPanel, labelFreqValue);
                // 等高線図
                //FValuePanel.Invalidate();
                FValuePanel.Refresh();
                // 固有値チャート初期化
                // この段階ではMaxModeの値が0なので、後に計算値ロード後一回だけ初期化する
                ResetEigenValueChart(BetaChart);
                // 固有ベクトル表示(空のデータで初期化)
                SetEigenVecToChart(EigenVecChart);
            }
        }

        /// <summary>
        /// 規格化周波数をセットする(自動計算モード用)
        /// </summary>
        /// <param name="normalizedFreq"></param>
        public void SetNormalizedFrequency(double normalizedFreq)
        {
            // 波長をセット
            WaveLength = FemSolver.GetWaveLengthFromNormalizedFreq(normalizedFreq, WaveguideWidth);
        }

        /// <summary>
        /// 出力データだけ初期化する(自動計算モード用)
        /// </summary>
        /// <param name="CadPanel"></param>
        /// <param name="FValuePanel"></param>
        /// <param name="FValueLegendPanel"></param>
        /// <param name="labelFreqValue"></param>
        /// <param name="SMatChart"></param>
        /// <param name="BetaChart"></param>
        /// <param name="EigenVecChart"></param>
        public void InitOutputData(
            Panel CadPanel,
            Panel FValuePanel,
            Panel FValueLegendPanel, Label labelFreqValue,
            Chart SMatChart,
            Chart BetaChart,
            Chart EigenVecChart
            )
        {
            initOutput();

            // メッシュ描画
            //using (Graphics g = CadPanel.CreateGraphics())
            //{
            //    DrawMesh(g, CadPanel);
            //}
            //CadPanel.Invalidate();

            if (!IsAutoCalc)
            {
                // チャート初期化
                ResetSMatChart(SMatChart);
                // 等高線図の凡例
                UpdateFValueLegend(FValueLegendPanel, labelFreqValue);
                //FValuePanel.Invalidate();
                // 等高線図
                FValuePanel.Refresh();
                // 固有値チャート初期化
                // この段階ではMaxModeの値が0なので、後に計算値ロード後一回だけ初期化する
                ResetEigenValueChart(BetaChart);
                // 固有ベクトル表示(空のデータで初期化)
                SetEigenVecToChart(EigenVecChart);
            }
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
            IList<int[]> nodesBoundaryList = null;
            int[] nodesRegion = null;
            Complex[] valuesAll = null;
            bool ret = FemOutputDatFile.LoadFromFile(
                filename, freqNo,
                out WaveLength, out MaxMode, out incidentPortNo,
                out nodesBoundaryList, out EigenValuesList, out EigenVecsList,
                out nodesRegion, out valuesAll,
                out ScatterVecList);

            if (ret)
            {
                //System.Diagnostics.Debug.Assert(maxMode == MaxMode);
                System.Diagnostics.Debug.Assert(incidentPortNo == IncidentPortNo);

                // メモリ節約の為必要なモード数だけ取り出す
                if (EigenValuesList != null)
                {
                    for (int portIndex = 0; portIndex < EigenValuesList.Count; portIndex++)
                    {
                        Complex[] eigenValues = EigenValuesList[portIndex];
                        Complex[] eigenValues2 = new Complex[ShowMaxMode];
                        for (int imode = 0; imode < eigenValues.Length; imode++)
                        {
                            if (imode >= ShowMaxMode) break;
                            eigenValues2[imode] = eigenValues[imode];
                        }
                        // 入れ替える
                        EigenValuesList[portIndex] = eigenValues2;
                        eigenValues = null;
                        eigenValues2 = null;
                    }
                }
                if (EigenVecsList != null)
                {
                    for (int portIndex = 0; portIndex < EigenVecsList.Count; portIndex++)
                    {
                        Complex[,] eigenVecs = EigenVecsList[portIndex];
                        Complex[,] eigenVecs2 = new Complex[ShowMaxMode, eigenVecs.GetLength(1)];
                        for (int imode = 0; imode < eigenVecs.GetLength(0); imode++)
                        {
                            if (imode >= ShowMaxMode) break;
                            for (int ino = 0; ino < eigenVecs.GetLength(1); ino++)
                            {
                                eigenVecs2[imode, ino] = eigenVecs[imode, ino];
                            }
                        }
                        // 入れ替える
                        EigenVecsList[portIndex] = eigenVecs2;
                        eigenVecs = null;
                        eigenVecs2 = null;
                    }
                }
                if (ScatterVecList != null)
                {
                    for (int portIndex = 0; portIndex < ScatterVecList.Count; portIndex++)
                    {
                        Complex[] portScatterVec = ScatterVecList[portIndex];
                        Complex[] portScatterVec2 = new Complex[ShowMaxMode];
                        for (int imode = 0; imode < portScatterVec.Length; imode++)
                        {
                            if (imode >= ShowMaxMode) break;
                            portScatterVec2[imode] = portScatterVec[imode];
                        }
                        // 入れ替える
                        ScatterVecList[portIndex] = portScatterVec2;
                        portScatterVec = null;
                        portScatterVec2 = null;
                    }
                }

                // 要素にフィールド値をセットする
                setupFieldValueToElements(nodesRegion, valuesAll);

                // 媒質境界の辺を取得する
                setupMediaBEdgeList();
            }
            return ret;
        }

        /// <summary>
        /// 要素にフィールド値をセットする
        /// </summary>
        /// <param name="nodesRegion">節点番号リスト</param>
        /// <param name="valuesAll">節点のフィールド値のリスト</param>
        private void setupFieldValueToElements(int[] nodesRegion, Complex[] valuesAll)
        {
            //System.Diagnostics.Debug.Assert(Math.Abs(WaveLength) < Constants.PrecisionLowerLimit);
            if (Math.Abs(WaveLength) < Constants.PrecisionLowerLimit)
            {
                return;
            }

            // 定数
            const double pi = Constants.pi;
            const double c0 = Constants.c0;
            // 波数
            double k0 = 2.0 * pi / WaveLength;
            // 角周波数
            double omega = k0 * c0;
            // 回転に掛ける因子
            Complex factorForRot = 1.0;
            if (WaveModeDv == FemSolver.WaveModeDV.TM)
            {
                factorForRot = - 1.0 * Complex.ImaginaryOne / (omega * Constants.eps0);
            }
            else
            {
                factorForRot = Complex.ImaginaryOne / (omega * Constants.myu0);
            }

            /// 領域内節点の節点番号→インデックスマップ
            Dictionary<int, int> nodesRegionToIndex = new Dictionary<int, int>();
            // 節点番号→インデックスのマップ作成
            for (int ino = 0; ino < nodesRegion.Length; ino++)
            {
                int nodeNumber = nodesRegion[ino];
                if (!nodesRegionToIndex.ContainsKey(nodeNumber))
                {
                    nodesRegionToIndex.Add(nodeNumber, ino);
                }
            }
            // 要素リストにフィールド値を格納
            foreach (FemElement element in Elements)
            {
                MediaInfo media = Medias[element.MediaIndex];
                double[,] media_Q = null;
                if (WaveModeDv == FemSolver.WaveModeDV.TM)
                {
                    // TMモードの場合、比誘電率
                    media_Q = media.P;
                }
                else
                {
                    // TEモードの場合、比透磁率
                    media_Q = media.Q;
                }
                element.SetFieldValueFromAllValues(valuesAll, nodesRegionToIndex,
                    factorForRot, media_Q, WaveModeDv);
            }

            // フィールド値の絶対値の最小、最大
            double minFValue = double.MaxValue;
            double maxFValue = double.MinValue;
            double minRotFValue = double.MaxValue;
            double maxRotFValue = double.MinValue;
            double minPoyntingFValue = double.MaxValue;
            double maxPoyntingFValue = double.MinValue;
            foreach (FemElement element in Elements)
            {
                int nno = element.NodeNumbers.Length;
                for (int ino = 0; ino < nno; ino++)
                {
                    Complex fValue = element.getFValue(ino);
                    Complex rotXFValue = element.getRotXFValue(ino);
                    Complex rotYFValue = element.getRotYFValue(ino);
                    Complex poyntingXFValue = element.getPoyntingXFValue(ino);
                    Complex poyntingYFValue = element.getPoyntingYFValue(ino);
                    double fValueAbs = Complex.Abs(fValue);
                    //double rotFValueAbs = Math.Sqrt(Math.Pow(rotXFValue.Magnitude, 2) + Math.Pow(rotYFValue.Magnitude, 2));
                    double rotFValueAbs = Math.Sqrt(Math.Pow(rotXFValue.Real, 2) + Math.Pow(rotYFValue.Real, 2));
                    //double poyntingFValueAbs = Math.Sqrt(Math.Pow(poyntingXFValue.Magnitude, 2) + Math.Pow(poyntingYFValue.Magnitude, 2));
                    double poyntingFValueAbs = Math.Sqrt(Math.Pow(poyntingXFValue.Real, 2) + Math.Pow(poyntingYFValue.Real, 2));

                    if (fValueAbs > maxFValue)
                    {
                        maxFValue = fValueAbs;
                    }
                    if (fValueAbs < minFValue)
                    {
                        minFValue = fValueAbs;
                    }
                    if (rotFValueAbs > maxRotFValue)
                    {
                        maxRotFValue = rotFValueAbs;
                    }
                    if (rotFValueAbs < minRotFValue)
                    {
                        minRotFValue = rotFValueAbs;
                    }

                    if (poyntingFValueAbs > maxPoyntingFValue)
                    {
                        maxPoyntingFValue = poyntingFValueAbs;
                    }
                    if (poyntingFValueAbs < minPoyntingFValue)
                    {
                        minPoyntingFValue = poyntingFValueAbs;
                    }
                }
            }
            // 節点上の値より要素内部の値の方が大きいことがある
            double scaleFactor = 1.05;
            MinFValue = minFValue * scaleFactor;
            MaxFValue = maxFValue * scaleFactor;
            MinRotFValue = minRotFValue * scaleFactor;
            MaxRotFValue = maxRotFValue * scaleFactor;
            MinPoyntingFValue = minPoyntingFValue * scaleFactor;
            MaxPoyntingFValue = maxPoyntingFValue * scaleFactor;

            /*
            // 等高線図描画の為に最大、最小値を取得する
            // フィールド値の絶対値の最小、最大
            double minfValue = double.MaxValue;
            double maxfValue = double.MinValue;
            foreach (Complex fValue in valuesAll)
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
            MinFValue = minfValue;
            MaxFValue = maxfValue;
             */
        }

        /// <summary>
        /// 媒質境界の辺を取得
        /// </summary>
        private void setupMediaBEdgeList()
        {
            // 辺と要素番号の対応マップを取得
            Dictionary<string, IList<int>> edgeToElementNoH = new Dictionary<string, IList<int>>();
            FemSolver.MkEdgeToElementNoH(Elements, ref edgeToElementNoH);

            MediaBEdgeList.Clear();
            // 媒質の境界の辺を取得
            foreach (KeyValuePair<string, IList<int>> pair in edgeToElementNoH)
            {
                string edgeKeyStr = pair.Key;
                IList<int> elementNoList = pair.Value;
                if (elementNoList.Count >= 2)
                {
                    if (Elements[elementNoList[0] - 1].MediaIndex != Elements[elementNoList[1] - 1].MediaIndex)
                    {
                        MediaBEdgeList.Add(edgeKeyStr);
                    }
                }
            }
        }

        /// <summary>
        /// データが準備できてる？
        /// </summary>
        /// <returns></returns>
        public bool IsDataReady()
        {
            return isInputDataReady() && isOutputDataReady();
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
            if (Math.Abs(WaveguideWidth - FemSolver.DefWaveguideWidth) < Constants.PrecisionLowerLimit)
            {
                return isReady;
            }
            if (Ports.Count == 0)
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
            //if (NodesBoundaryList.Count == 0)
            //{
            //    return isReady;
            //}
            if (EigenValuesList.Count == 0)
            {
                return isReady;
            }
            if (EigenVecsList.Count == 0)
            {
                return isReady;
            }
            //if (NodesRegion == null)
            //{
            //    return isReady;
            //}
            //if (ValuesAll == null)
            //{
            //    return isReady;
            //}
            if (ScatterVecList.Count == 0)
            {
                return isReady;
            }

            isReady = true;
            return isReady;
        }
        
        /// <summary>
        /// 出力をGUIへセットする
        /// </summary>
        /// <param name="addFlg">周波数特性グラフに読み込んだ周波数のデータを追加する？</param>
        public void SetOutputToGui(
            string FemOutputDatFilePath,
            Panel CadPanel,
            Panel FValuePanel,
            Panel FValueLegendPanel, Label labelFreqValue,
            Chart SMatChart,
            Chart BetaChart,
            Chart EigenVecChart,
            bool addFlg = true)
        {
            if (!isInputDataReady())
            {
                return;
            }
            if (!isOutputDataReady())
            {
                return;
            }
            if (IsAutoCalc)
            {
                ResetSMatChart(SMatChart);
                ResetEigenValueChart(BetaChart);
                /*
                // チャートのデータをクリア
                foreach (Series series in SMatChart.Series)
                {
                    series.Points.Clear();
                }
                foreach (Series series in BetaChart.Series)
                {
                    series.Points.Clear();
                }
                 */
                //foreach (Series series in EigenVecChart.Series)
                //{
                //    series.Points.Clear();
                //}
            }

            if (addFlg)
            {
                // Sマトリックス周波数特性グラフに計算した点を追加
                AddScatterMatrixToChart(SMatChart);
                int firstFreqNo;
                int lastFreqNo;
                if (GetCalculatedFreqCnt(FemOutputDatFilePath, out firstFreqNo, out lastFreqNo) == 1)
                {
                    // 固有値チャート初期化(モード数が変わっているので再度初期化する)
                    ResetEigenValueChart(BetaChart);
                }
                // 固有値(伝搬定数)周波数特性グラフに計算した点を追加
                AddEigenValueToChart(BetaChart);
            }

            // 等高線図の凡例
            UpdateFValueLegend(FValueLegendPanel, labelFreqValue);
            // 等高線図
            //FValuePanel.Invalidate();
            FValuePanel.Refresh();
            // 固有ベクトル表示
            SetEigenVecToChart(EigenVecChart);

            if (IsAutoCalc)
            {
                // チャートの表示をポイント表示にする
                ShowChartDataLabel(SMatChart, BetaChart);
            }
        }
        
        
        /// <summary>
        /// メッシュ描画
        /// </summary>
        /// <param name="g"></param>
        /// <param name="panel"></param>
        public void DrawMesh(Graphics g, Panel panel, bool fitFlg = false, bool transparent = false)
        {
            DrawMesh(g, panel, panel.ClientRectangle, fitFlg, transparent);
        }

        public void DrawMesh(Graphics g, Panel panel, Rectangle clientRectangle, bool fitFlg = false, bool transparent = false)
        {
            //if (!isInputDataReady())
            // ポートが指定されていなくてもメッシュを表示できるように条件を変更
            if (!(Elements != null && Elements.Length > 0 && Nodes != null && Nodes.Length > 0))
            {
                return;
            }
            Size ofs;
            Size delta;
            Size regionSize;
            if (!fitFlg)
            {
                //getDrawRegion(panel, out delta, out ofs, out regionSize);
                getDrawRegion(clientRectangle.Width, clientRectangle.Height, out delta, out ofs, out regionSize);
            }
            else
            {
                //getFitDrawRegion(panel, out delta, out ofs, out regionSize);
                getFitDrawRegion(clientRectangle.Width, clientRectangle.Height, out delta, out ofs, out regionSize);
            }
            ofs.Width += clientRectangle.Left;
            ofs.Height += clientRectangle.Top;

            foreach (FemElement element in Elements)
            {
                element.LineColor = panel.ForeColor;
                Color saveBackColor = element.BackColor;
                Color saveLineColor = element.LineColor;
                if (transparent)
                {
                    element.BackColor = Color.FromArgb(64, saveBackColor.R, saveBackColor.G, saveBackColor.B);
                    element.LineColor = element.BackColor;
                }
                element.Draw(g, ofs, delta, regionSize, true);
                if (transparent)
                {
                    element.LineColor = saveLineColor;
                    element.BackColor = saveBackColor;
                }
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
            getDrawRegion(panel.Width, panel.Height, out delta, out ofs, out regionSize);
        }

        private void getDrawRegion(int panelWidth, int panelHeight, out Size delta, out Size ofs, out Size regionSize)
        {
            // 描画領域の方眼桝目の寸法を決定
            double deltaxx = panelWidth / (double)(Constants.MaxDiv.Width + 2);
            int deltax = (int)deltaxx;
            double deltayy = panelHeight / (double)(Constants.MaxDiv.Height + 2);
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
            getFitDrawRegion(panel.Width, panel.Height, out delta, out ofs, out regionSize);
        }

        private void getFitDrawRegion(int panelWidth, int panelHeight, out Size delta, out Size ofs, out Size regionSize)
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

            int panel_width = panelWidth;
            int panel_height = panel_height = (int)((double)panelWidth * (Constants.MaxDiv.Height + 2) / (double)(Constants.MaxDiv.Width + 2));
            if (panelHeight < panel_height)
            {
                panel_height = panelHeight;
                panel_width = (int)((double)panelHeight * (Constants.MaxDiv.Width + 2) / (double)(Constants.MaxDiv.Height + 2));
            }
            // 描画領域の方眼桝目の寸法を決定
            // 図形をパネルのサイズにあわせて拡縮する
            int w = (int)(maxPt[0] - minPt[0]);
            int h = (int)(maxPt[1] - minPt[1]);
            int boxWidth = w > h ? w : h;
            System.Diagnostics.Debug.Assert(boxWidth > 0);
            double marginxx = panel_width / (double)(Constants.MaxDiv.Width + 2);
            int marginx = (int)marginxx;
            double marginyy = panel_height / (double)(Constants.MaxDiv.Height + 2);
            int marginy = (int)marginyy;
            double deltaxx = (panel_width - marginx * 2) / (double)boxWidth;
            int deltax = (int)deltaxx;
            double deltayy = (panel_height - marginy * 2) / (double)boxWidth;
            int deltay = (int)deltayy;
            // 図形の左下がパネルの左下にくるようにする
            int ofsx = marginx - (int)(deltaxx * (minPt[0] - 0));
            int ofsy = marginy - (int)(deltayy * ((Constants.MaxDiv.Height - minPt[1]) - Constants.MaxDiv.Height));
            // 図形の中央がパネルの中央に来るようにする
            ofsx += (int)(deltaxx * (boxWidth - w) * 0.5);
            ofsy -= (int)(deltayy * (boxWidth - h) * 0.5);
            // アスペクト比を調整した分
            ofsx += (int)((panelWidth - panel_width) * 0.5);
            ofsy += (int)((panelHeight - panel_height) * 0.5);

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
            DrawFieldEx(g, panel, panel.ClientRectangle, ShowFieldDv, ShowValueDv);
        }

        /// <summary>
        /// フィールド値等高線図描画
        /// </summary>
        /// <param name="g"></param>
        /// <param name="panel"></param>
        public void DrawFieldEx(Graphics g, Panel panel, Rectangle clientRectangle, FemElement.FieldDV fieldDv, FemElement.ValueDV valueDv)
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
            //getFitDrawRegion(panel, out delta, out ofs, out regionSize);
            getFitDrawRegion(clientRectangle.Width, clientRectangle.Height, out delta, out ofs, out regionSize);
            ofs.Width += clientRectangle.Left;
            ofs.Height += clientRectangle.Top;

            double min = 0.0;
            double max = 1.0;
            if (fieldDv == FemElement.FieldDV.Field)
            {
                min = MinFValue;
                max = MaxFValue;
            }
            else if (fieldDv == FemElement.FieldDV.RotX || fieldDv == FemElement.FieldDV.RotY)
            {
                min = MinRotFValue;
                max = MaxRotFValue;
            }
            else
            {
                return;
            }

            // カラーマップに最小、最大を設定
            if (valueDv == FemElement.ValueDV.Real || valueDv == FemElement.ValueDV.Imaginary)
            {
                FValueColorMap.Min = -max;
                FValueColorMap.Max = max;
            }
            else
            {
                // 既定値は絶対値で処理する
                //FValueColorMap.Min = min;
                FValueColorMap.Min = 0.0;
                FValueColorMap.Max = max;
            }

            foreach (FemElement element in Elements)
            {
                // 等高線描画
                element.DrawField(g, ofs, delta, regionSize, fieldDv, valueDv, FValueColorMap);
            }
        }

        /// <summary>
        /// フィールドの回転ベクトル描画
        /// </summary>
        /// <param name="g"></param>
        /// <param name="panel"></param>
        public void DrawRotField(Graphics g, Panel panel)
        {
            DrawRotFieldEx(g, panel, panel.ClientRectangle, ShowFieldDv);
        }

        /// <summary>
        /// フィールドの回転ベクトル描画
        /// </summary>
        /// <param name="g"></param>
        /// <param name="panel"></param>
        public void DrawRotFieldEx(Graphics g, Panel panel, Rectangle clientRectangle, FemElement.FieldDV fieldDv)
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
            //getFitDrawRegion(panel, out delta, out ofs, out regionSize);
            getFitDrawRegion(clientRectangle.Width, clientRectangle.Height, out delta, out ofs, out regionSize);
            ofs.Width += clientRectangle.Left;
            ofs.Height += clientRectangle.Top;

            Color drawColor = Color.Gray;
            double min = 0.0;
            double max = 1.0;
            if (fieldDv == FemElement.FieldDV.PoyntingXY)
            {
                drawColor = Color.Green;//Color.YellowGreen;
                min = -MaxPoyntingFValue;
                max = MaxPoyntingFValue;
            }
            else if (fieldDv == FemElement.FieldDV.RotXY)
            {
                drawColor = Color.Red;
                min = -MaxRotFValue;
                max = MaxRotFValue;
            }
            else
            {
                return;
            }
            foreach (FemElement element in Elements)
            {
                // 回転ベクトル描画
                element.DrawRotField(g, ofs, delta, regionSize, drawColor, fieldDv, min, max);
            }
        }

        /// <summary>
        /// 媒質の境界を描画する
        /// </summary>
        /// <param name="g"></param>
        /// <param name="panel"></param>
        public void DrawMediaB(Graphics g, Panel panel, bool fitFlg = false)
        {
            DrawMediaB(g, panel, panel.ClientRectangle, fitFlg);
        }

        public void DrawMediaB(Graphics g, Panel panel, Rectangle clientRectangle, bool fitFlg = false)
        {
            if (!isInputDataReady())
            {
                return;
            }
            // 線の色
            Color lineColor = Color.Black;
            // 線の太さ
            int lineWidth = 1;
            Size ofs;
            Size delta;
            Size regionSize;
            if (!fitFlg)
            {
                //getDrawRegion(panel, out delta, out ofs, out regionSize);
                getDrawRegion(clientRectangle.Width, clientRectangle.Height, out delta, out ofs, out regionSize);
            }
            else
            {
                //getFitDrawRegion(panel, out delta, out ofs, out regionSize);
                getFitDrawRegion(clientRectangle.Width, clientRectangle.Height, out delta, out ofs, out regionSize);
            }
            ofs.Width += clientRectangle.Left;
            ofs.Height += clientRectangle.Top;

            foreach (string edgeKeyStr in MediaBEdgeList)
            {
                string[] tokens = edgeKeyStr.Split('_');
                System.Diagnostics.Debug.Assert(tokens.Length == 2);
                if (tokens.Length != 2)
                {
                    continue;
                }
                int[] nodeNumbers = { int.Parse(tokens[0]), int.Parse(tokens[1]) };
                double[][] pps = new double[2][]
                {
                    Nodes[nodeNumbers[0] - 1].Coord,
                    Nodes[nodeNumbers[1] - 1].Coord
                };
                Point[] points = new Point[2];
                for (int ino = 0; ino < 2; ino++)
                {
                    points[ino] = new Point();
                    points[ino].X = (int)((double)pps[ino][0] * delta.Width);
                    points[ino].Y = (int)(regionSize.Height - (double)pps[ino][1] * delta.Height);
                    points[ino] += ofs;
                }
                using (Pen pen = new Pen(lineColor, lineWidth))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    g.DrawLine(pen, points[0], points[1]);
                }
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

            if (ShowFieldDv == FemElement.FieldDV.PoyntingXY || ShowFieldDv == FemElement.FieldDV.RotXY)
            {
                // ベクトル表示時はカラーマップを表示しない
                return;
            }
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
            const int ofsX = 22;
            const int ofsY = 0;
            const int height = 20; // 1目盛の高さ
            double divValue = 1.0 / (double)cnt;

            using (Font font = new Font("MS UI Gothic", 9))
            using (Brush brush = new SolidBrush(FValueLegendColorPanel.ForeColor))
            {
                if (ShowValueDv == FemElement.ValueDV.Abs)
                {
                    for (int i = 0; i < cnt + 1; i++)
                    {
                        int y = i * height;
                        string text = string.Format("{0:F1}", (cnt - i) * divValue);
                        g.DrawString(text, font, brush, new Point(ofsX, y + ofsY));
                    }
                }
                else
                {
                    for (int i = 0; i < cnt + 1; i++)
                    {
                        int y = i * height;
                        double value = ((cnt - i) * 2.0 - cnt) * divValue;
                        string text = string.Format(value >= 0 ? "+{0:F1}" : "{0:F1}", value);
                        g.DrawString(text, font, brush, new Point(ofsX, y + ofsY));
                    }
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
            if (Math.Abs(WaveLength) < Constants.PrecisionLowerLimit)
            {
                labelFreqValue.Text = "---";
            }
            else
            {
                labelFreqValue.Text = string.Format("{0:F2}", GetNormalizedFrequency());
            }
            // BUGFIX [次の周波数][前の周波数]ボタンで周波数が遅れて表示される不具合を修正
            labelFreqValue.Refresh();
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
        public void ResetSMatChart(Chart chart1)
        {
            double normalizedFreq1 = FemSolver.GetNormalizedFreq(FirstWaveLength, WaveguideWidth);
            normalizedFreq1 = Math.Round(normalizedFreq1, 2);
            double normalizedFreq2 = FemSolver.GetNormalizedFreq(LastWaveLength, WaveguideWidth);
            normalizedFreq2 = Math.Round(normalizedFreq2, 2);

            // チャート初期化
            setupChartColor(chart1);
            chart1.Titles[0].Text = "散乱係数周波数特性";
            chart1.ChartAreas[0].Axes[0].Title = "2W/λ";
            chart1.ChartAreas[0].Axes[1].Title = string.Format("|Si{0}|", IncidentPortNo);
            SetChartFreqRange(chart1, normalizedFreq1, normalizedFreq2);
            chart1.ChartAreas[0].Axes[1].Minimum = 0.0;
            chart1.ChartAreas[0].Axes[1].Maximum = 1.0;
            chart1.ChartAreas[0].Axes[1].Interval = 0.2;
            chart1.Series.Clear();
            if (!isInputDataReady())
            {
                //MessageBox.Show("入力データがセットされていません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int showMaxMode = ShowMaxMode;
            if (showMaxMode == 1)
            {
                for (int portIndex = 0; portIndex < Ports.Count; portIndex++)
                {
                    Series series = new Series();
                    series.Name = string.Format("|S{0}{1}|", portIndex + 1, IncidentPortNo);
                    series.ChartType = SeriesChartType.Line;
                    series.BorderDashStyle = ChartDashStyle.Solid;
                    chart1.Series.Add(series);
                }
            }
            else
            {
                const int incidentModeIndex = 0;
                for (int portIndex = 0; portIndex < Ports.Count; portIndex++)
                {
                    for (int iMode = 0; iMode < showMaxMode; iMode++)
                    {
                        Series series = new Series();
                        series.Name = string.Format("|S{0}{1}{2}{3}|", portIndex + 1, (iMode + 1), IncidentPortNo, (incidentModeIndex + 1));
                        series.ChartType = SeriesChartType.Line;
                        series.BorderDashStyle = ChartDashStyle.Solid;
                        chart1.Series.Add(series);
                    }
                }
            }
            // 基本モード以外への電力損失のルート値
            {
                Series series = new Series();
                series.Name = "√|loss|";
                series.ChartType = SeriesChartType.Line;
                series.BorderDashStyle = ChartDashStyle.Dash;
                series.Color = chart1.ChartAreas[0].AxisX.LineColor;//chart1.Parent.ForeColor;
                chart1.Series.Add(series);
            }
            // 計算されたグラフのプロパティ値をAutoに設定
            chart1.ResetAutoValues();
        }

        /// <summary>
        /// チャートの周波数範囲をセットする
        /// </summary>
        /// <param name="chart1"></param>
        public void SetChartFreqRange(Chart chart1, double normalizedFreq1, double normalizedFreq2)
        {
            //chart1.ChartAreas[0].Axes[0].Minimum = 1.0;
            //chart1.ChartAreas[0].Axes[0].Maximum = 2.0;
            //chart1.ChartAreas[0].Axes[0].Minimum = Constants.DefNormalizedFreqRange[0];
            //chart1.ChartAreas[0].Axes[0].Maximum = Constants.DefNormalizedFreqRange[1];
            double minFreq = normalizedFreq1;
            minFreq = Math.Floor(minFreq * 10.0) * 0.1;
            double maxFreq = normalizedFreq2;
            maxFreq = Math.Ceiling(maxFreq * 10.0) * 0.1;
            chart1.ChartAreas[0].Axes[0].Minimum = minFreq;
            chart1.ChartAreas[0].Axes[0].Maximum = maxFreq;
            //chart1.ChartAreas[0].Axes[0].Interval = (maxFreq - minFreq >= 0.9)? 0.2 : 0.1;
            double range = maxFreq - minFreq;
            double interval = 0.2;
            interval = range / 5 ;
            if (1.0 <= interval)
            {
                interval = Math.Round(interval);
            }
            else if (0.1 <= interval && interval < 1.0)
            {
                interval = Math.Round(interval/0.1) * 0.1;
            }
            else if (0.01 <= interval && interval < 0.1)
            {
                interval = Math.Round(interval/0.01) * 0.01;
            }
            else if (0.001 <= interval && interval < 0.01)
            {
                interval = Math.Round(interval/0.001) * 0.001;
            }
            else if (interval < 0.001)
            {
                interval = Math.Round(interval/0.0001) * 0.0001;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
            chart1.ChartAreas[0].Axes[0].Interval = interval;
        }

        /// <summary>
        /// 散乱係数周波数特性チャートのY軸最大値を調整する
        /// </summary>
        /// <param name="chart1"></param>
        public void AdjustSMatChartYAxisMax(Chart chart1)
        {
            double maxValue = 0.0;
            foreach (Series series in chart1.Series)
            {
                foreach (DataPoint dataPoint in series.Points)
                {
                    double workMax = dataPoint.YValues.Max();
                    if (workMax > maxValue)
                    {
                        maxValue = workMax;
                    }
                }
            }
            if (maxValue <= 1.0)
            {
                // 散乱係数が1.0以下の正常な場合は、最大値を1.0固定で指定する
                chart1.ChartAreas[0].Axes[1].Maximum = 1.0;
            }
            else
            {
                chart1.ChartAreas[0].Axes[1].Maximum = maxValue;
            }
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
            int showMaxMode = ShowMaxMode;
            double totalPower = 0.0;
            for (int portIndex = 0; portIndex < Ports.Count; portIndex++)
            {
                Complex[] portScatterVec = ScatterVecList[portIndex];
                int modeCnt = portScatterVec.Length;
                for (int iMode = 0; iMode < showMaxMode && iMode < modeCnt; iMode++)
                {
                    Series series = chart1.Series[iMode + portIndex * showMaxMode];

                    Complex sim10 = portScatterVec[iMode];
                    //DataPoint point = series.Points.Add(Complex.Abs(si1));
                    //point.AxisLabel = string.Format("{0:G4}", 2.0 * WaveguideWidth/WaveLength);
                    //series.Points.AddXY(2.0 * WaveguideWidth / WaveLength, Complex.Abs(si1));
                    series.Points.AddXY(GetNormalizedFrequency(), Complex.Abs(sim10));

                    totalPower += (sim10 * Complex.Conjugate(sim10)).Real;
                }
            }
            if (chart1.Series.Count > 0)
            {
                // 基本モード以外への損失のルート値をプロットする
                double loss = 1.0 - totalPower;
                if (loss < 0.0)
                {
                    //loss = 0.0;
                    loss = -loss; // 誤差扱い
                }
                Series series = chart1.Series[chart1.Series.Count - 1];
                series.Points.AddXY(GetNormalizedFrequency(), Math.Sqrt(loss));
            }
            AdjustSMatChartYAxisMax(chart1);
        }

        /// <summary>
        /// 伝搬定数分散特性(グラフの初期化
        /// </summary>
        /// <param name="chart1"></param>
        public void ResetEigenValueChart(Chart chart1)
        {
            double normalizedFreq1 = FemSolver.GetNormalizedFreq(FirstWaveLength, WaveguideWidth);
            normalizedFreq1 = Math.Round(normalizedFreq1, 2);
            double normalizedFreq2 = FemSolver.GetNormalizedFreq(LastWaveLength, WaveguideWidth);
            normalizedFreq2 = Math.Round(normalizedFreq2, 2);

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
            SetChartFreqRange(chart1, normalizedFreq1, normalizedFreq2);
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
                        if (Math.Abs(beta.Imaginary / k0) >= Constants.PrecisionLowerLimit)
                        {
                            modeDescr = "減衰モード";
                        }
                        else if (Math.Abs(beta.Real / k0) >= Constants.PrecisionLowerLimit)
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
                IList<int> portNodes = Ports[portIndex];
                Complex[,] eigenVecs = EigenVecsList[portIndex];
                int nodeCnt = eigenVecs.GetLength(1);
                for (int modeIndex = 0; modeIndex < showMaxMode; modeIndex++)
                {
                    Complex beta = EigenValuesList[portIndex][modeIndex];
                    /*
                    if (Math.Abs(beta.Imaginary/k0) >= Constants.PrecisionLowerLimit)
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
                    {
                        int ino = 0;  // 強制境界を除いた節点のインデックス
                        for (int inoB = 0; inoB < portNodes.Count; inoB++)
                        {
                            int nodeNumber = portNodes[inoB];
                            // 正確には座標を取ってくる必要があるが等間隔が保障されていれば、下記で規格化された位置は求まる
                            double x0;
                            x0 = inoB / (double)(portNodes.Count - 1);
                            if (ForceNodes.Contains(nodeNumber))
                            {
                                seriesReal.Points.AddXY(x0, 0.0); // 実数部
                                seriesImag.Points.AddXY(x0, 0.0); // 虚数部
                            }
                            else
                            {
                                double real = eigenVecs[modeIndex, ino].Real / maxValue;
                                double imag = eigenVecs[modeIndex, ino].Imaginary / maxValue;
                                seriesReal.Points.AddXY(x0, real); // 実数部
                                seriesImag.Points.AddXY(x0, imag); // 虚数部
                                ino++;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// チャートのデータをラベル表示する(自動計算モード用)
        /// </summary>
        /// <param name="SMatChart"></param>
        /// <param name="BetaChart"></param>
        public void ShowChartDataLabel(Chart SMatChart, Chart BetaChart)
        {
            foreach (Series series in SMatChart.Series)
            {
                series.ChartType = SeriesChartType.Point;
                series.Label = "#VALY{N4}";
            }
            foreach (Series series in BetaChart.Series)
            {
                series.ChartType = SeriesChartType.Point;
                series.Label = "#VALY{N4}";
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
            return FemSolver.GetNormalizedFreq(WaveLength, WaveguideWidth);
        }
    }
}
