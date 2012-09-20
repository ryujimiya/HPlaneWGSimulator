using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Numerics; // Complex

namespace HPlaneWGSimulator
{
    /// <summary>
    /// Fem要素クラス
    /// </summary>
    class FemElement
    {
        /// <summary>
        /// 要素番号
        /// </summary>
        public int No;
        /// <summary>
        /// 要素節点1-3の全体節点番号
        /// </summary>
        public int[] NodeNumbers;
        /// <summary>
        /// 媒質インデックス
        /// </summary>
        public int MediaIndex;
        /// <summary>
        /// 線の色
        /// </summary>
        public Color LineColor;
        /// <summary>
        /// 背景色
        /// </summary>
        public Color BackColor;
        /// <summary>
        /// 節点(クラス内部で使用)
        /// </summary>
        protected FemNode[] _Nodes;
        /// <summary>
        /// フィールド値(クラス内部で使用)
        /// </summary>
        protected Complex[] _FValues;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FemElement()
        {
            No = 0;
            NodeNumbers = null;
            MediaIndex = 0;
            LineColor = Color.Black;
            BackColor = Color.White;
            _Nodes = null;
            _FValues = null;
        }

        /// <summary>
        /// コピー
        /// </summary>
        /// <param name="src"></param>
        public virtual void CP(FemElement src)
        {
            if (src == this)
            {
                return;
            }
            No = src.No;
            NodeNumbers = null;
            if (src.NodeNumbers != null)
            {
                NodeNumbers = new int[src.NodeNumbers.Length];
                for (int i = 0; i < src.NodeNumbers.Length; i++)
                {
                    NodeNumbers[i] = src.NodeNumbers[i];
                }
            }
            MediaIndex = src.MediaIndex;
            LineColor = src.LineColor;
            BackColor = src.BackColor;
        }

        /// <summary>
        /// 節点情報をセットする
        /// </summary>
        /// <param name="nodes">節点情報配列（強制境界を含む全節点を節点番号順に格納した配列)</param>
        public void SetNodesFromAllNodes(FemNode[] nodes)
        {
            _Nodes = new FemNode[NodeNumbers.Length];
            for (int i = 0; i < NodeNumbers.Length; i++)
            {
                int nodeNumber = NodeNumbers[i];
                _Nodes[i] = nodes[nodeNumber - 1];
                System.Diagnostics.Debug.Assert(nodeNumber == _Nodes[i].No);
            }
        }

        /// <summary>
        /// フィールド値をセットする
        /// </summary>
        /// <param name="valuesAll"></param>
        /// <param name="nodesRegionToIndex"></param>
        public void SetFieldValueFromAllValues(Complex[] valuesAll, Dictionary<int, int> nodesRegionToIndex)
        {
            _FValues = new Complex[NodeNumbers.Length];
            for (int ino = 0; ino < NodeNumbers.Length; ino++)
            {
                int nodeNumber = NodeNumbers[ino];
                if (nodesRegionToIndex.ContainsKey(nodeNumber))
                {
                    int nodeIndex = nodesRegionToIndex[nodeNumber];
                    _FValues[ino] = valuesAll[nodeIndex];
                }
                else
                {
                    // 強制境界とみなす
                    _FValues[ino] = new Complex();
                }
            }
        }

        /// <summary>
        /// 要素境界を描画する
        /// </summary>
        /// <param name="g"></param>
        /// <param name="panel"></param>
        public void Draw(Graphics g, Size ofs, Size delta, Size regionSize, bool backFillFlg = false)
        {
            //const int vertexCnt = Constants.TriVertexCnt; //3; // 三角形の頂点の数(2次要素でも同じ)
            Constants.FemElementShapeDV elemShapeDv;
            int order;
            int vertexCnt;
            FemMeshLogic.GetElementShapeDvAndOrderByElemNodeCnt(this.NodeNumbers.Length, out elemShapeDv, out order, out vertexCnt);

            // 三角形(or 四角形)の頂点を取得
            Point[] points = new Point[vertexCnt];
            for (int ino = 0; ino < vertexCnt; ino++)
            {
                FemNode node = _Nodes[ino];
                System.Diagnostics.Debug.Assert(node.Coord.Length == 2);
                int x = (int)((double)node.Coord[0] * delta.Width);
                int y = (int)(regionSize.Height - (double)node.Coord[1] * delta.Height);
                points[ino] = new Point(x, y) + ofs;
            }
            // 三角形(or 四角形)を描画
            if (backFillFlg)
            {
                // 要素の背景を塗りつぶす
                using (Brush brush = new SolidBrush(BackColor))
                {
                    g.FillPolygon(brush, points);
                }
            }
            using (Pen selectedPen = new Pen(LineColor, 1))
            {
                // 境界線の描画
                //selectedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                g.DrawPolygon(selectedPen, points);
            }
        }

        /// <summary>
        /// フィールド値を描画する
        /// </summary>
        /// <param name="g"></param>
        /// <param name="ofs"></param>
        /// <param name="delta"></param>
        /// <param name="regionSize"></param>
        /// <param name="colorMap"></param>
        public virtual void DrawField(Graphics g, Size ofs, Size delta, Size regionSize, ColorMap colorMap)
        {
        }
    }
}
