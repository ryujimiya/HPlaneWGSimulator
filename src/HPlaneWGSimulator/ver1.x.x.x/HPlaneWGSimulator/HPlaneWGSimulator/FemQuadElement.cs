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
        /// フィールドの回転を取得する
        /// </summary>
        /// <param name="rotXFValues"></param>
        /// <param name="rotYFValues"></param>
        protected override void calcRotField(out Complex[] rotXFValues, out Complex[] rotYFValues)
        {
            base.calcRotField(out rotXFValues, out rotYFValues);

            rotXFValues = new Complex[NodeNumbers.Length];
            rotYFValues = new Complex[NodeNumbers.Length];

            const int ndim = Constants.CoordDim2D; //2;      // 座標の次元数
            //const int vertexCnt = Constants.QuadVertexCnt; //4; // 四角形形の頂点の数(2次要素でも同じ)
            //const int nodeCnt = Constants.QuadNodeCnt_SecondOrder_Type2; //8;  // 四角形形2次要素
            int nodeCnt = NodeNumbers.Length;
            if (nodeCnt != Constants.QuadNodeCnt_SecondOrder_Type2 && nodeCnt != Constants.QuadNodeCnt_FirstOrder)
            {
                return;
            }

            // 四角形の節点座標を取得
            double[][] pp = new double[nodeCnt][];
            for (int ino = 0; ino < pp.GetLength(0); ino++)
            {
                FemNode node = _Nodes[ino];
                System.Diagnostics.Debug.Assert(node.Coord.Length == ndim);
                pp[ino] = new double[ndim];
                pp[ino][0] = node.Coord[0];
                pp[ino][1] = node.Coord[1];
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

            // 節点上のrot(Ez)を求める
            int nno = nodeCnt;
            for (int ino = 0; ino < nno; ino++)
            {
                double r = n_pts[ino][0];
                double s = n_pts[ino][1];

                // 形状関数
                double[] N = new double[nno];
                // 形状関数のr, s方向微分
                double[] dNdr = new double[nno];
                double[] dNds = new double[nno];
                if (nodeCnt == Constants.QuadNodeCnt_SecondOrder_Type2)
                {
                    // 節点0～3 : 四角形の頂点
                    for (int i = 0; i < 4; i++)
                    {
                        // 節点の局所座標
                        double ri = n_pts[i][0];
                        double si = n_pts[i][1];
                        // 形状関数N
                        N[i] = 0.25 * (1.0 + ri * r) * (1.0 + si * s) * (ri * r + si * s - 1.0);
                        // 形状関数のr方向微分
                        dNdr[i] = 0.25 * ri * (1.0 + si * s) * (2.0 * ri * r + si * s);
                        // 形状関数のs方向微分
                        dNds[i] = 0.25 * si * (1.0 + ri * r) * (ri * r + 2.0 * si * s);
                    }
                    // 節点4,6 : r方向辺上中点
                    foreach (int i in new int[] { 4, 6 })
                    {
                        // 節点の局所座標
                        double ri = n_pts[i][0];
                        double si = n_pts[i][1];
                        // 形状関数N
                        N[i] = 0.5 * (1.0 - r * r) * (1.0 + si * s);
                        // 形状関数のr方向微分
                        dNdr[i] = -1.0 * r * (1.0 + si * s);
                        // 形状関数のs方向微分
                        dNds[i] = 0.5 * si * (1.0 - r * r);
                    }
                    // 節点5,7 : s方向辺上中点
                    foreach (int i in new int[] { 5, 7 })
                    {
                        // 節点の局所座標
                        double ri = n_pts[i][0];
                        double si = n_pts[i][1];
                        // 形状関数N
                        N[i] = 0.5 * (1.0 + ri * r) * (1.0 - s * s);
                        // 形状関数のr方向微分
                        dNdr[i] = 0.5 * ri * (1.0 - s * s);
                        // 形状関数のs方向微分
                        dNds[i] = -1.0 * s * (1.0 + ri * r);
                    }
                }
                else if (nodeCnt == Constants.QuadNodeCnt_FirstOrder)
                {
                    // 節点0～3 : 四角形の頂点
                    for (int i = 0; i < nno; i++)
                    {
                        // 節点の局所座標
                        double ri = n_pts[i][0];
                        double si = n_pts[i][1];
                        // 形状関数N
                        N[i] = 0.25 * (1.0 + ri * r) * (1.0 + si * s);
                        // 形状関数のr方向微分
                        dNdr[i] = 0.25 * ri * (1.0 + si * s);
                        // 形状関数のs方向微分
                        dNds[i] = 0.25 * si * (1.0 + ri * r);
                    }
                }

                // ヤコビアン行列
                double j11;
                double j12;
                double j21;
                double j22;
                j11 = 0;
                j12 = 0;
                j21 = 0;
                j22 = 0;

                for (int i = 0; i < nno; i++)
                {
                    j11 += dNdr[i] * pp[i][0];
                    j12 += dNdr[i] * pp[i][1];
                    j21 += dNds[i] * pp[i][0];
                    j22 += dNds[i] * pp[i][1];
                }
                // ヤコビアン
                double detj = j11 * j22 - j12 * j21;

                // gradr[0] : gradrのx成分 grad[1] : gradrのy成分
                // grads[0] : gradsのx成分 grads[1] : gradsのy成分
                double[] gradr = new double[2];
                double[] grads = new double[2];
                gradr[0] = j22 / detj;
                gradr[1] = -j21 / detj;
                grads[0] = -j12 / detj;
                grads[1] = j11 / detj;

                // 形状関数のx, y方向微分
                double[,] dNdX = new double[ndim, nno];
                for (int i = 0; i < nno; i++)
                {
                    for (int direction = 0; direction < ndim; direction++)
                    {
                        dNdX[direction, i] = dNdr[i] * gradr[direction] + dNds[i] * grads[direction];
                    }
                }

                rotXFValues[ino] = new Complex();
                rotYFValues[ino] = new Complex();
                for (int k = 0; k < nodeCnt; k++)
                {
                    // (rot(Ez)x = dEz/dy
                    rotXFValues[ino] += _FValues[k] * dNdX[1, k];
                    // (rot(Ez)y = - dEz/dx
                    rotYFValues[ino] += -1.0 * _FValues[k] * dNdX[0, k];
                }
                // rot(Ez)を磁界の値に変換する
                rotXFValues[ino] *= _FactorForRot / _media_Q[0, 0];
                rotYFValues[ino] *= _FactorForRot / _media_Q[1, 1];
            }
        }

        /// <summary>
        /// フィールド値を描画する
        /// </summary>
        /// <param name="g"></param>
        /// <param name="ofs"></param>
        /// <param name="delta"></param>
        /// <param name="regionSize"></param>
        /// <param name="colorMap"></param>
        /// <param name="valueDv"></param>
        public override void DrawField(Graphics g, Size ofs, Size delta, Size regionSize, FemElement.FieldDV fieldDv, FemElement.ValueDV valueDv, ColorMap colorMap)
        {
            //base.DrawField(g, ofs, delta, regionSize, colorMap);
            if (_Nodes == null || _FValues == null || _RotXFValues == null || _RotYFValues == null || _PoyntingXFValues == null || _PoyntingYFValues == null)
            {
                return;
            }
            Complex[] tagtValues = null;
            if (fieldDv == FemElement.FieldDV.Field)
            {
                tagtValues = _FValues;
            }
            else if (fieldDv == FemElement.FieldDV.RotX)
            {
                tagtValues = _RotXFValues;
            }
            else if (fieldDv == FemElement.FieldDV.RotY)
            {
                tagtValues = _RotYFValues;
            }
            else
            {
                return;
            }

            const int ndim = Constants.CoordDim2D; //2;      // 座標の次元数
            const int vertexCnt = Constants.QuadVertexCnt; //3; // 四角形形の頂点の数(2次要素でも同じ)
            //const int nodeCnt = Constants.QuadNodeCnt_SecondOrder_Type2; //8;  // 四角形2次要素
            int nodeCnt = NodeNumbers.Length;
            if (nodeCnt != Constants.QuadNodeCnt_SecondOrder_Type2 && nodeCnt != Constants.QuadNodeCnt_FirstOrder)
            {
                return;
            }
            // 四角形節点座標を取得
            double[][] pp = new double[nodeCnt][];
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

            int ndiv = this.IsCoarseFieldMesh ? (Constants.TriDrawFieldMshDivCnt / 2) : Constants.TriDrawFieldMshDivCnt;
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
                        cvalue += tagtValues[k] * workN[k];
                    }
                    // 四角形の頂点（描画用）
                    Point[] rectp = new Point[rectVCnt];
                    for (int ino = 0; ino < rectVCnt; ino++)
                    {
                        rectp[ino] = new Point((int)rectpp[ino][0], (int)rectpp[ino][1]);
                    }
                    try
                    {
                        // 表示する値
                        double showValue = 0.0;
                        if (valueDv == ValueDV.Real)
                        {
                            showValue = cvalue.Real;
                        }
                        else if (valueDv == ValueDV.Imaginary)
                        {
                            showValue = cvalue.Imaginary;
                        }
                        else
                        {
                            // 既定値は絶対値
                            showValue = Complex.Abs(cvalue);
                        }
                        // 塗りつぶし色の取得
                        Color fillColor = colorMap.GetColor(showValue);
                        // 塗りつぶし
                        using (Brush brush = new SolidBrush(fillColor))
                        {
                            g.FillPolygon(brush, rectp);
                        }
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message + " " + exception.StackTrace);
                    }
                }
            }
        }

        /// <summary>
        /// フィールド値の回転を描画する
        /// </summary>
        /// <param name="g"></param>
        /// <param name="ofs"></param>
        /// <param name="delta"></param>
        /// <param name="regionSize"></param>
        /// <param name="drawColor"></param>
        /// <param name="fieldDv"></param>
        /// <param name="minRotFValue"></param>
        /// <param name="maxRotFValue"></param>
        public override void DrawRotField(Graphics g, Size ofs, Size delta, Size regionSize, Color drawColor, FemElement.FieldDV fieldDv, double minRotFValue, double maxRotFValue)
        {
            if (_Nodes == null || _FValues == null || _RotXFValues == null || _RotYFValues == null || _PoyntingXFValues == null || _PoyntingYFValues == null)
            {
                return;
            }
            Complex[] tagtXValues = null;
            Complex[] tagtYValues = null;
            if (fieldDv == FemElement.FieldDV.PoyntingXY)
            {
                tagtXValues = _PoyntingXFValues;
                tagtYValues = _PoyntingYFValues;
            }
            else if (fieldDv == FemElement.FieldDV.RotXY)
            {
                tagtXValues = _RotXFValues;
                tagtYValues = _RotYFValues;
            }
            else
            {
                return;
            }

            const int ndim = Constants.CoordDim2D; //2;      // 座標の次元数
            //const int vertexCnt = Constants.QuadVertexCnt; //4; // 四角形形の頂点の数(2次要素でも同じ)
            //const int nodeCnt = Constants.QuadNodeCnt_SecondOrder_Type2; //8;  // 四角形形2次要素
            int nodeCnt = NodeNumbers.Length;
            if (nodeCnt != Constants.QuadNodeCnt_SecondOrder_Type2 && nodeCnt != Constants.QuadNodeCnt_FirstOrder)
            {
                return;
            }

            // 四角形の節点座標を取得
            double[][] pp = new double[nodeCnt][];
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

            // 節点上のrot(Ez)を求める
            int nno = nodeCnt;
            {
                double r = 0;
                double s = 0;

                // 形状関数
                double[] N = new double[nno];
                if (nodeCnt == Constants.QuadNodeCnt_SecondOrder_Type2)
                {
                    // 節点0～3 : 四角形の頂点
                    for (int i = 0; i < 4; i++)
                    {
                        // 節点の局所座標
                        double ri = n_pts[i][0];
                        double si = n_pts[i][1];
                        // 形状関数N
                        N[i] = 0.25 * (1.0 + ri * r) * (1.0 + si * s) * (ri * r + si * s - 1.0);
                    }
                    // 節点4,6 : r方向辺上中点
                    foreach (int i in new int[] { 4, 6 })
                    {
                        // 節点の局所座標
                        double ri = n_pts[i][0];
                        double si = n_pts[i][1];
                        // 形状関数N
                        N[i] = 0.5 * (1.0 - r * r) * (1.0 + si * s);
                    }
                    // 節点5,7 : s方向辺上中点
                    foreach (int i in new int[] { 5, 7 })
                    {
                        // 節点の局所座標
                        double ri = n_pts[i][0];
                        double si = n_pts[i][1];
                        // 形状関数N
                        N[i] = 0.5 * (1.0 + ri * r) * (1.0 - s * s);
                    }
                }
                else if (nodeCnt == Constants.QuadNodeCnt_FirstOrder)
                {
                    // 節点0～3 : 四角形の頂点
                    for (int i = 0; i < nno; i++)
                    {
                        // 節点の局所座標
                        double ri = n_pts[i][0];
                        double si = n_pts[i][1];
                        // 形状関数N
                        N[i] = 0.25 * (1.0 + ri * r) * (1.0 + si * s);
                    }
                }
                // 表示する位置
                double showPosX = 0;
                double showPosY = 0;
                for (int k = 0; k < nodeCnt; k++)
                {
                    showPosX += pp[k][0] * N[k];
                    showPosY += pp[k][1] * N[k];
                }
                Complex cvalueX = new Complex(0, 0);
                Complex cvalueY = new Complex(0, 0);
                for (int k = 0; k < nodeCnt; k++)
                {
                    cvalueX += tagtXValues[k] * N[k];
                    cvalueY += tagtYValues[k] * N[k];
                }
                try
                {
                    double showScale = ((double)regionSize.Width / DefPanelWidth) * ArrowLength;
                    // 実数部のベクトル表示
                    int lenX = (int)((double)(cvalueX.Real / maxRotFValue) * showScale);
                    int lenY = (int)((double)(cvalueY.Real / maxRotFValue) * showScale);
                    if (lenX != 0 || lenY != 0)
                    {
                        // Y方向は表示上逆になる
                        lenY = -lenY;
                        using (Pen pen = new Pen(drawColor, 1))
                        {
                            //pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                            //pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                            pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                            //pen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(3, 3, false); // 重い
                            g.DrawLine(pen, (int)showPosX, (int)showPosY, (int)(showPosX + lenX), (int)(showPosY + lenY));
                        }
                    }
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception.Message + " " + exception.StackTrace);
                }
            }
        }
    }
}
