using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Numerics; // Complex
using System.Text.RegularExpressions;
using MyUtilLib.Matrix;

namespace HPlaneWGSimulator
{
    /// <summary>
    /// 解析機
    /// </summary>
    class FemSolver
    {
        ////////////////////////////////////////////////////////////////////////
        // 定数
        ////////////////////////////////////////////////////////////////////////
        private static readonly double pi = 3.1416;
        private static readonly double c0 = 2.99792458e+8;
        private static readonly double myu0 = 4.0e-7 * pi;
        private static readonly double eps0 = 8.85418782e-12;//1.0 / (myu0 * c0 * c0);
        /// <summary>
        /// 考慮モード数
        /// </summary>
        private const int MaxModeCnt = Constants.MaxModeCount;
        /// <summary>
        /// 導波路の幅既定値  規格化周波数が定義できるように初期値を設定
        /// </summary>
        public const double DefWaveguideWidth = 1000.0; // ありえない幅

        ////////////////////////////////////////////////////////////////////////
        // 型
        ////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////
        // フィールド
        ////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 節点リスト
        /// </summary>
        private IList<FemNode> Nodes = new List<FemNode>();
        /// <summary>
        /// 要素リスト
        /// </summary>
        private IList<FemElement> Elements = new List<FemElement>();
        /// <summary>
        /// ポートリスト
        ///   各ポートのリスト要素は節点のリスト
        /// </summary>
        private IList<IList<int>> Ports = new List<IList<int>>();
        /// <summary>
        /// 強制境界節点リスト
        /// </summary>
        private IList<int> ForceBCNodes = new List<int>();
        /// <summary>
        /// 領域全体の強制境界節点番号ハッシュ
        /// </summary>
        private Dictionary<int, bool> ForceNodeNumberH = new Dictionary<int, bool>();
        /// <summary>
        /// 入射ポート番号
        /// </summary>
        private int IncidentPortNo = 1;
        /// <summary>
        /// 媒質リスト
        /// </summary>
        private MediaInfo[] Medias = new MediaInfo[Constants.MaxMediaCount];
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
        /// 辺と要素番号の対応マップ
        /// </summary>
        private Dictionary<string, IList<int>> EdgeToElementNoH = new Dictionary<string, IList<int>>();
        /// <summary>
        /// 導波路の幅
        /// </summary>
        private double WaveguideWidth;
        /// <summary>
        /// 計算中止された？
        /// </summary>
        public bool IsCalcAborted
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FemSolver()
        {
            init();
        }

        /// <summary>
        /// 入力データの初期化
        /// </summary>
        private void init()
        {
            IsCalcAborted = false;
            Nodes.Clear();
            Elements.Clear();
            foreach (IList<int> portNodes in Ports)
            {
                portNodes.Clear();
            }
            Ports.Clear();
            ForceBCNodes.Clear();
            ForceNodeNumberH.Clear();
            IncidentPortNo = 1;
            for (int i = 0; i < Medias.Length; i++)
            {
                MediaInfo media = new MediaInfo();
                media.BackColor = CadLogic.MediaBackColors[i];
                Medias[i] = media;
            }
            EdgeToElementNoH.Clear();
            //WaveguideWidth = 0.0;
            WaveguideWidth = DefWaveguideWidth;  // 規格化周波数が定義できるように初期値を設定
            FirstWaveLength = 0.0;
            LastWaveLength = 0.0;
            CalcFreqCnt = 0;
        }

        /// <summary>
        /// 入力データの初期化
        /// </summary>
        public void InitData()
        {
            init();
        }

        /// <summary>
        /// 入力データ読み込み
        /// </summary>
        /// <param name="filename"></param>
        public void Load(string filename)
        {
            // 入力データ初期化
            init();

            IList<FemNode> nodes = null;
            IList<FemElement> elements = null;
            IList<IList<int>> ports = null;
            IList<int> forceBCNodes = null;
            int incidentPortNo = 1;
            MediaInfo[] medias = null;
            double firstWaveLength = 0.0;
            double lastWaveLength = 0.0;
            int calcCnt = 0;
            bool ret = FemInputDatFile.LoadFromFile(filename, out nodes, out elements, out ports, out forceBCNodes, out incidentPortNo, out medias, out firstWaveLength, out lastWaveLength, out calcCnt);
            if (ret)
            {
                System.Diagnostics.Debug.Assert(medias.Length == Medias.Length);
                Nodes = nodes;
                Elements = elements;
                Ports = ports;
                ForceBCNodes = forceBCNodes;
                IncidentPortNo = incidentPortNo;
                Medias = medias;
                FirstWaveLength = firstWaveLength;
                LastWaveLength = lastWaveLength;
                CalcFreqCnt = calcCnt;

                // 強制境界節点番号ハッシュの作成(2D節点番号)
                foreach (int nodeNumber in ForceBCNodes)
                {
                    if (!ForceNodeNumberH.ContainsKey(nodeNumber))
                    {
                        ForceNodeNumberH[nodeNumber] = true;
                    }
                }

                // 辺と要素の対応マップ作成
                MkEdgeToElementNoH(Elements, ref EdgeToElementNoH);
                // 導波管幅の決定
                setupWaveguideWidth();

                if (CalcFreqCnt == 0)
                {
                    // 旧型式のデータの可能性があるので既定値をセットする（ファイル読み込みエラーにはなっていないので）
                    FirstWaveLength = GetWaveLengthFromNormalizedFreq(Constants.DefNormalizedFreqRange[0], WaveguideWidth);
                    LastWaveLength = GetWaveLengthFromNormalizedFreq(Constants.DefNormalizedFreqRange[1], WaveguideWidth);
                    CalcFreqCnt = Constants.DefCalcFreqencyPointCount;
                }
            }
        }

        /// <summary>
        /// 規格化周波数→波長変換
        /// </summary>
        /// <param name="normalizedFreq">規格化周波数</param>
        /// <param name="waveguideWidth">導波路の幅</param>
        /// <returns>波長</returns>
        public static double GetWaveLengthFromNormalizedFreq(double normalizedFreq, double waveguideWidth)
        {
            if (Math.Abs(normalizedFreq) < Constants.PrecisionLowerLimit)
            {
                return 0.0;
            }
            return 2.0 * waveguideWidth / normalizedFreq;
        }

        /// <summary>
        /// 波長→規格化周波数変換
        /// </summary>
        /// <param name="waveLength">波長</param>
        /// <param name="waveguideWidth">導波路の幅</param>
        /// <returns>規格化周波数</returns>
        public static double GetNormalizedFreq(double waveLength, double waveguideWidth)
        {
            if (Math.Abs(waveLength) < Constants.PrecisionLowerLimit)
            {
                return 0.0;
            }
            return 2.0 * waveguideWidth / waveLength;
        }

        /// <summary>
        /// 計算開始規格化周波数
        /// </summary>
        public double FirstNormalizedFreq
        {
            get
            {
                double normalizedFreq1 = (Math.Abs(FirstWaveLength) < Constants.PrecisionLowerLimit) ? 0.0 : GetNormalizedFreq(FirstWaveLength, WaveguideWidth);
                normalizedFreq1 = Math.Round(normalizedFreq1, 2);
                return normalizedFreq1;
            }
        }
        /// <summary>
        /// 計算終了規格化周波数
        /// </summary>
        public double LastNormalizedFreq
        {
            get
            {
                double normalizedFreq2 = (Math.Abs(LastWaveLength) < Constants.PrecisionLowerLimit) ? 0.0 : GetNormalizedFreq(LastWaveLength, WaveguideWidth);
                normalizedFreq2 = Math.Round(normalizedFreq2, 2);
                return normalizedFreq2;
            }
        }

        /// <summary>
        /// 計算対象周波数範囲を入力ファイルに書き込む
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="normalizedFreq1"></param>
        /// <param name="normalizedFreq2"></param>
        /// <param name="calcCnt"></param>
        public void UpdateFreqRangeToInputFile(string filename, double normalizedFreq1, double normalizedFreq2, int calcCnt)
        {
            // 計算対象周波数を波長に変換
            double firstWaveLength = GetWaveLengthFromNormalizedFreq(normalizedFreq1, WaveguideWidth);
            double lastWaveLength = GetWaveLengthFromNormalizedFreq(normalizedFreq2, WaveguideWidth);

            // セット
            FirstWaveLength = firstWaveLength;
            LastWaveLength = lastWaveLength;
            CalcFreqCnt = calcCnt;

            // FEM入力ファイルへ更新書き込み
            FemInputDatFile.UpdateToFile(filename, FirstWaveLength, LastWaveLength, CalcFreqCnt);
        }

