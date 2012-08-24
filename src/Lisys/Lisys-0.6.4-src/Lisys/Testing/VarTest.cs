using System;
using System.Collections.Generic;
using System.Text;

namespace KrdLab.Lisys.Testing
{
    /// <summary>
    /// 等分散性の検定（F検定）
    /// </summary>
    public class F
    {
        /// <summary>
        /// 等分散性の検定を行う．
        /// </summary>
        /// <param name="set1">検定対象となる数値群</param>
        /// <param name="set2">検定対象となる数値群</param>
        /// <param name="level">有意水準</param>
        /// <param name="p">p値が格納される（out）</param>
        /// <param name="f">検定統計量（絶対値）が格納される（out）</param>
        /// <returns>trueの場合は「有意差あり」，falseの場合は「有意差無し」を意味する</returns>
        public static bool Test(IVector set1, IVector set2, double level, out double p, out double f)
        {
            int size1 = set1.Size;
            int size2 = set2.Size;
            if (size1 < 2 || size2 < 2)
            {
                throw new Exception.IllegalArgumentException();
            }

            // 統計量
            double u1 = set1.Variance;
            double u2 = set2.Variance;

            int dof1, dof2;

            if (u1 > u2)
            {
                f = u1 / u2;
                dof1 = size1 - 1;
                dof2 = size2 - 1;
            }
            else
            {
                f = u2 / u1;
                dof1 = size2 - 1;
                dof2 = size1 - 1;
            }

            p = GSL.Functions.cdf_fdist_Q(f, dof1, dof2);

            return p <= level;
        }
    }
}
