using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using System.Numerics; // Complex
using KrdLab.clapack; // KrdLab.clapack.Complex
//using System.Text.RegularExpressions;
using MyUtilLib.Matrix;

namespace HPlaneWGSimulator
{
    /// <summary>
    /// ２次四角形要素(セレンディピティserendipity族)：ヘルムホルツ方程式の要素行列
    /// </summary>
    class FemMat_Quad_Second
    {
        /* 数値積分版
        /// <summary>
        /// ヘルムホルツ方程式に対する有限要素マトリクス作成
        /// </summary>
        /// <param name="waveLength">波長</param>
        /// <param name="toSorted">ソートされた節点インデックス（ 2D節点番号→ソート済みリストインデックスのマップ）</param>
        /// <param name="element">有限要素</param>
        /// <param name="Nodes">節点リスト</param>
        /// <param name="Medias">媒質リスト</param>
        /// <param name="ForceNodeNumberH">強制境界節点ハッシュ</param>
        /// <param name="WaveModeDv">計算する波のモード区分</param>
        /// <param name="mat">マージされる全体行列</param>
        public static  void AddElementMat(double waveLength,
            Dictionary<int, int> toSorted,
            FemElement element,
            IList<FemNode> Nodes,
            MediaInfo[] Medias,
            Dictionary<int, bool> ForceNodeNumberH,
            FemSolver.WaveModeDv WaveModeDv,
            ref MyComplexMatrix mat)
        {
            // 定数
            const double pi = Constants.pi;
            const double c0 = Constants.c0;
            // 波数
            double k0 = 2.0 * pi / waveLength;
            // 角周波数
            double omega = k0 * c0;

            // 要素頂点数
            const int vertexCnt = Constants.QuadVertexCnt; //4;
            // 要素内節点数
            const int nno = Constants.QuadNodeCnt_SecondOrder_Type2; //8;  // 2次セレンディピティ
            // 座標次元数
            const int ndim = Constants.CoordDim2D; //2;

            int[] nodeNumbers = element.NodeNumbers;
            int[] no_c = new int[nno];
            MediaInfo media = Medias[element.MediaIndex];
            double[,] media_P = null;
            double[,] media_Q = null;
            if (WaveModeDv == FemSolver.WaveModeDv.TE)
            {
                media_P = media.P;
                media_Q = media.Q;
            }
            else if (WaveModeDv == FemSolver.WaveModeDv.TM)
            {
                media_P = media.Q;
                media_Q = media.P;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
            // [p]は逆数をとる
            media_P = MyMatrixUtil.matrix_Inverse(media_P);

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

            //// 四角形の辺の長さを求める
            //double[] le = new double[4];
            //le[0] = FemMeshLogic.GetDistance(pp[0], pp[1]);
            //le[1] = FemMeshLogic.GetDistance(pp[1], pp[2]);
            //le[2] = FemMeshLogic.GetDistance(pp[2], pp[3]);
            //le[3] = FemMeshLogic.GetDistance(pp[3], pp[0]);

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


            // ガウスルジャンドルの積分公式
            double[][] g_pts = new double[5][]
                {
                    // ポイント(ξ: [-1 +1]区間)、重み
                    new double[] { -0.90617985, 0.23692689},
                    new double[] { -0.53846931, 0.47862867},
                    new double[] {0.0, 0.56888889},
                    new double[] {0.53846931, 0.47862867},
                    new double[] {0.90617985, 0.23692689}
                };

            // 要素剛性行列を作る
            double[,] emat = new Complex[nno, nno];
            for (int ino = 0; ino < nno; ino++)
            {
                for (int jno = 0; jno < nno; jno++)
                {
                    emat[ino, jno] = 0.0;
                    double detjsum = 0; //check
                    foreach (double[] s_g_pt in g_pts)
                    {                    
                        foreach (double[] r_g_pt in g_pts)
                        {
                            // 積分点
                            double r = r_g_pt[0];
                            double s = s_g_pt[0];
                            // 重み(2次元)
                            double weight = r_g_pt[1] * s_g_pt[1];
                            // 形状関数
                            double[] N = new double[nno];
                            // 形状関数のr, s方向微分
                            double[] dNdr = new double[nno];
                            double[] dNds = new double[nno];
                            // 節点0～3 : 四角形の頂点
                            for (int i = 0; i < 4; i++)
                            {
                                // 節点の局所座標
                                double ri = n_pts[i][0];
                                double si = n_pts[i][1];
                                // 形状関数N
                                N[i] = 0.25 * (1.0 + ri * r) * (1.0 + si * s) * (ri* r + si * s - 1.0);
                                // 形状関数のr方向微分
                                dNdr[i] = 0.25 * ri * (1.0 + si * s) * (2.0 * ri * r + si * s);
                                // 形状関数のs方向微分
                                dNds[i] = 0.25 * si * (1.0 + ri * r) * (ri * r + 2.0 * si * s);
                            }
                            // 節点4,6 : r方向辺上中点
                            foreach (int i in new int[]{ 4, 6})
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

                            // ヤコビアン行列
                            double j11;
                            double j12;
                            double j21;
                            double j22;
                            j11 = 0;
                            j12 = 0;
                            j21 = 0;
                            j22 = 0;

                            //for (int i = 0; i < vertexCnt; i++)
                            //{
                            //    // 頂点の座標の微分
                            //    // 座標の形状関数は一次四角形のものを使用する
                            //    // 節点の局所座標
                            //    double ri = n_pts[i][0];
                            //    double si = n_pts[i][1];
                            //    double dNdr_1stOrder = 0.25 * ri * (1.0 + si * s);
                            //    double dNds_1stOrder = 0.25 * (1.0 + ri * r) * si;
                            //    j11 += dNdr_1stOrder * pp[i][0];
                            //    j12 += dNdr_1stOrder * pp[i][1];
                            //    j21 += dNds_1stOrder * pp[i][0];
                            //    j22 += dNds_1stOrder * pp[i][1];
                            //}

                            for (int i = 0; i < nno; i++)
                            {
                                j11 += dNdr[i] * pp[i][0];
                                j12 += dNdr[i] * pp[i][1];
                                j21 += dNds[i] * pp[i][0];
                                j22 += dNds[i] * pp[i][1];
                            }
                            // ヤコビアン
                            double detj = j11 * j22 - j12 * j21;
                            detjsum += detj * weight;
                            //Console.WriteLine("det:{0}", detj);

                            // gradr[0] : gradrのx成分 grad[1] : gradrのy成分
                            // grads[0] : gradsのx成分 grads[1] : gradsのy成分
                            double[] gradr = new double[2];
                            double[] grads = new double[2];
                            gradr[0] =   j22 / detj;
                            gradr[1] = - j21 / detj;
                            grads[0] = - j12 / detj;
                            grads[1] =   j11 / detj;

                            // 形状関数のx, y方向微分
                            double[,] dNdX = new double[ndim, nno];
                            for (int i = 0; i < nno; i++)
                            {
                                for (int direction = 0; direction < ndim; direction++)
                                {
                                    dNdX[direction, i] = dNdr[i] * gradr[direction] + dNds[i] * grads[direction];
                                }
                            }

                            // 汎関数
                            double functional = media_P[0, 0] * dNdX[1, ino] * dNdX[1, jno] + media_P[1, 1] * dNdX[0, ino] * dNdX[0, jno]
                                             - k0 * k0 * media_Q[2, 2] * N[ino] * N[jno];
                            emat[ino, jno] += detj * weight * functional;
                        }
                    }
                    //Console.WriteLine("detsum: {0}", detjsum);
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
         */
        /// <summary>
        /// ヘルムホルツ方程式に対する有限要素マトリクス作成
        /// </summary>
        /// <param name="waveLength">波長</param>
        /// <param name="toSorted">ソートされた節点インデックス（ 2D節点番号→ソート済みリストインデックスのマップ）</param>
        /// <param name="element">有限要素</param>
        /// <param name="Nodes">節点リスト</param>
        /// <param name="Medias">媒質リスト</param>
        /// <param name="ForceNodeNumberH">強制境界節点ハッシュ</param>
        /// <param name="WGStructureDv">導波路構造区分</param>
        /// <param name="WaveModeDv">計算する波のモード区分</param>
        /// <param name="waveguideWidthForEPlane">導波路幅(E面解析用)</param>
        /// <param name="mat">マージされる全体行列</param>
        public static void AddElementMat(double waveLength,
            Dictionary<int, int> toSorted,
            FemElement element,
            IList<FemNode> Nodes,
            MediaInfo[] Medias,
            Dictionary<int, bool> ForceNodeNumberH,
            FemSolver.WGStructureDV WGStructureDv,
            FemSolver.WaveModeDV WaveModeDv,
            double waveguideWidthForEPlane,
            ref MyComplexMatrix mat)
        {
            // 定数
            const double pi = Constants.pi;
            const double c0 = Constants.c0;
            // 波数
            double k0 = 2.0 * pi / waveLength;
            // 角周波数
            double omega = k0 * c0;

            // 要素頂点数
            //const int vertexCnt = Constants.QuadVertexCnt; //4;
            // 要素内節点数
            const int nno = Constants.QuadNodeCnt_SecondOrder_Type2; //8;  // 2次セレンディピティ
            // 座標次元数
            const int ndim = Constants.CoordDim2D; //2;

            int[] nodeNumbers = element.NodeNumbers;
            int[] no_c = new int[nno];
            MediaInfo media = Medias[element.MediaIndex];
            double[,] media_P = null;
            double[,] media_Q = null;
            // ヘルムホルツ方程式のパラメータP,Qを取得する
            FemSolver.GetHelmholtzMediaPQ(
                k0,
                media,
                WGStructureDv,
                WaveModeDv,
                waveguideWidthForEPlane,
                out media_P,
                out media_Q);

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

            // 四角形の辺の長さを求める
            double[] le = new double[4];
            le[0] = FemMeshLogic.GetDistance(pp[0], pp[1]);
            le[1] = FemMeshLogic.GetDistance(pp[1], pp[2]);
            le[2] = FemMeshLogic.GetDistance(pp[2], pp[3]);
            le[3] = FemMeshLogic.GetDistance(pp[3], pp[0]);
            System.Diagnostics.Debug.Assert(Math.Abs(le[0] - le[2]) < Constants.PrecisionLowerLimit);
            System.Diagnostics.Debug.Assert(Math.Abs(le[1] - le[3]) < Constants.PrecisionLowerLimit);
            double lx = le[0];
            double ly = le[1];

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

            // Ni = a0(r^2*s) + a1(r^2) + a2(r) + a3(rs) + a4(rs^2) + a5(s^2) + a6(s) + a7
            double[,] Ni_a = new double[nno, 8];
            for (int i = 0; i < 4; i++)
            {
                // 節点の局所座標
                double ri = n_pts[i][0];
                double si = n_pts[i][1];
                Ni_a[i, 0] = 0.25 * ri * ri * si;
                Ni_a[i, 1] = 0.25 * ri * ri;
                Ni_a[i, 2] = 0.0;
                Ni_a[i, 3] = 0.25 * ri * si;
                Ni_a[i, 4] = 0.25 * ri * si * si;
                Ni_a[i, 5] = 0.25 * si * si;
                Ni_a[i, 6] = 0.0;
                Ni_a[i, 7] = -0.25;
            }
            foreach (int i in new int[] { 4, 6 })
            {
                // 節点の局所座標
                double ri = n_pts[i][0];
                double si = n_pts[i][1];
                Ni_a[i, 0] = -0.5 * si;
                Ni_a[i, 1] = -0.5;
                Ni_a[i, 2] = 0.0;
                Ni_a[i, 3] = 0.0;
                Ni_a[i, 4] = 0.0;
                Ni_a[i, 5] = 0.0;
                Ni_a[i, 6] = 0.5 * si;
                Ni_a[i, 7] = 0.5;
            }
            foreach (int i in new int[] { 5, 7 })
            {
                // 節点の局所座標
                double ri = n_pts[i][0];
                double si = n_pts[i][1];
                Ni_a[i, 0] = 0.0;
                Ni_a[i, 1] = 0.0;
                Ni_a[i, 2] = 0.5 * ri;
                Ni_a[i, 3] = 0.0;
                Ni_a[i, 4] = -0.5 * ri;
                Ni_a[i, 5] = -0.5;
                Ni_a[i, 6] = 0.0;
                Ni_a[i, 7] = 0.5;
            }

            // dNidr = a0(r^2*s) + a1(r^2) + a2(r) + a3(rs) + a4(rs^2) + a5(s^2) + a6(s) + a7
            double[,] dNidr_a = new double[nno, 8];
            for (int i = 0; i < 4; i++)
            {
                // 節点の局所座標
                double ri = n_pts[i][0];
                double si = n_pts[i][1];
                dNidr_a[i, 0] = 0.0;
                dNidr_a[i, 1] = 0.0;  // r^2
                dNidr_a[i, 2] = 0.25 * 2.0 * ri * ri;  // r
                dNidr_a[i, 3] = 0.25 * 2.0 * ri * ri * si;  // rs
                dNidr_a[i, 4] = 0.0;
                dNidr_a[i, 5] = 0.25 * ri * si * si;  // s^2
                dNidr_a[i, 6] = 0.25 * ri * si;  // s
                dNidr_a[i, 7] = 0.0;  //1
            }
            foreach (int i in new int[] { 4, 6 })
            {
                // 節点の局所座標
                double ri = n_pts[i][0];
                double si = n_pts[i][1];
                dNidr_a[i, 0] = 0.0;
                dNidr_a[i, 1] = 0.0;  // r^2
                dNidr_a[i, 2] = -1.0;  // r
                dNidr_a[i, 3] = -si;  // rs
                dNidr_a[i, 4] = 0.0;
                dNidr_a[i, 5] = 0.0;  // s^2
                dNidr_a[i, 6] = 0.0;  // s
                dNidr_a[i, 7] = 0.0;  // 1
            }
            foreach (int i in new int[] { 5, 7 })
            {
                // 節点の局所座標
                double ri = n_pts[i][0];
                double si = n_pts[i][1];
                dNidr_a[i, 0] = 0.0;
                dNidr_a[i, 1] = 0.0;  // r^2
                dNidr_a[i, 2] = 0.0;  // r
                dNidr_a[i, 3] = 0.0;  // rs
                dNidr_a[i, 4] = 0.0;
                dNidr_a[i, 5] = -0.5 * ri;  // s^2
                dNidr_a[i, 6] = 0.0;  // s
                dNidr_a[i, 7] = 0.5 * ri;  // 1
            }

            // dNids = a0(r^2*s) + a1(r^2) + a2(r) + a3(rs) + a4(rs^2) + a5(s^2) + a6(s) + a7
            double[,] dNids_a = new double[nno, 8];
            for (int i = 0; i < 4; i++)
            {
                // 節点の局所座標
                double ri = n_pts[i][0];
                double si = n_pts[i][1];
                dNids_a[i, 0] = 0.0;
                dNids_a[i, 1] = 0.25 * ri * ri * si;  // r^2
                dNids_a[i, 2] = 0.25 * ri * si;  // r
                dNids_a[i, 3] = 0.25 * 2.0 * ri * si * si;  // rs
                dNids_a[i, 4] = 0.0;
                dNids_a[i, 5] = 0.0;  // s^2
                dNids_a[i, 6] = 0.25 * 2.0 * si * si;  // s
                dNids_a[i, 7] = 0.0;  //1
            }
            foreach (int i in new int[] { 4, 6 })
            {
                // 節点の局所座標
                double ri = n_pts[i][0];
                double si = n_pts[i][1];
                dNids_a[i, 0] = 0.0;
                dNids_a[i, 1] = -0.5 * si;  // r^2
                dNids_a[i, 2] = 0.0;  // r
                dNids_a[i, 3] = 0.0;  // rs
                dNids_a[i, 4] = 0.0;
                dNids_a[i, 5] = 0.0;  // s^2
                dNids_a[i, 6] = 0.0;  // s
                dNids_a[i, 7] = 0.5 * si;  //1
            }
            foreach (int i in new int[] { 5, 7 })
            {
                // 節点の局所座標
                double ri = n_pts[i][0];
                double si = n_pts[i][1];
                dNids_a[i, 0] = 0.0;
                dNids_a[i, 1] = 0.0;  // r^2
                dNids_a[i, 2] = 0.0;  // r
                dNids_a[i, 3] = -ri;  // rs
                dNids_a[i, 4] = 0.0;
                dNids_a[i, 5] = 0.0;  // s^2
                dNids_a[i, 6] = -1.0;  // s
                dNids_a[i, 7] = 0.0;  //1
            }

            // ∫dN/dndN/dn dxdy
            //     integralDNDX[n, ino, jno]  n = 0 --> ∫dN/dxdN/dx dxdy
            //                                n = 1 --> ∫dN/dydN/dy dxdy
            double[, ,] integralDNDX = new double[ndim, nno, nno];
            // ∫N N dxdy
            double[,] integralN = new double[nno, nno];
            for (int ino = 0; ino < nno; ino++)
            {
                for (int jno = 0; jno < nno; jno++)
                {
                    integralN[ino, jno] = lx * ly / 4.0 *
                        (
                        // r^4s^2
                        4.0 / 15.0 * Ni_a[ino, 0] * Ni_a[jno, 0]
                        // r^2s^2
                        + 4.0 / 9.0 * (Ni_a[ino, 6] * Ni_a[jno, 0] + Ni_a[ino, 5] * Ni_a[jno, 1] + Ni_a[ino, 4] * Ni_a[jno, 2] + Ni_a[ino, 3] * Ni_a[jno, 3]
                                     + Ni_a[ino, 2] * Ni_a[jno, 4] + Ni_a[ino, 1] * Ni_a[jno, 5] + Ni_a[ino, 0] * Ni_a[jno, 6])
                        // r^4
                        + 4.0 / 5.0 * Ni_a[ino, 1] * Ni_a[jno, 1]
                        // r^2
                        + 4.0 / 3.0 * (Ni_a[ino, 7] * Ni_a[jno, 1] + Ni_a[ino, 2] * Ni_a[jno, 2] + Ni_a[ino, 1] * Ni_a[jno, 7])
                        // r^2s^4
                        + 4.0 / 15.0 * Ni_a[ino, 4] * Ni_a[jno, 4]
                        // s^4
                        + 4.0 / 5.0 * Ni_a[ino, 5] * Ni_a[jno, 5]
                        // s^2
                        + 4.0 / 3.0 * (Ni_a[ino, 7] * Ni_a[jno, 5] + Ni_a[ino, 6] * Ni_a[jno, 6] + Ni_a[ino, 5] * Ni_a[jno, 7])
                        // 1
                        + 4.0 * Ni_a[ino, 7] * Ni_a[jno, 7]
                        );
                    integralDNDX[0, ino, jno] = ly / lx *
                        (
                        // r^4s^2
                        4.0 / 15.0 * dNidr_a[ino, 0] * dNidr_a[jno, 0]
                        // r^2s^2
                        + 4.0 / 9.0 * (dNidr_a[ino, 6] * dNidr_a[jno, 0] + dNidr_a[ino, 5] * dNidr_a[jno, 1] + dNidr_a[ino, 4] * dNidr_a[jno, 2]
                                     + dNidr_a[ino, 3] * dNidr_a[jno, 3]
                                     + dNidr_a[ino, 2] * dNidr_a[jno, 4] + dNidr_a[ino, 1] * dNidr_a[jno, 5] + dNidr_a[ino, 0] * dNidr_a[jno, 6])
                        // r^4
                        + 4.0 / 5.0 * dNidr_a[ino, 1] * dNidr_a[jno, 1]
                        // r^2
                        + 4.0 / 3.0 * (dNidr_a[ino, 7] * dNidr_a[jno, 1] + dNidr_a[ino, 2] * dNidr_a[jno, 2] + dNidr_a[ino, 1] * dNidr_a[jno, 7])
                        // r^2s^4
                        + 4.0 / 15.0 * dNidr_a[ino, 4] * dNidr_a[jno, 4]
                        // s^4
                        + 4.0 / 5.0 * dNidr_a[ino, 5] * dNidr_a[jno, 5]
                        // s^2
                        + 4.0 / 3.0 * (dNidr_a[ino, 7] * dNidr_a[jno, 5] + dNidr_a[ino, 6] * dNidr_a[jno, 6] + dNidr_a[ino, 5] * dNidr_a[jno, 7])
                        // 1
                        + 4.0 * dNidr_a[ino, 7] * dNidr_a[jno, 7]
                        );
                    integralDNDX[1, ino, jno] = lx / ly *
                        (
                        // r^4s^2
                        4.0 / 15.0 * dNids_a[ino, 0] * dNids_a[jno, 0]
                        // r^2s^2
                        + 4.0 / 9.0 * (dNids_a[ino, 6] * dNids_a[jno, 0] + dNids_a[ino, 5] * dNids_a[jno, 1] + dNids_a[ino, 4] * dNids_a[jno, 2]
                                     + dNids_a[ino, 3] * dNids_a[jno, 3]
                                     + dNids_a[ino, 2] * dNids_a[jno, 4] + dNids_a[ino, 1] * dNids_a[jno, 5] + dNids_a[ino, 0] * dNids_a[jno, 6])
                        // r^4
                        + 4.0 / 5.0 * dNids_a[ino, 1] * dNids_a[jno, 1]
                        // r^2
                        + 4.0 / 3.0 * (dNids_a[ino, 7] * dNids_a[jno, 1] + dNids_a[ino, 2] * dNids_a[jno, 2] + dNids_a[ino, 1] * dNids_a[jno, 7])
                        // r^2s^4
                        + 4.0 / 15.0 * dNids_a[ino, 4] * dNids_a[jno, 4]
                        // s^4
                        + 4.0 / 5.0 * dNids_a[ino, 5] * dNids_a[jno, 5]
                        // s^2
                        + 4.0 / 3.0 * (dNids_a[ino, 7] * dNids_a[jno, 5] + dNids_a[ino, 6] * dNids_a[jno, 6] + dNids_a[ino, 5] * dNids_a[jno, 7])
                        // 1
                        + 4.0 * dNids_a[ino, 7] * dNids_a[jno, 7]
                        );
                }
            }

            // 要素剛性行列を作る
            double[,] emat = new double[nno, nno];
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

                    //mat[inoGlobal, jnoGlobal] += emat[ino, jno];
                    //mat._body[inoGlobal + jnoGlobal * mat.RowSize] += emat[ino, jno];
                    // 実数部に加算する
                    //mat._body[inoGlobal + jnoGlobal * mat.RowSize].Real += emat[ino, jno];
                    // バンドマトリクス対応
                    mat._body[mat.GetBufferIndex(inoGlobal, jnoGlobal)].Real += emat[ino, jno];
                }
            }
        }
    }
}
