#pragma once
/*
 * CLW.h (clapack Wrapper)
 *
 * @author KrdLab
 *
 */

using namespace System;

namespace KrdLab {
    namespace clapack {

        using namespace exception;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 以下  ryujimiya追加分
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Complexを使用したI/F
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 複素数構造体
        ///   clapackのdoublecomplexに対応します
        ///   pin_ptrを介してdoublecomplex型でアクセスするのがこの構造体の主要用途です
        /// </summary>
        public value class Complex
        {
        private:
            /// <summary>
            /// 実数部
            /// </summary>
            double r;
            /// <summary>
            /// 虚数部
            /// </summary>
            double i;
        public:
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="r_">実数部</param>
            /// <param name="i_">虚数部</param>
            Complex(double r_, double i_)
            {
                r = r_;
                i = i_;
            }
            /// <summary>
            /// 実数部
            /// </summary>
            property double Real
            {
                double get() { return r; }
                void set(double value) { r = value; }
            }
            /// <summary>
            /// 虚数部
            /// </summary>
            property double Imaginary
            {
                double get() { return i; }
                void set(double value) { i = value; }
            }
            /// <summary>
            /// 大きさ（絶対値)
            /// </summary>
            property double Magnitude
            {
                double get() { return System::Math::Sqrt(r * r + i * i); }
            }
            /// <summary>
            /// フェーズ
            /// </summary>
            property double Phase
            {
                double get() { return System::Math::Atan2(r, i); }
            }
            /// <summary>
            /// 等しい？
            /// </summary>
            /// <param name="value">オブジェクト</param>
            /// <returns></returns>
            virtual bool Equals(Object^ value) override
            {
                return Complex::Equals((Complex)value);
            }
            /// <summary>
            /// 等しい？
            /// </summary>
            /// <param name="value">複素数</param>
            /// <returns></returns>
            virtual bool Equals(Complex value) sealed
            {
                return (r == value.r && i == value.i);
            }

            /*
            inline Complex operator +=(Complex value)
            {
                r += value.r;
                i += value.i;
                return *this;
            }

            inline Complex operator -=(Complex value)
            {
                r -= value.r;
                i -= value.i;
                return *this;
            }

            inline Complex operator *=(Complex value)
            {
                double lhs_r = r;
                double lhs_i = i;
                r = lhs_r * value.r - lhs_i * value.i;
                i = lhs_r * value.i + lhs_i * value.r;
                return *this;
            }

            inline Complex operator /=(Complex value)
            {
                double lhs_r = r;
                double lhs_i = i;
                double valueSquare = value.r * value.r + value.i * value.i;
                r = (lhs_r * value.r + lhs_i * value.i) / valueSquare;
                i = (-lhs_r * value.i + lhs_i * value.r) / valueSquare;
                
                return *this;
            }
            */
        public:
            /// <summary>
            /// 複素数 0
            /// </summary>
            static const Complex Zero;
            /// <summary>
            /// 虚数単位
            /// </summary>
            static const Complex ImaginaryOne = Complex(0.0, 1.0);
        public:
            /// <summary>
            /// double→Complex
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            static inline operator Complex(double value)
            {
                return Complex(value, 0);
            }
            /// <summary>
            /// int→Complex
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            static inline operator Complex(int value)
            {
                return Complex(value, 0);
            }
            /// <summary>
            /// System::Numerics::Complex→Complex
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            //static inline operator Complex(System::Numerics::Complex value)
            //{
            //    return Complex(value.Real, value.Imaginary);
            //}
            /// <summary>
            /// Complex→System::Numerics::Complex
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            static System::Numerics::Complex ToDotNetComplex(Complex value)
            {
                return System::Numerics::Complex(value.r, value.i);
            }
            /// <summary>
            /// 複素数の大きさ(絶対値)を取得する
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            static double Abs(Complex value)
            {
                return value.Magnitude;
            }
            /// <summary>
            /// 複素数の複素共役を取得する
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            static Complex Conjugate(Complex value)
            {
                return Complex(value.r, -value.i);
            }
            /// <summary>
            /// 複素数の平方根を取得する
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            static Complex Sqrt(Complex value)
            {
                System::Numerics::Complex ret = System::Numerics::Complex::Sqrt(ToDotNetComplex(value));
                return Complex(ret.Real, ret.Imaginary);
            }
            /// <summary>
            /// exp(複素数)
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            static Complex Exp(Complex value)
            {
                System::Numerics::Complex ret = System::Numerics::Complex::Exp(ToDotNetComplex(value));
                return Complex(ret.Real, ret.Imaginary);
            }

            /// <summary>
            /// 加算
            /// </summary>
            /// <param name="lhs"></param>
            /// <param name="rhs"></param>
            /// <returns></returns>
            static inline Complex Add(Complex lhs, Complex rhs)
            {
                return Complex(lhs.r + rhs.r, lhs.i + rhs.i);
            }
            static inline Complex Add(double d, Complex rhs)
            {
                return Complex(d + rhs.r, rhs.i);
            }
            static inline Complex Add(Complex lhs, double d)
            {
                return Complex(lhs.r + d, lhs.i);
            }
            //
            static inline Complex operator+(Complex lhs, Complex rhs)
            {
                return Complex(lhs.r + rhs.r, lhs.i + rhs.i);
            }
            static inline Complex operator+(double d, Complex rhs)
            {
                return Complex(d + rhs.r, rhs.i);
            }
            static inline Complex operator+(Complex lhs, double d)
            {
                return Complex(lhs.r + d, lhs.i);
            }
            /// <summary>
            /// 減算
            /// </summary>
            /// <param name="lhs"></param>
            /// <param name="rhs"></param>
            /// <returns></returns>
            static inline Complex Subtract(Complex lhs, Complex rhs)
            {
                return Complex(lhs.r - rhs.r, lhs.i - rhs.i);
            }
            static inline Complex Subtract(double d, Complex rhs)
            {
                return Complex(d - rhs.r, -rhs.i);
            }
            static inline Complex Subtract(Complex lhs, double d)
            {
                return Complex(lhs.r - d, lhs.i);
            }
            //
            static inline Complex operator-(Complex lhs, Complex rhs)
            {
                return Complex(lhs.r - rhs.r, lhs.i - rhs.i);
            }
            static inline Complex operator-(double d, Complex rhs)
            {
                return Complex(d - rhs.r, -rhs.i);
            }
            static inline Complex operator-(Complex lhs, double d)
            {
                return Complex(lhs.r - d, lhs.i);
            }
            /// <summary>
            /// 乗算
            /// </summary>
            /// <param name="lhs"></param>
            /// <param name="rhs"></param>
            /// <returns></returns>
            static inline Complex Multiply(Complex lhs, Complex rhs)
            {
                return Complex(lhs.r * rhs.r - lhs.i * rhs.i, lhs.r * rhs.i + lhs.i * rhs.r);
            }
            static inline Complex Multiply(double d, Complex rhs)
            {
                return Complex(d * rhs.r, d * rhs.i);
            }
            static inline Complex Multiply(Complex lhs, double d)
            {
                return Complex(lhs.r * d, lhs.i * d);
            }
            //
            static inline Complex operator*(Complex lhs, Complex rhs)
            {
                return Complex(lhs.r * rhs.r - lhs.i * rhs.i, lhs.r * rhs.i + lhs.i * rhs.r);
            }
            static inline Complex operator*(double d, Complex rhs)
            {
                return Complex(d * rhs.r, d * rhs.i);
            }
            static inline Complex operator*(Complex lhs, double d)
            {
                return Complex(lhs.r * d, lhs.i * d);
            }
            /// <summary>
            /// 除算
            ///    lhs * conj(rhs) / |rhs|^2
            /// </summary>
            /// <param name="lhs"></param>
            /// <param name="rhs"></param>
            /// <returns></returns>
            static inline Complex Divide(Complex lhs, Complex rhs)
            {
                double rhsSquare = rhs.r * rhs.r + rhs.i * rhs.i;
                return Complex((lhs.r * rhs.r + lhs.i * rhs.i) / rhsSquare, (-lhs.r * rhs.i + lhs.i * rhs.r) / rhsSquare);
            }
            static inline Complex Divide(double d, Complex rhs)
            {
                double rhsSquare = rhs.r * rhs.r + rhs.i * rhs.i;
                return Complex(d * rhs.r / rhsSquare, -d * rhs.i / rhsSquare);
            }
            static inline Complex Divide(Complex lhs, double d)
            {
                return Complex(lhs.r / d, lhs.i / d);
            }
            //
            static inline Complex operator/(Complex lhs, Complex rhs)
            {
                double rhsSquare = rhs.r * rhs.r + rhs.i * rhs.i;
                return Complex((lhs.r * rhs.r + lhs.i * rhs.i) / rhsSquare, (-lhs.r * rhs.i + lhs.i * rhs.r) / rhsSquare);
            }
            static inline Complex operator/(double d, Complex rhs)
            {
                double rhsSquare = rhs.r * rhs.r + rhs.i * rhs.i;
                return Complex(d * rhs.r / rhsSquare, -d * rhs.i / rhsSquare);
            }
            static inline Complex operator/(Complex lhs, double d)
            {
                return Complex(lhs.r / d, lhs.i / d);
            }
        };

