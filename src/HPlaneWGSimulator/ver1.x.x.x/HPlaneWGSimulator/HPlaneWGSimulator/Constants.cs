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
        public const double pi = 3.1416;
        public const double c0 = 2.99792458e+8;
        public const double myu0 = 4.0e-7 * pi;
        public const double eps0 = 8.85418782e-12;//1.0 / (myu0 * c0 * c0);
        /// <summary>
        /// 計算精度下限
        /// </summary>
        public const double PrecisionLowerLimit = 1.0e-12;

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
        //public const int MaxModeCount = 20;
        public const int MaxModeCount = int.MaxValue; // 固有値解析で取得可能なすべてのモードを考慮する(境界の節点数)
        /// <summary>
        /// 界分布表示における三角形要素内の分割数
        /// </summary>
        public const int TriDrawFieldMshDivCnt = 4;

        /////////////////////////////////////////////////////////////////////////
        // 要素関連
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 要素形状区分
        /// </summary>
        public enum FemElementShapeDV { Line, Triangle/*, QuadType1*/, QuadType2 };
        /// <summary>
        /// ２次元座標次数
        /// </summary>
        public const int CoordDim2D = 2;
        /// <summary>
        /// 三角形要素頂点数
        /// </summary>
        public const int TriVertexCnt = 3;
        /// <summary>
        /// 四角形要素頂点数
        /// </summary>
        public const int QuadVertexCnt = 4;
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
        /// <summary>
        /// 三角形要素節点数
        /// </summary>
        public const int TriNodeCnt_FirstOrder = 3;
        public const int TriNodeCnt_SecondOrder = 6;
        /// <summary>
        /// 三角形要素の辺の数
        /// </summary>
        public const int TriEdgeCnt_FirstOrder = 3;
        public const int TriEdgeCnt_SecondOrder = 6; // 要素内部の辺は含まない
        /// <summary>
        /// 四角形要素頂点数
        /// </summary>
        public const int QuadNodeCnt_FirstOrder = 4;
        public const int QuadNodeCnt_SecondOrder_Type1 = 9;
        public const int QuadNodeCnt_SecondOrder_Type2 = 8; // セレンディピティ族四角形
        /// <summary>
        /// 四角形要素の辺の数
        /// </summary>
        public const int QuadEdgeCnt_FirstOrder = 4;
        public const int QuadEdgeCnt_SecondOrder = 8; // 要素内部の辺は含まない

        /// <summary>
        /// 有限要素の形状区分既定値
        /// </summary>
        //public const FemElementShapeDV DefElemShapeDv = FemElementShapeDV.Triangle;
        public const FemElementShapeDV DefElemShapeDv = FemElementShapeDV.QuadType2;  // 素のHPlaneWGSimulatorの既定値を四角形要素に変更
        /// <summary>
        /// 有限要素の補間次数既定値
        /// </summary>
        public const int DefElementOrder = SecondOrder;
        //public const int DefElementOrder = FirstOrder;

    }
}
