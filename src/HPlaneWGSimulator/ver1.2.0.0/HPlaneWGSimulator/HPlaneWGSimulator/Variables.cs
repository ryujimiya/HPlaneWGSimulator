using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HPlaneWGSimulator
{
    class Variables
    {
        /// <summary>
        /// 計算周波数範囲
        /// </summary>
        public static double[] NormalizedFreqRange = new double[] { 0.0, 0.0 };
        /// <summary>
        /// 計算する周波数の数
        /// </summary>
        public static int CalcFreqencyPointCount = 0;

        /// <summary>
        /// 既定値で初期化
        /// </summary>
        public static void Reset()
        {
            for (int i = 0; i < NormalizedFreqRange.Length; i++)
            {
                NormalizedFreqRange[i] = Constants.DefNormalizedFreqRange[i];
            }
            CalcFreqencyPointCount = Constants.DefCalcFreqencyPointCount;
        }
    }
}
