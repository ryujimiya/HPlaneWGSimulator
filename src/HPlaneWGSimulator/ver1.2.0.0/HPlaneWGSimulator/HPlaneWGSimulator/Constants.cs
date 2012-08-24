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
    }
}
