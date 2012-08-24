#pragma once
/*
 * @author KrdLab
 */

namespace KrdLab {
	namespace clapack {

		/// <summary>
		/// 計算上必要になるチェックルーチンを定義する．
		/// </summary>
		public ref class CalculationChecker
		{
		public:
			/// <summary>
			/// 精度が下限値より下であるかどうかを調べる．
			/// </summary>
			/// <param name="value">調べたい値</param>
			/// <returns>下限値を下回る場合はtrue，その他はfalseを返す．</returns>
			static bool IsLessThanLimit(double value)
			{
				if(value < Function::CalculationLowerLimit)
				{
					return true;
				}
				return false;
			}
		};
	}
}
