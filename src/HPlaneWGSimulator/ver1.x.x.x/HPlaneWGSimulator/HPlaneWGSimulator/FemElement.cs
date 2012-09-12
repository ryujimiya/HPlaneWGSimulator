using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Numerics; // Complex

namespace HPlaneWGSimulator
{
    /// <summary>
    /// Fem要素クラス
    /// </summary>
    class FemElement
    {
        /// <summary>
        /// 要素番号
        /// </summary>
        public int No;
        /// <summary>
        /// 要素節点1-3の全体節点番号
        /// </summary>
        public int[] NodeNumbers;
        /// <summary>
        /// 媒質インデックス
        /// </summary>
        public int MediaIndex;
        /// <summary>
        /// 節点(クラス内部で使用)
        /// </summary>
        private FemNode[] _Nodes;
        /// <summary>
        /// フィールド値(クラス内部で使用)
        /// </summary>
        private Complex[] _FValues;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FemElement()
        {
            No = 0;
            NodeNumbers = null;
            MediaIndex = 0;
            _Nodes = null;
            _FValues = null;
        }

        /// <summary>
        /// コピー
        /// </summary>
        /// <param name="src"></param>
        public void CP(FemElement src)
        {
            if (src == this)
            {
                return;
            }
            No = src.No;
            NodeNumbers = null;
            if (src.NodeNumbers != null)
            {
                NodeNumbers = new int[src.NodeNumbers.Length];
                for (int i = 0; i < src.NodeNumbers.Length; i++)
                {
                    NodeNumbers[i] = src.NodeNumbers[i];
                }
            }
            MediaIndex = src.MediaIndex;
        }

        /// <summary>
        /// 節点情報をセットする
        /// </summary>
        /// <param name="nodes">節点情報配列（強制境界を含む全節点を節点番号順に格納した配列)</param>
        public void SetNodesFromAllNodes(FemNode[] nodes)
        {
            _Nodes = new FemNode[NodeNumbers.Length];
            for (int i = 0; i < NodeNumbers.Length; i++)
            {
                int nodeNumber = NodeNumbers[i];
                _Nodes[i] = nodes[nodeNumber - 1];
                System.Diagnostics.Debug.Assert(nodeNumber == _Nodes[i].No);
            }
        }

        /// <summary>
        /// フィールド値をセットする
        /// </summary>
        /// <param name="valuesAll"></param>
        /// <param name="nodesRegionToIndex"></param>
        public void SetFieldValueFromAllValues(Complex[] valuesAll, Dictionary<int, int> nodesRegionToIndex)
        {
            _FValues = new Complex[NodeNumbers.Length];
            for (int ino = 0; ino < NodeNumbers.Length; ino++)
            {
                int nodeNumber = NodeNumbers[ino];
                if (nodesRegionToIndex.ContainsKey(nodeNumber))
                {
                    int nodeIndex = nodesRegionToIndex[nodeNumber];
                    _FValues[ino] = valuesAll[nodeIndex];
                }
                else
                {
                    // 強制境界とみなす
                    _FValues[ino] = new Complex();
                }
            }
        }

        /// <summary>
        /// 要素境界を描画する
        /// </summary>
        /// <param name="g"></param>
        /// <param name="panel"></param>
        public void Draw(Graphics g, Size ofs, Size delta, Size regionSize)
        {
            const int vertexCnt = Constants.TriVertexCnt; //3; // 三角形の頂点の数(2次要素でも同じ)
            // 三角形の頂点を取得
            Point[] points = new Point[vertexCnt];
            for (int ino = 0; ino < vertexCnt; ino++)
            {
                FemNode node = _Nodes[ino];
                System.Diagnostics.Debug.Assert(node.Coord.Length == 2);
                int x = (int)((double)node.Coord[0] * delta.Width);
                int y = (int)(regionSize.Height - (double)node.Coord[1] * delta.Height);
                points[ino] = new Point(x, y) + ofs;
            }
            // 三角形を描画
            using (Pen selectedPen = new Pen(Color.Black, 1))
            {
                //selectedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                g.DrawPolygon(selectedPen, points);
            }
        }

