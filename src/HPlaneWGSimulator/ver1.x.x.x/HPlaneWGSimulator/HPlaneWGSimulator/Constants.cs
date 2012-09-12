using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace HPlaneWGSimulator
{
    class Constants
    {
        ////////////////////////////////////////////////////////////////////////
        // 定数
        ////////////////////////////////////////////////////////////////////////
        public static readonly double pi = 3.1416;
        public static readonly double c0 = 2.99792458e+8;
        public static readonly double myu0 = 4.0e-7 * pi;
        public static readonly double eps0 = 8.85418782e-12;//1.0 / (myu0 * c0 * c0);
        /// <summary>
        /// 計算精度下限
        /// </summary>
        public static readonly double PrecisionLowerLimit = 1.0e-12;

        /// <summary>
        /// 分割数
        /// </summary>
        public static readonly Size MaxDiv = new Size(30, 30);
        /// <summary>
        /// CADデータファイル拡張子
        /// </summary>
        public static readonly string CadExt = ".cad";
        /// <summary>
        /// Fem入力データファイル拡張子
        /// </summary>
        public static readonly string FemInputExt = ".fem";
        /// <summary>
        /// Fem出力結果データファイル拡張子
        /// </summary>
        public static readonly string FemOutputExt = ".out";
        /// <summary>
        /// Fem出力結果インデックスファイル拡張子
        /// </summary>
        public static readonly string FemOutputIndexExt = ".idx";
        /// <summary>
        /// 媒質の個数
        /// </summary>
        public const int MaxMediaCount = 3;
        /// <summary>
        /// 計算周波数範囲(既定値)
        /// </summary>
        public static readonly double[] DefNormalizedFreqRange = new double[] { 1.0, 2.0 };
        /// <summary>
        /// 計算する周波数の数(既定値)
        /// </summary>
        public const int DefCalcFreqencyPointCount = 20;
        /// <summary>
        /// 考慮するモード数
        /// </summary>
        public const int MaxModeCount = 20;


        /////////////////////////////////////////////////////////////////////////
        // 要素関連
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 要素形状区分
        /// </summary>
        public enum FemElementShapeDV { Line, Triangle };
        /// <summary>
        /// ２次元座標次数
        /// </summary>
        public const int CoordDim2D = 2;
        /// <summary>
        /// 三角形要素頂点数
        /// </summary>
        public const int TriVertexCnt = 3;
        /// <summary>
        /// 線要素頂点数
        /// </summary>
        public const int LineVertexCnt = 2;
        /// <summary>
        /// 要素次数１次
        /// </summary>
        public const int FirstOrder = 1;
        /// <summary>
        /// 要素次数２次
        /// </summary>
        public const int SecondOrder = 2;
        /// <summary>
        /// 線要素節点数
        /// </summary>
        public const int LineNodeCnt_FirstOrder = 2;
        public const int LineNodeCnt_SecondOrder = 3;
        //public static readonly int[] LineNodeCnt = { LineNodeCnt_FirstOrder, LineNodeCnt_SecondOrder };
        /// <summary>
        /// 三角形要素節点数
        /// </summary>
        public const int TriNodeCnt_FirstOrder = 3;
        public const int TriNodeCnt_SecondOrder = 6;
        //public static readonly int[] TriNodeCnt = { TriNodeCnt_FirstOrder, TriNodeCnt_SecondOrder };
        /// <summary>
        /// 三角形要素の辺の数
        /// </summary>
        public const int TriEdgeCnt_FirstOrder = 3;
        public const int TriEdgeCnt_SecondOrder = 6; // 要素内部の辺は含まない
        //public static readonly int[] TriEdgeCnt = { TriEdgeCnt_FirstOrder, TriEdgeCnt_SecondOrder };
    }
}
