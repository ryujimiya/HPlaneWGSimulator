using System;
using System.Collections.Generic;
using System.Text;

namespace KrdLab.Lisys.Testing
{
    /// <summary>
    /// 検定の種類
    /// </summary>
    public enum Method
    {
        /// <summary>
        /// 等分散性を仮定する
        /// </summary>
        AssumedEqualityOfVariances,

        /// <summary>
        /// 等分散性を仮定しない
        /// </summary>
        NotAssumedEqualityOfVariances,

        /// <summary>
        /// 値に対応関係がある（使用不可，<see cref="NotSupportedException"/>がthrowされる）
        /// </summary>
        PairedValues
    }

    /// <summary>
    /// 平均値の差の検定
    /// </summary>
    public class T
    {
        /// <summary>
        /// 母平均に対する平均値の検定を行う．
        /// </summary>
        /// <param name="set">検定対象となる数値群</param>
        /// <param name="population">母平均</param>
        /// <param name="level">有意水準</param>
        /// <param name="p">p値が格納される（out）</param>
        /// <param name="t">t値が格納される（out）</param>
        /// <returns>trueの場合は「有意差あり」，falseの場合は「有意差無し」を意味する</returns>
        public static bool Test(IVector set, double population, double level, out double p, out double t)
        {
            VectorChecker.ZeroSize(set);

            t = (set.Average - population) / Math.Sqrt(set.Variance);
            p = GSL.Functions.cdf_tdist_Q(Math.Abs(t), set.Size - 1);
            return p <= level;
        }

        /// <summary>
        /// 2群に対する平均値の差の検定を行う．
        /// </summary>
        /// <param name="set1">検定対象となる数値群</param>
        /// <param name="set2">検定対象となる数値群</param>
        /// <param name="type">t検定の種類</param>
        /// <param name="level">有意水準</param>
        /// <param name="p">p値が格納される（out）</param>
        /// <param name="t">検定統計量（絶対値）が格納される（out）</param>
        /// <returns>trueの場合は「有意差あり」，falseの場合は「有意差無し」を意味する</returns>
        public static bool Test(IVector set1, IVector set2, Method type, double level, out double p, out double t)
        {
            p = 1;  // pとtは，必ず計算値が割り当てられる
            t = 0;
            switch (type)
            {
                case Method.AssumedEqualityOfVariances:
                    Default(set1, set2, out p, out t);
                    break;
                case Method.NotAssumedEqualityOfVariances:
                    Welch(set1, set2, out p, out t);
                    break;
                case Method.PairedValues:
                    Paired(set1, set2, out p, out t);
                    break;
            }
            return p <= level;
        }

        #region 内部メソッド

        private static void Default(IVector set1, IVector set2, out double p, out double t)
        {
            int size1 = set1.Size;
            int size2 = set2.Size;

            int phi_e = size1 + size2 - 2;  // 自由度
            if (phi_e < 1)
            {
                throw new Exception.IllegalArgumentException();
            }

            // 2つの群を併せた分散の推定値
            double ue = (set1.Scatter + set2.Scatter) / phi_e;

            // 統計量
            t = Math.Abs(set1.Average - set2.Average) / Math.Sqrt(ue * (1.0 / size1 + 1.0 / size2));

            // p値
            p = GSL.Functions.cdf_tdist_Q(t, phi_e);
        }

        private static void Welch(IVector set1, IVector set2, out double p, out double t)
        {
            int size1 = set1.Size;
            int size2 = set2.Size;

            if (size1 < 2 || size2 < 2)
            {
                throw new Exception.IllegalArgumentException();
            }

            // 不偏分散値
            double u1 = set1.Variance;
            double u2 = set2.Variance;

            double u = u1 / size1 + u2 / size2;

            // 統計量
            t = Math.Abs(set1.Average - set2.Average) / Math.Sqrt(u);

            // 自由度
            double phi_e = (u * u)
                            / (u1 * u1 / (size1 * size1 * (size1 - 1)) + u2 * u2 / (size2 * size2 * (size2 - 1)));

            // p値
            p = GSL.Functions.cdf_tdist_Q(t, phi_e);
        }

        // ↓正当性の検討が必要
        private static void Paired(IVector set1, IVector set2, out double p, out double t)
        {
            throw new NotSupportedException();

            //VectorChecker.SizeEquals(set1, set2);
            //VectorChecker.IsNotZeroSize(set1);

            //int n = set1.Size;
            //IVector d = new Vector(n);
            //for (int i = 0; i < n; ++i)
            //{
            //    d[i] = set1[i] - set2[i];
            //}

            //// 統計量
            //t = Math.Abs(d.Average) / Math.Sqrt(d.Scatter / n * (n - 1));

            //// p値
            //p = GSL.Functions.cdf_tdist_Q(t, n - 1);
        }

        #endregion
    }
}
