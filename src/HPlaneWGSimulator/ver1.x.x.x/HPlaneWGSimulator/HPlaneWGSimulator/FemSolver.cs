using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
//using System.Numerics; // Complex
using KrdLab.clapack; // KrdLab.clapack.Complex
//using System.Text.RegularExpressions;
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
        private const double pi = Constants.pi;
        private const double c0 = Constants.c0;
        private const double myu0 = Constants.myu0;
        private const double eps0 = Constants.eps0;
        /// <summary>
        /// 考慮モード数
        /// </summary>
        private const int MaxModeCnt = Constants.MaxModeCount;
        /// <summary>
        /// 導波路の幅既定値  規格化周波数が定義できるように初期値を設定
        /// </summary>
        public const double DefWaveguideWidth = 1000.0; // ありえない幅
        /// <summary>
        /// 入射モードインデックス
        /// </summary>
        private const int IncidentModeIndex = 0;

        ////////////////////////////////////////////////////////////////////////
        // 型
        ////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// リニアシステムソルバー区分
        /// </summary>
        public enum LinearSystemEqnSoverDV
        {
            // 既定値を使用
            //Default,
            // clapack zgesvを使用(KrdLab Lisys)
            Zgesv,
            // clapack zgbsv
            Zgbsv,
            // Preconditioned Orthogonal Conjugate Gradient Method(PCOCG)を使用(DelFEM)
            PCOCG
        }
        /// <summary>
        /// 波のモード区分
        /// </summary>
        public enum WaveModeDV { TE, TM };

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
        /// 計算する波のモード区分
        /// </summary>
        public WaveModeDV WaveModeDv
        {
            get;
            set;
        }

        /// <summary>
        /// 設定された要素形状区分(これはSolver内部では使用しない。要素形状は要素分割データから判断する)
        /// </summary>
        public Constants.FemElementShapeDV ElemShapeDvToBeSet
        {
            get;
            set;
        }
        /// <summary>
        /// 設定された要素の補間次数(これはSolver内部では使用しない。補間次数は要素分割データから判断する)
        /// </summary>
        public int ElemOrderToBeSet
        {
            get;
            set;
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
        /// clapack使用時のFEM行列
        /// </summary>
        private MyComplexMatrix FemMat = null;
        /// <summary>
        /// ソート済み節点番号リスト
        /// </summary>
        private IList<int> SortedNodes = null;
        /// <summary>
        /// FEM行列の非０要素パターン
        /// </summary>
        private bool[,] FemMatPattern = null;
        /// <summary>
        /// 線形方程式解法区分
        /// </summary>
        public LinearSystemEqnSoverDV LsEqnSolverDv
        {
            get;
            set;
        }

        /// <summary>
        /// 文字列→線形方程式解法区分変換
        /// </summary>
        /// <param name="confValue"></param>
        /// <returns></returns>
        public static LinearSystemEqnSoverDV StrToLinearSystemEqnSolverDV(string confValue)
        {
            LinearSystemEqnSoverDV lsEqnSoverDv = LinearSystemEqnSoverDV.Zgbsv;
            if (confValue == "PCOCG")
            {
                lsEqnSoverDv = LinearSystemEqnSoverDV.PCOCG;
            }
            else if (confValue == "Zgesv")
            {
                lsEqnSoverDv = LinearSystemEqnSoverDV.Zgesv;
            }
            else if (confValue == "Zgbsv")
            {
                lsEqnSoverDv = LinearSystemEqnSoverDV.Zgbsv;
            }            
            else
            {
                throw new InvalidDataException("線形方程式解法区分が不正です");
            }
            return lsEqnSoverDv;
        }

        /// <summary>
        /// 線形方程式解法区分→文字列変換
        /// </summary>
        /// <param name="lsEqnSolverDv"></param>
        /// <returns></returns>
        public static string LinearSystemEqnSolverDVToStr(LinearSystemEqnSoverDV lsEqnSolverDv)
        {
            string confValue = "";
            if (lsEqnSolverDv == LinearSystemEqnSoverDV.PCOCG)
            {
                confValue = "PCOCG";
            }
            else if (lsEqnSolverDv == LinearSystemEqnSoverDV.Zgesv)
            {
                confValue = "Zgesv";
            }
            else if (lsEqnSolverDv == LinearSystemEqnSoverDV.Zgbsv)
            {
                confValue = "Zgbsv";
            }
            else
            {
                throw new InvalidDataException("線形方程式解法区分が不正です");
            }
            return confValue;
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
            WaveModeDv = Constants.DefWaveModeDv;
            ElemShapeDvToBeSet = Constants.DefElemShapeDv;
            ElemOrderToBeSet = Constants.DefElementOrder;
            LsEqnSolverDv = Constants.DefLsEqnSolverDv;
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
            LinearSystemEqnSoverDV lsEqnSolverDv = LinearSystemEqnSoverDV.Zgbsv;
            bool ret = FemInputDatFile.LoadFromFile(
                filename,
                out nodes, out elements, out ports, out forceBCNodes, out incidentPortNo, out medias,
                out firstWaveLength, out lastWaveLength, out calcCnt, out lsEqnSolverDv);
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
                LsEqnSolverDv = lsEqnSolverDv;

                // 要素形状と次数の判定
                if (Elements.Count > 0)
                {
                    Constants.FemElementShapeDV elemShapeDv;
                    int order;
                    int vertexCnt;
                    FemMeshLogic.GetElementShapeDvAndOrderByElemNodeCnt(Elements[0].NodeNumbers.Length, out elemShapeDv, out order, out vertexCnt);
                    ElemShapeDvToBeSet = elemShapeDv;
                    ElemOrderToBeSet = order;
                }

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
                    LsEqnSolverDv = Constants.DefLsEqnSolverDv;
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
        /// 周波数番号から規格化周波数を取得する
        /// </summary>
        /// <param name="freqNo"></param>
        /// <returns></returns>
        public double GetNormalizedFreqFromFreqNo(int freqNo)
        {
            double normalizedFreq = 0;
            int freqIndex = freqNo - 1;
            int calcFreqCnt = CalcFreqCnt;
            double firstNormalizedFreq = FirstNormalizedFreq;
            double lastNormalizedFreq = LastNormalizedFreq;
            double deltaf = (lastNormalizedFreq - firstNormalizedFreq) / calcFreqCnt;

            normalizedFreq = firstNormalizedFreq + freqIndex * deltaf;

            return normalizedFreq;
        }

        /// <summary>
        /// 計算対象周波数範囲を入力ファイルに書き込む
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="normalizedFreq1"></param>
        /// <param name="normalizedFreq2"></param>
        /// <param name="calcCnt"></param>
        /// <param name="lsEqnSolverDv"></param>
        public void UpdateAndSaveToInputFile(string filename,
            double normalizedFreq1, double normalizedFreq2, int calcCnt,
            LinearSystemEqnSoverDV lsEqnSolverDv)
        {
            // 計算対象周波数を波長に変換
            double firstWaveLength = GetWaveLengthFromNormalizedFreq(normalizedFreq1, WaveguideWidth);
            double lastWaveLength = GetWaveLengthFromNormalizedFreq(normalizedFreq2, WaveguideWidth);

            // セット
            FirstWaveLength = firstWaveLength;
            LastWaveLength = lastWaveLength;
            CalcFreqCnt = calcCnt;
            LsEqnSolverDv = lsEqnSolverDv;

            // FEM入力ファイルへ更新書き込み
            FemInputDatFile.UpdateToFile(filename,
                FirstWaveLength, LastWaveLength, CalcFreqCnt,
                LsEqnSolverDv);
        }

        /// <summary>
        /// 辺と要素の対応マップ作成
        /// </summary>
        /// <param name="in_Elements"></param>
        /// <param name="out_EdgeToElementNoH"></param>
        public static void MkEdgeToElementNoH(IList<FemElement> in_Elements, ref Dictionary<string, IList<int>> out_EdgeToElementNoH)
        {
            FemMeshLogic.MkEdgeToElementNoH(in_Elements, ref out_EdgeToElementNoH);
        }

        /// <summary>
        /// 節点と要素番号のマップ作成
        /// </summary>
        /// <param name="in_Elements"></param>
        /// <param name="out_NodeToElementNoH"></param>
        public static void MkNodeToElementNoH(IList<FemElement> in_Elements, ref Dictionary<int, IList<int>> out_NodeToElementNoH)
        {
            FemMeshLogic.MkNodeToElementNoH(in_Elements, ref out_NodeToElementNoH);
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
            double w1 = FemMeshLogic.GetDistance(Nodes[port1NodeNumber1 - 1].Coord, Nodes[port1NodeNumber2 - 1].Coord);

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
            return FemMeshLogic.IsPointInElement(element, test_pp, nodes);
        }

        /// <summary>
        /// Fem入力データの取得
        /// </summary>
        /// <param name="outNodes">節点リスト</param>
        /// <param name="outElements">要素リスト</param>
        /// <param name="outMedias">媒質リスト</param>
        /// <param name="outPorts">ポート節点番号リストのリスト</param>
        /// <param name="outForceNodes">強制境界節点番号リスト</param>
        /// <param name="outIncidentPortNo">入射ポート番号</param>
        /// <param name="outWaveguideWidth">導波路の幅</param>
        public void GetFemInputInfo(
            out FemNode[] outNodes, out FemElement[] outElements,
            out MediaInfo[] outMedias,
            out IList<int[]> outPorts,
            out int[] outForceNodes,
            out int outIncidentPortNo, out double outWaveguideWidth)
        {
            outNodes = null;
            outElements = null;
            outMedias = null;
            outPorts = null;
            outForceNodes = null;
            outIncidentPortNo = 1;
            outWaveguideWidth = DefWaveguideWidth;

            /* データの判定は取得した側が行う（メッシュ表示で、ポートを指定しないとメッシュが表示されないのを解消するためここで判定するのを止める)
            if (!isInputDataValid())
            {
                return;
            }
             */

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
                //FemElement femElement = new FemElement();
                FemElement femElement = FemMeshLogic.CreateFemElementByElementNodeCnt(Elements[i].NodeNumbers.Length);
                femElement.CP(Elements[i]);
                outElements[i] = femElement;
            }
            if (Medias != null)
            {
                outMedias = new MediaInfo[Medias.Length];
                for (int i = 0; i < Medias.Length; i++)
                {
                    outMedias[i] = Medias[i].Clone() as MediaInfo;
                }
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
            //string indexfilename = basefilename + Constants.FemOutputIndexExt;
            // BUGFIX インデックスファイル名は.out.idx
            string indexfilename = outfilename + Constants.FemOutputIndexExt;

            // 結果出力ファイルの削除(結果を追記モードで書き込むため)
            if (File.Exists(outfilename))
            {
                File.Delete(outfilename);
            }
            if (File.Exists(indexfilename))
            {
                File.Delete(indexfilename);
            }

            FemMat = null;
            SortedNodes = null;
            FemMatPattern = null;
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
            FemMat = null;
            SortedNodes = null;
            FemMatPattern = null;
        }

        /// <summary>
        /// 周波数１箇所だけ計算する
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="in_freqNo"></param>
        public void RunAtOneFreq(string filename, int in_freqNo, object eachDoneCallbackObj, Delegate eachDoneCallback, bool appendFileFlg = false)
        {
            IsCalcAborted = false;
            if (!isInputDataValid())
            {
                return;
            
            }
            string basefilename = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(filename);
            string outfilename = basefilename + Constants.FemOutputExt;
            //string indexfilename = basefilename + Constants.FemOutputIndexExt;
            // BUGFIX インデックスファイル名は.out.idx
            string indexfilename = outfilename + Constants.FemOutputIndexExt;
            if (!appendFileFlg)
            {
                // 結果出力ファイルの削除(結果を追記モードで書き込むため)
                if (File.Exists(outfilename))
                {
                    File.Delete(outfilename);
                }
                if (File.Exists(indexfilename))
                {
                    File.Delete(indexfilename);
                }
            }

            FemMat = null;
            SortedNodes = null;
            FemMatPattern = null;
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

            int calcFreqCnt = CalcFreqCnt;
            double firstNormalizedFreq = FirstNormalizedFreq;
            double lastNormalizedFreq = LastNormalizedFreq;
            int maxMode = MaxModeCnt;
            double deltaf = (lastNormalizedFreq - firstNormalizedFreq) / calcFreqCnt;

            {
                int freqIndex = in_freqNo - 1;
                if (freqIndex < 0 || freqIndex >= calcFreqCnt + 1)
                {
                    return;
                }
                double normalizedFreq = firstNormalizedFreq + freqIndex * deltaf;
                if (normalizedFreq < Constants.PrecisionLowerLimit)
                {
                    normalizedFreq = 1.0e-4;
                }
                double waveLength = GetWaveLengthFromNormalizedFreq(normalizedFreq, WaveguideWidth);
                Console.WriteLine("2w/lamda = {0}", normalizedFreq);
                int freqNo = freqIndex + 1;
                runEach(freqNo, outfilename, waveLength, maxMode);
                eachDoneCallback.Method.Invoke(eachDoneCallbackObj, new object[] { new object[] { }, });
            }
            FemMat = null;
            SortedNodes = null;
            FemMatPattern = null;
        }

        /// <summary>
        /// 各波長について計算実行
        /// </summary>
        /// <param name="freqNo">計算する周波数に対応する番号(1,...,CalcFreqCnt - 1)</param>
        /// <param name="filename">出力ファイル</param>
        /// <param name="waveLength">波長</param>
        private void runEach(int freqNo, string filename, double waveLength, int maxMode)
        {
            Console.WriteLine("runEach 1");
            bool ret;
            // 全体剛性行列作成
            int[] nodesRegion = null;
            MyComplexMatrix mat = null;
            ret = getHelmholtzLinearSystemMatrix(waveLength, out nodesRegion, out mat);
            if (!ret)
            {
                Console.WriteLine("Error at getHelmholtzLinearSystemMatrix ret: {0}", ret);
                // 計算を中止する
                IsCalcAborted = true;
                return;
            }
            Console.WriteLine("runEach 2");

            // 残差ベクトル初期化
            int nodeCnt = nodesRegion.Length;
            Complex[] resVec = new Complex[nodeCnt];
            /*
            for (int i = 0; i < nodeCnt; i++)
            {
                resVec[i] = new Complex();
            }
             */
            Console.WriteLine("runEach 3");

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
            Console.WriteLine("runEach 4");
            Complex[] valuesAll = null;
            if (this.LsEqnSolverDv == LinearSystemEqnSoverDV.Zgbsv)
            {
                System.Diagnostics.Debug.Assert(mat is MyComplexBandMatrix);

                valuesAll = null;

                MyComplexBandMatrix bandMat = mat as MyComplexBandMatrix;
                int rowcolSize = bandMat.RowSize;
                int subdiaSize = bandMat.SubdiaSize;
                int superdiaSize = bandMat.SuperdiaSize;

                // リニア方程式を解く
                Complex[] X = null;
                // clapackの行列の1次元ベクトルへの変換は列を先に埋める
                // バンドマトリクス用の格納方法で格納する
                Complex[] A = MyMatrixUtil.matrix_ToBuffer(bandMat, false);
                Complex[] B = resVec;
                int x_row = nodeCnt;
                int x_col = 1;
                int a_row = rowcolSize;
                int a_col = rowcolSize;
                int kl = subdiaSize;
                int ku = superdiaSize;
                int b_row = nodeCnt;
                int b_col = 1;
                Console.WriteLine("run zgbsv");
                try
                {
                    KrdLab.clapack.FunctionExt.zgbsv(ref X, ref x_row, ref x_col, A, a_row, a_col, kl, ku, B, b_row, b_col);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message + " " + exception.StackTrace);
                    MessageBox.Show(string.Format("計算中にエラーが発生しました。2W/λ = {0}" + System.Environment.NewLine + "    {1}",
                        GetNormalizedFreq(waveLength, WaveguideWidth), exception.Message));
                    return;
                    // ダミーデータ
                    //X = new ValueType[nodeCnt];
                    //for (int i = 0; i < nodeCnt; i++) { X[i] = new Complex(); }
                }
                valuesAll = X;
            }
            else if (this.LsEqnSolverDv == LinearSystemEqnSoverDV.Zgesv)
            {
                //---------------------------------------------------------
                // clapack zgesv (KrdLab Lisys)
                //---------------------------------------------------------
                valuesAll = null;

                // リニア方程式を解く
                Complex[] X = null;
                // clapackの行列の1次元ベクトルへの変換は列を先に埋める
                Complex[] A = MyMatrixUtil.matrix_ToBuffer(mat, false);
                /*
                Complex[] B = new Complex[nodeCnt];
                for (int ino = 0; ino < nodeCnt; ino++)
                {
                    B[ino] = resVec[ino];
                }
                 */
                Complex[] B = resVec;
                ///////////////////////
                /*
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
                 */
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
                    KrdLab.clapack.FunctionExt.zgesv(ref X, ref x_row, ref x_col, A, a_row, a_col, B, b_row, b_col);
                    //KrdLab.clapack.FunctionExt.zgesv(ref X, ref x_row, ref x_col, A, a_row, a_col, B, b_row, b_col, true);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message + " " + exception.StackTrace);
                    MessageBox.Show(string.Format("計算中にエラーが発生しました。2W/λ = {0}" + System.Environment.NewLine + "    {1}",
                        GetNormalizedFreq(waveLength, WaveguideWidth), exception.Message));
                    return;
                    // ダミーデータ
                    //X = new ValueType[nodeCnt];
                    //for (int i = 0; i < nodeCnt; i++) { X[i] = new Complex(); }
                }

                /*
                valuesAll = new Complex[nodeCnt];
                for (int ino = 0; ino < nodeCnt; ino++)
                {
                    Complex c = (Complex)X[ino];
                    //Console.WriteLine("({0})  {1} + {2}i", ino, c.Real, c.Imaginary);
                    valuesAll[ino] = c;
                }
                 */
                valuesAll = X;
            }
            else
            {
                MessageBox.Show("Not implemented solver");
                return;
            }

            // 散乱行列Sij
            // ポートj = IncidentPortNoからの入射のみ対応
            /*
            Complex[] scatterVec = new Complex[Ports.Count];
            for (int portIndex = 0; portIndex < Ports.Count; portIndex++)
            {
                int iMode = IncidentModeIndex;
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
             */
            // 拡張散乱行列Simjn
            //   出力ポートi モードmの散乱係数
            //   入射ポートj = IncindentPortNo n = 0(基本モード)のみ対応
            IList<Complex[]> scatterVecList = new List<Complex[]>();
            double totalPower = 0.0;
            for (int portIndex = 0; portIndex < Ports.Count; portIndex++)
            {
                int[] nodesBoundary = nodesBoundaryList[portIndex];
                MyDoubleMatrix ryy_1d = ryy_1dList[portIndex];
                Complex[] eigenValues = eigenValuesList[portIndex];
                Complex[,] eigenVecs = eigenVecsList[portIndex];
                int modeCnt = eigenValues.Length;
                Complex[] portScatterVec = new Complex[modeCnt];
                Console.WriteLine("port {0}", portIndex);
                for (int iMode = 0; iMode < eigenValues.Length; iMode++)
                {
                    bool isPropagationMode = (eigenValues[iMode].Real >= Constants.PrecisionLowerLimit);
                    bool isIncidentMode = ((portIndex == IncidentPortNo - 1) && iMode == 0);
                    Complex sim10 = getWaveguidePortReflectionCoef(waveLength, iMode, isIncidentMode,
                                                                 nodesBoundary, ryy_1d, eigenValues, eigenVecs,
                                                                 nodesRegion, valuesAll);
                    portScatterVec[iMode] = sim10;
                    if (isPropagationMode)
                    {
                        totalPower += (sim10 * Complex.Conjugate(sim10)).Real;
                    }
                    // check
                    if (iMode == 0)
                    {
                        Console.WriteLine("  {0} s{1}{2}{3}{4} = {5} + {6}i " + System.Environment.NewLine + "        (|S| = {7} |S|^2 = {8})",
                            isPropagationMode ? "P" : "E",
                            portIndex + 1, (iMode + 1), IncidentPortNo, (IncidentModeIndex + 1),
                            sim10.Real, sim10.Imaginary, Complex.Abs(sim10), Complex.Abs(sim10) * Complex.Abs(sim10));
                    }
                }
                scatterVecList.Add(portScatterVec);
            }
            Console.WriteLine("totalPower:{0}", totalPower);

            /////////////////////////////////////
            // 結果をファイルに出力
            ////////////////////////////////////
            FemOutputDatFile.AppendToFile(
                filename, freqNo, waveLength, maxMode,
                Ports.Count, IncidentPortNo,
                nodesBoundaryList, eigenValuesList, eigenVecsList,
                nodesRegion, valuesAll,
                scatterVecList);
        }

        /// <summary>
        /// ヘルムホルツ方程式の剛性行列を作成する
        /// </summary>
        /// <param name="waveLength"></param>
        /// <param name="nodesRegion"></param>
        /// <param name="mat"></param>
        /// <returns>true: 成功 false:失敗(メモリの確保に失敗)</returns>
        private bool getHelmholtzLinearSystemMatrix(double waveLength, out int[] nodesRegion, out MyComplexMatrix mat)
        {
            nodesRegion = null;
            mat = null;

            // 2D節点番号リスト（ソート済み）
            IList<int> sortedNodes = new List<int>();
            // 2D節点番号→ソート済みリストインデックスのマップ
            Dictionary<int, int> toSorted = new Dictionary<int, int>();
            // 非０要素のパターン
            bool[,] matPattern = null;

            if (SortedNodes == null)
            {
                // 節点番号リストをソートする
                //   強制境界の除去する
                //   バンドマトリクスのバンド幅を縮小する

                // 強制境界節点と内部領域節点を分離
                foreach (FemNode node in Nodes)
                {
                    int nodeNumber = node.No;
                    if (ForceNodeNumberH.ContainsKey(nodeNumber))
                    {
                        //forceNodes.Add(nodeNumber);
                    }
                    else
                    {
                        sortedNodes.Add(nodeNumber);
                        toSorted.Add(nodeNumber, sortedNodes.Count - 1);
                    }
                }
                {
                    // バンド幅を縮小する
                    // 非０要素のパターンを取得
                    getMatNonzeroPattern(Nodes, Elements, Ports, ForceBCNodes, toSorted, out matPattern);
                    // subdiagonal、superdiagonalのサイズを取得する
                    int subdiaSizeInitial = 0;
                    int superdiaSizeInitial = 0;
                    {
                        Console.WriteLine("/////initial BandMat info///////");
                        int rowcolSize;
                        int subdiaSize;
                        int superdiaSize;
                        getBandMatrixSubDiaSizeAndSuperDiaSize(matPattern, out rowcolSize, out subdiaSize, out superdiaSize);
                        subdiaSizeInitial = subdiaSize;
                        superdiaSizeInitial = superdiaSize;
                    }

                    // 非０要素出現順に節点番号を格納
                    IList<int> optNodes = new List<int>();
                    Queue<int> chkQueue = new Queue<int>();
                    int[] remainNodes = new int[matPattern.GetLength(0)];
                    for (int i = 0; i < matPattern.GetLength(0); i++)
                    {
                        remainNodes[i] = i;
                    }
                    while (optNodes.Count < sortedNodes.Count)
                    {
                        // 飛び地領域対応
                        for (int rIndex = 0; rIndex < remainNodes.Length; rIndex++)
                        {
                            int i = remainNodes[rIndex];
                            if (i == -1) continue;
                            //System.Diagnostics.Debug.Assert(!optNodes.Contains(i));
                            chkQueue.Enqueue(i);
                            remainNodes[rIndex] = -1;
                            break;
                        }
                        while (chkQueue.Count > 0)
                        {
                            int i = chkQueue.Dequeue();
                            optNodes.Add(i);
                            for (int rIndex = 0; rIndex < remainNodes.Length; rIndex++)
                            {
                                int j = remainNodes[rIndex];
                                if (j == -1) continue;
                                //System.Diagnostics.Debug.Assert(i != j);
                                if (matPattern[i, j])
                                {
                                    //System.Diagnostics.Debug.Assert(!optNodes.Contains(j) && !chkQueue.Contains(j));
                                    chkQueue.Enqueue(j);
                                    remainNodes[rIndex] = -1;
                                }
                            }
                        }
                    }
                    IList<int> optNodesGlobal = new List<int>();
                    Dictionary<int, int> toOptNodes = new Dictionary<int, int>();
                    foreach (int i in optNodes)
                    {
                        int ino = sortedNodes[i];
                        optNodesGlobal.Add(ino);
                        toOptNodes.Add(ino, optNodesGlobal.Count - 1);
                    }
                    System.Diagnostics.Debug.Assert(optNodesGlobal.Count == sortedNodes.Count);
                    // 改善できないこともあるのでチェックする
                    bool improved = false;
                    bool[,] optMatPattern = null;
                    // 非０パターンを取得
                    getMatNonzeroPattern(Nodes, Elements, Ports, ForceBCNodes, toOptNodes, out optMatPattern);
                    // check
                    {
                        Console.WriteLine("/////opt BandMat info///////");
                        int rowcolSize;
                        int subdiaSize;
                        int superdiaSize;
                        getBandMatrixSubDiaSizeAndSuperDiaSize(optMatPattern, out rowcolSize, out subdiaSize, out superdiaSize);
                        if (subdiaSize <= subdiaSizeInitial && superdiaSize <= superdiaSizeInitial)
                        {
                            improved = true;
                        }
                    }
                    if (improved)
                    {
                        // 置き換え
                        sortedNodes = optNodesGlobal;
                        toSorted = toOptNodes;
                        matPattern = optMatPattern;
                    }
                    else
                    {
                        Console.WriteLine("band with not optimized!");
                    }
                }
                SortedNodes = sortedNodes;
                FemMatPattern = matPattern;
            }
            else
            {
                // ソート済み節点番号リストを取得
                sortedNodes = SortedNodes;

                // 2D節点番号→ソート済みリストインデックスのマップ作成
                for (int i = 0; i < sortedNodes.Count; i++)
                {
                    int nodeNumber = sortedNodes[i];
                    if (!toSorted.ContainsKey(nodeNumber))
                    {
                        toSorted.Add(nodeNumber, i);
                    }
                }

                // 非０パターンを取得
                //getMatNonzeroPattern(Nodes, Elements, Ports, ForceBCNodes, toSorted, out matPattern);
                matPattern = FemMatPattern;
            }

            // 総節点数
            int nodeCnt = sortedNodes.Count;

            // 全体節点を配列に格納
            nodesRegion = sortedNodes.ToArray();

            // 全体剛性行列初期化
            //mat = new MyComplexMatrix(nodeCnt, nodeCnt);
            // メモリ割り当てのコストが高いので変更する
            if (FemMat == null)
            {
                try
                {
                    if (this.LsEqnSolverDv == LinearSystemEqnSoverDV.Zgbsv)
                    {
                        // バンドマトリクス(zgbsv)
                        // バンドマトリクス情報を取得する
                        int rowcolSize = 0;
                        int subdiaSize = 0;
                        int superdiaSize = 0;
                        getBandMatrixSubDiaSizeAndSuperDiaSize(matPattern, out rowcolSize, out subdiaSize, out superdiaSize);
                        FemMat = new MyComplexBandMatrix(rowcolSize, subdiaSize, superdiaSize);
                    }
                    else
                    {
                        // 一般行列(zgesv)
                        FemMat = new MyComplexMatrix(nodeCnt, nodeCnt);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message + " " + exception.StackTrace);
                    MessageBox.Show("メモリの確保に失敗しました。");
                    return false;
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(FemMat.RowSize == nodeCnt);
                int size = FemMat._rsize * FemMat._csize;
                for (int i = 0; i < size; i++)
                {
                    FemMat._body[i].Real = 0;
                    FemMat._body[i].Imaginary = 0;
                }
            }
            mat = FemMat;
            /*
            for (int i = 0; i < nodeCnt * nodeCnt; i++)
            {
                mat._body[i] = new Complex();
            }
             */
            foreach (FemElement element in Elements)
            {
                addElementMat(waveLength, toSorted, element, ref mat);
            }

            return true;
        }


        /// <summary>
        /// FEM行列の非０パターンを取得する
        /// </summary>
        /// <param name="Nodes"></param>
        /// <param name="Elements"></param>
        /// <param name="Ports"></param>
        /// <param name="ForceBCNodes"></param>
        /// <param name="toSorted">ソート済み節点番号→ソート済み節点リストのインデックスマップ</param>
        /// <param name="matPattern">非０パターンの配列(非０の要素はtrue、０要素はfalse)</param>
        private static void getMatNonzeroPattern(
            IList<FemNode> Nodes,
            IList<FemElement> Elements,
            IList<IList<int>> Ports,
            IList<int> ForceBCNodes,
            Dictionary<int, int> toSorted,
            out bool[,] matPattern
            )
        {
            matPattern = null;

            // 総節点数
            int nodeCnt = toSorted.Count;

            int matLen = Nodes.Count - ForceBCNodes.Count;

            // 行列の非０パターンを取得する
            matPattern = new bool[matLen, matLen];
            for (int ino_global = 0; ino_global < matLen; ino_global++)
            {
                for (int jno_global = 0; jno_global < matLen; jno_global++)
                {
                    matPattern[ino_global, jno_global] = false;
                }
            }
            // 領域の節点の行列要素パターン
            foreach (FemElement element in Elements)
            {
                int[] nodeNumbers = element.NodeNumbers;

                foreach (int iNodeNumber in nodeNumbers)
                {
                    //if (ForceBCNodes.Contains(iNodeNumber)) continue;
                    //int ino_global = sortedNodes.IndexOf(iNodeNumber);
                    //if (ino_global == -1) continue;
                    if (!toSorted.ContainsKey(iNodeNumber)) continue;
                    int ino_global = toSorted[iNodeNumber];
                    foreach (int jNodeNumber in nodeNumbers)
                    {
                        //if (ForceBCNodes.Contains(jNodeNumber)) continue;
                        //int jno_global = sortedNodes.IndexOf(jNodeNumber);
                        //if (jno_global == -1) continue;
                        if (!toSorted.ContainsKey(jNodeNumber)) continue;
                        int jno_global = toSorted[jNodeNumber];
                        matPattern[ino_global, jno_global] = true;
                    }
                }
            }
            // 境界上の節点の行列要素パターン
            foreach (IList<int> portNodes in Ports)
            {
                foreach (int iNodeNumber in portNodes)
                {
                    //if (ForceBCNodes.Contains(iNodeNumber)) continue;
                    //int ino_global = sortedNodes.IndexOf(iNodeNumber);
                    //if (ino_global == -1) continue;
                    if (!toSorted.ContainsKey(iNodeNumber)) continue;
                    int ino_global = toSorted[iNodeNumber];
                    foreach (int jNodeNumber in portNodes)
                    {
                        //if (ForceBCNodes.Contains(jNodeNumber)) continue;
                        //int jno_global = sortedNodes.IndexOf(jNodeNumber);
                        if (!toSorted.ContainsKey(jNodeNumber)) continue;
                        int jno_global = toSorted[jNodeNumber];
                        if (!matPattern[ino_global, jno_global])
                        {
                            matPattern[ino_global, jno_global] = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// FEM行列のバンドマトリクス情報を取得する
        /// </summary>
        /// <param name="matPattern">非０パターンの配列</param>
        /// <param name="rowcolSize">行数=列数</param>
        /// <param name="subdiaSize">subdiagonalのサイズ</param>
        /// <param name="superdiaSize">superdiagonalのサイズ</param>
        private static void getBandMatrixSubDiaSizeAndSuperDiaSize(
            bool[,] matPattern,
            out int rowcolSize,
            out int subdiaSize,
            out int superdiaSize)
        {
            rowcolSize = matPattern.GetLength(0);

            // subdiaサイズ、superdiaサイズを取得する
            subdiaSize = 0;
            superdiaSize = 0;
            // Note: c == rowcolSize - 1は除く
            for (int c = 0; c < rowcolSize - 1; c++)
            {
                if (subdiaSize >= (rowcolSize - 1 - c))
                {
                    break;
                }
                int cnt = 0;
                for (int r = rowcolSize - 1; r >= c + 1; r--)
                {
                    // 非０要素が見つかったら抜ける
                    if (matPattern[r, c])
                    {
                        cnt = r - c;
                        break;
                    }
                }
                if (cnt > subdiaSize)
                {
                    subdiaSize = cnt;
                }
            }
            // Note: c == 0は除く
            for (int c = rowcolSize - 1; c >= 1; c--)
            {
                if (superdiaSize >= c)
                {
                    break;
                }
                int cnt = 0;
                for (int r = 0; r <= c - 1; r++)
                {
                    // 非０要素が見つかったら抜ける
                    if (matPattern[r, c])
                    {
                        cnt = c - r;
                        break;
                    }
                }
                if (cnt > superdiaSize)
                {
                    superdiaSize = cnt;
                }
            }
            Console.WriteLine("rowcolSize: {0} subdiaSize: {1} superdiaSize: {2}", rowcolSize, subdiaSize, superdiaSize);
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
            Constants.FemElementShapeDV elemShapeDv;
            int order;
            int vertexCnt;
            FemMeshLogic.GetElementShapeDvAndOrderByElemNodeCnt(element.NodeNumbers.Length, out elemShapeDv, out order, out vertexCnt);

            if (elemShapeDv == Constants.FemElementShapeDV.Triangle && order == Constants.SecondOrder)
            {
                // ２次三角形要素の要素行列を全体行列に加算する
                FemMat_Tri_Second.AddElementMat(
                    waveLength,
                    toSorted,
                    element,
                    Nodes,
                    Medias,
                    ForceNodeNumberH,
                    WaveModeDv,
                    ref mat);
            }
            else if (elemShapeDv == Constants.FemElementShapeDV.QuadType2 && order == Constants.SecondOrder)
            {
                // ２次四角形要素の要素行列を全体行列に加算する
                FemMat_Quad_Second.AddElementMat(
                    waveLength,
                    toSorted,
                    element,
                    Nodes,
                    Medias,
                    ForceNodeNumberH,
                    WaveModeDv,
                    ref mat);
            }
            else if (elemShapeDv == Constants.FemElementShapeDV.Triangle && order == Constants.FirstOrder)
            {
                // １次三角形要素の要素行列を全体行列に加算する
                FemMat_Tri_First.AddElementMat(
                    waveLength,
                    toSorted,
                    element,
                    Nodes,
                    Medias,
                    ForceNodeNumberH,
                    WaveModeDv,
                    ref mat);
            }
            else if (elemShapeDv == Constants.FemElementShapeDV.QuadType2 && order == Constants.FirstOrder)
            {
                // １次四角形要素の要素行列を全体行列に加算する
                FemMat_Quad_First.AddElementMat(
                    waveLength,
                    toSorted,
                    element,
                    Nodes,
                    Medias,
                    ForceNodeNumberH,
                    WaveModeDv,
                    ref mat);
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
            /*
            for (int inoB = 0; inoB < nodeCnt; inoB++)
            {
                for (int jnoB = 0; jnoB < nodeCnt; jnoB++)
                {
                    matB[inoB, jnoB] = new Complex();
                }
            }
             */
            for (int imode = 0; imode < maxMode; imode++)
            {
                Complex betam = eigenValues[imode];

                Complex[] fmVec = MyMatrixUtil.matrix_GetRowVec(eigenVecs, (int)imode);
                // 2Dの境界積分
                Complex[] veci = MyMatrixUtil.product(ryy_1d, fmVec);
                // モード電力規格化の積分(1Dの積分)
                //Complex[] vecj = MyMatrixUtil.product(MyMatrixUtil.matrix_ConjugateTranspose(ryy_1d), MyMatrixUtil.vector_Conjugate(fmVec));
                // [ryy]が実数の場合
                Complex[] vecj = MyMatrixUtil.product(ryy_1d, MyMatrixUtil.vector_Conjugate(fmVec));
                for (int inoB = 0; inoB < nodeCnt; inoB++)
                {
                    for (int jnoB = 0; jnoB < nodeCnt; jnoB++)
                    {
                        Complex cvalue = (Complex.ImaginaryOne / (omega * myu0)) * betam * Complex.Abs(betam) * veci[inoB] * vecj[jnoB];
                        //matB[inoB, jnoB] += cvalue;
                        matB._body[inoB + jnoB * matB.RowSize] += cvalue;
                    }
                }
            }
            //MyMatrixUtil.printMatrix("matB", matB);

            // 残差ベクトルの作成
            Complex[] resVecB = new Complex[nodeCnt];
            /*
            for (int inoB = 0; inoB < nodeCnt; inoB++)
            {
                resVecB[inoB] = 0.0;
            }
             */
            if (isInputPort)
            {
                int imode = IncidentModeIndex;
                Complex betam = eigenValues[imode];
                Complex[] fmVec = MyMatrixUtil.matrix_GetRowVec(eigenVecs, (int)imode);
                // 2Dの境界積分
                Complex[] veci = MyMatrixUtil.product(ryy_1d, fmVec);
                for (int inoB = 0; inoB < nodeCnt; inoB++)
                {
                    Complex cvalue = 2.0 * Complex.ImaginaryOne * betam * veci[inoB];
                    //resVecB[inoB] = cvalue;
                    resVecB[inoB].Real = cvalue.Real;
                    resVecB[inoB].Imaginary = cvalue.Imaginary;
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

                    //mat[inoGlobal, jnoGlobal] += matB[inoB, jnoB];
                    //mat._body[inoGlobal + jnoGlobal * mat.RowSize] += matB._body[inoB + jnoB * matB.RowSize];
                    // Note: matBは一般行列 matはバンド行列
                    mat._body[mat.GetBufferIndex(inoGlobal, jnoGlobal)] += matB._body[inoB + jnoB * matB.RowSize];
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

            // {tmp_vec}*t = {fm}*t[ryy]*t
            // {tmp_vec}* = [ryy]* {fm}*
            //   ([ryy]*)t = [ryy]*
            //    [ryy]が実数のときは、[ryy]* -->[ryy]
            Complex[] fmVec = MyMatrixUtil.matrix_GetRowVec(eigenVecs, iMode);
            //Complex[] tmp_vec = MyMatrixUtil.product(MyMatrixUtil.matrix_ConjugateTranspose(ryy_1d), MyMatrixUtil.vector_Conjugate(fmVec));
            // ryyが実数のとき
            Complex[] tmp_vec = MyMatrixUtil.product(ryy_1d, MyMatrixUtil.vector_Conjugate(fmVec));

            // s11 = {tmp_vec}t {value_all}
            s11 = MyMatrixUtil.vector_Dot(tmp_vec, valuesB);
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
        private void solvePortWaveguideEigen(double waveLength, int portNo, int maxModeSpecified, out int[] nodesBoundary, out MyDoubleMatrix ryy_1d, out Complex[] eigenValues, out Complex[,] eigenVecs)
        {
            //Console.WriteLine("solvePortWaveguideEigen: {0},{1}", waveLength, portNo);
            nodesBoundary = null;
            ryy_1d = null;
            eigenValues = null;
            eigenVecs = null;

            // 2D次元数
            const int ndim2d = Constants.CoordDim2D; //2;
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
            IList<FemLineElement> elements = new List<FemLineElement>();
            // 1D節点番号リスト（ソート済み）
            IList<int> sortedNodes = new List<int>();
            // 1D節点番号→ソート済みリストインデックスのマップ
            Dictionary<int, int> toSorted = new Dictionary<int, int>();

            // 2Dの要素から次数を取得する
            Constants.FemElementShapeDV elemShapeDv2d;
            int order;
            int vertexCnt2d;
            FemMeshLogic.GetElementShapeDvAndOrderByElemNodeCnt(Elements[0].NodeNumbers.Length, out elemShapeDv2d, out order, out vertexCnt2d);

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
                double x = FemMeshLogic.GetDistance(coord, coord0);
                //Console.WriteLine("{0},{1},{2},{3}", nodeIndex, coord[0], coord[1], x);
                coords.Add(x);
            }

            // 線要素を作成する
            if (order == Constants.FirstOrder)
            {
                // １次線要素
                FemMat_Line_First.MkElements(
                    nodes,
                    EdgeToElementNoH,
                    Elements,
                    ref elements);
            }
            else
            {
                // ２次線要素
                FemMat_Line_Second.MkElements(
                    nodes,
                    EdgeToElementNoH,
                    Elements,
                    ref elements);
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
            int maxMode = maxModeSpecified;
            if (maxMode > nodeCnt)
            {
                maxMode = nodeCnt;
            }
            eigenValues = new Complex[maxMode];
            eigenVecs = new Complex[maxMode, nodeCnt];
            // 固有モード解析でのみ使用するuzz_1d, txx_1d
            MyDoubleMatrix txx_1d = new MyDoubleMatrix(nodeCnt, nodeCnt);
            MyDoubleMatrix uzz_1d = new MyDoubleMatrix(nodeCnt, nodeCnt);
            // ryy_1dマトリクス (線要素)
            ryy_1d = new MyDoubleMatrix(nodeCnt, nodeCnt);

            /*
            for (int i = 0; i < nodeCnt; i++)
            {
                for (int j = 0; j < nodeCnt; j++)
                {
                    ryy_1d[i, j] = 0.0;
                    txx_1d[i, j] = 0.0;
                    uzz_1d[i, j] = 0.0;
                }
            }
            */
            for (int elemIndex = 0; elemIndex < elements.Count; elemIndex++)
            {
                // 線要素
                FemLineElement element = elements[elemIndex];

                // 1Dヘルムホルツ方程式固有値問題の要素行列を加算する
                if (order == Constants.FirstOrder)
                {
                    // １次線要素
                    FemMat_Line_First.AddElementMatOf1dEigenValueProblem(
                        element,
                        coords,
                        toSorted,
                        Medias,
                        WaveModeDv,
                        ref txx_1d, ref ryy_1d, ref uzz_1d);
                }
                else
                {
                    // ２次線要素
                    FemMat_Line_Second.AddElementMatOf1dEigenValueProblem(
                        element,
                        coords,
                        toSorted,
                        Medias,
                        WaveModeDv,
                        ref txx_1d, ref ryy_1d, ref uzz_1d);
                }
            }

            // [A] = [Txx] - k0 * k0 *[Uzz]
            //MyDoubleMatrix matA = minus(txx_1d, product((k0 * k0), uzz_1d));
            //メモリ節約
            MyDoubleMatrix matA = new MyDoubleMatrix(nodeCnt, nodeCnt);
            for (int ino = 0; ino < nodeCnt; ino++)
            {
                for (int jno = 0; jno < nodeCnt; jno++)
                {
                    // 剛性行列
                    matA[ino, jno] = txx_1d[ino, jno] - (k0 * k0) * uzz_1d[ino, jno];
                }
            }
            // 定式化のBUGFIX
            //matA = MyMatrixUtil.product(MyMatrixUtil.matrix_Inverse(ryy_1d), matA);
            // ( [txx] - k0^2[uzz] + β^2[ryy]){Ez} = {0}より
            // [A]{x} = λ[B]{x}としたとき、λ = β^2 とすると[B] = -[ryy]
            MyDoubleMatrix matB = MyMatrixUtil.product(-1.0, ryy_1d);
            // ([B])-1[A]
            matA = MyMatrixUtil.product(MyMatrixUtil.matrix_Inverse(matB), matA);

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
                // 定式化BUGFIX
                //   減衰定数は符号がマイナス(β = -jα)
                if (betam.Imaginary >= 0.0)
                {
                    betam = new Complex(betam.Real, -betam.Imaginary);
                }
                // 固有ベクトル
                Complex[] evec = MyMatrixUtil.matrix_GetRowVec(evecs, tagtModeIdx);
                // 規格化定数を求める
                //Complex[] workVec = MyMatrixUtil.product(MyMatrixUtil.matrix_ConjugateTranspose(ryy_1d), evec);
                // 実数の場合 [ryy]*t = [ryy]t ryyは対称行列より[ryy]t = [ryy]
                Complex[] workVec = MyMatrixUtil.product(ryy_1d, evec);
                double dm = Complex.Abs(MyMatrixUtil.vector_Dot(MyMatrixUtil.vector_Conjugate(evec), workVec));
                dm = Math.Sqrt(omega * myu0 / Complex.Abs(betam) / dm);
                //Console.WriteLine("dm = " + dm);

                // 伝搬定数の格納
                eigenValues[imode] = betam;
                // check
                if (imode < 5)
                {
                    //Console.WriteLine("eigenValues [ " + imode + "] = " + betam.Real + " + " + betam.Imaginary + " i " + " tagtModeIdx :" + tagtModeIdx + " " );
                    Console.WriteLine("β/k0 [ " + imode + "] = " + betam.Real / k0 + " + " + betam.Imaginary / k0 + " i " + " tagtModeIdx :" + tagtModeIdx + " ");
                }
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
        /// Lisys(Lapack)による固有値解析(複素数版)
        /// </summary>
        private static bool solveEigen(MyComplexMatrix srcMat, out Complex[] evals, out Complex[,] evecs)
        {
            // Lisys(Lapack)による固有値解析
            Complex[] X = MyMatrixUtil.matrix_ToBuffer(srcMat, false);
            Complex[] c_evals = null;
            Complex[][] c_evecs = null;
            KrdLab.clapack.FunctionExt.zgeev(X, srcMat.RowSize, srcMat.ColumnSize, ref c_evals, ref c_evecs);

            /*
            evals = new Complex[c_evals.Length];
            for (int i = 0; i < evals.Length; i++)
            {
                evals[i] = c_evals[i];
                //Console.WriteLine("( " + i + " ) = " + evals[i].Real + " + " + evals[i].Imaginary + " i ");
            }
             */
            evals = c_evals;
            evecs = new Complex[c_evecs.Length, c_evecs[0].Length];
            for (int i = 0; i < evecs.GetLength(0); i++)
            {
                for (int j = 0; j < evecs.GetLength(1); j++)
                {
                    evecs[i, j] = c_evecs[i][j];
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
                //evals[i] = new Complex(r_evals[i], i_evals[i]);
                evals[i].Real = r_evals[i];
                evals[i].Imaginary = i_evals[i];
                //Console.WriteLine("( " + i + " ) = " + evals[i].Real + " + " + evals[i].Imaginary + " i ");
            }
            evecs = new Complex[r_evecs.Length, r_evecs[0].Length];
            for (int i = 0; i < evecs.GetLength(0); i++)
            {
                for (int j = 0; j < evecs.GetLength(1); j++)
                {
                    //evecs[i, j] = new Complex(r_evecs[i][j], i_evecs[i][j]);
                    evecs[i, j].Real = r_evecs[i][j];
                    evecs[i, j].Imaginary = i_evecs[i][j];
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
            /*
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
             */

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
    }
}
