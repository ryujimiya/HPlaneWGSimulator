using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace HPlaneWGSimulator
{
    /// <summary>
    /// CadLogicのデータを管理
    /// </summary>
    class CadLogicBase
    {
        ////////////////////////////////////////////////////////////////////////
        // 型
        ////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Cadモード
        ///   None 操作なし
        ///   Locatiton 矩形領域位置移動
        ///   Area マス目選択
        ///   AreaFH マス目選択(フリーハンド)
        ///   AreaLine マス目選択(直線)
        ///   AreaEllipse マス目選択(楕円)
        ///   Port ポート境界選択
        ///   Erase 消しゴム
        ///   EraseFH 消しゴム(フリーハンド)
        ///   EraseLine 消しゴム(直線)
        ///   IncidentPort 入力ポート選択
        ///   PortNumbering 入出力ポート番号振り
        /// </summary>
        public enum CadModeType { None, Location, Area, AreaFH, AreaLine, AreaEllipse, Port, Erase, EraseFH, EraseLine, IncidentPort, PortNumbering };

        ////////////////////////////////////////////////////////////////////////
        // 定数
        ////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 分割数
        /// </summary>
        protected static readonly Size MaxDiv = Constants.MaxDiv;
        /// <summary>
        /// 媒質の個数
        /// </summary>
        protected const int MaxMediaCount = Constants.MaxMediaCount;
        public const int VacumnMediaIndex = 0; // 真空領域
        /// <summary>
        /// 媒質インデックスの既定値
        /// </summary>
        protected const int DefMediaIndex = VacumnMediaIndex;  // 真空
        /// <summary>
        /// 媒質の表示背景色
        /// </summary>
        public static Color[] MediaBackColors = new Color[MaxMediaCount]
            {
                Color.Gray, Color.MistyRose, Color.LightGreen
            };

        ////////////////////////////////////////////////////////////////////////
        // フィールド
        ////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 領域選択フラグアレイ
        /// </summary>
        protected bool[,] AreaSelection = new bool[MaxDiv.Height, MaxDiv.Width];
        /// <summary>
        /// 領域 - 媒質インデックスマップ
        /// </summary>
        protected int[,] AreaToMediaIndex = new int[MaxDiv.Height, MaxDiv.Width];
        /// <summary>
        /// Y軸方向境界フラグアレイ
        /// </summary>
        protected bool[,] YBoundarySelection = new bool[MaxDiv.Height, MaxDiv.Width + 1];
        /// <summary>
        /// X軸方向境界フラグアレイ
        /// </summary>
        protected bool[,] XBoundarySelection = new bool[MaxDiv.Height + 1, MaxDiv.Width];
        /// <summary>
        /// 境界リストのリスト
        /// </summary>
        protected IList<Edge> EdgeList = new List<Edge>();
        /// <summary>
        /// 入射ポート番号
        /// </summary>
        protected int IncidentPortNo = 1;
        /// <summary>
        /// 媒質リスト
        /// </summary>
        protected MediaInfo[] Medias = new MediaInfo[Constants.MaxMediaCount];
        /// <summary>
        /// Cadモード
        /// </summary>
        protected CadModeType _CadMode = CadModeType.None;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CadLogicBase()
        {
            init();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected void init()
        {
            _CadMode = CadModeType.None;

            for (int y = 0; y < MaxDiv.Height; y++)
            {
                for (int x = 0; x < MaxDiv.Width; x++)
                {
                    AreaSelection[y, x] = false;
                    AreaToMediaIndex[y, x] = 0;
                }
            }
            for (int x = 0; x < MaxDiv.Width + 1; x++)
            {
                for (int y = 0; y < MaxDiv.Height; y++)
                {
                    YBoundarySelection[y, x] = false;
                }
            }
            for (int y = 0; y < MaxDiv.Height + 1; y++)
            {
                for (int x = 0; x < MaxDiv.Width; x++)
                {
                    XBoundarySelection[y, x] = false;
                }
            }
            EdgeList.Clear();
            IncidentPortNo = 1;
            for (int i = 0; i < Medias.Length; i++)
            {
                MediaInfo media = new MediaInfo();
                media.BackColor = MediaBackColors[i];
                Medias[i] = media;
            }
        }

        /// <summary>
        /// Cadデータをコピーする
        /// </summary>
        /// <param name="src"></param>
        public void CopyData(CadLogicBase src)
        {
            // CadモードもUndo/Redo対象に入れる
            _CadMode = src._CadMode;

            for (int y = 0; y < MaxDiv.Height; y++)
            {
                for (int x = 0; x < MaxDiv.Width; x++)
                {
                    AreaSelection[y, x] = src.AreaSelection[y, x];
                    AreaToMediaIndex[y, x] = src.AreaToMediaIndex[y, x];
                }
            }
            for (int x = 0; x < MaxDiv.Width + 1; x++)
            {
                for (int y = 0; y < MaxDiv.Height; y++)
                {
                    YBoundarySelection[y, x] = src.YBoundarySelection[y, x];
                }
            }
            for (int y = 0; y < MaxDiv.Height + 1; y++)
            {
                for (int x = 0; x < MaxDiv.Width; x++)
                {
                    XBoundarySelection[y, x] = src.XBoundarySelection[y, x];
                }
            }
            EdgeList.Clear();
            foreach (Edge srcEdge in src.EdgeList)
            {
                Edge edge = new Edge(srcEdge.Delta);
                edge.CP(srcEdge);
                EdgeList.Add(edge);
            }
            IncidentPortNo = src.IncidentPortNo;
            //SelectedMediaIndex = src.SelectedMediaIndex;
            for (int i = 0; i < src.Medias.Length; i++)
            {
                Medias[i].SetP(src.Medias[i].P);
                Medias[i].SetQ(src.Medias[i].Q);
            }
        }

    }
}
