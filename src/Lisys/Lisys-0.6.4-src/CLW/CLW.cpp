#include "stdafx.h"

/*
 * インクルードファイル群
 * CLAPACKのインクルードは試行錯誤しているため，ちょっと怪しい...
 * 
 * @author KrdLab
 *
 */

// CLAPACKは，Cのライブラリ
extern "C" {
#include "f2c.h"
#include "fblaswr.h"
#include "clapack.h"

#include "f2cCanceller.h"
};

#include "Exceptions.h"
#include "CLW.h"
#include "Checker.h"