        /// <summary>
        /// 辺と要素の対応マップ作成
        /// </summary>
        /// <param name="in_Elements"></param>
        /// <param name="out_EdgeToElementNoH"></param>
        public static void MkEdgeToElementNoH(IList<FemElement> in_Elements, ref Dictionary<string, IList<int>> out_EdgeToElementNoH)
        {
            // 辺と要素の対応マップ作成
            // 2次三角形要素の辺の始点、終点ローカル番号
            int[][] edgeStEdNoList = new int[Constants.TriEdgeCnt_SecondOrder][]
                        {
                            new int[]{1, 4},
                            new int[]{4, 2},
                            new int[]{2, 5},
                            new int[]{5, 3},
                            new int[]{3, 6},
                            new int[]{6, 1}
                        };
            out_EdgeToElementNoH.Clear();
            foreach (FemElement element in in_Elements)
            {
                foreach (int[] edgeStEdNo in edgeStEdNoList)
                {
                    int stNodeNumber = element.NodeNumbers[edgeStEdNo[0] - 1];
                    int edNodeNumber = element.NodeNumbers[edgeStEdNo[1] - 1];
                    string edgeKey = "";
                    if (stNodeNumber < edNodeNumber)
                    {
                        edgeKey = string.Format("{0}_{1}", stNodeNumber, edNodeNumber);
                    }
                    else
                    {
                        edgeKey = string.Format("{0}_{1}", edNodeNumber, stNodeNumber);
                    }
                    if (out_EdgeToElementNoH.ContainsKey(edgeKey))
                    {
                        out_EdgeToElementNoH[edgeKey].Add(element.No);
                    }
                    else
                    {
                        out_EdgeToElementNoH[edgeKey] = new List<int>();
                        out_EdgeToElementNoH[edgeKey].Add(element.No);
                    }
                }
            }
        }

        /// <summary>
        /// 節点と要素番号のマップ作成
        /// </summary>
        /// <param name="in_Elements"></param>
        /// <param name="out_NodeToElementNoH"></param>
        public static void MkNodeToElementNoH(IList<FemElement> in_Elements, ref Dictionary<int, IList<int>> out_NodeToElementNoH)
        {
            //check
            for (int ieleno = 0; ieleno < in_Elements.Count; ieleno++)
            {
                System.Diagnostics.Debug.Assert(in_Elements[ieleno].No == ieleno + 1);
            }
            // 節点と要素番号のマップ作成
            foreach (FemElement element in in_Elements)
            {
                foreach (int nodeNumber in element.NodeNumbers)
                {
                    if (out_NodeToElementNoH.ContainsKey(nodeNumber))
                    {
                        out_NodeToElementNoH[nodeNumber].Add(element.No);
                    }
                    else
                    {
                        out_NodeToElementNoH[nodeNumber] = new List<int>();
                        out_NodeToElementNoH[nodeNumber].Add(element.No);
                    }
                }
            }
        }

        /// <summary>
        /// 導波管幅の決定
        /// </summary>
        private void setupWaveguideWidth()
        {
            if (Ports.Count == 0)
            {
                WaveguideWidth = DefWaveguideWidth; // 規格化周波数が定義できるように初期値を設定
                return;
            }
            // ポート1の導波管幅
            int port1NodeNumber1 = Ports[0][0];
            int port1NodeNumber2 = Ports[0][Ports[0].Count - 1];
            double w1 = getDistance(Nodes[port1NodeNumber1 - 1].Coord, Nodes[port1NodeNumber2 - 1].Coord);

            WaveguideWidth = w1;
            Console.WriteLine("WaveguideWidth:{0}", w1);
        }

        /// <summary>
        /// 点が要素内に含まれる？
        /// </summary>
        /// <param name="element">要素</param>
        /// <param name="test_pp">テストする点</param>
        /// <param name="Nodes">節点リスト（要素は座標を持たないので節点リストが必要）</param>
        /// <returns></returns>
        public static bool IsPointInElement(FemElement element, double[] test_pp, IList<FemNode> nodes)
        {
            bool hit = false;

            // 三角形の頂点数
            const int vertexCnt = Constants.TriVertexCnt;
            double[][] pps = new double[vertexCnt][];
            // 2次三角形要素の最初の３点＝頂点の座標を取得
            for (int ino = 0; ino < vertexCnt; ino++)
            {
                int nodeNumber = element.NodeNumbers[ino];
                FemNode node = nodes[nodeNumber - 1];
                System.Diagnostics.Debug.Assert(node.No == nodeNumber);
                pps[ino] = node.Coord;
            }
            // 頂点？
            foreach (double[] pp in pps)
            {
                if (Math.Abs(pp[0] - test_pp[0]) < Constants.PrecisionLowerLimit && Math.Abs(pp[1] - test_pp[1]) < Constants.PrecisionLowerLimit)
                {
                    hit = true;
                    break;
                }
            }
            if (!hit)
            {
                // 面積から内部判定する
                double area = KerEMatTri.TriArea(pps[0], pps[1], pps[2]);
                double sumOfSubArea = 0.0;
                for (int ino = 0; ino < vertexCnt; ino++)
                {
                    double[][] subArea_pp = new double[vertexCnt][];
                    subArea_pp[0] = pps[ino];
                    subArea_pp[1] = pps[(ino + 1)% vertexCnt];
                    subArea_pp[2] = test_pp;
                    //foreach (double[] work_pp in subArea_pp)
                    //{
                    //    Console.Write("{0},{1}  ", work_pp[0], work_pp[1]);
                    //}
                    double subArea = KerEMatTri.TriArea(subArea_pp[0], subArea_pp[1], subArea_pp[2]);
                    //Console.Write("  subArea = {0}", subArea);
                    //Console.WriteLine();
                    if (subArea <= 0.0)
                    {
                        sumOfSubArea = 0.0;
                        break;
                        // 外側？
                    }
                    sumOfSubArea += Math.Abs(subArea);
                }
                if (Math.Abs(area - sumOfSubArea) < Constants.PrecisionLowerLimit)
                {
                    hit = true;
                }
            }
            return hit;
        }

        /// <summary>
        /// Fem入力データの取得
        /// </summary>
        /// <param name="outNodes">節点リスト</param>
        /// <param name="outElements">要素リスト</param>
        /// <param name="outPorts">ポート節点番号リストのリスト</param>
        /// <param name="outForceNodes">強制境界節点番号リスト</param>
        /// <param name="outIncidentPortNo">入射ポート番号</param>
        /// <param name="outWaveguideWidth">導波路の幅</param>
        public void GetFemInputInfo(
            out FemNode[] outNodes, out FemElement[] outElements,
            out IList<int[]> outPorts,
            out int[] outForceNodes,
            out int outIncidentPortNo, out double outWaveguideWidth)
        {
            outNodes = null;
            outElements = null;
            outPorts = null;
            outForceNodes = null;
            outIncidentPortNo = 1;
            outWaveguideWidth = DefWaveguideWidth;

            if (!isInputDataValid())
            {
                return;
            }

            int nodeCnt = Nodes.Count;
            outNodes = new FemNode[nodeCnt];
            for (int i = 0; i < nodeCnt; i++)
            {
                FemNode femNode = new FemNode();
                femNode.CP(Nodes[i]);
                outNodes[i] = femNode;
            }
            int elementCnt = Elements.Count;
            outElements = new FemElement[elementCnt];
            for (int i = 0; i < elementCnt; i++)
            {
                FemElement femElement = new FemElement();
                femElement.CP(Elements[i]);
                outElements[i] = femElement;
            }
            int portCnt = Ports.Count;
            outPorts = new List<int[]>();
            foreach (IList<int> portNodes in Ports)
            {
                int[] outPortNodes = new int[portNodes.Count];
                for (int inoB = 0; inoB < portNodes.Count; inoB++)
                {
                    outPortNodes[inoB] = portNodes[inoB];
                }
                outPorts.Add(outPortNodes);
            }
            outForceNodes = new int[ForceBCNodes.Count];
            for (int i = 0; i < ForceBCNodes.Count; i++)
            {
                outForceNodes[i] = ForceBCNodes[i];
            }
            outIncidentPortNo = IncidentPortNo;

            outWaveguideWidth = WaveguideWidth;
        }
        
