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
    /// ２次四角形要素
    /// </summary>
    class FemQuadElement : FemElement
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FemQuadElement()
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

            const int ndim = Constants.CoordDim2D; //2;      // 座標の次元数
            const int vertexCnt = Constants.QuadVertexCnt; //3; // 四角形形の頂点の数(2次要素でも同じ)
            //const int nodeCnt = Constants.QuadNodeCnt_SecondOrder_Type2; //8;  // 四角形2次要素
            int nodeCnt = NodeNumbers.Length;
            if (nodeCnt != Constants.QuadNodeCnt_SecondOrder_Type2 && nodeCnt != Constants.QuadNodeCnt_FirstOrder)
            {
                return;
            }
            // 四角形の頂点を取得
            double[][] pp = new double[vertexCnt][];
            for (int ino = 0; ino < pp.GetLength(0); ino++)
            {
                FemNode node = _Nodes[ino];
                System.Diagnostics.Debug.Assert(node.Coord.Length == ndim);
                pp[ino] = new double[ndim];
                pp[ino][0] = node.Coord[0] * delta.Width + ofs.Width;
                pp[ino][1] = regionSize.Height - node.Coord[1] * delta.Height + ofs.Height;
            }
            // 四角形内部を四角形で分割
            // 要素節点座標( 局所r,s成分 )
            //        s
            //        |
            //    3+  6  +2
            //    |   |   |
            // ---7---+---5-->r
            //    |   |   |
            //    0+  4  +1
            //        |
            //
            double[][] n_pts = 
                {
                    // r, s
                    new double[] {-1.0, -1.0},  //0
                    new double[] { 1.0, -1.0},  //1
                    new double[] { 1.0,  1.0},  //2
                    new double[] {-1.0,  1.0},  //3
                    new double[] {   0, -1.0},  //4
                    new double[] { 1.0,    0},  //5
                    new double[] {   0,  1.0},  //6
                    new double[] {-1.0,    0},  //7
                };

            int ndiv = 4;
            double defdr = 2.0 / (double)ndiv;
            double defds = defdr;
            for (int i1 = 0; i1 < ndiv; i1++)
            {
                double r =  - 1.0 + i1 * defdr;
                double rNext = r + defdr;
                for (int i2 = 0; i2 < ndiv; i2++)
                {
                    double s = -1.0 + i2 * defds;
                    double sNext = s + defds;

                    // 四角形の頂点
                    const int rectVCnt = 4;
                    double[][] rect_local_p = new double[rectVCnt][]
                    {
                        new double[]{r    , s    },
                        new double[]{rNext, s    },
                        new double[]{rNext, sNext},
                        new double[]{r    , sNext}
                    };
                    double[][] rectpp = new double[rectVCnt][];
                    for (int ino = 0; ino < rectVCnt; ino++)
                    {
                        double work_r = rect_local_p[ino][0];
                        double work_s = rect_local_p[ino][1];
                        double xx = 0.0;
                        double yy = 0.0;
                        for (int k = 0; k < vertexCnt; k++)
                        {
                            double ri = n_pts[k][0];
                            double si = n_pts[k][1];
                            xx += pp[k][0] * 0.25 * (1 + ri * work_r) * (1 + si * work_s);
                            yy += pp[k][1] * 0.25 * (1 + ri * work_r) * (1 + si * work_s);
                        }
                        rectpp[ino] = new double[] { xx, yy };
                    }
                    // 表示する位置
                    double[] disp_p = new double[] { (rect_local_p[0][0] + rect_local_p[1][0]) * 0.5, (rect_local_p[0][1] + rect_local_p[3][1]) * 0.5 };

                    // 表示する値
                    Complex cvalue = new Complex(0.0, 0.0);
                    // 表示する位置の形状関数値
                    double[] workN = new double[nodeCnt];
                    if (nodeCnt == Constants.QuadNodeCnt_FirstOrder)
                    {
                        double work_r = disp_p[0];
                        double work_s = disp_p[1];
                        for (int i = 0; i < 4; i++)
                        {
                            // 節点の局所座標
                            double ri = n_pts[i][0];
                            double si = n_pts[i][1];
                            workN[i] = 0.25 * (1.0 + ri * work_r) * (1.0 + si * work_s);
                        }
                    }
                    else
                    {
                        double work_r = disp_p[0];
                        double work_s = disp_p[1];
                        // 節点0～3 : 四角形の頂点
                        for (int i = 0; i < 4; i++)
                        {
                            // 節点の局所座標
                            double ri = n_pts[i][0];
                            double si = n_pts[i][1];
                            // 形状関数N
                            workN[i] = 0.25 * (1.0 + ri * work_r) * (1.0 + si * work_s) * (ri * work_r + si * work_s - 1.0);
                        }
                        // 節点4,6 : r方向辺上中点
                        foreach (int i in new int[] { 4, 6 })
                        {
                            // 節点の局所座標
                            double ri = n_pts[i][0];
                            double si = n_pts[i][1];
                            // 形状関数N
                            workN[i] = 0.5 * (1.0 - work_r * work_r) * (1.0 + si * work_s);
                        }
                        // 節点5,7 : s方向辺上中点
                        foreach (int i in new int[] { 5, 7 })
                        {
                            // 節点の局所座標
                            double ri = n_pts[i][0];
                            double si = n_pts[i][1];
                            // 形状関数N
                            workN[i] = 0.5 * (1.0 + ri * work_r) * (1.0 - work_s * work_s);
                        }
                    }
                    for (int k = 0; k < nodeCnt; k++)
                    {
                        cvalue += _FValues[k] * workN[k];
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
