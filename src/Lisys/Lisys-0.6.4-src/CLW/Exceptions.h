#pragma once

using namespace System;

namespace KrdLab {
	namespace clapack {
		namespace exception {

			/// <summary>
			/// clapack Function の例外基本クラス
			/// </summary>
			public ref class ClwException : public Exception
			{
			public:
				ClwException()
				{}
				ClwException(String^ message)
					: Exception(message)
				{}
				ClwException(String^ message, Exception^ inner)
					: Exception(message, inner)
				{}
			};

			/// <summary>
			/// CLAPACKの計算結果が無効な場合にthrowされる．
			/// </summary>
			public ref class IllegalClapackResultException : public ClwException
			{
			private:
				int	_info;

			public:

				/// <summary>
				/// エラーの状態を表す数値を取得する．
				/// </summary>
				property int Info
				{
					int get()
					{
						return this->_info;
					}
				}
				
				IllegalClapackResultException(int info)
				{
					this->_info = info;
				}

				IllegalClapackResultException(String^ message, int info)
					: ClwException(message)
				{
					this->_info = info;
				}

				IllegalClapackResultException(String^ message, int info, Exception^ inner)
					: ClwException(message, inner)
				{
					this->_info = info;
				}
			};

			/// <summary>
			/// CLAPACKに渡された引数に問題がある場合に throw される．
			/// </summary>
			public ref class IllegalClapackArgumentException : public ClwException
			{
			private:
				int _index;

			public:
				/// <summary>
				/// 問題のある引数の位置を取得する．
				/// </summary>
				property int Index
				{
					int get()
					{
						return this->_index;
					}
				}

				IllegalClapackArgumentException(int index)
				{
					this->_index = index;
				}

				IllegalClapackArgumentException(String^ message, int index)
					: ClwException(message)
				{
					this->_index = index;
				}

				IllegalClapackArgumentException(String^ message, int index, Exception^ inner)
					: ClwException(message, inner)
				{
					this->_index = index;
				}
			};

			/// <summary>
			/// 処理対象となる行列やベクトルのサイズが一致していない場合にthrowされる．
			/// </summary>
			public ref class MismatchSizeException : public ClwException
			{
			public:
				MismatchSizeException()
				{}

				MismatchSizeException(String^ message)
					: ClwException(message)
				{}

				MismatchSizeException(String^ message, Exception^ inner)
					: ClwException(message, inner)
				{}
			};

		}// end namespace exception
	}// end namespace clapack
}// end namespace KrdLab