        /// <summary>
        /// 入力データ妥当？(解析開始前にメッセージを表示する)
        /// </summary>
        /// <returns></returns>
        public bool ChkInputData()
        {
            return isInputDataValid(true);
        }

        /// <summary>
        /// 入力データ妥当？
        /// </summary>
        /// <returns></returns>
        private bool isInputDataValid(bool showMessageFlg = false)
        {
            bool valid = false;
            if (Nodes.Count == 0)
            {
                if (showMessageFlg)
                {
                    MessageBox.Show("節点がありません", "計算できません", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return valid;
            }
            if (Elements.Count == 0)
            {
                if (showMessageFlg)
                {
                    MessageBox.Show("要素がありません", "計算できません", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return valid;
            }
            if (ForceBCNodes.Count == 0)
            {
                if (showMessageFlg)
                {
                    MessageBox.Show("強制境界条件がありません", "計算できません", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                // Note: H面導波路対象なので強制境界はあるはず
                return valid;
            }
            if (Ports.Count == 0 || Ports[0].Count == 0)
            {
                if (showMessageFlg)
                {
                    MessageBox.Show("入出力ポートがありません", "計算できません", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return valid;
            }
            if (Math.Abs(WaveguideWidth - DefWaveguideWidth) < Constants.PrecisionLowerLimit)
            {
                if (showMessageFlg)
                {
                    MessageBox.Show("入力ポートがありません", "計算できません", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return valid;
            }
            if (CalcFreqCnt == 0)
            {
                if (showMessageFlg)
                {
                    MessageBox.Show("計算間隔が未設定です", "計算できません", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return valid;
            }
            if (LastNormalizedFreq <= FirstNormalizedFreq)
            {
                if (showMessageFlg)
                {
                    MessageBox.Show("計算範囲が不正です", "計算できません", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return valid;
            }

            valid = true;
            return valid;
        }

        /// <summary>
        /// 計算実行
        /// </summary>
        public void Run(string filename, object eachDoneCallbackObj, Delegate eachDoneCallback)
        {
            IsCalcAborted = false;
            if (!isInputDataValid())
            {
                return;
            }
            string basefilename = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(filename);
            string outfilename = basefilename + Constants.FemOutputExt;
            string indexfilename = basefilename + Constants.FemOutputIndexExt;

            // 結果出力ファイルの削除(結果を追記モードで書き込むため)
            if (File.Exists(outfilename))
            {
                File.Delete(outfilename);
            }
            if (File.Exists(indexfilename))
            {
                File.Delete(indexfilename);
            }

            int calcFreqCnt = CalcFreqCnt;
            double firstNormalizedFreq = FirstNormalizedFreq;
            double lastNormalizedFreq = LastNormalizedFreq;
            int maxMode = MaxModeCnt;
            double deltaf = (lastNormalizedFreq - firstNormalizedFreq) / calcFreqCnt;
            // 始点と終点も計算するように変更
            for (int freqIndex = 0; freqIndex < calcFreqCnt + 1; freqIndex++)
            {
                double normalizedFreq = firstNormalizedFreq + freqIndex * deltaf;
                if (normalizedFreq < Constants.PrecisionLowerLimit)
                {
                    normalizedFreq = 1.0e-4;
                }
                double waveLength = GetWaveLengthFromNormalizedFreq(normalizedFreq, WaveguideWidth);
                Console.WriteLine("2w/lamda = {0}", normalizedFreq);
                int freqNo = freqIndex + 1;
                runEach(freqNo, outfilename, waveLength, maxMode);
                eachDoneCallback.Method.Invoke(eachDoneCallbackObj, new object[]{new object[]{}, });
                if (IsCalcAborted)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 各波長について計算実行
        /// </summary>
        /// <param name="freqNo">計算する周波数に対応する番号(1,...,CalcFreqCnt - 1)</param>
        /// <param name="filename">出力ファイル</param>
        /// <param name="waveLength">波長</param>
        private void runEach(int freqNo, string filename, double waveLength, int maxMode)
        {
            // 全体剛性行列作成
            int[] nodesRegion = null;
            MyComplexMatrix mat = null;
            getHelmholtzLinearSystemMatrix(waveLength, out nodesRegion, out mat);

            // 残差ベクトル初期化
            int nodeCnt = nodesRegion.Length;
            Complex[] resVec = new Complex[nodeCnt];
            for (int i = 0; i < nodeCnt; i++)
            {
                resVec[i] = new Complex();
            }

            // 開口面境界条件の適用
            int portCnt = Ports.Count;
            IList<int[]> nodesBoundaryList = new List<int[]>();
            IList<MyDoubleMatrix> ryy_1dList = new List<MyDoubleMatrix>();
            IList<Complex[]> eigenValuesList = new List<Complex[]>();
            IList<Complex[,]> eigenVecsList = new List<Complex[,]>();
            for (int i = 0; i < portCnt; i++)
            {
                int portNo = i + 1;
                int[] nodesBoundary;
                MyDoubleMatrix ryy_1d;
                Complex[] eigenValues;
                Complex[,] eigenVecs;

                // ポート固有値解析
                solvePortWaveguideEigen(waveLength, portNo, maxMode, out nodesBoundary, out ryy_1d, out eigenValues, out eigenVecs);
                nodesBoundaryList.Add(nodesBoundary);
                ryy_1dList.Add(ryy_1d);
                eigenValuesList.Add(eigenValues);
                eigenVecsList.Add(eigenVecs);

                // 入射ポートの判定
                bool isInputPort = (i == (IncidentPortNo - 1));
                // 境界条件をリニア方程式に追加
                addPortBC(waveLength, isInputPort, nodesBoundary, ryy_1d, eigenValues, eigenVecs, nodesRegion, mat, resVec);
            }

            // リニア方程式を解く
            ValueType[] X = null;
            // clapackの行列の1次元ベクトルへの変換は列を先に埋める
            ValueType[] A = MyMatrixUtil.matrix_ToBuffer(mat, false);
            ValueType[] B = new ValueType[nodeCnt];
            for (int ino = 0; ino < nodeCnt; ino++)
            {
                B[ino] = resVec[ino];
            }
            ///////////////////////
            // TEST
            MyMatrixUtil.compressVec(ref A); // 配列圧縮
            mat._body = null;
            mat = null;
            resVec = null;
            try
            {
                Console.WriteLine("TotalMemory: {0}", GC.GetTotalMemory(false));
                // GC.Collect 呼び出し後に GC.WaitForPendingFinalizers を呼び出します。これにより、すべてのオブジェクトに対するファイナライザが呼び出されるまで、現在のスレッドは待機します。
                // ファイナライザ作動後は、回収すべき、(ファイナライズされたばかりの) アクセス不可能なオブジェクトが増えます。もう1度 GC.Collect を呼び出し、それらを回収します。
                GC.Collect(); // アクセス不可能なオブジェクトを除去
                GC.WaitForPendingFinalizers(); // ファイナライゼーションが終わるまでスレッド待機
                GC.Collect(0); // ファイナライズされたばかりのオブジェクトに関連するメモリを開放
                Console.WriteLine("TotalMemory: {0}", GC.GetTotalMemory(false));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
                MessageBox.Show(exception.Message);
            }
            ///////////////////////
            int x_row = nodeCnt;
            int x_col = 1;
            int a_row = nodeCnt;
            int a_col = nodeCnt;
            int b_row = nodeCnt;
            int b_col = 1;
            Console.WriteLine("run zgesv");
            try
            {
                //KrdLab.clapack.Function.zgesv(ref X, ref x_row, ref x_col, A, a_row, a_col, B, b_row, b_col);
                KrdLab.clapack.Function.zgesv(ref X, ref x_row, ref x_col, A, a_row, a_col, B, b_row, b_col, true);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
                MessageBox.Show(exception.Message);
                MessageBox.Show(string.Format("計算中にエラーが発生しました。2W/λ = {0}", GetNormalizedFreq(waveLength, WaveguideWidth)));
                return;
            }
            Complex[] valuesAll = new Complex[nodeCnt];
            for (int ino = 0; ino < nodeCnt; ino++)
            {
                Complex c = (Complex)X[ino];
                //Console.WriteLine("({0})  {1} + {2}i", ino, c.Real, c.Imaginary);
                valuesAll[ino] = c;
            }

            // 散乱行列Sij
            // ポートj = IncidentPortNoからの入射のみ対応
            Complex[] scatterVec = new Complex[Ports.Count];
            for (int portIndex = 0; portIndex < Ports.Count; portIndex++)
            {
                int iMode = 0;
                bool isIncidentMode = (portIndex == IncidentPortNo - 1);
                int[] nodesBoundary = nodesBoundaryList[portIndex];
                MyDoubleMatrix ryy_1d = ryy_1dList[portIndex];
                Complex[] eigenValues = eigenValuesList[portIndex];
                Complex[,] eigenVecs = eigenVecsList[portIndex];
                Complex si1 = getWaveguidePortReflectionCoef(waveLength, iMode, isIncidentMode,
                                                             nodesBoundary, ryy_1d, eigenValues, eigenVecs,
                                                             nodesRegion, valuesAll);
                Console.WriteLine("s{0}{1} = {2} + {3}i (|S| = {4} |S|^2 = {5})", portIndex + 1, IncidentPortNo, si1.Real, si1.Imaginary, Complex.Abs(si1), Complex.Abs(si1) * Complex.Abs(si1));
                scatterVec[portIndex] = si1;
            }

            /////////////////////////////////////
            // 結果をファイルに出力
            ////////////////////////////////////
            FemOutputDatFile.AppendToFile(
                filename, freqNo, waveLength, maxMode,
                Ports.Count, IncidentPortNo,
                nodesBoundaryList, eigenValuesList, eigenVecsList,
                nodesRegion, valuesAll,
                scatterVec);
        }

        /// <summary>
        /// ヘルムホルツ方程式の剛性行列を作成する
        /// </summary>
        /// <param name="waveLength"></param>
        /// <param name="nodesRegion"></param>
        /// <param name="mat"></param>
        private void getHelmholtzLinearSystemMatrix(double waveLength, out int[] nodesRegion, out MyComplexMatrix mat)
        {
            nodesRegion = null;
            mat = null;

            // 2D節点番号リスト（ソート済み）
            IList<int> sortedNodes = new List<int>();
            // 2D節点番号→ソート済みリストインデックスのマップ
            Dictionary<int, int> toSorted = new Dictionary<int, int>();

            // 強制境界節点と内部領域節点を分離
            foreach (FemNode node in Nodes)
            {
                int nodeNumber = node.No;
                if (ForceNodeNumberH.ContainsKey(nodeNumber))
                {
//                    forceNodes.Add(nodeNumber);
                }
                else
                {
                    sortedNodes.Add(nodeNumber);
                }
            }
            // 2D節点番号→ソート済みリストインデックスのマップ作成
            for (int i = 0; i < sortedNodes.Count; i++)
            {
                int nodeNumber = sortedNodes[i];
                if (!toSorted.ContainsKey(nodeNumber))
                {
                    toSorted.Add(nodeNumber, i);
                }
            }

            // 総節点数
            int nodeCnt = sortedNodes.Count;

            // 全体節点を配列に格納
            nodesRegion = sortedNodes.ToArray();

            // 全体剛性行列初期化
            mat = new MyComplexMatrix(nodeCnt, nodeCnt);
            for (int i = 0; i < nodeCnt; i++)
            {
                for (int j = 0; j < nodeCnt; j++)
                {
                    mat[i, j] = new Complex();
                }
            }
            foreach (FemElement element in Elements)
            {
                addElementMat(waveLength, toSorted, element, ref mat);
            }
        }

        /// <summary>
        /// ヘルムホルツ方程式に対する有限要素マトリクス作成
        /// </summary>
        /// <param name="waveLength">波長</param>
        /// <param name="toSorted">ソートされた節点インデックス（ 2D節点番号→ソート済みリストインデックスのマップ）</param>
        /// <param name="element">有限要素</param>
        /// <param name="mat">マージされる全体行列</param>
        private void addElementMat(double waveLength, Dictionary<int, int> toSorted, FemElement element, ref MyComplexMatrix mat)
        {
            // 波数
            double k0 = 2.0 * pi / waveLength;
            // 角周波数
            double omega = k0 * c0;

            // 要素頂点数
            const int vertexCnt = Constants.TriVertexCnt; //3;
            // 要素内節点数
            const int nno = Constants.TriNodeCnt_SecondOrder; //6;  // 2次三角形要素
            // 座標次元数
            const int ndim = Constants.CoordDim2D; //2;

            int[] nodeNumbers = element.NodeNumbers;
            int[] no_c = new int[nno];
            MediaInfo media = Medias[element.MediaIndex];  // ver1.1.0.0 媒質情報の取得
            double[,] media_P = media.P;
            media_P = MyMatrixUtil.matrix_Inverse(media_P);
            double[,] media_Q = media.Q;
            // 節点座標(IFの都合上配列の配列形式の2次元配列を作成)
            double[][] pp = new double[nno][];
            for (int ino = 0; ino < nno; ino++)
            {
                int nodeNumber = nodeNumbers[ino];
                int nodeIndex = nodeNumber - 1;
                FemNode node = Nodes[nodeIndex];

                no_c[ino] = nodeNumber;
                pp[ino] = new double[ndim];
                for (int n = 0; n < ndim; n++)
                {
                    pp[ino][n] = node.Coord[n];
                }
            }
            // 面積を求める
            double area = KerEMatTri.TriArea(pp[0], pp[1], pp[2]);
            //Console.WriteLine("Elem No {0} area:  {1}", element.No, area);
            System.Diagnostics.Debug.Assert(area >= 0.0);

            // 面積座標の微分を求める
            //   dldx[k, n] k面積座標Lkのn方向微分
            double[,] dldx = null;
            double[] const_term = null;
            KerEMatTri.TriDlDx(out dldx, out const_term, pp[0], pp[1], pp[2]);

            // 形状関数の微分の係数を求める
            //    dndxC[ino,n,k]  ino節点のn方向微分のLk(k面積座標)の係数
            //       dNino/dn = dndxC[ino, n, 0] * L0 + dndxC[ino, n, 1] * L1 + dndxC[ino, n, 2] * L2 + dndxC[ino, n, 3]
            double[, ,] dndxC = new double[nno, ndim, vertexCnt + 1]
                {
                    {
                        {4.0 * dldx[0, 0], 0.0, 0.0, -1.0 * dldx[0, 0]},
                        {4.0 * dldx[0, 1], 0.0, 0.0, -1.0 * dldx[0, 1]},
                    },
                    {
                        {0.0, 4.0 * dldx[1, 0], 0.0, -1.0 * dldx[1, 0]},
                        {0.0, 4.0 * dldx[1, 1], 0.0, -1.0 * dldx[1, 1]},
                    },
                    {
                        {0.0, 0.0, 4.0 * dldx[2, 0], -1.0 * dldx[2, 0]},
                        {0.0, 0.0, 4.0 * dldx[2, 1], -1.0 * dldx[2, 1]},
                    },
                    {
                        {4.0 * dldx[1, 0], 4.0 * dldx[0, 0], 0.0, 0.0},
                        {4.0 * dldx[1, 1], 4.0 * dldx[0, 1], 0.0, 0.0},
                    },
                    {
                        {0.0, 4.0 * dldx[2, 0], 4.0 * dldx[1, 0], 0.0},
                        {0.0, 4.0 * dldx[2, 1], 4.0 * dldx[1, 1], 0.0},
                    },
                    {
                        {4.0 * dldx[2, 0], 0.0, 4.0 * dldx[0, 0], 0.0},
                        {4.0 * dldx[2, 1], 0.0, 4.0 * dldx[0, 1], 0.0},
                    },
                };

            // ∫dN/dndN/dn dxdy
            //     integralDNDX[n, ino, jno]  n = 0 --> ∫dN/dxdN/dx dxdy
            //                                n = 1 --> ∫dN/dydN/dy dxdy
            double[, ,] integralDNDX = new double[ndim, nno, nno];
            for (int n = 0; n < ndim; n++)
            {
                for (int ino = 0; ino < nno; ino++)
                {
                    for (int jno = 0; jno < nno; jno++)
                    {
                        integralDNDX[n, ino, jno]
                            = area / 6.0 * (dndxC[ino, n, 0] * dndxC[jno, n, 0] + dndxC[ino, n, 1] * dndxC[jno, n, 1] + dndxC[ino, n, 2] * dndxC[jno, n, 2])
                                  + area / 12.0 * (dndxC[ino, n, 0] * dndxC[jno, n, 1] + dndxC[ino, n, 0] * dndxC[jno, n, 2]
                                                      + dndxC[ino, n, 1] * dndxC[jno, n, 0] + dndxC[ino, n, 1] * dndxC[jno, n, 2]
                                                      + dndxC[ino, n, 2] * dndxC[jno, n, 0] + dndxC[ino, n, 2] * dndxC[jno, n, 1])
                                  + area / 3.0 * (dndxC[ino, n, 0] * dndxC[jno, n, 3] + dndxC[ino, n, 1] * dndxC[jno, n, 3]
                                                      + dndxC[ino, n, 2] * dndxC[jno, n, 3]
                                                      + dndxC[ino, n, 3] * dndxC[jno, n, 0] + dndxC[ino, n, 3] * dndxC[jno, n, 1]
                                                      + dndxC[ino, n, 3] * dndxC[jno, n, 2])
                                  + area * dndxC[ino, n, 3] * dndxC[jno, n, 3];
                    }
                }
            }
            // ∫N N dxdy
            double[,] integralN = new double[nno, nno]
                {
                    {  6.0 * area / 180.0, -1.0 * area / 180.0, -1.0 * area / 180.0,                 0.0, -4.0 * area / 180.0,                 0.0},
                    { -1.0 * area / 180.0,  6.0 * area / 180.0, -1.0 * area / 180.0,                 0.0,                 0.0, -4.0 * area / 180.0},
                    { -1.0 * area / 180.0, -1.0 * area / 180.0,  6.0 * area / 180.0, -4.0 * area / 180.0,                 0.0,                 0.0},
                    {                 0.0,                 0.0, -4.0 * area / 180.0, 32.0 * area / 180.0, 16.0 * area / 180.0, 16.0 * area / 180.0},
                    { -4.0 * area / 180.0,                 0.0,                 0.0, 16.0 * area / 180.0, 32.0 * area / 180.0, 16.0 * area / 180.0},
                    {                 0.0, -4.0 * area / 180.0,                 0.0, 16.0 * area / 180.0, 16.0 * area / 180.0, 32.0 * area / 180.0},
                };

            // 要素剛性行列を作る
            Complex[,] emat = new Complex[nno, nno];
            for (int ino = 0; ino < nno; ino++)
            {
                for (int jno = 0; jno < nno; jno++)
                {
                    emat[ino, jno] = media_P[0, 0] * integralDNDX[1, ino, jno] + media_P[1, 1] * integralDNDX[0, ino, jno]
                                         - k0 * k0 * media_Q[2, 2] * integralN[ino, jno];
                }
            }

            // 要素剛性行列にマージする
            for (int ino = 0; ino < nno; ino++)
            {
                int iNodeNumber = no_c[ino];
                if (ForceNodeNumberH.ContainsKey(iNodeNumber)) continue;
                int inoGlobal = toSorted[iNodeNumber];
                for (int jno = 0; jno < nno; jno++)
                {
                    int jNodeNumber = no_c[jno];
                    if (ForceNodeNumberH.ContainsKey(jNodeNumber)) continue;
                    int jnoGlobal = toSorted[jNodeNumber];

                    mat[inoGlobal, jnoGlobal] += emat[ino, jno];
                }
            }
        }

        /// <summary>
        /// 入出力ポート境界条件の追加
        /// </summary>
        /// <param name="waveLength"></param>
        /// <param name="isInputPort"></param>
        /// <param name="nodesBoundary"></param>
        /// <param name="ryy_1d"></param>
        /// <param name="eigenValues"></param>
        /// <param name="eigenVecs"></param>
        /// <param name="nodesRegion"></param>
        /// <param name="mat"></param>
        /// <param name="resVec"></param>
        private void addPortBC(double waveLength, bool isInputPort, int[] nodesBoundary, MyDoubleMatrix ryy_1d, Complex[] eigenValues, Complex[,] eigenVecs, int[] nodesRegion, MyComplexMatrix mat, Complex[] resVec)
        {
            double k0 = 2.0 * pi / waveLength;
            double omega = k0 / Math.Sqrt(myu0 * eps0);

            // 境界上の節点数(1次線要素を想定)
            int nodeCnt = nodesBoundary.Length;
            // 考慮するモード数
            int maxMode = eigenValues.Length;

            // 全体剛性行列の作成
            MyComplexMatrix matB = new MyComplexMatrix(nodeCnt, nodeCnt);
            for (int inoB = 0; inoB < nodeCnt; inoB++)
            {
                for (int jnoB = 0; jnoB < nodeCnt; jnoB++)
                {
                    matB[inoB, jnoB] = new Complex();
                }
            }
            for (int imode = 0; imode < maxMode; imode++)
            {
                Complex betam = eigenValues[imode];

                Complex[] fmVec = MyMatrixUtil.matrix_GetRowVec(eigenVecs, (int)imode);
                Complex[] veci = MyMatrixUtil.product(ryy_1d, fmVec);
                Complex[] vecj = MyMatrixUtil.product(ryy_1d, MyMatrixUtil.vector_Conjugate(fmVec));
                for (int inoB = 0; inoB < nodeCnt; inoB++)
                {
                    for (int jnoB = 0; jnoB < nodeCnt; jnoB++)
                    {
                        matB[inoB, jnoB] += ((new Complex(0.0, 1.0)) / (omega * myu0)) * betam * Complex.Abs(betam) * veci[inoB] * vecj[jnoB];
                    }
                }
            }
            //MyMatrixUtil.printMatrix("matB", matB);

            // 残差ベクトルの作成
            Complex[] resVecB = new Complex[nodeCnt];
            for (int inoB = 0; inoB < nodeCnt; inoB++)
            {
                resVecB[inoB] = 0.0;
            }
            if (isInputPort)
            {
                int imode = 0;
                Complex betam = eigenValues[imode];
                Complex[] fmVec = MyMatrixUtil.matrix_GetRowVec(eigenVecs, (int)imode);
                Complex[] veci = MyMatrixUtil.product(ryy_1d, fmVec);
                for (int inoB = 0; inoB < nodeCnt; inoB++)
                {
                    resVecB[inoB] = 2.0 * (new Complex(0.0, 1.0)) * betam * veci[inoB];
                }
            }
            //printVec("resVecB", resVecB);

            // 2D節点番号→ソート済みリストインデックスのマップ
            Dictionary<int, int> toSorted = new Dictionary<int, int>();
            // 2D節点番号→ソート済みリストインデックスのマップ作成
            for (int i = 0; i < nodesRegion.Length; i++)
            {
                int nodeNumber = nodesRegion[i];
                if (!toSorted.ContainsKey(nodeNumber))
                {
                    toSorted.Add(nodeNumber, i);
                }
            }

            // 要素剛性行列にマージ
            //   この定式化では行列のスパース性は失われている(隣接していない要素の節点間にも関連がある)
            // 要素剛性行列にマージする
            for (int inoB = 0; inoB < nodeCnt; inoB++)
            {
                int iNodeNumber = nodesBoundary[inoB];
                if (ForceNodeNumberH.ContainsKey(iNodeNumber)) continue;
                int inoGlobal = toSorted[iNodeNumber];
                for (int jnoB = 0; jnoB < nodeCnt; jnoB++)
                {
                    int jNodeNumber = nodesBoundary[jnoB];
                    if (ForceNodeNumberH.ContainsKey(jNodeNumber)) continue;
                    int jnoGlobal = toSorted[jNodeNumber];

                    mat[inoGlobal, jnoGlobal] += matB[inoB, jnoB];
                }
            }

            // 残差ベクトルにマージ
            for (int inoB = 0; inoB < nodeCnt; inoB++)
            {
                int iNodeNumber = nodesBoundary[inoB];
                if (ForceNodeNumberH.ContainsKey(iNodeNumber)) continue;
                int inoGlobal = toSorted[iNodeNumber];

                resVec[inoGlobal] += resVecB[inoB];
            }

        }

        /// <summary>
        /// 散乱行列の計算
        /// </summary>
        /// <param name="waveLength"></param>
        /// <param name="iMode"></param>
        /// <param name="isIncidentMode"></param>
        /// <param name="nodesBoundary"></param>
        /// <param name="ryy_1d"></param>
        /// <param name="eigenValues"></param>
        /// <param name="eigenVecs"></param>
        /// <param name="nodesRegion"></param>
        /// <param name="valuesAll"></param>
        /// <returns></returns>
        private Complex getWaveguidePortReflectionCoef(double waveLength, int iMode, bool isIncidentMode,
                                                    int[] nodesBoundary, MyDoubleMatrix ryy_1d, Complex[] eigenValues, Complex[,] eigenVecs,
                                                    int[] nodesRegion, Complex[] valuesAll)
        {
            // 2D節点番号→ソート済みリストインデックスのマップ
            Dictionary<int, int> toSorted = new Dictionary<int, int>();
            // 2D節点番号→ソート済みリストインデックスのマップ作成
            for (int i = 0; i < nodesRegion.Length; i++)
            {
                int nodeNumber = nodesRegion[i];
                if (!toSorted.ContainsKey(nodeNumber))
                {
                    toSorted.Add(nodeNumber, i);
                }
            }

            // ポート上の界を取得する
            int nodeCnt = nodesBoundary.Length;
            Complex[] valuesB = new Complex[nodeCnt];
            for (int ino = 0; ino < nodeCnt; ino++)
            {
                int nodeNumber = nodesBoundary[ino];
                int inoGlobal = toSorted[nodeNumber];
                valuesB[ino] = valuesAll[inoGlobal];
            }

            double k0 = 2.0 * pi / waveLength;
            double omega = k0 * c0;

            Complex s11 = new Complex(0.0, 0.0);

            int maxMode = eigenValues.Length;

            Complex[] tmp_vec = new Complex[nodeCnt];
            // {tmp_vec}t = {fm}t[uzz]
            // {tmp_vec} = [uzz]t {fm}
            //   [uzz]t = [uzz]
            for (int ino_boundary = 0; ino_boundary < nodeCnt; ino_boundary++)
            {
                Complex sum = new Complex(0.0, 0.0);
                for (int k_tmp = 0; k_tmp < nodeCnt; k_tmp++)
                {
                    Complex fm = eigenVecs[iMode, k_tmp];
                    //Console.WriteLine( "(" + fm.Real + "," + fm.Imag + ")" + Complex.Norm(fm));
                    sum += ryy_1d[ino_boundary, k_tmp] * Complex.Conjugate(fm);
                }
                tmp_vec[ino_boundary] = sum;
            }
            // s11 = {tmp_vec}t {value_all}
            for (int ino_boundary = 0; ino_boundary < nodeCnt; ino_boundary++)
            {
                s11 += tmp_vec[ino_boundary] * valuesB[ino_boundary];
                //Console.WriteLine(nodesBoundary[ino_boundary] + " " + "(" + valuesB[ino_boundary].Real + ", " + valuesB[ino_boundary].Imaginary + ") " + Complex.Abs(valuesB[ino_boundary]));
                //Complex fm = eigen_vecs[imode, ino_boundary];
                //Console.WriteLine( no_c_all[ino_boundary] + " " + "(" + fm.Real + "," + fm.Imag + ")" + Complex.Norm(fm));
            }
            Complex betam = eigenValues[iMode];
            s11 *= (Complex.Abs(betam) / (omega * myu0));

            //Console.WriteLine("field impedance:" + omega*myu0/Complex.Norm(betam));
            //Console.WriteLine("beta:" + Complex.Norm(betam));
            if (isIncidentMode)
            {
                s11 += -1.0;
            }
            return s11;
        }

        /// <summary>
        /// ポート固有値解析
        /// </summary>
        private void solvePortWaveguideEigen(double waveLength, int portNo, int maxMode, out int[] nodesBoundary, out MyDoubleMatrix ryy_1d, out Complex[] eigenValues, out Complex[,] eigenVecs)
        {
            //Console.WriteLine("solvePortWaveguideEigen: {0},{1}", waveLength, portNo);
            nodesBoundary = null;
            ryy_1d = null;
            eigenValues = null;
            eigenVecs = null;

            // 2D次元数
            const int ndim2d = Constants.CoordDim2D; //2;
            // 要素内節点数
            const int nno = Constants.LineNodeCnt_SecondOrder; //3; // 2次線要素
            // 波数
            double k0 = 2.0 * pi / waveLength;
            // 角周波数
            double omega = k0 * c0;
            // 節点番号リスト(要素インデックス: 1D節点番号 - 1 要素:2D節点番号)
            IList<int> nodes = Ports[portNo - 1];
            // 2D→1D節点番号マップ
            Dictionary<int, int> to1dNodes = new Dictionary<int, int>();
            // 節点座標リスト
            IList<double> coords = new List<double>();
            // 要素リスト
            IList<FemElement> elements = new List<FemElement>();
            // 1D節点番号リスト（ソート済み）
            IList<int> sortedNodes = new List<int>();
            // 1D節点番号→ソート済みリストインデックスのマップ
            Dictionary<int, int> toSorted = new Dictionary<int, int>();

            // 2D→1D節点番号マップ作成
            for (int i = 0; i < nodes.Count; i++)
            {
                int nodeNumber2d = nodes[i];
                if (!to1dNodes.ContainsKey(nodeNumber2d))
                {
                    to1dNodes.Add(nodeNumber2d, i + 1);
                }
            }
            // 原点
            int nodeNumber0 = nodes[0];
            int nodeIndex0 = nodeNumber0 - 1;
            FemNode node0 = Nodes[nodeIndex0];
            double[] coord0 = new double[ndim2d];
            coord0[0] = node0.Coord[0];
            coord0[1] = node0.Coord[1];
            // 座標リスト作成
            double[] coord = new double[ndim2d];
            foreach (int nodeNumber in nodes)
            {
                int nodeIndex = nodeNumber - 1;
                FemNode node = Nodes[nodeIndex];
                coord[0] = node.Coord[0];
                coord[1] = node.Coord[1];
                double x = getDistance(coord, coord0);
                //Console.WriteLine("{0},{1},{2},{3}", nodeIndex, coord[0], coord[1], x);
                coords.Add(x);
            }

            // 要素リスト作成
            int elemCnt = (nodes.Count - 1) / 2 ;
            for (int elemIndex = 0; elemIndex < elemCnt; elemIndex++)
            {
                // 線要素の要素内節点
                // 節点番号はポート上の1D節点番号(1起点の番号)
                //  1   3   2
                //  +---+---+   並びに注意:頂点→内部の点
                FemElement element = new FemElement();
                element.No = elemIndex + 1;
                element.NodeNumbers = new int[nno];
                element.NodeNumbers[0] = 2 * elemIndex     + 1;
                element.NodeNumbers[1] = 2 * elemIndex + 2 + 1;
                element.NodeNumbers[2] = 2 * elemIndex + 1 + 1;
                element.MediaIndex = 0;
                elements.Add(element);
            }

            // 要素を１辺とする2D領域の要素番号を取得
            for (int elemIndex = 0; elemIndex < elements.Count; elemIndex++)
            {
                FemElement element = elements[elemIndex];
                int[] nodeNumbers = element.NodeNumbers;

                // 1辺だけ調べればよい(1-3をチェック)
                int stNodeNumber = nodes[nodeNumbers[0] - 1];
                int edNodeNumber = nodes[nodeNumbers[2] - 1];
                string edgeKey = "";
                if (stNodeNumber < edNodeNumber)
                {
                    edgeKey = string.Format("{0}_{1}", stNodeNumber, edNodeNumber);
                }
                else
                {
                    edgeKey = string.Format("{0}_{1}", edNodeNumber, stNodeNumber);
                }
                if (!EdgeToElementNoH.ContainsKey(edgeKey))
                {
                    Console.WriteLine("logical error: Not find edge {0}", edgeKey);
                }
                else
                {
                    // 隣接する2Dの要素を1つ取得する
                    int elemNo2d = EdgeToElementNoH[edgeKey][0];
                    FemElement element2d = Elements[elemNo2d - 1];

                    // 媒質インデックスをセット
                    element.MediaIndex = element2d.MediaIndex;
                }
            }

            // 強制境界節点と内部領域節点を分離
            foreach (int nodeNumber2d in nodes)
            {
                int nodeNumber = to1dNodes[nodeNumber2d];
                if (ForceNodeNumberH.ContainsKey(nodeNumber2d))
                {
                    Console.WriteLine("{0}:    {1}    {2}", nodeNumber, Nodes[nodeNumber2d - 1].Coord[0], Nodes[nodeNumber2d - 1].Coord[1]); 
                }
                else
                {
                    sortedNodes.Add(nodeNumber);
                }
            }
            // 1D節点番号→ソート済みリストインデックス変換マップ作成
            for (int i = 0; i < sortedNodes.Count; i++)
            {
                int nodeNumber = sortedNodes[i];
                if (!toSorted.ContainsKey(nodeNumber))
                {
                    toSorted.Add(nodeNumber, i);
                }
            }
            // ソート済み1D節点インデックス→2D節点番号マップ
            nodesBoundary = new int[sortedNodes.Count];
            for (int i = 0; i < sortedNodes.Count; i++)
            {
                int nodeNumber = sortedNodes[i];
                int nodeIndex = nodeNumber - 1;
                int nodeNumber2d = nodes[nodeIndex];
                nodesBoundary[i] = nodeNumber2d;
            }

            // 節点数
            int nodeCnt = sortedNodes.Count;
            // 固有値、固有ベクトル
            eigenValues = new Complex[maxMode];
            eigenVecs = new Complex[maxMode, nodeCnt];
            // 固有モード解析でのみ使用するuzz_1d, txx_1d
            MyDoubleMatrix txx_1d = new MyDoubleMatrix(nodeCnt, nodeCnt);
            MyDoubleMatrix uzz_1d = new MyDoubleMatrix(nodeCnt, nodeCnt);
            // ryy_1dマトリクス (線要素)
            ryy_1d = new MyDoubleMatrix(nodeCnt, nodeCnt);

            for (int i = 0; i < nodeCnt; i++)
            {
                for (int j = 0; j < nodeCnt; j++)
                {
                    ryy_1d[i, j] = 0.0;
                    txx_1d[i, j] = 0.0;
                    uzz_1d[i, j] = 0.0;
                }
            }

            for (int elemIndex = 0; elemIndex < elements.Count; elemIndex++)
            {
                // ２次線要素
                FemElement element = elements[elemIndex];
                addElementMatOf1dEigenValueProblem(element, coords, toSorted, ref txx_1d, ref ryy_1d, ref uzz_1d);
            }

            // [A] = [Txx] - k0 * k0 *[Uzz]
            //MyDoubleMatrix matA = minus(txx_1d, product((k0 * k0), uzz_1d));
            //メモリ節約
            MyDoubleMatrix matA = new MyDoubleMatrix(nodeCnt, nodeCnt);
            for (int ino = 0; ino < nodeCnt; ino++)
            {
                for (int jno = 0; jno < nodeCnt; jno++)
                {
                    matA[ino, jno] = txx_1d[ino, jno] - (k0 * k0) * uzz_1d[ino, jno];
                }
            }
            // [Ryy]-1[A]
            matA = MyMatrixUtil.product(MyMatrixUtil.matrix_Inverse(ryy_1d), matA);

            Complex[] evals = null;
            Complex[,] evecs = null;
            try
            {
                // 固有値、固有ベクトルを求める
                solveEigen(matA, out evals, out evecs);
                // 固有値のソート
                sort1DEigenMode(k0, evals, evecs);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " " + exception.StackTrace);
                System.Diagnostics.Debug.Assert(false);
            }

            int tagtModeIdx = evals.Length - 1;
            for (int imode = 0; imode < maxMode; imode++)
            {
                /*
                for (; tagtModeIdx >= 0; tagtModeIdx--)
                {
                    if (Math.Abs(evals[tagtModeIdx].Real) < 1.0e-6)
                    {
                        continue;
                    }
                    if (evals[tagtModeIdx].Real > 0.0 && Math.Abs(evals[tagtModeIdx].Imaginary) < 1.0e-6)
                    {
                        break;
                    }
                }
                */
                if (tagtModeIdx == -1)
                {
                    // fail safe
                    eigenValues[imode] = 0.0;
                    continue;
                }
                // 伝搬定数は固有値のsqrt
                Complex betam = Complex.Sqrt(evals[tagtModeIdx]);
                // 固有ベクトル
                Complex[] evec = MyMatrixUtil.matrix_GetRowVec(evecs, tagtModeIdx);
                // 規格化定数を求める
                Complex[] workVec = MyMatrixUtil.product(ryy_1d, evec);
                double dm = Complex.Abs(MyMatrixUtil.vector_Dot(MyMatrixUtil.vector_Conjugate(evec), workVec));
                dm = Math.Sqrt(omega * myu0 / Complex.Abs(betam) / dm);
                //Console.WriteLine("dm = " + dm);

                // 伝搬定数の格納
                eigenValues[imode] = betam;
                //Console.WriteLine("eigenValues [ " + imode + "] = " + betam.Real + " + " + betam.Imaginary + " i " + " tagtModeIdx :" + tagtModeIdx + " " );
                Console.WriteLine("β/k0 [ " + imode + "] = " + betam.Real/k0 + " + " + betam.Imaginary/k0 + " i " + " tagtModeIdx :" + tagtModeIdx + " " );
                // 固有ベクトルの格納(規格化定数を掛ける)
                for (int inoSorted = 0; inoSorted < nodeCnt; inoSorted++)
                {
                    Complex fm = dm * evec[inoSorted];
                    eigenVecs[imode, inoSorted] = fm;
                    //Console.WriteLine("eigenVecs [ " + imode + ", " + inoSorted + "] = " + fm.Real + " + " + fm.Imaginary + " i  Abs:" + Complex.Abs(fm));
                }

                tagtModeIdx--;
            }
        }

        /// <summary>
        /// 1Dヘルムホルツ方程式固有値問題の要素行列を加算する
        /// </summary>
        /// <param name="element">線要素</param>
        /// <param name="coords">座標リスト</param>
        /// <param name="toSorted">節点番号→ソート済み節点インデックスマップ</param>
        /// <param name="txx_1d">txx行列</param>
        /// <param name="ryy_1d">ryy行列</param>
        /// <param name="uzz_1d">uzz行列</param>
        private void addElementMatOf1dEigenValueProblem(FemElement element, IList<double> coords, Dictionary<int, int> toSorted, ref MyDoubleMatrix txx_1d, ref MyDoubleMatrix ryy_1d, ref MyDoubleMatrix uzz_1d)
        {
            // ２次線要素
            const int nno = Constants.LineNodeCnt_SecondOrder; // 3;
            
            int[] nodeNumbers = element.NodeNumbers;
            System.Diagnostics.Debug.Assert(nno == nodeNumbers.Length);

            // 座標の取得
            double[] elementCoords = new double[nno];
            for (int n = 0; n < nno; n++)
            {
                int nodeIndex = nodeNumbers[n] - 1;
                elementCoords[n] = coords[nodeIndex];
            }
            // 線要素の長さ
            double elen = Math.Abs(elementCoords[1] - elementCoords[0]);
            // 媒質インデックス
            int mediaIndex = element.MediaIndex;
            // 媒質
            MediaInfo media = Medias[mediaIndex];
            double[,] media_P = media.P;
            media_P = MyMatrixUtil.matrix_Inverse(media_P);
            double[,] media_Q = media.Q;
            double[,] integralN = new double[nno, nno]
                {
                    {  4.0 / 30.0 * elen, -1.0 / 30.0 * elen,  2.0 / 30.0 * elen },
                    { -1.0 / 30.0 * elen,  4.0 / 30.0 * elen,  2.0 / 30.0 * elen },
                    {  2.0 / 30.0 * elen,  2.0 / 30.0 * elen, 16.0 / 30.0 * elen },
                };
            double[,] integralDNDY = new double[nno, nno]
                {
                    {  7.0 / (3.0 * elen),  1.0 / (3.0 * elen), -8.0 / (3.0 * elen) },
                    {  1.0 / (3.0 * elen),  7.0 / (3.0 * elen), -8.0 / (3.0 * elen) },
                    { -8.0 / (3.0 * elen), -8.0 / (3.0 * elen), 16.0 / (3.0 * elen) },
                };
            for (int ino = 0; ino < nno; ino++)
            {
                int inoBoundary = nodeNumbers[ino];
                int inoSorted;
                if (!toSorted.ContainsKey(inoBoundary)) continue;
                inoSorted = toSorted[inoBoundary];
                for (int jno = 0; jno < nno; jno++)
                {
                    int jnoBoundary = nodeNumbers[jno];
                    int jnoSorted;
                    if (!toSorted.ContainsKey(jnoBoundary)) continue;
                    jnoSorted = toSorted[jnoBoundary];

                    double e_txx_1d_inojno = media_P[0, 0] * integralDNDY[ino, jno];
                    double e_ryy_1d_inojno = media_P[1, 1] * integralN[ino, jno];
                    double e_uzz_1d_inojno = media_Q[2, 2] * integralN[ino, jno];
                    txx_1d[inoSorted, jnoSorted] += e_txx_1d_inojno;
                    ryy_1d[inoSorted, jnoSorted] += e_ryy_1d_inojno;
                    uzz_1d[inoSorted, jnoSorted] += e_uzz_1d_inojno;
                }
            }
        }
        
        /// <summary>
        /// Lisys(Lapack)による固有値解析(複素数版)
        /// </summary>
        private static bool solveEigen(MyComplexMatrix srcMat, out Complex[] evals, out Complex[,] evecs)
        {
            // Lisys(Lapack)による固有値解析
            ValueType[] X = MyMatrixUtil.matrix_ToBuffer(srcMat, false);
            ValueType[] c_evals = null;
            ValueType[][] c_evecs = null;
            KrdLab.clapack.Function.zgeev(X, srcMat.RowSize, srcMat.ColumnSize, ref c_evals, ref c_evecs);

            evals = new Complex[c_evals.Length];
            for (int i = 0; i < evals.Length; i++)
            {
                evals[i] = (Complex)c_evals[i];
                //Console.WriteLine("( " + i + " ) = " + evals[i].Real + " + " + evals[i].Imaginary + " i ");
            }
            evecs = new Complex[c_evecs.Length, c_evecs[0].Length];
            for (int i = 0; i < evecs.GetLength(0); i++)
            {
                for (int j = 0; j < evecs.GetLength(1); j++)
                {
                    evecs[i, j] = (Complex)c_evecs[i][j];
                    //Console.WriteLine("( " + i + ", " + j + " ) = " + evecs[i, j].Real + " + " + evecs[i, j].Imaginary + " i ");
                }
            }

            return true;
        }


        /// <summary>
        /// Lisys(Lapack)による固有値解析
        /// </summary>
        private static bool solveEigen(MyDoubleMatrix srcMat, out Complex[] evals, out Complex[,] evecs)
        {
            // Lisys(Lapack)による固有値解析             
            double[] X = MyMatrixUtil.matrix_ToBuffer(srcMat, false);
            double[] r_evals = null;
            double[] i_evals = null;
            double[][] r_evecs = null;
            double[][] i_evecs = null;
            KrdLab.clapack.Function.dgeev(X, srcMat.RowSize, srcMat.ColumnSize, ref r_evals, ref i_evals, ref r_evecs, ref i_evecs);

            evals = new Complex[r_evals.Length];
            for (int i = 0; i < evals.Length; i++)
            {
                evals[i] = new Complex(r_evals[i], i_evals[i]);
                //Console.WriteLine("( " + i + " ) = " + evals[i].Real + " + " + evals[i].Imaginary + " i ");
            }
            evecs = new Complex[r_evecs.Length, r_evecs[0].Length];
            for (int i = 0; i < evecs.GetLength(0); i++)
            {
                for (int j = 0; j < evecs.GetLength(1); j++)
                {
                    evecs[i, j] = new Complex(r_evecs[i][j], i_evecs[i][j]);
                    //Console.WriteLine("( " + i + ", " + j + " ) = " + evecs[i, j].Real + " + " + evecs[i, j].Imaginary + " i ");
                }
            }

            return true;
        }

        /// <summary>
        /// 固有値のソート用クラス
        /// </summary>
        class EigenModeItem : IComparable<EigenModeItem>
        {
            public int index;
            public Complex eval;
            public Complex[] evec;

            public EigenModeItem(int index_, Complex eval_, Complex[] evec_)
            {
                index = index_;
                eval = eval_;
                evec = evec_;
            }
            int IComparable<EigenModeItem>.CompareTo(EigenModeItem other)
            {
                //BUG: 小数点以下の場合、比較できない
                //int cmp = (int)(this.eval.Real - other.eval.Real);
                double cmpf = this.eval.Real - other.eval.Real;
                int cmp = 0;
                if (Math.Abs(cmpf) < 1.0E-15)
                {
                    cmp = 0;
                }
                else
                {
                    if (cmpf > 0)
                    {
                        cmp = 1;
                    }
                    else
                    {
                        cmp = -1;
                    }
                }
                return cmp;
            }

        }

        /// <summary>
        /// 固有モードをソートする(伝搬モードを先に)
        /// </summary>
        /// <param name="evals"></param>
        /// <param name="evecs"></param>
        private void sort1DEigenMode(double k0, Complex[] evals, Complex[,] evecs)
        {
            // 符号調整
            {
                // 正の固有値をカウント
                //int positive_cnt = 0;
                //foreach (Complex c in evals)
                //{
                //    if (c.Real > 0.0 && Math.Abs(c.Imaginary) <Constants.PrecisionLowerLimit)
                //    {
                //        positive_cnt++;
                //    }
                //}
                // 計算範囲では、正の固有値(伝搬定数の二乗が正)は高々1個のはず
                // 半分以上正の固有値であれば、固有値が逆転して計算されていると判断する
                // 誘電体導波路の場合、これでは破綻する？
                // 最大、最小値を求め、最大値 > 最小値の絶対値なら逆転している
                double minEval = double.MaxValue;
                double maxEval = double.MinValue;
                foreach (Complex c in evals)
                {
                    if (minEval > c.Real) { minEval = c.Real; }
                    if (maxEval < c.Real) { maxEval = c.Real; }
                }
                //if (positive_cnt >= evals.Length / 2)  // 導波管の場合
                if (Math.Abs(maxEval) > Math.Abs(minEval))
                {
                    for (int i = 0; i < evals.Length; i++)
                    {
                        evals[i] = -evals[i];
                    }
                    Console.WriteLine("eval sign changed");
                }
            }

            EigenModeItem[] items = new EigenModeItem[evals.Length];
            for (int i = 0; i < evals.Length; i++)
            {
                Complex[] evec = MyMatrixUtil.matrix_GetRowVec(evecs, i);
                items[i] = new EigenModeItem(i, evals[i], evec);
            }
            Array.Sort(items);

            int imode = 0;
            foreach (EigenModeItem item in items)
            {
                evals[imode] = item.eval;
                MyMatrixUtil.matrix_setRowVec(evecs, (int)imode, item.evec);
                //Console.WriteLine("[sorted]( " + imode + " ) = " + evals[imode].Real + " + " + evals[imode].Imaginary + " i ");
                imode++;
            }

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
        /// 複素数のパース
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Complex ComplexParse(string str)
        {
            Match match = Regex.Match(str, "(.+?)\\+(.+)i");
            double real = double.Parse(match.Groups[1].Value);
            double imag = double.Parse(match.Groups[2].Value);
            return new Complex(real, imag);
        }
    }
}