        public void DrawField(Graphics g, Size ofs, Size delta, Size regionSize, ColorMap colorMap)
        {
            const int ndim = Constants.CoordDim2D; //2;      // 座標の次元数
            const int vertexCnt = Constants.TriVertexCnt; //3; // 三角形の頂点の数(2次要素でも同じ)
            const int nodeCnt = Constants.TriNodeCnt_SecondOrder; //6;  // 三角形2次要素
            // 三角形の頂点を取得
            double[][] pp = new double[vertexCnt][];
            for (int ino = 0; ino < pp.GetLength(0); ino++)
            {
                FemNode node = _Nodes[ino];
                System.Diagnostics.Debug.Assert(node.Coord.Length == ndim);
                pp[ino] = new double[ndim];
                pp[ino][0] = node.Coord[0] * delta.Width + ofs.Width;
                pp[ino][1] = regionSize.Height - node.Coord[1] * delta.Height + ofs.Height;
            }
            // 三角形内部を四角形で分割
            // 面積座標L1方向分割数
            int ndiv = 4;
            double defdL1 = 1.0 / (double)ndiv;
            double defdL2 = defdL1;
            for (int i1 = 0; i1 < ndiv; i1++)
            {
                double vL1 = i1 * defdL1;
                double vL1Next = (i1 + 1) * defdL1;
                if (i1 == ndiv - 1)
                {
                    vL1Next = 1.0;
                }
                double vL2max = 1.0 - vL1;
                if (vL2max < 0.0)
                {
                    // ERROR
                    Console.WriteLine("logic error vL2max = {0}", vL2max);
                    continue;
                }
                double fdiv2 = (double)ndiv * vL2max;
                int ndiv2 = (int)fdiv2;
                if (fdiv2 - (double)ndiv2 > Constants.PrecisionLowerLimit)
                {
                    ndiv2++;
                }
                for (int i2 = 0; i2 < ndiv2; i2++)
                {
                    double vL2 = i2 * defdL2;
                    double vL2Next = (i2 + 1) * defdL2;
                    if (i2 == ndiv2 - 1)
                    {
                        vL2Next = vL2max;
                    }
                    double vL3 = 1.0 - vL1 - vL2;
                    if (vL3 < 0.0)
                    {
                        // ERROR
                        Console.WriteLine("logic error vL3 = {0}", vL3);
                        continue;
                    }

                    // 四角形の頂点
                    const int rectVCnt = 4;
                    double[][] rectLi = new double[rectVCnt][]
                    {
                        new double[]{vL1    , vL2    , 0},
                        new double[]{vL1Next, vL2    , 0},
                        new double[]{vL1Next, vL2Next, 0},
                        new double[]{vL1    , vL2Next, 0}
                    };
                    if ((i1 == ndiv - 1) || (i2 == ndiv2 - 1))
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            rectLi[2][k] = rectLi[3][k];
                        }
                    }
                    double[][] rectpp = new double[rectVCnt][];
                    for (int ino = 0; ino < rectVCnt; ino++)
                    {
                        if (rectLi[ino][0] < 0.0)
                        {
                            rectLi[ino][0] = 0.0;
                            Console.WriteLine("logical error rectLi[{0}][0] = {1}", ino, rectLi[ino][0]);
                        }
                        if (rectLi[ino][0] > 1.0)
                        {
                            rectLi[ino][0] = 1.0;
                            Console.WriteLine("logical error rectLi[{0}][0] = {1}", ino, rectLi[ino][0]);
                        }
                        if (rectLi[ino][1] < 0.0)
                        {
                            rectLi[ino][1] = 0.0;
                            Console.WriteLine("logical error rectLi[{0}][1] = {1}", ino, rectLi[ino][1]);
                        }
                        if (rectLi[ino][1] > (1.0 - rectLi[ino][0]))  // L2最大値(1 - L1)チェック
                        {
                            rectLi[ino][1] = 1.0 - rectLi[ino][0];
                        }
                        rectLi[ino][2] = 1.0 - rectLi[ino][0] - rectLi[ino][1];
                        if (rectLi[ino][2] < 0.0)
                        {
                            Console.WriteLine("logical error rectLi[{0}][2] = {1}", ino, rectLi[ino][2]);
                        }
                    }
                    for (int ino = 0; ino < rectVCnt; ino++)
                    {
                        double[] vLpp = rectLi[ino];
                        double xx = 0.0;
                        double yy = 0.0;
                        for (int k = 0; k < vertexCnt; k++)
                        {
                            xx += pp[k][0] * vLpp[k];
                            yy += pp[k][1] * vLpp[k];
                        }
                        rectpp[ino] = new double[]{ xx, yy };
                    }
                    // 表示する位置
                    double[] vLi = new double[] { (rectLi[0][0] + rectLi[1][0]) * 0.5, (rectLi[0][1] + rectLi[3][1]) * 0.5, 0 };
                    if (vLi[0] < 0.0)
                    {
                        vLi[0] = 0.0;
                    }
                    if (vLi[0] > 1.0)
                    {
                        vLi[0] = 1.0;
                    }
                    if (vLi[1] < 0.0)
                    {
                        vLi[1] = 0.0;
                    }
                    if (vLi[1] > (1.0 - vLi[0]))
                    {
                        vLi[1] = (1.0 - vLi[0]);
                    }
                    vLi[2] = 1.0 - vLi[0] - vLi[1];
                    if (vLi[2] < 0.0)
                    {
                        Console.WriteLine("logic error vLi[2] = {0}", vLi[2]);
                    }

                    // 表示する値
                    Complex cvalue = new Complex(0.0, 0.0);
                    // 表示する位置の形状関数値
                    double[] vNi = new double[nodeCnt]
                    {
                        vLi[0] * (2.0*vLi[0] - 1.0),
                        vLi[1] * (2.0*vLi[1] - 1.0),
                        vLi[2] * (2.0*vLi[2] - 1.0),
                        4.0 * vLi[0] * vLi[1],
                        4.0 * vLi[1] * vLi[2],
                        4.0 * vLi[2] * vLi[0],
                    };
                    for (int k = 0; k < nodeCnt; k++)
                    {
                        cvalue += _FValues[k] * vNi[k];
                    }
                    // 四角形の頂点（描画用）
                    Point[] rectp = new Point[rectVCnt];
                    for (int ino = 0; ino < rectVCnt; ino++)
                    {
                        rectp[ino] = new Point((int)rectpp[ino][0], (int)rectpp[ino][1]);
                    }
                    try
                    {
                        // 塗りつぶし色の取得
                        Color fillColor = colorMap.GetColor(Complex.Abs(cvalue));
                        // 塗りつぶし
                        using (Brush brush = new SolidBrush(fillColor))
                        {
                            g.FillPolygon(brush, rectp);
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message + " " + exception.StackTrace);
                    }                    
                }
            }
        }
    }
}
