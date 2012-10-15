using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
//using System.Numerics; // Complex
using KrdLab.clapack; // KrdLab.clapack.Complex

namespace HPlaneWGSimulator
{
    /// <summary>
    /// ２次三角形要素
    /// </summary>
    class FemTriElement : FemElement
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FemTriElement()
            : base()
        {
        }

        /// <summary>
        /// フィールド値を描画する
        /// </summary>
        /// <param name="g"></param>
        /// <param name="ofs"></param>
        /// <param name="delta"></param>
        /// <param name="regionSize"></param>
        /// <param name="colorMap"></param>
        public override void DrawField(Graphics g, Size ofs, Size delta, Size regionSize, ColorMap colorMap)
        {
            //base.DrawField(g, ofs, delta, regionSize, colorMap);
            if (_Nodes == null || _FValues == null)
            {
                return;
            }

            const int ndim = Constants.CoordDim2D; //2;      // 座標の次元数
            const int vertexCnt = Constants.TriVertexCnt; //3; // 三角形の頂点の数(2次要素でも同じ)
            //const int nodeCnt = Constants.TriNodeCnt_SecondOrder; //6;  // 三角形2次要素
            int nodeCnt = NodeNumbers.Length;
            if (nodeCnt != Constants.TriNodeCnt_SecondOrder && nodeCnt != Constants.TriNodeCnt_FirstOrder)
            {
                return;
            }
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

            // 下記分割ロジックの原点となる頂点
            //   頂点0固定で計算していたが、原点の内角が直角のとき長方形メッシュになるので原点を2（頂点を0,1,2としたとき）にする
            int orginVertexNo = 2;
            // 内角が最大の頂点を取得し、その頂点を原点とする(後のロジックは原点が頂点を0,1,2としたとき、2になっている
            {
                double minCosth = double.MaxValue;
                int minCosthVertexNo = 0;
                for (int ino = 0; ino < vertexCnt; ino++)
                {
                    const int vecCnt = 2;
                    double[][] vec = new double[vecCnt][] { new double[ndim]{0, 0}, new double[ndim]{0, 0} };
                    double[] len = new double[vecCnt];
                    double costh;
                    {
                        int n1 = ino;
                        int n2 = (ino + 1) % 3;
                        int n3 = (ino + 2) % 3;
                        vec[0][0] = pp[n2][0] - pp[n1][0];
                        vec[0][1] = pp[n2][1] - pp[n1][1];
                        vec[1][0] = pp[n3][0] - pp[n1][0];
                        vec[1][1] = pp[n3][1] - pp[n1][1];
                        len[0] = FemMeshLogic.GetDistance(pp[n1], pp[n2]);
                        len[1] = FemMeshLogic.GetDistance(pp[n1], pp[n3]);
                        costh = (vec[0][0] * vec[1][0] + vec[0][1] * vec[1][1]) / (len[0] * len[1]);
                        if (costh < minCosth)
                        {
                            minCosth = costh;
                            minCosthVertexNo = ino;
                        }
                    }
                }
                orginVertexNo = (minCosthVertexNo + 2) % 3;
            }
            // 三角形内部を四角形で分割
            // 面積座標L1方向分割数
            //int ndiv = 4;
            int ndiv = Constants.TriDrawFieldMshDivCnt;
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
                            xx += pp[k][0] * vLpp[(k + orginVertexNo) % vertexCnt];
                            yy += pp[k][1] * vLpp[(k + orginVertexNo) % vertexCnt];
                        }
                        rectpp[ino] = new double[] { xx, yy };
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
                    double[] vNi = null;
                    if (nodeCnt == Constants.TriNodeCnt_FirstOrder)
                    {
                        vNi = new double[]
                            {
                                vLi[(0 + orginVertexNo) % vertexCnt],
                                vLi[(1 + orginVertexNo) % vertexCnt],
                                vLi[(2 + orginVertexNo) % vertexCnt]
                            };
                    }
                    else
                    {
                        double[] workLi = new double[vertexCnt];
                        for (int i = 0; i < vertexCnt; i++)
                        {
                            workLi[i] = vLi[(i + orginVertexNo) % vertexCnt];
                        }
                        vNi = new double[]
                            {
                                workLi[0] * (2.0 * workLi[0] - 1.0),
                                workLi[1] * (2.0 * workLi[1] - 1.0),
                                workLi[2] * (2.0 * workLi[2] - 1.0),
                                4.0 * workLi[0] * workLi[1],
                                4.0 * workLi[1] * workLi[2],
                                4.0 * workLi[2] * workLi[0],
                            };
                    }
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