        /// <summary>
        /// CLAPACK を CLR 上で利用するためのラッパークラス
        ///   ryujimiya追加分を別にしました
        /// </summary>
        public ref class FunctionExt : public Function
        {

        public:
            /// <summary>
            /// <para>A * X = B を解く（ X が解）．</para>
            /// <para>A は n×n の行列，X と B は n×nrhs の行列である．</para>
            /// </summary>
            /// <param name="X"><c>A * X = B</c> の解である X が格納される（実際には B と同じオブジェクトを指す）</param>
            /// <param name="x_row">行列 X の行数が格納される（<c>== <paramref name="b_row"/></c>）</param>
            /// <param name="x_col">行列 X の列数が格納される（<c>== <paramref name="b_col"/></c>）</param>
            /// <param name="A">係数行列（LU分解の結果である P*L*U に書き換えられる．P*L*Uについては<see cref="dgetrf"/>を参照）</param>
            /// <param name="a_row">行列Aの行数</param>
            /// <param name="a_col">行列Aの列数</param>
            /// <param name="B">行列 B（内部のCLAPACK関数により X の値が格納される）</param>
            /// <param name="b_row">行列Bの行数</param>
            /// <param name="b_col">行列Bの列数</param>
            /// <returns>常に 0 が返ってくる．</returns>
            /// <exception cref="IllegalClapackArgumentException">
            /// 内部で zgesv_関数に渡された引数に問題があると throw される．
            /// </exception>
            /// <exception cref="IllegalClapackResultException">
            /// 行列 A の LU分解において，U[i, i] が 0 となってしまった場合に throw される．
            /// この場合，解を求めることができない．
            /// </exception>
            /// <remarks>
            /// <para>対応するCLAPACK関数（CLAPACK/SRC/zgesv.c）</para>
            /// <code>
            /// int zgesv_(integer *n, integer *nrhs,
            ///            doublecomplex *a, integer *lda, integer *ipiv,
            ///            doublecomplex *b, integer *ldb, integer *info)
            /// </code>
            /// <para>zgesv_ 関数の内部では LU分解が使用されている．</para>
            /// </remarks>
            static int zgesv(array<Complex>^% X, int% x_row, int% x_col,
                             array<Complex>^  A, int  a_row, int  a_col,
                             array<Complex>^  B, int  b_row, int  b_col)
            {
                integer n    = a_row;    // input: 連立一次方程式の式数．正方行列[A]の次数(n≧0)． 
                integer nrhs = b_col;    // input: 行列BのColumn数
                
                pin_ptr<void> a_ptr = &A[0];
                doublecomplex* a = (doublecomplex *)(void *)a_ptr;
                
                pin_ptr<void> b_ptr = &B[0];
                doublecomplex* b = (doublecomplex *)(void *)b_ptr;

                // 配列形式は a[lda×n]
                //   input : n行n列の係数行列[A]
                //   output: LU分解後の行列[L]と行列[U]．ただし，行列[L]の単位対角要素は格納されない．
                //           A=[P]*[L]*[U]であり，[P]は行と列を入れ替える操作に対応する置換行列と呼ばれ，0か1が格納される．
                
                integer lda = n;
                // input: 行列Aの第一次元(のメモリ格納数)．lda≧max(1,n)であり，通常は lda==n で良い．

                integer* ipiv = new integer[n];
                // output: 大きさnの配列．置換行列[P]を定義する軸選択用添え字．

                // input/output: 配列形式はb[ldb×nrhs]．通常はnrhsが1なので，配列形式がb[ldb]となる．
                // input : b[ldb×nrhs]の配列形式をした右辺行列{B}．
                // output: info==0 の場合に，b[ldb×nrhs]形式の解行列{X}が格納される．

                integer ldb = b_row;
                // input: 配列bの第一次元(のメモリ格納数)．ldb≧max(1,n)であり，通常は ldb==n で良い．

                integer info = 1;
                // output:
                // info==0: 正常終了
                // info < 0: info==-i ならば，i番目の引数の値が間違っていることを示す．
                // 0 < info <N-1: 固有ベクトルは計算されていないことを示す．
                // info > N: LAPACK内で問題が生じたことを示す．

                int ret;
                try
                {
                    ret = zgesv_(&n, &nrhs, a, &lda, ipiv, b, &ldb, &info);
                    
                    if(info == 0)
                    {
                        X = B;
                        x_row = b_row;
                        x_col = b_col;
                    }
                    else
                    {
                        X = nullptr;
                        x_row = 0;
                        x_col = 0;
                        
                        if(info < 0)
                        {
                            throw gcnew IllegalClapackArgumentException("Error occurred: " + -info + "-th argument had an illegal value in the clapack.Function.zgesv", -info);
                        }
                        else
                        {
                            throw gcnew IllegalClapackResultException("Error occurred: zgesv_", info);
                        }
                    }
                }
                finally
                {
                    delete[] ipiv; ipiv = nullptr;
                }

                return ret;
            }

            /// <summary>
            /// <para>A * X = B を解く（X が解）．</para>
            /// <para>A は n×n のバンド行列，X と B は n×nrhs の行列である．</para>
            /// </summary>
            /// <param name="X"><c>A * X = B</c> の解である X が格納される（実際には B と同じオブジェクトを指す）</param>
            /// <param name="x_row">行列 X の行数が格納される（<c>== <paramref name="b_row"/></c>）</param>
            /// <param name="x_col">行列 X の列数が格納される（<c>== <paramref name="b_col"/></c>）</param>
            /// <param name="A">バンドストレージ形式で格納された係数行列（LU分解の結果である P*L*U に書き換えられる．）</param>
            /// <param name="a_row">行列Aの行数</param>
            /// <param name="a_col">行列Aの列数</param>
            /// <param name="kl">バンド行列A内のsubdiagonalの数</param>
            /// <param name="ku">バンド行列A内のsuperdiagonalの数</param>
            /// <param name="B">行列 B（内部のCLAPACK関数により X の値が格納される）</param>
            /// <param name="b_row">行列Bの行数</param>
            /// <param name="b_col">行列Bの列数</param>
            /// <returns>常に 0 が返ってくる．</returns>
            /// <exception cref="IllegalClapackArgumentException">
            /// 内部で zgbsv_関数に渡された引数に問題があると throw される．
            /// </exception>
            /// <exception cref="IllegalClapackResultException">
            /// 行列 A の LU分解において，U[i, i] が 0 となってしまった場合に throw される．
            /// この場合，解を求めることができない．
            /// </exception>
            /// <remarks>
            /// <para>対応するCLAPACK関数（CLAPACK/SRC/zgbsv.c）</para>
            /// <code>
            /// int zgbsv_(integer *n, integer *kl, integer *ku, integer *nrhs,
            ///            doublecomplex *ab, integer *ldab, integer *ipiv,
            ///            doublecomplex *b, integer *ldb, integer *info)
            /// </code>
            /// <para>zgbsv_ 関数の内部では LU分解が使用されている．</para>
            /// </remarks>
            static int zgbsv(array<Complex>^% X, int% x_row, int% x_col,
                             array<Complex>^  A, int  a_row, int  a_col,
                             int kl, int ku,
                             array<Complex>^  B, int  b_row, int  b_col)
            {
                pin_ptr<void> a_ptr = &A[0];
                doublecomplex* a = (doublecomplex *)(void *)a_ptr;
                // COMPLEX*16 array, dimension (LDAB,N)
                // On entry, the matrix A in band storage, in rows KL+1 to 2*KL+KU+1; rows 1 to KL of the array need not be set.
                // The j-th column of A is stored in the j-th column of the array AB as follows:
                // AB(KL+KU+1+i-j,j) = A(i,j) for max(1,j-KU)<=i<=min(N,j+KL)
                
                pin_ptr<void> b_ptr = &B[0];
                doublecomplex* b = (doublecomplex *)(void *)b_ptr;
                // COMPLEX*16 array, dimension (LDB,NRHS)
                // On entry, the N-by-NRHS right hand side matrix B.
                
                integer n = a_row;
                // The number of linear equations, i.e., the order of the matrix A.  N >= 0.

                integer kl_ = kl;
                // The number of subdiagonals within the band of A.  KL >= 0.

                integer ku_ = ku;
                // The number of superdiagonals within the band of A.  KU >= 0.

                integer nrhs = b_col;
                // The number of right hand sides, i.e., the number of columns

                integer lda = 2 * kl + ku + 1;
                // The leading dimension of the array AB.  LDAB >= 2*KL+KU+1.

                integer* ipiv = new integer[n];

                integer ldb = b_row;

                integer info = 1;
                // output:
                // info==0: 正常終了
                // info < 0: info==-i ならば，i番目の引数の値が間違っていることを示す．
                // 0 < info <N-1: 固有ベクトルは計算されていないことを示す．
                // info > N: LAPACK内で問題が生じたことを示す．

                int ret;
                try
                {
                    ret = zgbsv_(&n, &kl_, &ku_, &nrhs, a, &lda, ipiv, b, &ldb, &info);
                    
                    if(info == 0)
                    {
                        X = B;
                        x_row = b_row;
                        x_col = b_col;
                    }
                    else
                    {
                        X = nullptr;
                        x_row = 0;
                        x_col = 0;
                        
                        if(info < 0)
                        {
                            throw gcnew IllegalClapackArgumentException("Error occurred: " + -info + "-th argument had an illegal value in the clapack.Function.zgbsv", -info);
                        }
                        else
                        {
                            throw gcnew IllegalClapackResultException("Error occurred: zgbsv_", info);
                        }
                    }
                }
                finally
                {
                    delete[] ipiv; ipiv = nullptr;
                }

                return ret;
            }

            /// <summary>
            /// <para>A * X = B を解く（X が解）．</para>
            /// <para>A は n×n のバンド行列，X と B は n×nrhs の行列である．</para>
            /// </summary>
            /// <param name="X"><c>A * X = B</c> の解である X が格納される（実際には B と同じオブジェクトを指す）</param>
            /// <param name="x_row">行列 X の行数が格納される（<c>== <paramref name="b_row"/></c>）</param>
            /// <param name="x_col">行列 X の列数が格納される（<c>== <paramref name="b_col"/></c>）</param>
            /// <param name="A">バンドストレージ形式で格納された係数行列（LU分解の結果である P*L*U に書き換えられる．）</param>
            /// <param name="a_row">行列Aの行数</param>
            /// <param name="a_col">行列Aの列数</param>
            /// <param name="kl">バンド行列A内のsubdiagonalの数</param>
            /// <param name="ku">バンド行列A内のsuperdiagonalの数</param>
            /// <param name="B">行列 B（内部のCLAPACK関数により X の値が格納される）</param>
            /// <param name="b_row">行列Bの行数</param>
            /// <param name="b_col">行列Bの列数</param>
            /// <returns>常に 0 が返ってくる．</returns>
            /// <exception cref="IllegalClapackArgumentException">
            /// 内部で dgbsv_関数に渡された引数に問題があると throw される．
            /// </exception>
            /// <exception cref="IllegalClapackResultException">
            /// 行列 A の LU分解において，U[i, i] が 0 となってしまった場合に throw される．
            /// この場合，解を求めることができない．
            /// </exception>
            /// <remarks>
            /// <para>対応するCLAPACK関数（CLAPACK/SRC/dgbsv.c）</para>
            /// <code>
            /// int dgbsv_(integer *n, integer *kl, integer *ku, integer *nrhs,
            ///            doublereal *ab, integer *ldab, integer *ipiv,
            ///            doublereal *b, integer *ldb, integer *info)
            /// </code>
            /// <para>dgbsv_ 関数の内部では LU分解が使用されている．</para>
            /// </remarks>
            static int dgbsv(array<double>^% X, int% x_row, int% x_col,
                             array<double>^  A, int  a_row, int  a_col,
                             int kl, int ku,
                             array<double>^  B, int  b_row, int  b_col)
            {
                pin_ptr<double> a_ptr = &A[0];
                doublereal* a = (doublereal *)a_ptr;
                // DOUBLE PRECISION array, dimension (LDAB,N)
                // On entry, the matrix A in band storage, in rows KL+1 to 2*KL+KU+1; rows 1 to KL of the array need not be set.
                // The j-th column of A is stored in the j-th column of the array AB as follows:
                // AB(KL+KU+1+i-j,j) = A(i,j) for max(1,j-KU)<=i<=min(N,j+KL)
                
                pin_ptr<double> b_ptr = &B[0];
                doublereal* b = (doublereal *)b_ptr;
                // DOUBLE PRECISION array, dimension (LDB,NRHS)
                // On entry, the N-by-NRHS right hand side matrix B.
                
                integer n = a_row;
                // The number of linear equations, i.e., the order of the matrix A.  N >= 0.

                integer kl_ = kl;
                // The number of subdiagonals within the band of A.  KL >= 0.

                integer ku_ = ku;
                // The number of superdiagonals within the band of A.  KU >= 0.

                integer nrhs = b_col;
                // The number of right hand sides, i.e., the number of columns

                integer lda = 2 * kl + ku + 1;
                // The leading dimension of the array AB.  LDAB >= 2*KL+KU+1.

                integer* ipiv = new integer[n];

                integer ldb = b_row;

                integer info = 1;
                // output:
                // info==0: 正常終了
                // info < 0: info==-i ならば，i番目の引数の値が間違っていることを示す．
                // 0 < info <N-1: 固有ベクトルは計算されていないことを示す．
                // info > N: LAPACK内で問題が生じたことを示す．

                int ret;
                try
                {
                    ret = dgbsv_(&n, &kl_, &ku_, &nrhs, a, &lda, ipiv, b, &ldb, &info);
                    
                    if(info == 0)
                    {
                        X = B;
                        x_row = b_row;
                        x_col = b_col;
                    }
                    else
                    {
                        X = nullptr;
                        x_row = 0;
                        x_col = 0;
                        
                        if(info < 0)
                        {
                            throw gcnew IllegalClapackArgumentException("Error occurred: " + -info + "-th argument had an illegal value in the clapack.Function.dgbsv", -info);
                        }
                        else
                        {
                            throw gcnew IllegalClapackResultException("Error occurred: dgbsv_", info);
                        }
                    }
                }
                finally
                {
                    delete[] ipiv; ipiv = nullptr;
                }

                return ret;
            }

            /// <summary>
            /// <para>固有値分解</para>
            /// <para>計算された固有ベクトルは，大きさ（ユークリッドノルム）が 1 に規格化されている．</para>
            /// </summary>
            /// <param name="X">固有値分解される行列（計算の過程で上書きされる）</param>
            /// <param name="x_row">行列 <paramref name="X"/> の行数</param>
            /// <param name="x_col">行列 <paramref name="X"/> の列数</param>
            /// <param name="evals">固有値</param>
            /// <param name="evecs">固有ベクトル</param>
            /// <returns>常に 0 が返ってくる．</returns>
            /// <remarks>
            /// <para>対応するCLAPACK関数（CLAPACK/BLAS/SRC/zgeev.c）</para>
            /// <code>
            /// int zgeev_(char *jobvl, char *jobvr, integer *n, 
            ///            doublecomplex *a, integer *lda, doublecomplex *w, doublecomplex *vl, 
            ///            integer *ldvl, doublecomplex *vr, integer *ldvr, doublecomplex *work, 
            ///            integer *lwork, doublereal *rwork, integer *info)
            /// </code>
            /// </remarks>
            static int zgeev(array<Complex>^ X, int x_row, int x_col,
                             array<Complex>^% evals,
                             array< array<Complex>^ >^% evecs)
            {
                char jobvl = 'N';
                // 左固有ベクトルを
                //   if jobvl == 'V' then 計算する
                //   if jobvl == 'N' then 計算しない

                char jobvr = 'V';
                // 右固有ベクトルを
                //   if jobvr == 'V' then 計算する
                //   if jobvr == 'N' then 計算しない

                integer n = x_col;
                // 行列 X の大きさ（N×Nなので，片方だけでよい）
                
                integer lda = n;
                // the leading dimension of the array A. lda >= max(1, N).

                pin_ptr<void> a_ptr = &X[0];
                doublecomplex* a = (doublecomplex *)(void *)a_ptr;

                // [lda, n] N×N の行列 X
                // 配列 a （行列 X）は，計算の過程で上書きされる．
                
                doublecomplex* w = new doublecomplex[n];
                

                /*
                 * ※左固有ベクトルは計算しない
                 */
                

                integer ldvl = 1;
                // 必ず 1 <= ldvl を満たす必要がある．
                // if jobvl == 'V' then N <= ldvl

                doublecomplex* vl = nullptr;
                // vl is not referenced, because jobvl == 'N'.
                
                /*
                 * ※右固有ベクトルは計算する
                 */

                integer ldvr = n;
                // 必ず 1 <= ldvr を満たす必要がある．
                // if jobvr == 'V' then N <= ldvr

                doublecomplex* vr = new doublecomplex[ldvr * n];
                // if jobvr == 'V' then 右固有ベクトルが vr の各列に，固有値と同じ順序で格納される．
                // if jobvr == 'N' then vr is not referenced.
                
                //
                // その他

                //integer lwork = 4*n;
                integer lwork = 2*n;
                // max(1,2*N) <= lwork
                // 良いパフォーマンスを得るために，大抵の場合 lwork は大きくすべきだ．
                doublecomplex* work = new doublecomplex[lwork];
                // if info == 0 then work[0] returns the optimal lwork.

                doublereal* rwork = new doublereal[2 * n];
                
                integer info = 0;
                // if info == 0 then 正常終了
                // if info <  0 then -info 番目の引数の値が間違っている．
                // if info >  0 then QRアルゴリズムは，全ての固有値を計算できなかった．
                //                   固有ベクトルは計算されていない．
                //                   wr[info+1:N] と wl[info+1:N] には，収束した固有値が含まれている．
                
                
                int ret;
                try
                {
                    // CLAPACKルーチン
                    ret = zgeev_(&jobvl, &jobvr, &n, a, &lda, w, vl, &ldvl, vr, &ldvr, work, &lwork, rwork, &info);

                    if(info == 0)
                    {
                        //
                        // 固有値を格納
                        evals = gcnew array<Complex>(n);
                        for (int i = 0; i < n; i++)
                        {
                            evals[i] = Complex(w[i].r, w[i].i);
                        }
                        
                        //
                        // 固有ベクトルを格納
                        evecs = gcnew array< array<Complex>^ >(n);
                        for (int i = 0; i < n; i++)
                        {
                            // 通常の格納処理
                            evecs[i] = gcnew array<Complex>(ldvr);
                            for(int j=0; j<ldvr; ++j)
                            {
                                doublecomplex v = vr[i*ldvr + j];
                                evecs[i][j] = Complex(v.r, v.i);
                            }
                        }// end for i
                    }// end if info == 0
                    else
                    {
                        if(info < 0)
                        {
                            throw gcnew IllegalClapackArgumentException(
                                "Error occurred: " + -info
                                    + "-th argument had an illegal value in the clapack.Function.zgeev", -info);
                        }
                        else
                        {
                            throw gcnew IllegalClapackResultException("Error occurred: zgeev_", info);
                        }
                    }
                }
                finally
                {
                    // unmanaged code の後始末
                    delete[] w; w = nullptr;
                    delete[] vr; vr = nullptr;
                    delete[] work; work = nullptr;
                    delete[] rwork; rwork = nullptr;
                }

                return ret;
            }

            /// <summary>
            /// <para>一般化固有値問題 Ax=λBxを解く</para>
            /// <para>計算された固有ベクトルは，大きさ（ユークリッドノルム）が 1 に規格化されている．</para>
            /// </summary>
            /// <param name="A">固有値分解される行列A（計算の過程で上書きされる）</param>
            /// <param name="a_row">行列 <paramref name="A"/> の行数</param>
            /// <param name="a_col">行列 <paramref name="A"/> の列数</param>
            /// <param name="B">固有値分解される行列B（計算の過程で上書きされる）</param>
            /// <param name="b_row">行列 <paramref name="B"/> の行数</param>
            /// <param name="b_col">行列 <paramref name="B"/> の列数</param>
            /// <param name="r_evals">固有値の実数部</param>
            /// <param name="i_evals">固有値の虚数部</param>
            /// <param name="r_evecs">固有ベクトルの実数部</param>
            /// <param name="i_evecs">固有ベクトルの虚数部</param>
            /// <returns>常に 0 が返ってくる．</returns>
            /// <remarks>
            /// <para>対応するCLAPACK関数（CLAPACK/BLAS/SRC/dggev.c）</para>
            /// <code>
            /// int dggev_(char *jobvl, char *jobvr, integer *n,
            ///            doublereal *a, integer *lda,
            ///            doublereal *b, integer *ldb,
            ///            doublereal *alphar, doublereal *alphai,
            ///            doublereal *beta,
            ///            doublereal *vl, integer *ldvl, doublereal *vr, integer *ldvr,
            ///            doublereal *work, integer *lwork, integer *info);
            /// </code>
            /// </remarks>
            static int dggev(array<doublereal>^ A, int a_row, int a_col,
                             array<doublereal>^ B, int b_row, int b_col,
                             array<doublereal>^% r_evals,
                             array<doublereal>^% i_evals,
                             array< array<doublereal>^ >^% r_evecs,
                             array< array<doublereal>^ >^% i_evecs )
            {
                char jobvl = 'N';
                // 左固有ベクトルを
                //   if jobvl == 'V' then 計算する
                //   if jobvl == 'N' then 計算しない

                char jobvr = 'V';
                // 右固有ベクトルを
                //   if jobvr == 'V' then 計算する
                //   if jobvr == 'N' then 計算しない

                integer n = a_col;
                // 行列 A, B の大きさ（N×Nなので，片方だけでよい）

                integer lda = n;
                // the leading dimension of the array A. lda >= max(1, N).

                pin_ptr<doublereal> a = &A[0];
                // [lda, n] N×N の行列 A
                // 配列 a （行列 A）は，計算の過程で上書きされる．
                
                integer ldb = n;
                // the leading dimension of B. ldb >= max(1,N).

                pin_ptr<doublereal> b = &B[0];
                // [ldb, n] N×N の行列 B
                // 配列 b （行列 B）は，計算の過程で上書きされる．

                doublereal* alphar = new doublereal[n];
                // 計算された固有値の実部の分子部分が入る．

                doublereal* alphai = new doublereal[n];
                // 計算された固有値の虚部の分子部分が入る．
                // 複素共役対の場合は，alphai[j]=(正値)，alphai[j+1]=(負値) の順に入る．

                doublereal* beta = new doublereal[n];
                // 計算された固有値の分母部分が入る
                //  (alphar[j] + alphai[j] * i) / beta[j] (i:虚数単位)が一般化固有値となる

                /*
                 * ※左固有ベクトルは計算しない
                 */


                integer ldvl = 1;
                // 必ず 1 <= ldvl を満たす必要がある．
                // if jobvl == 'V' then N <= ldvl

                doublereal* vl = nullptr;
                // vl is not referenced, because jobvl == 'N'.

                /*
                 * ※右固有ベクトルは計算する
                 */

                integer ldvr = n;
                // 必ず 1 <= ldvr を満たす必要がある．
                // if jobvr == 'V' then N <= ldvr

                doublereal* vr = new doublereal[ldvr * n];
                // if jobvr == 'V' then 右固有ベクトルが vr の各列に，固有値と同じ順序で格納される．
                // if jobvr == 'N' then vr is not referenced.

                //
                // その他

                integer lwork = 8*n;
                // max(1, 8*N) <= lwork
                // if jobvl == 'V' or jobvr == 'V' then 8*N <= lwork
                // 良いパフォーマンスを得るために，大抵の場合 lwork は大きくすべきだ．
                doublereal* work = new doublereal[lwork];
                // if info == 0 then work[0] returns the optimal lwork.

                integer info = 0;
                // if info == 0 then 正常終了
                // if info <  0 then -info 番目の引数の値が間違っている．
                // if info >  0 then QRアルゴリズムは，全ての固有値を計算できなかった．
                //                   固有ベクトルは計算されていない．
                //                   wr[info+1:N] と wl[info+1:N] には，収束した固有値が含まれている．


                int ret;
                try
                {
                    // CLAPACKルーチン
                    ret = dggev_(&jobvl, &jobvr, &n, a, &lda, b, &ldb, alphar, alphai, beta, vl, &ldvl, vr, &ldvr, work, &lwork, &info);

                    if(info == 0)
                    {
                        //
                        // 固有値を格納
                        r_evals = gcnew array<doublereal>(n);
                        i_evals = gcnew array<doublereal>(n);

                        for(int i=0; i<n; ++i)
                        {
                            if (Math::Abs(beta[i]) < CalculationLowerLimit)
                            {
                                r_evals[i] = ((Math::Abs(alphar[i]) < CalculationLowerLimit) ?
                                    (Double::NaN)
                                    : ((Math::Abs(alphar[i]) > 0) ? (Double::PositiveInfinity) : (Double::NegativeInfinity)));
                                i_evals[i] = ((Math::Abs(alphai[i]) < CalculationLowerLimit) ?
                                    (Double::NaN)
                                    : ((Math::Abs(alphai[i]) > 0) ? (Double::PositiveInfinity) : (Double::NegativeInfinity)));
                            }
                            else
                            {
                                r_evals[i] = ((Math::Abs(alphar[i]) < CalculationLowerLimit) ? (0.0) : (alphar[i] / beta[i]));
                                i_evals[i] = ((Math::Abs(alphai[i]) < CalculationLowerLimit) ? (0.0) : (alphai[i] / beta[i]));
                            }
                        }

                        //
                        // 固有ベクトルを格納
                        r_evecs = gcnew array< array<doublereal>^ >(n);
                        i_evecs = gcnew array< array<doublereal>^ >(n);

                        for(int i=0; i<n; ++i)
                        {
                            if(Math::Abs(alphai[i]) < CalculationLowerLimit)
                            {
                                // 通常の格納処理
                                r_evecs[i] = gcnew array<doublereal>(ldvr);
                                i_evecs[i] = gcnew array<doublereal>(ldvr);

                                for(int j=0; j<ldvr; ++j)
                                {
                                    r_evecs[i][j] = vr[i*ldvr + j];
                                    i_evecs[i][j] = 0.0;
                                }
                            }
                            else
                            {
                                // 虚数になったとき
                                array<doublereal>^ realvec1 = gcnew array<doublereal>(ldvr);
                                array<doublereal>^ realvec2 = gcnew array<doublereal>(ldvr);
                                array<doublereal>^ imgyvec1 = gcnew array<doublereal>(ldvr);
                                array<doublereal>^ imgyvec2 = gcnew array<doublereal>(ldvr);

                                for(int j=0; j<ldvr; ++j)
                                {
                                    realvec1[j] =  vr[ i   *ldvr + j];
                                    realvec2[j] =  vr[ i   *ldvr + j];
                                    imgyvec1[j] =  vr[(i+1)*ldvr + j];
                                    imgyvec2[j] = -vr[(i+1)*ldvr + j];
                                }
                                r_evecs[i  ] = realvec1;
                                r_evecs[i+1] = realvec2;
                                i_evecs[i  ] = imgyvec1;
                                i_evecs[i+1] = imgyvec2;

                                ++i;    // 2列ずつ参照するので，ここでカウントアップ
                            }

                        }// end for i
                    }// end if info == 0
                    else
                    {
                        if(info < 0)
                        {
                            throw gcnew IllegalClapackArgumentException(
                                "Error occurred: " + -info
                                    + "-th argument had an illegal value in the clapack.Function.dggev", -info);
                        }
                        else
                        {
                            throw gcnew IllegalClapackResultException("Error occurred: dggev_", info);
                        }
                    }
                }
                finally
                {
                    // unmanaged code の後始末
                    delete[] alphar; alphar = nullptr;
                    delete[] alphai; alphai = nullptr;
                    delete[] beta; beta = nullptr;
                    delete[] vr; vr = nullptr;
                    delete[] work; work = nullptr;
                }

                return ret;
            }

            /// <summary>
            /// <para>一般化固有値問題 Ax=λBxを解く</para>
            /// <para>Aは対称帯行列. Bは対称帯行列かつ正定値行列である. 計算された固有ベクトルは，大きさ（ユークリッドノルム）が 1 に規格化されている．</para>
            /// </summary>
            /// <param name="A">固有値分解される帯行列A（計算の過程で上書きされる）</param>
            /// <param name="a_row">行列 <paramref name="A"/> の行数</param>
            /// <param name="a_col">行列 <paramref name="A"/> の列数</param>
            /// <param name="ka">行列 <paramref name="A"/> のsuperdiagonalsのサイズ</param>
            /// <param name="B">固有値分解される帯行列B（計算の過程で上書きされる）</param>
            /// <param name="b_row">行列 <paramref name="B"/> の行数</param>
            /// <param name="b_col">行列 <paramref name="B"/> の列数</param>
            /// <param name="kb">行列 <paramref name="B"/> のsuperdiagonalsのサイズ</param>
            /// <param name="evals">固有値</param>
            /// <param name="evecs">固有ベクトル</param>
            /// <returns>常に 0 が返ってくる．</returns>
            /// <remarks>
            /// <para>対応するCLAPACK関数（CLAPACK/BLAS/SRC/dsbgv.c）</para>
            /// <code>
            /// int dsbgv_(char *jobz, char *uplo, integer *n,
            ///            integer *ka, integer *kb, 
            ///            doublereal *ab, integer *ldab,
            ///            doublereal *bb, integer *ldbb,
            ///            doublereal *w,
            ///            doublereal *z, integer *ldz,
            ///            doublereal *work, integer *info);
            /// </code>
            /// </remarks>
            static int dsbgv(array<doublereal>^ A, int a_row, int a_col, int ka, 
                             array<doublereal>^ B, int b_row, int b_col, int kb,
                             array<doublereal>^% evals,
                             array< array<doublereal>^ >^% evecs )
            {
                char jobz = 'V';
                // 固有ベクトルを
                //   if jobvl == 'V' then 計算する
                //   if jobvl == 'N' then 計算しない

                char uplo = 'U';
                // if uplo == 'U' then upper triangles of A and B are stored.
                // if uplo == 'L' then lower triangles of A and B are stored.

                integer n = a_col;
                // 行列 A, B の大きさ（N×Nなので，片方だけでよい）

                integer ka_ = ka;
                // the number of superdiagonals of the matrix A if uplo == 'U'
                // or the number of subdiagonals if uplo == 'L'. ka >= 0.
                
                integer kb_ = kb;
                // the number of superdiagonals of the matrix B if uplo == 'U'
                // or the number of subdiagonals if uplo == 'L'. kb >= 0.

                integer ldab = ka_ + 1;
                // the leading dimension of the array AB. ldab >= ka + 1.

                pin_ptr<doublereal> ab = &A[0];
                // [ldab, n] N×N の行列 Aの上三角行列（または下三角行列)を帯行列形式で格納
                // if UPLO = 'U', AB(ka+1+i-j,j) = A(i,j) for max(1,j-ka)<=i<=j;
                // if UPLO = 'L', AB(1+i-j,j)    = A(i,j) for j<=i<=min(n,j+ka)
                // 配列 ab （行列 A）は，計算の過程で上書きされる．
                
                integer ldbb = kb_ + 1;
                // the leading dimension of the array BB. ldbb >= kb + 1.

                pin_ptr<doublereal> bb = &B[0];
                // [ldbb, n] N×N の行列 Bの上三角行列（または下三角行列)を帯行列形式で格納
                // if UPLO = 'U', BB(kb+1+i-j,j) = B(i,j) for max(1,j-kb)<=i<=j;
                // if UPLO = 'L', BB(1+i-j,j)    = B(i,j) for j<=i<=min(n,j+kb).
                // 配列 bb （行列 B）は，計算の過程で上書きされる．

                doublereal* w = new doublereal[n];
                // 計算された固有値が入る．

                integer ldz = n;
                // 必ず 1 <= ldz を満たす必要がある．
                // if jobz == 'V' then N <= ldz

                doublereal* z = new doublereal[ldz * n];
                // if jobz == 'V' then 固有ベクトルが z の各列に，固有値と同じ順序で格納される．
                // if jobz == 'N' then vr is not referenced.

                //
                // その他

                doublereal* work = new doublereal[3*n];
                // dimension (3*N)

                integer info = 0;
                // if info == 0 then 正常終了
                // if info <  0 then -info 番目の引数の値が間違っている．
                // if info >  0 then QRアルゴリズムは，全ての固有値を計算できなかった．
                //                   固有ベクトルは計算されていない．
                //                   wr[info+1:N] と wl[info+1:N] には，収束した固有値が含まれている．

                int ret;
                try
                {
                    // CLAPACKルーチン
                    ret = dsbgv_(&jobz, &uplo, &n, &ka_, &kb_, ab, &ldab, bb, &ldbb, w, z, &ldz, work, &info);

                    if(info == 0)
                    {
                        //
                        // 固有値を格納
                        evals = gcnew array<doublereal>(n);

                        for(int i=0; i<n; ++i)
                        {
                            evals[i] = ((Math::Abs(w[i]) < CalculationLowerLimit) ? (0.0) : (w[i]));
                        }

                        //
                        // 固有ベクトルを格納
                        evecs = gcnew array< array<doublereal>^ >(n);

                        for(int i=0; i<n; ++i)
                        {
                            // 通常の格納処理
                            evecs[i] = gcnew array<doublereal>(ldz);

                            for(int j=0; j<ldz; ++j)
                            {
                                evecs[i][j] = z[i*ldz + j];
                            }
                        }// end for i
                    }// end if info == 0
                    else
                    {
                        if(info < 0)
                        {
                            throw gcnew IllegalClapackArgumentException(
                                "Error occurred: " + -info
                                    + "-th argument had an illegal value in the clapack.Function.dsbgv", -info);
                        }
                        else if (info > n && info <= 2*n)
                        {
                            throw gcnew IllegalClapackResultException(
                                "Error occurred: " +  "B(" + (info - n - 1) + ", " + (info - n - 1) + ") was was negative. B is not positive definite.", (info - n - 1));
                        }
                        else
                        {
                            throw gcnew IllegalClapackResultException("Error occurred: dsbgv_", info);
                        }
                    }
                }
                finally
                {
                    // unmanaged code の後始末
                    delete[] w; w = nullptr;
                    delete[] z; z = nullptr;
                    delete[] work; work = nullptr;
                }

                return ret;
            }

            /// <summary>
            /// <para>一般化固有値問題 Ax=λBxを解く</para>
            /// <para>計算された固有ベクトルは，大きさ（ユークリッドノルム）が 1 に規格化されている．</para>
            /// </summary>
            /// <param name="A">固有値分解される行列A（計算の過程で上書きされる）</param>
            /// <param name="a_row">行列 <paramref name="A"/> の行数</param>
            /// <param name="a_col">行列 <paramref name="A"/> の列数</param>
            /// <param name="B">固有値分解される行列B（計算の過程で上書きされる）</param>
            /// <param name="b_row">行列 <paramref name="B"/> の行数</param>
            /// <param name="b_col">行列 <paramref name="B"/> の列数</param>
            /// <param name="evals">固有値</param>
            /// <param name="evecs">固有ベクトル</param>
            /// <returns>常に 0 が返ってくる．</returns>
            /// <remarks>
            /// <para>対応するCLAPACK関数（CLAPACK/BLAS/SRC/zggev.c）</para>
            /// <code>
            /// int zggev_(char *jobvl, char *jobvr, integer *n, 
            ///            doublecomplex *a, integer *lda, doublecomplex *b, integter *ldb,
            ///            doublecomplex *alpha, doublecomplex *beta,
            ///            doublecomplex *vl, integer *ldvl, doublecomplex *vr, integer *ldvr,
            ///            doublecomplex *work, integer *lwork, doublereal *rwork, integer *info)
            /// </code>
            /// </remarks>
            static int zggev(array<Complex>^ A, int a_row, int a_col,
                             array<Complex>^ B, int b_row, int b_col,
                             array<Complex>^% evals,
                             array< array<Complex>^ >^% evecs)
            {
                char jobvl = 'N';
                // 左固有ベクトルを
                //   if jobvl == 'V' then 計算する
                //   if jobvl == 'N' then 計算しない

                char jobvr = 'V';
                // 右固有ベクトルを
                //   if jobvr == 'V' then 計算する
                //   if jobvr == 'N' then 計算しない

                integer n = a_col;
                // 行列 A, B の大きさ（N×Nなので，片方だけでよい）
                
                integer lda = n;
                // the leading dimension of the array A. lda >= max(1, N).

                pin_ptr<void> a_ptr = &A[0];
                doublecomplex* a = (doublecomplex *)(void *)a_ptr;
                // [lda, n] N×N の行列 A
                // 配列 a （行列 A）は，計算の過程で上書きされる．

                integer ldb = n;
                // The leading dimension of B.  ldb >= max(1, N).

                pin_ptr<void> b_ptr = &B[0];
                doublecomplex* b = (doublecomplex *)(void *)b_ptr;
                // [ldb, n] N×N の行列 B
                // 配列 b （行列 B）は，計算の過程で上書きされる．
                
                doublecomplex* alpha = new doublecomplex[n];
                // 計算された固有値の分子部分が入る．

                doublecomplex* beta = new doublecomplex[n];
                // 計算された固有値の分母部分が入る．
                

                /*
                 * ※左固有ベクトルは計算しない
                 */
                

                integer ldvl = 1;
                // 必ず 1 <= ldvl を満たす必要がある．
                // if jobvl == 'V' then N <= ldvl

                doublecomplex* vl = nullptr;
                // vl is not referenced, because jobvl == 'N'.
                
                /*
                 * ※右固有ベクトルは計算する
                 */

                integer ldvr = n;
                // 必ず 1 <= ldvr を満たす必要がある．
                // if jobvr == 'V' then N <= ldvr

                doublecomplex* vr = new doublecomplex[ldvr * n];
                // if jobvr == 'V' then 右固有ベクトルが vr の各列に，固有値と同じ順序で格納される．
                // if jobvr == 'N' then vr is not referenced.
                
                //
                // その他

                integer lwork = 2*n;
                // max(1,2*N) <= lwork
                // 良いパフォーマンスを得るために，大抵の場合 lwork は大きくすべきだ．
                doublecomplex* work = new doublecomplex[lwork];
                // if info == 0 then work[0] returns the optimal lwork.

                doublereal* rwork = new doublereal[8 * n];
                // workspace dimension (8*N)
                
                integer info = 0;
                // if info == 0 then 正常終了
                // if info <  0 then -info 番目の引数の値が間違っている．
                // if info >  0 then QRアルゴリズムは，全ての固有値を計算できなかった．
                //                   固有ベクトルは計算されていない．
                //                   wr[info+1:N] と wl[info+1:N] には，収束した固有値が含まれている．
                
                
                int ret;
                try
                {
                    // CLAPACKルーチン
                    ret = zggev_(&jobvl, &jobvr, &n, a, &lda, b, &ldb, alpha, beta, vl, &ldvl, vr, &ldvr, work, &lwork, rwork, &info);

                    if(info == 0)
                    {
                        //
                        // 固有値を格納
                        evals = gcnew array<Complex>(n);
                        for (int i = 0; i < n; i++)
                        {
                            if (Math::Abs(beta[i].r) + Math::Abs(beta[i].i) < CalculationLowerLimit)
                            {
                                double realValue = ((Math::Abs(alpha[i].r) < CalculationLowerLimit) ?
                                    (Double::NaN)
                                    : ((Math::Abs(alpha[i].r) > 0) ? (Double::PositiveInfinity) : (Double::NegativeInfinity)));
                                double imagValue = ((Math::Abs(alpha[i].i) < CalculationLowerLimit) ?
                                    (Double::NaN)
                                    : ((Math::Abs(alpha[i].i) > 0) ? (Double::PositiveInfinity) : (Double::NegativeInfinity)));
                                evals[i] = Complex(realValue, imagValue);
                            }
                            else
                            {
                                //evals[i] = Complex(alpha[i].r, alpha[i].i) / Complex(beta[i].r, beta[i].i);
                                double betaSquareNorm = beta[i].r * beta[i].r + beta[i].i * beta[i].i;
                                evals[i] = Complex(
                                    (alpha[i].r * beta[i].r + alpha[i].i * beta[i].i) / betaSquareNorm,
                                    (-alpha[i].r * beta[i].i + alpha[i].i * beta[i].r) / betaSquareNorm
                                    );
                            }
                        }
                        
                        //
                        // 固有ベクトルを格納
                        evecs = gcnew array< array<Complex>^ >(n);
                        for (int i = 0; i < n; i++)
                        {
                            // 通常の格納処理
                            evecs[i] = gcnew array<Complex>(ldvr);
                            for(int j=0; j<ldvr; ++j)
                            {
                                doublecomplex v = vr[i*ldvr + j];
                                evecs[i][j] = Complex(v.r, v.i);
                            }
                        }// end for i
                    }// end if info == 0
                    else
                    {
                        if(info < 0)
                        {
                            throw gcnew IllegalClapackArgumentException(
                                "Error occurred: " + -info
                                    + "-th argument had an illegal value in the clapack.Function.zggev", -info);
                        }
                        else
                        {
                            throw gcnew IllegalClapackResultException("Error occurred: zggev_", info);
                        }
                    }
                }
                finally
                {
                    // unmanaged code の後始末
                    delete[] alpha; alpha = nullptr;
                    delete[] beta; beta = nullptr;
                    delete[] vr; vr = nullptr;
                    delete[] work; work = nullptr;
                    delete[] rwork; rwork = nullptr;
                }

                return ret;
            }

            /// <summary>
            /// <para>一般化固有値問題 Ax=λBxを解く</para>
            /// <para>Aはエルミート帯行列. Bはエルミート帯行列かつ正定値行列である. 計算された固有ベクトルは，大きさ（ユークリッドノルム）が 1 に規格化されている．</para>
            /// </summary>
            /// <param name="A">固有値分解される帯行列A（計算の過程で上書きされる）</param>
            /// <param name="a_row">行列 <paramref name="A"/> の行数</param>
            /// <param name="a_col">行列 <paramref name="A"/> の列数</param>
            /// <param name="ka">行列 <paramref name="A"/> のsuperdiagonalsのサイズ</param>
            /// <param name="B">固有値分解される帯行列B（計算の過程で上書きされる）</param>
            /// <param name="b_row">行列 <paramref name="B"/> の行数</param>
            /// <param name="b_col">行列 <paramref name="B"/> の列数</param>
            /// <param name="kb">行列 <paramref name="B"/> のsuperdiagonalsのサイズ</param>
            /// <param name="evals">固有値</param>
            /// <param name="evecs">固有ベクトル</param>
            /// <returns>常に 0 が返ってくる．</returns>
            /// <remarks>
            /// <para>対応するCLAPACK関数（CLAPACK/BLAS/SRC/dsbgv.c）</para>
            /// <code>
            /// int zhbgv_(char *jobz, char *uplo, integer *n,
            ///            integer *ka, integer *kb, 
            ///            doublecomplex *ab, integer *ldab,
            ///            doublecomplex *bb, integer *ldbb,
            ///            doublecomplex *w,
            ///            doublecomplex *z, integer *ldz,
            ///            doublecomplex *work, doublereal *rwork, integer *info);
            /// </code>
            /// </remarks>
            static int zhbgv(array<Complex>^ A, int a_row, int a_col, int ka, 
                             array<Complex>^ B, int b_row, int b_col, int kb,
                             array<doublereal>^% evals,
                             array< array<Complex>^ >^% evecs )
            {
                char jobz = 'V';
                // 固有ベクトルを
                //   if jobvl == 'V' then 計算する
                //   if jobvl == 'N' then 計算しない

                char uplo = 'U';
                // if uplo == 'U' then upper triangles of A and B are stored.
                // if uplo == 'L' then lower triangles of A and B are stored.

                integer n = a_col;
                // 行列 A, B の大きさ（N×Nなので，片方だけでよい）

                integer ka_ = ka;
                // the number of superdiagonals of the matrix A if uplo == 'U'
                // or the number of subdiagonals if uplo == 'L'. ka >= 0.
                
                integer kb_ = kb;
                // the number of superdiagonals of the matrix B if uplo == 'U'
                // or the number of subdiagonals if uplo == 'L'. kb >= 0.

                integer ldab = ka_ + 1;
                // the leading dimension of the array AB. ldab >= ka + 1.

                pin_ptr<void> ab_ptr = &A[0];
                doublecomplex *ab = (doublecomplex *)(void *)ab_ptr;
                // [ldab, n] N×N の行列 Aの上三角行列（または下三角行列)を帯行列形式で格納
                // if UPLO = 'U', AB(ka+1+i-j,j) = A(i,j) for max(1,j-ka)<=i<=j;
                // if UPLO = 'L', AB(1+i-j,j)    = A(i,j) for j<=i<=min(n,j+ka)
                // 配列 ab （行列 A）は，計算の過程で上書きされる．
                
                integer ldbb = kb_ + 1;
                // the leading dimension of the array BB. ldbb >= kb + 1.

                pin_ptr<void> bb_ptr = &B[0];
                doublecomplex *bb = (doublecomplex *)(void *)bb_ptr;
                // [ldbb, n] N×N の行列 Bの上三角行列（または下三角行列)を帯行列形式で格納
                // if UPLO = 'U', BB(kb+1+i-j,j) = B(i,j) for max(1,j-kb)<=i<=j;
                // if UPLO = 'L', BB(1+i-j,j)    = B(i,j) for j<=i<=min(n,j+kb).
                // 配列 bb （行列 B）は，計算の過程で上書きされる．

                doublereal* w = new doublereal[n];
                // 計算された固有値が入る．

                integer ldz = n;
                // 必ず 1 <= ldz を満たす必要がある．
                // if jobz == 'V' then N <= ldz

                doublecomplex* z = new doublecomplex[ldz * n];
                // if jobz == 'V' then 固有ベクトルが z の各列に，固有値と同じ順序で格納される．
                // if jobz == 'N' then z is not referenced.

                //
                // その他

                doublecomplex* work = new doublecomplex[n];
                // dimension(N)
                doublereal *rwork = new doublereal[3 * n];
                // dimension (3*N)

                integer info = 0;
                // if info == 0 then 正常終了
                // if info <  0 then -info 番目の引数の値が間違っている．
                // if info >  0 then QRアルゴリズムは，全ての固有値を計算できなかった．
                //                   固有ベクトルは計算されていない．
                //                   wr[info+1:N] と wl[info+1:N] には，収束した固有値が含まれている．

                int ret;
                try
                {
                    // CLAPACKルーチン
                    ret = zhbgv_(&jobz, &uplo, &n, &ka_, &kb_, ab, &ldab, bb, &ldbb, w, z, &ldz, work, rwork, &info);

                    if(info == 0)
                    {
                        //
                        // 固有値を格納
                        evals = gcnew array<doublereal>(n);

                        for(int i=0; i<n; ++i)
                        {
                            evals[i] = ((Math::Abs(w[i]) < CalculationLowerLimit) ? (0.0) : (w[i]));
                        }

                        //
                        // 固有ベクトルを格納
                        evecs = gcnew array< array<Complex>^ >(n);

                        for(int i=0; i<n; ++i)
                        {
                            // 通常の格納処理
                            evecs[i] = gcnew array<Complex>(ldz);

                            for(int j=0; j<ldz; ++j)
                            {
                                doublecomplex v = z[i*ldz + j];
                                evecs[i][j] = Complex(v.r, v.i);
                            }
                        }// end for i
                    }// end if info == 0
                    else
                    {
                        if(info < 0)
                        {
                            throw gcnew IllegalClapackArgumentException(
                                "Error occurred: " + -info
                                    + "-th argument had an illegal value in the clapack.Function.zhbgv", -info);
                        }
                        else if (info > n && info <= 2*n)
                        {
                            throw gcnew IllegalClapackResultException(
                                "Error occurred: " +  "B(" + (info - n - 1) + ", " + (info - n - 1) + ") was was negative. B is not positive definite.", (info - n - 1));
                        }
                        else
                        {
                            throw gcnew IllegalClapackResultException("Error occurred: zhbgv_", info);
                        }
                    }
                }
                finally
                {
                    // unmanaged code の後始末
                    delete[] w; w = nullptr;
                    delete[] z; z = nullptr;
                    delete[] work; work = nullptr;
                    delete[] rwork; rwork = nullptr;
                }

                return ret;
            }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // System::Numerics::Complexを使用したI/F
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public:
            /// <summary>
            /// <para>A * X = B を解く（ X が解）．</para>
            /// <para>A は n×n の行列，X と B は n×nrhs の行列である．</para>
            /// </summary>
            /// <param name="X"><c>A * X = B</c> の解である X が格納される（実際には B と同じオブジェクトを指す）</param>
            /// <param name="x_row">行列 X の行数が格納される（<c>== <paramref name="b_row"/></c>）</param>
            /// <param name="x_col">行列 X の列数が格納される（<c>== <paramref name="b_col"/></c>）</param>
            /// <param name="A">係数行列（LU分解の結果である P*L*U に書き換えられる．P*L*Uについては<see cref="dgetrf"/>を参照）</param>
            /// <param name="a_row">行列Aの行数</param>
            /// <param name="a_col">行列Aの列数</param>
            /// <param name="B">行列 B（内部のCLAPACK関数により X の値が格納される）</param>
            /// <param name="b_row">行列Bの行数</param>
            /// <param name="b_col">行列Bの列数</param>
            /// <returns>常に 0 が返ってくる．</returns>
            /// <exception cref="IllegalClapackArgumentException">
            /// 内部で zgesv_関数に渡された引数に問題があると throw される．
            /// </exception>
            /// <exception cref="IllegalClapackResultException">
            /// 行列 A の LU分解において，U[i, i] が 0 となってしまった場合に throw される．
            /// この場合，解を求めることができない．
            /// </exception>
            /// <remarks>
            /// <para>対応するCLAPACK関数（CLAPACK/SRC/zgesv.c）</para>
            /// <code>
            /// int zgesv_(integer *n, integer *nrhs,
            ///            doublecomplex *a, integer *lda, integer *ipiv,
            ///            doublecomplex *b, integer *ldb, integer *info)
            /// </code>
            /// <para>zgesv_ 関数の内部では LU分解が使用されている．</para>
            /// </remarks>
            static int zgesv(array<System::Numerics::Complex>^% X, int% x_row, int% x_col,
                             array<System::Numerics::Complex>^  A, int  a_row, int  a_col,
                             array<System::Numerics::Complex>^  B, int  b_row, int  b_col)
            {
                integer n    = a_row;    // input: 連立一次方程式の式数．正方行列[A]の次数(n≧0)． 
                integer nrhs = b_col;    // input: 行列BのColumn数
                
                // Aをnativeの複素数構造体に変換
                doublecomplex* a = nullptr;
                try
                {
                    a = new doublecomplex[a_row * a_col];
                    for (int i = 0; i < a_row * a_col; i++)
                    {
                        a[i].r = A[i].Real;
                        a[i].i = A[i].Imaginary;
                    }
                }
                catch (System::Exception^ /*exception*/)
                {
                    if (a != nullptr)
                    {
                        delete[] a;
                        a = nullptr;
                    }
                    throw;
                }
                // Bをnativeの複素数構造体に変換
                doublecomplex* b = nullptr;
                try
                {
                    b = new doublecomplex[b_row * b_col];
                    for (int i = 0; i < b_row * b_col; i++)
                    {
                        b[i].r = B[i].Real;
                        b[i].i = B[i].Imaginary;
                    }
                }
                catch (System::Exception^ /*exception*/)
                {
                    if (a != nullptr)
                    {
                        delete[] a;
                        a = nullptr;
                    }
                    if (b != nullptr)
                    {
                        delete[] b;
                        b = nullptr;
                    }
                    throw;
                }

                // 配列形式は a[lda×n]
                //   input : n行n列の係数行列[A]
                //   output: LU分解後の行列[L]と行列[U]．ただし，行列[L]の単位対角要素は格納されない．
                //           A=[P]*[L]*[U]であり，[P]は行と列を入れ替える操作に対応する置換行列と呼ばれ，0か1が格納される．
                
                integer lda = n;
                // input: 行列Aの第一次元(のメモリ格納数)．lda≧max(1,n)であり，通常は lda==n で良い．

                integer* ipiv = new integer[n];
                // output: 大きさnの配列．置換行列[P]を定義する軸選択用添え字．

                // input/output: 配列形式はb[ldb×nrhs]．通常はnrhsが1なので，配列形式がb[ldb]となる．
                // input : b[ldb×nrhs]の配列形式をした右辺行列{B}．
                // output: info==0 の場合に，b[ldb×nrhs]形式の解行列{X}が格納される．

                integer ldb = b_row;
                // input: 配列bの第一次元(のメモリ格納数)．ldb≧max(1,n)であり，通常は ldb==n で良い．

                integer info = 1;
                // output:
                // info==0: 正常終了
                // info < 0: info==-i ならば，i番目の引数の値が間違っていることを示す．
                // 0 < info <N-1: 固有ベクトルは計算されていないことを示す．
                // info > N: LAPACK内で問題が生じたことを示す．

                int ret;
                try
                {
                    ret = zgesv_(&n, &nrhs, a, &lda, ipiv, b, &ldb, &info);
                    
                    for (int i = 0; i < a_row * a_col; i++)
                    {
                        A[i] = System::Numerics::Complex(a[i].r, a[i].i);
                    }
                    for (int i = 0; i < b_row * b_col; i++)
                    {
                        B[i] = System::Numerics::Complex(b[i].r, b[i].i);
                    }

                    if(info == 0)
                    {
                        X = B;
                        x_row = b_row;
                        x_col = b_col;
                    }
                    else
                    {
                        X = nullptr;
                        x_row = 0;
                        x_col = 0;
                        
                        if(info < 0)
                        {
                            throw gcnew IllegalClapackArgumentException("Error occurred: " + -info + "-th argument had an illegal value in the clapack.Function.zgesv", -info);
                        }
                        else
                        {
                            throw gcnew IllegalClapackResultException("Error occurred: zgesv_", info);
                        }
                    }
                }
                finally
                {
                    delete[] ipiv; ipiv = nullptr;
                    delete[] a;
                    delete[] b;
                }

                return ret;
            }

            /// <summary>
            /// <para>固有値分解</para>
            /// <para>計算された固有ベクトルは，大きさ（ユークリッドノルム）が 1 に規格化されている．</para>
            /// </summary>
            /// <param name="X">固有値分解される行列（計算の過程で上書きされる）</param>
            /// <param name="x_row">行列 <paramref name="X"/> の行数</param>
            /// <param name="x_col">行列 <paramref name="X"/> の列数</param>
            /// <param name="evals">固有値</param>
            /// <param name="evecs">固有ベクトル</param>
            /// <returns>常に 0 が返ってくる．</returns>
            /// <remarks>
            /// <para>対応するCLAPACK関数（CLAPACK/BLAS/SRC/zgeev.c）</para>
            /// <code>
            /// int zgeev_(char *jobvl, char *jobvr, integer *n, 
            ///            doublecomplex *a, integer *lda, doublecomplex *w, doublecomplex *vl, 
            ///            integer *ldvl, doublecomplex *vr, integer *ldvr, doublecomplex *work, 
            ///            integer *lwork, doublereal *rwork, integer *info)
            /// </code>
            /// </remarks>
            static int zgeev(array<System::Numerics::Complex>^ X, int x_row, int x_col,
                             array<System::Numerics::Complex>^% evals,
                             array< array<System::Numerics::Complex>^ >^% evecs)
            {
                char jobvl = 'N';
                // 左固有ベクトルを
                //   if jobvl == 'V' then 計算する
                //   if jobvl == 'N' then 計算しない

                char jobvr = 'V';
                // 右固有ベクトルを
                //   if jobvr == 'V' then 計算する
                //   if jobvr == 'N' then 計算しない

                integer n = x_col;
                // 行列 X の大きさ（N×Nなので，片方だけでよい）
                
                integer lda = n;
                // the leading dimension of the array A. lda >= max(1, N).

                /////pin_ptr<doublereal> a = &X[0];
                doublecomplex* a = nullptr;
                try
                {
                    a = new doublecomplex[x_row * x_col];
                    for (int i = 0; i < x_row * x_col; i++)
                    {
                        a[i].r = X[i].Real;
                        a[i].i = X[i].Imaginary;
                    }
                }
                catch (System::Exception^ /*exception*/)
                {
                    if (a != nullptr)
                    {
                        delete[] a;
                        a = nullptr;
                    }
                    throw;
                }

                // [lda, n] N×N の行列 X
                // 配列 a （行列 X）は，計算の過程で上書きされる．
                
                doublecomplex* w = new doublecomplex[n];
                

                /*
                 * ※左固有ベクトルは計算しない
                 */
                

                integer ldvl = 1;
                // 必ず 1 <= ldvl を満たす必要がある．
                // if jobvl == 'V' then N <= ldvl

                doublecomplex* vl = nullptr;
                // vl is not referenced, because jobvl == 'N'.
                
                /*
                 * ※右固有ベクトルは計算する
                 */

                integer ldvr = n;
                // 必ず 1 <= ldvr を満たす必要がある．
                // if jobvr == 'V' then N <= ldvr

                doublecomplex* vr = new doublecomplex[ldvr * n];
                // if jobvr == 'V' then 右固有ベクトルが vr の各列に，固有値と同じ順序で格納される．
                // if jobvr == 'N' then vr is not referenced.
                
                //
                // その他

                integer lwork = 4*n;
                // max(1,2*N) <= lwork
                // 良いパフォーマンスを得るために，大抵の場合 lwork は大きくすべきだ．
                doublecomplex* work = new doublecomplex[lwork];
                // if info == 0 then work[0] returns the optimal lwork.

                doublereal* rwork = new doublereal[2 * n];
                
                integer info = 0;
                // if info == 0 then 正常終了
                // if info <  0 then -info 番目の引数の値が間違っている．
                // if info >  0 then QRアルゴリズムは，全ての固有値を計算できなかった．
                //                   固有ベクトルは計算されていない．
                //                   wr[info+1:N] と wl[info+1:N] には，収束した固有値が含まれている．
                
                
                int ret;
                try
                {
                    // CLAPACKルーチン
                    ret = zgeev_(&jobvl, &jobvr, &n, a, &lda, w, vl, &ldvl, vr, &ldvr, work, &lwork, rwork, &info);

                    if(info == 0)
                    {
                        //
                        // 固有値を格納
                        evals = gcnew array<System::Numerics::Complex>(n);
                        for(int i=0; i<n; ++i)
                        {
                            evals[i] = System::Numerics::Complex(w[i].r, w[i].i);
                        }
                        
                        //
                        // 固有ベクトルを格納
                        evecs = gcnew array< array<System::Numerics::Complex>^ >(n);
                        for(int i=0; i<n; ++i)
                        {
                            // 通常の格納処理
                            evecs[i] = gcnew array<System::Numerics::Complex>(ldvr);
                            for(int j=0; j<ldvr; ++j)
                            {
                                doublecomplex v = vr[i*ldvr + j];
                                evecs[i][j] = System::Numerics::Complex(v.r, v.i);
                            }
                        }// end for i
                    }// end if info == 0
                    else
                    {
                        if(info < 0)
                        {
                            throw gcnew IllegalClapackArgumentException(
                                "Error occurred: " + -info
                                    + "-th argument had an illegal value in the clapack.Function.zgeev", -info);
                        }
                        else
                        {
                            throw gcnew IllegalClapackResultException("Error occurred: zgeev_", info);
                        }
                    }
                }
                finally
                {
                    // unmanaged code の後始末
                    delete[] w; w = nullptr;
                    delete[] vr; vr = nullptr;
                    delete[] work; work = nullptr;
                    delete[] rwork; rwork = nullptr;
                    delete[] a; // BUGFIX delete忘れ
                }

                return ret;
            }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // ValueTypeを使用したI/F
        //   (本当はComplexのつもりだったのですが、ごめんなさい。ValueType(System::Numerics::Complex^)での受け渡しになっています)
        //
        //   内部でnativeの構造体doublerealのメモリ確保を行うため、メモリが足りなくなることがある
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public:
            /// <summary>
            /// <para>Complex配列を圧縮する</para>
            /// <para>圧縮前のサイズと圧縮後のサイズは同じ.nullptrに置き換わる分メモリ削減になる</para>
            /// </summary>
            /// <param name="A">[IN]対象Complex配列、[OUT]圧縮されたComplex配列</param>
            static void CompressMatFor_zgesv(array<System::Numerics::Complex^>^% A)
            {
                array<System::Numerics::Complex^>^ compressed = gcnew array<System::Numerics::Complex^>(A->Length);
                int indexCounter = 0;
                int zeroCounter = 0;
                for (int i = 0; i < A->Length; i++)
                {
                    if (A[i] == nullptr || System::Numerics::Complex::Abs(*A[i]) < CalculationLowerLimit)
                    {
                        zeroCounter++;
                        if (i == A->Length - 1 && zeroCounter > 0)
                        {
                            compressed[indexCounter++] = gcnew System::Numerics::Complex();
                            compressed[indexCounter++] = gcnew System::Numerics::Complex((double)zeroCounter, 0);
                            zeroCounter = 0;
                        }
                    }
                    else
                    {
                        if (zeroCounter > 0)
                        {
                            compressed[indexCounter++] = gcnew System::Numerics::Complex();
                            compressed[indexCounter++] = gcnew System::Numerics::Complex((double)zeroCounter, 0);
                            zeroCounter = 0;
                        }
                        compressed[indexCounter++] = A[i];
                    }
                }
                if (A->Length != indexCounter)
                {
                    A = compressed;
                }
                compressed = nullptr;
            }

        private:
            /// <summary>
            /// <para>圧縮されたComplex配列を元に戻す</para>
            /// <para>A(解凍前)のサイズとa(解凍後)のサイズは同じ.</para>
            /// </summary>
            /// <param name="A">圧縮されたComplex配列</param>
            /// <param name="a">出力doublecomplex配列(メモリ確保は行われているものとする)</param>
            static void deCompressMat(array<System::Numerics::Complex^>^ A, doublecomplex *a)
            {
                int aryIndex = 0;
                int i = 0;
                while (i < A->Length && A[i] != nullptr)
                {
                    if (System::Numerics::Complex::Abs(*A[i]) < CalculationLowerLimit)
                    {
                        // 1つ読み飛ばす
                        i++;
                        int zeroCnt = (int)A[i++]->Real;
                        for (int k = 0; k < zeroCnt; k++)
                        {
                            a[aryIndex].r = 0;
                            a[aryIndex].i = 0;
                            aryIndex++;
                        }
                    }
                    else
                    {
                        a[aryIndex].r = A[i]->Real;
                        a[aryIndex].i = A[i]->Imaginary;
                        i++;
                        aryIndex++;
                    }
                }
            }

        public:
            /// <summary>
            /// <para>A * X = B を解く（ X が解）．</para>
            /// <para>A は n×n の行列，X と B は n×nrhs の行列である．</para>
            /// </summary>
            /// <param name="X"><c>A * X = B</c> の解である X が格納される（実際には B と同じオブジェクトを指す）</param>
            /// <param name="x_row">行列 X の行数が格納される（<c>== <paramref name="b_row"/></c>）</param>
            /// <param name="x_col">行列 X の列数が格納される（<c>== <paramref name="b_col"/></c>）</param>
            /// <param name="A">係数行列（LU分解の結果である P*L*U に書き換えられる．P*L*Uについては<see cref="dgetrf"/>を参照）</param>
            /// <param name="a_row">行列Aの行数</param>
            /// <param name="a_col">行列Aの列数</param>
            /// <param name="B">行列 B（内部のCLAPACK関数により X の値が格納される）</param>
            /// <param name="b_row">行列Bの行数</param>
            /// <param name="b_col">行列Bの列数</param>
            /// <returns>常に 0 が返ってくる．</returns>
            /// <exception cref="IllegalClapackArgumentException">
            /// 内部で zgesv_関数に渡された引数に問題があると throw される．
            /// </exception>
            /// <exception cref="IllegalClapackResultException">
            /// 行列 A の LU分解において，U[i, i] が 0 となってしまった場合に throw される．
            /// この場合，解を求めることができない．
            /// </exception>
            /// <remarks>
            /// <para>対応するCLAPACK関数（CLAPACK/SRC/zgesv.c）</para>
            /// <code>
            /// int zgesv_(integer *n, integer *nrhs,
            ///            doublecomplex *a, integer *lda, integer *ipiv,
            ///            doublecomplex *b, integer *ldb, integer *info)
            /// </code>
            /// <para>zgesv_ 関数の内部では LU分解が使用されている．</para>
            /// </remarks>
            static int zgesv(array<System::Numerics::Complex^>^% X, int% x_row, int% x_col,
                             array<System::Numerics::Complex^>^  A, int  a_row, int  a_col,
                             array<System::Numerics::Complex^>^  B, int  b_row, int  b_col)
            {
                return zgesv(X, x_row, x_col, A, a_row, a_col, B, b_row, b_col, false);
            }
            static int zgesv(array<System::Numerics::Complex^>^% X, int% x_row, int% x_col,
                             array<System::Numerics::Complex^>^  A, int  a_row, int  a_col,
                             array<System::Numerics::Complex^>^  B, int  b_row, int  b_col,
                             bool compressFlg)
            {
                integer n    = a_row;    // input: 連立一次方程式の式数．正方行列[A]の次数(n≧0)． 
                integer nrhs = b_col;    // input: 行列BのColumn数
                
                // Aをnativeの複素数構造体に変換
                doublecomplex* a = nullptr;
                try
                {
                    a = new doublecomplex[a_row * a_col];
                    if (compressFlg)
                    {
                        deCompressMat(A, a);
                    }
                    else
                    {
                        for (int i = 0; i < a_row * a_col; i++)
                        {
                            a[i].r = A[i]->Real;
                            a[i].i = A[i]->Imaginary;
                        }
                    }
                }
                catch (System::Exception^ /*exception*/)
                {
                    if (a != nullptr)
                    {
                        delete[] a;
                        a = nullptr;
                    }
                    throw;
                }
                // Bをnativeの複素数構造体に変換
                doublecomplex* b = nullptr;
                try
                {
                    b = new doublecomplex[b_row * b_col];
                    for (int i = 0; i < b_row * b_col; i++)
                    {
                        b[i].r = B[i]->Real;
                        b[i].i = B[i]->Imaginary;
                    }
                }
                catch (System::Exception^ /*exception*/)
                {
                    if (a != nullptr)
                    {
                        delete[] a;
                        a = nullptr;
                    }
                    if (b != nullptr)
                    {
                        delete[] b;
                        b = nullptr;
                    }
                    throw;
                }

                // 配列形式は a[lda×n]
                //   input : n行n列の係数行列[A]
                //   output: LU分解後の行列[L]と行列[U]．ただし，行列[L]の単位対角要素は格納されない．
                //           A=[P]*[L]*[U]であり，[P]は行と列を入れ替える操作に対応する置換行列と呼ばれ，0か1が格納される．
                
                integer lda = n;
                // input: 行列Aの第一次元(のメモリ格納数)．lda≧max(1,n)であり，通常は lda==n で良い．

                integer* ipiv = new integer[n];
                // output: 大きさnの配列．置換行列[P]を定義する軸選択用添え字．

                // input/output: 配列形式はb[ldb×nrhs]．通常はnrhsが1なので，配列形式がb[ldb]となる．
                // input : b[ldb×nrhs]の配列形式をした右辺行列{B}．
                // output: info==0 の場合に，b[ldb×nrhs]形式の解行列{X}が格納される．

                integer ldb = b_row;
                // input: 配列bの第一次元(のメモリ格納数)．ldb≧max(1,n)であり，通常は ldb==n で良い．

                integer info = 1;
                // output:
                // info==0: 正常終了
                // info < 0: info==-i ならば，i番目の引数の値が間違っていることを示す．
                // 0 < info <N-1: 固有ベクトルは計算されていないことを示す．
                // info > N: LAPACK内で問題が生じたことを示す．

                int ret;
                try
                {
                    ret = zgesv_(&n, &nrhs, a, &lda, ipiv, b, &ldb, &info);
                    
                    for (int i = 0; i < a_row * a_col; i++)
                    {
                        System::Numerics::Complex^ c = gcnew System::Numerics::Complex(a[i].r, a[i].i);
                        A[i] = c;
                    }
                    for (int i = 0; i < b_row * b_col; i++)
                    {
                        System::Numerics::Complex^ c = gcnew System::Numerics::Complex(b[i].r, b[i].i);
                        B[i] = c;
                    }

                    if(info == 0)
                    {
                        X = B;
                        x_row = b_row;
                        x_col = b_col;
                    }
                    else
                    {
                        X = nullptr;
                        x_row = 0;
                        x_col = 0;
                        
                        if(info < 0)
                        {
                            throw gcnew IllegalClapackArgumentException("Error occurred: " + -info + "-th argument had an illegal value in the clapack.Function.zgesv", -info);
                        }
                        else
                        {
                            throw gcnew IllegalClapackResultException("Error occurred: zgesv_", info);
                        }
                    }
                }
                finally
                {
                    delete[] ipiv; ipiv = nullptr;
                    delete[] a;
                    delete[] b;
                }

                return ret;
            }

            /// <summary>
            /// <para>固有値分解</para>
            /// <para>計算された固有ベクトルは，大きさ（ユークリッドノルム）が 1 に規格化されている．</para>
            /// </summary>
            /// <param name="X">固有値分解される行列（計算の過程で上書きされる）</param>
            /// <param name="x_row">行列 <paramref name="X"/> の行数</param>
            /// <param name="x_col">行列 <paramref name="X"/> の列数</param>
            /// <param name="evals">固有値</param>
            /// <param name="evecs">固有ベクトル</param>
            /// <returns>常に 0 が返ってくる．</returns>
            /// <remarks>
            /// <para>対応するCLAPACK関数（CLAPACK/BLAS/SRC/zgeev.c）</para>
            /// <code>
            /// int zgeev_(char *jobvl, char *jobvr, integer *n, 
            ///            doublecomplex *a, integer *lda, doublecomplex *w, doublecomplex *vl, 
            ///            integer *ldvl, doublecomplex *vr, integer *ldvr, doublecomplex *work, 
            ///            integer *lwork, doublereal *rwork, integer *info)
            /// </code>
            /// </remarks>
            static int zgeev(array<System::Numerics::Complex^>^ X, int x_row, int x_col,
                             array<System::Numerics::Complex^>^% evals,
                             array< array<System::Numerics::Complex^>^ >^% evecs)
            {
                char jobvl = 'N';
                // 左固有ベクトルを
                //   if jobvl == 'V' then 計算する
                //   if jobvl == 'N' then 計算しない

                char jobvr = 'V';
                // 右固有ベクトルを
                //   if jobvr == 'V' then 計算する
                //   if jobvr == 'N' then 計算しない

                integer n = x_col;
                // 行列 X の大きさ（N×Nなので，片方だけでよい）
                
                integer lda = n;
                // the leading dimension of the array A. lda >= max(1, N).

                /////pin_ptr<doublereal> a = &X[0];
                doublecomplex* a = nullptr;
                try
                {
                    a = new doublecomplex[x_row * x_col];
                    for (int i = 0; i < x_row * x_col; i++)
                    {
                        a[i].r = X[i]->Real;
                        a[i].i = X[i]->Imaginary;
                    }
                }
                catch (System::Exception^ /*exception*/)
                {
                    if (a != nullptr)
                    {
                        delete[] a;
                        a = nullptr;
                    }
                    throw;
                }

                // [lda, n] N×N の行列 X
                // 配列 a （行列 X）は，計算の過程で上書きされる．
                
                doublecomplex* w = new doublecomplex[n];
                

                /*
                 * ※左固有ベクトルは計算しない
                 */
                

                integer ldvl = 1;
                // 必ず 1 <= ldvl を満たす必要がある．
                // if jobvl == 'V' then N <= ldvl

                doublecomplex* vl = nullptr;
                // vl is not referenced, because jobvl == 'N'.
                
                /*
                 * ※右固有ベクトルは計算する
                 */

                integer ldvr = n;
                // 必ず 1 <= ldvr を満たす必要がある．
                // if jobvr == 'V' then N <= ldvr

                doublecomplex* vr = new doublecomplex[ldvr * n];
                // if jobvr == 'V' then 右固有ベクトルが vr の各列に，固有値と同じ順序で格納される．
                // if jobvr == 'N' then vr is not referenced.
                
                //
                // その他

                integer lwork = 4*n;
                // max(1,2*N) <= lwork
                // 良いパフォーマンスを得るために，大抵の場合 lwork は大きくすべきだ．
                doublecomplex* work = new doublecomplex[lwork];
                // if info == 0 then work[0] returns the optimal lwork.

                doublereal* rwork = new doublereal[2 * n];
                
                integer info = 0;
                // if info == 0 then 正常終了
                // if info <  0 then -info 番目の引数の値が間違っている．
                // if info >  0 then QRアルゴリズムは，全ての固有値を計算できなかった．
                //                   固有ベクトルは計算されていない．
                //                   wr[info+1:N] と wl[info+1:N] には，収束した固有値が含まれている．
                
                
                int ret;
                try
                {
                    // CLAPACKルーチン
                    ret = zgeev_(&jobvl, &jobvr, &n, a, &lda, w, vl, &ldvl, vr, &ldvr, work, &lwork, rwork, &info);

                    if(info == 0)
                    {
                        //
                        // 固有値を格納
                        evals = gcnew array<System::Numerics::Complex^>(n);
                        for(int i=0; i<n; ++i)
                        {
                            evals[i] = gcnew System::Numerics::Complex(w[i].r, w[i].i);
                        }
                        
                        //
                        // 固有ベクトルを格納
                        evecs = gcnew array< array<System::Numerics::Complex^>^ >(n);
                        for(int i=0; i<n; ++i)
                        {
                            // 通常の格納処理
                            evecs[i] = gcnew array<System::Numerics::Complex^>(ldvr);
                            for(int j=0; j<ldvr; ++j)
                            {
                                doublecomplex v = vr[i*ldvr + j];
                                evecs[i][j] = gcnew System::Numerics::Complex(v.r, v.i);
                            }
                        }// end for i
                    }// end if info == 0
                    else
                    {
                        if(info < 0)
                        {
                            throw gcnew IllegalClapackArgumentException(
                                "Error occurred: " + -info
                                    + "-th argument had an illegal value in the clapack.Function.zgeev", -info);
                        }
                        else
                        {
                            throw gcnew IllegalClapackResultException("Error occurred: zgeev_", info);
                        }
                    }
                }
                finally
                {
                    // unmanaged code の後始末
                    delete[] w; w = nullptr;
                    delete[] vr; vr = nullptr;
                    delete[] work; work = nullptr;
                    delete[] rwork; rwork = nullptr;
                    delete[] a; // BUGFIX delete忘れ
                }

                return ret;
            }
        };

    }// end namespace clapack
}// end namespace KrdLab
