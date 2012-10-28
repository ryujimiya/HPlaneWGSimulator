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
    /// １次四角形要素(セレンディピティserendipity族)：ヘルムホルツ方程式の要素行列
    /// </summary>
    class FemMat_Quad_First
    {
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
        public static void AddElementMat(double waveLength,
            Dictionary<int, int> toSorted,
            FemElement element,
            IList<FemNode> Nodes,
            MediaInfo[] Medias,
            Dictionary<int, bool> ForceNodeNumberH,
            FemSolver.WaveModeDV WaveModeDv,
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
            const int nno = Constants.QuadNodeCnt_FirstOrder; //4;  // 1次セレンディピティ
            // 座標次元数
            const int ndim = Constants.CoordDim2D; //2;

            int[] nodeNumbers = element.NodeNumbers;
            int[] no_c = new int[nno];
            MediaInfo media = Medias[element.MediaIndex];
            double[,] media_P = null;
            double[,] media_Q = null;
            if (WaveModeDv == FemSolver.WaveModeDV.TE)
            {
                media_P = media.P;
                media_Q = media.Q;
            }
            else if (WaveModeDv == FemSolver.WaveModeDV.TM)
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
            //    3+  +  +2
            //    |   |   |
            // ---+---+---+-->r
            //    |   |   |
            //    0+  +  +1
            //        |
            //
            double[][] n_pts = 
                {
                    // r, s
                    new double[] {-1.0, -1.0},  //0
                    new double[] { 1.0, -1.0},  //1
                    new double[] { 1.0,  1.0},  //2
                    new double[] {-1.0,  1.0},  //3
                };

            // ∫dN/dndN/dn dxdy
            //     integralDNDX[n, ino, jno]  n = 0 --> ∫dN/dxdN/dx dxdy
            //                                n = 1 --> ∫dN/dydN/dy dxdy
            double[, ,] integralDNDX = new double[ndim, nno, nno]
                {
                    {
                        {  2.0 * ly /(6.0 * lx), -2.0 * ly /(6.0 * lx),  -1.0 * ly /(6.0 * lx),  1.0 * ly /(6.0 * lx) },
                        { -2.0 * ly /(6.0 * lx),  2.0 * ly /(6.0 * lx),   1.0 * ly /(6.0 * lx), -1.0 * ly /(6.0 * lx) },
                        { -1.0 * ly /(6.0 * lx),  1.0 * ly /(6.0 * lx),   2.0 * ly /(6.0 * lx), -2.0 * ly /(6.0 * lx) },
                        {  1.0 * ly /(6.0 * lx), -1.0 * ly /(6.0 * lx),  -2.0 * ly /(6.0 * lx),  2.0 * ly /(6.0 * lx) },
                    },
                    {
                        {  2.0 * lx /(6.0 * ly),  1.0 * lx /(6.0 * ly),  -1.0 * lx /(6.0 * ly), -2.0 * lx /(6.0 * ly) },
                        {  1.0 * lx /(6.0 * ly),  2.0 * lx /(6.0 * ly),  -2.0 * lx /(6.0 * ly), -1.0 * lx /(6.0 * ly) },
                        { -1.0 * lx /(6.0 * ly), -2.0 * lx /(6.0 * ly),   2.0 * lx /(6.0 * ly),  1.0 * lx /(6.0 * ly) },
                        { -2.0 * lx /(6.0 * ly), -1.0 * lx /(6.0 * ly),   1.0 * lx /(6.0 * ly),  2.0 * lx /(6.0 * ly) },
                    }
                };
            // ∫N N dxdy
            double[,] integralN = new double[nno, nno]
                {
                    { 4.0 * lx * ly / 36.0, 2.0 * lx * ly / 36.0, 1.0 * lx * ly / 36.0, 2.0 * lx * ly / 36.0 },
                    { 2.0 * lx * ly / 36.0, 4.0 * lx * ly / 36.0, 2.0 * lx * ly / 36.0, 1.0 * lx * ly / 36.0 },
                    { 1.0 * lx * ly / 36.0, 2.0 * lx * ly / 36.0, 4.0 * lx * ly / 36.0, 2.0 * lx * ly / 36.0 },
                    { 2.0 * lx * ly / 36.0, 1.0 * lx * ly / 36.0, 2.0 * lx * ly / 36.0, 4.0 * lx * ly / 36.0 },
                };

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
