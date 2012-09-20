using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace HPlaneWGSimulator
{
    /// <summary>
    /// 要素分割のロジックをここにまとめます
    /// </summary>
    class FemMeshLogic
    {
        /// <summary>
        /// ２次三角形要素メッシュを作成する
        /// </summary>
        /// <param name="maxDiv">図面の領域サイズ</param>
        /// <param name="areaSelection">マス目選択フラグ配列</param>
        /// <param name="areaToMediaIndex">マス目→媒質インデックスマップ配列</param>
        /// <param name="edgeList">ポート境界リスト</param>
        /// <param name="doubleCoords">[OUT]座標リスト（このリストのインデックス+1が節点番号として扱われます)</param>
        /// <param name="elements">[OUT]要素データリスト 要素データはint[] = { 要素番号, 媒質インデックス, 節点番号1, 節点番号2, ...}</param>
        /// <param name="portList">[OUT]ポート節点リストのリスト</param>
        /// <param name="forceBCNodeNumbers">強制境界節点配列</param>
        /// <returns></returns>
        public static bool MkTriMeshSecondOrder(
            Size maxDiv,
            bool[,] areaSelection, int[,] areaToMediaIndex,
            IList<Edge> edgeList,
            out IList<double[]> doubleCoords,
            out IList<int[]> elements,
            out IList<IList<int>> portList,
            out int[] forceBCNodeNumbers)
        {
            elements = new List<int[]>(); // 要素リスト

            int[,] nodeNumbers = new int[maxDiv.Height * 2 + 1, maxDiv.Width * 2 + 1]; // 座標 - 節点番号対応マップ
            IList<int[]> coords = new List<int[]>();  // 節点座標
            Dictionary<int, bool> forceBCNodeNumberDic = new Dictionary<int, bool>();
            int nodeCounter = 0; // 節点番号カウンター
            int elementCounter = 0; // 要素番号カウンター

            for (int yy = 0; yy < maxDiv.Height * 2 + 1; yy++)
            {
                for (int xx = 0; xx < maxDiv.Width * 2 + 1; xx++)
                {
                    nodeNumbers[yy, xx] = 0;
                }
            }
            for (int y = 0; y < maxDiv.Height; y++)
            {
                for (int x = 0; x < maxDiv.Width; x++)
                {
                    if (areaSelection[y, x])
                    {
                        // 媒質インデックス
                        int mediaIndex = areaToMediaIndex[y, x];

                        // 全体節点番号
                        //
                        //  1 +--2--+ 3
                        //  4 |  5  | 6
                        //  7 +--8--+ 9
                        //
                        int[][] pps = new int[9][]
                        {
                            new int[2]{ 2 * x    , 2 * y},
                            new int[2]{ 2 * x + 1, 2 * y},
                            new int[2]{ 2 * x + 2, 2 * y},
                            new int[2]{ 2 * x    , 2 * y + 1},
                            new int[2]{ 2 * x + 1, 2 * y + 1},
                            new int[2]{ 2 * x + 2, 2 * y + 1},
                            new int[2]{ 2 * x    , 2 * y + 2},
                            new int[2]{ 2 * x + 1, 2 * y + 2},
                            new int[2]{ 2 * x + 2, 2 * y + 2},
                        };

                        foreach (int[] pp in pps)
                        {
                            int xx = pp[0];
                            int yy = pp[1];
                            if (nodeNumbers[yy, xx] == 0)
                            {
                                nodeNumbers[yy, xx] = ++nodeCounter;
                                coords.Add(new int[2] { xx, yy });
                            }
                        }
                        // 強制境界判定
                        if (x == 0 || (x >= 1 && !areaSelection[y, x - 1]))
                        {
                            int nodeNumber;
                            int xx = 2 * x;
                            int[] yys = new int[3] { 2 * y, 2 * y + 1, 2 * y + 2 };
                            foreach (int yy in yys)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }
                        if (x == maxDiv.Width - 1 || (x <= maxDiv.Width - 2 && !areaSelection[y, x + 1]))
                        {
                            int nodeNumber;
                            int xx = 2 * x + 2;
                            int[] yys = new int[3] { 2 * y, 2 * y + 1, 2 * y + 2 };
                            foreach (int yy in yys)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }

                        if (y == 0 || (y >= 1 && !areaSelection[y - 1, x]))
                        {
                            int nodeNumber;
                            int yy = 2 * y;
                            int[] xxs = new int[3] { 2 * x, 2 * x + 1, 2 * x + 2 };
                            foreach (int xx in xxs)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }
                        if (y == maxDiv.Height - 1 || (y <= maxDiv.Height - 2 && !areaSelection[y + 1, x]))
                        {
                            int nodeNumber;
                            int yy = 2 * y + 2;
                            int[] xxs = new int[3] { 2 * x, 2 * x + 1, 2 * x + 2 };
                            foreach (int xx in xxs)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }

                        // 長方形領域を2つの三角形要素に分割
                        //   要素番号、節点1,2,3の全体節点番号を格納
                        if ((x + y) % 2 == 0)
                        {
                            //
                            //  1 +-6--+ 3
                            //  4 |  +5  
                            //  2 +
                            //
                            elements.Add(new int[]
                                {
                                    ++elementCounter,
                                    mediaIndex, // 追加 ver1.1.0.0
                                    nodeNumbers[2 * y, 2 * x], nodeNumbers[2 * y + 2, 2 * x], nodeNumbers[2 * y, 2 * x + 2],
                                    nodeNumbers[2 * y + 1, 2 * x], nodeNumbers[2 * y + 1, 2 * x + 1], nodeNumbers[2 * y, 2 * x + 1]
                                });
                            //
                            //         + 2
                            //     5+  | 4
                            //  3 +--6-+ 1
                            //
                            elements.Add(new int[]
                                {
                                    ++elementCounter,
                                    mediaIndex, // 追加 ver1.1.0.0
                                    nodeNumbers[2 * y + 2, 2 * x + 2], nodeNumbers[2 * y, 2 * x + 2], nodeNumbers[2 * y + 2, 2 * x],
                                    nodeNumbers[2 * y + 1, 2 * x + 2], nodeNumbers[2 * y + 1, 2 * x + 1], nodeNumbers[2 * y + 2, 2 * x + 1]
                                });
                        }
                        else
                        {
                            //
                            //  3 +
                            //  6 |  +5
                            //  1 +--4-+ 2
                            //
                            elements.Add(new int[]
                                {
                                    ++elementCounter,
                                    mediaIndex, // 追加 ver1.1.0.0
                                    nodeNumbers[2 * y + 2, 2 * x], nodeNumbers[2 * y + 2, 2 * x + 2], nodeNumbers[2 * y, 2 * x],
                                    nodeNumbers[2 * y + 2, 2 * x + 1], nodeNumbers[2 * y + 1, 2 * x + 1], nodeNumbers[2 * y + 1, 2 * x]
                                });
                            //
                            //  2 +-4--+ 1
                            //     5+  | 6
                            //         + 3
                            //
                            elements.Add(new int[]
                                {
                                    ++elementCounter,
                                    mediaIndex, // 追加 ver1.1.0.0
                                    nodeNumbers[2 * y, 2 * x + 2], nodeNumbers[2 * y, 2 * x], nodeNumbers[2 * y + 2, 2 * x + 2],
                                    nodeNumbers[2 * y, 2 * x + 1], nodeNumbers[2 * y + 1, 2 * x + 1], nodeNumbers[2 * y + 1, 2 * x + 2]
                                });
                        }
                    }
                }
            }

            // ポート境界
            int portCounter = 0;
            portList = new List<IList<int>>();
            IList<int> portNodes;

            foreach (Edge edge in edgeList)
            {
                portCounter++;
                System.Diagnostics.Debug.Assert(edge.No == portCounter);
                portNodes = new List<int>();
                if (edge.Delta.Width == 0)
                {
                    // 2次線要素
                    int x = edge.Points[0].X;
                    int sty = edge.Points[0].Y;
                    int edy = edge.Points[1].Y;
                    int xx = 2 * x;
                    for (int y = sty; y < edy; y++)
                    {
                        int[] yys = { 2 * y, 2 * y + 1 };
                        foreach (int yy in yys)
                        {
                            portNodes.Add(nodeNumbers[yy, xx]);
                        }
                    }
                    portNodes.Add(nodeNumbers[2 * edy, xx]);
                }
                else if (edge.Delta.Height == 0)
                {
                    // 2次線要素
                    int y = edge.Points[0].Y;
                    int stx = edge.Points[0].X;
                    int edx = edge.Points[1].X;
                    int yy = 2 * y;
                    for (int x = stx; x < edx; x++)
                    {
                        int[] xxs = { 2 * x, 2 * x + 1 };
                        foreach (int xx in xxs)
                        {
                            portNodes.Add(nodeNumbers[yy, xx]);
                        }
                    }
                    portNodes.Add(nodeNumbers[yy, 2 * edx]);
                }
                else
                {
                    MessageBox.Show("Not implemented");
                }
                portList.Add(portNodes);
            }

            // 強制境界からポート境界の節点を取り除く
            // ただし、始点と終点は強制境界なので残す
            foreach (IList<int> nodes in portList)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (i != 0 && i != nodes.Count - 1)
                    {
                        int nodeNumber = nodes[i];
                        if (forceBCNodeNumberDic.ContainsKey(nodeNumber))
                        {
                            forceBCNodeNumberDic.Remove(nodeNumber);
                        }
                    }
                }
            }

            // 座標値に変換
            doubleCoords = new List<double[]>();
            for (int i = 0; i < coords.Count; i++)
            {
                int[] coord = coords[i];
                int xx = coord[0];
                int yy = coord[1];
                double[] doubleCoord = new double[] { xx * 0.5, maxDiv.Height - yy * 0.5 };
                doubleCoords.Add(doubleCoord);
            }
            forceBCNodeNumbers = forceBCNodeNumberDic.Keys.ToArray();

            return true;
        }

        /// <summary>
        /// ２次四角形要素（セレンディピティ族）メッシュを作成する
        /// </summary>
        /// <param name="maxDiv">図面の領域サイズ</param>
        /// <param name="areaSelection">マス目選択フラグ配列</param>
        /// <param name="areaToMediaIndex">マス目→媒質インデックスマップ配列</param>
        /// <param name="edgeList">ポート境界リスト</param>
        /// <param name="doubleCoords">[OUT]座標リスト（このリストのインデックス+1が節点番号として扱われます)</param>
        /// <param name="elements">[OUT]要素データリスト 要素データはint[] = { 要素番号, 媒質インデックス, 節点番号1, 節点番号2, ...}</param>
        /// <param name="portList">[OUT]ポート節点リストのリスト</param>
        /// <param name="forceBCNodeNumbers">強制境界節点配列</param>
        /// <returns></returns>
        public static bool MkQuadMeshSecondOrderType2(
            Size maxDiv,
            bool[,] areaSelection, int[,] areaToMediaIndex,
            IList<Edge> edgeList,
            out IList<double[]> doubleCoords,
            out IList<int[]> elements,
            out IList<IList<int>> portList,
            out int[] forceBCNodeNumbers)
        {
            elements = new List<int[]>(); // 要素リスト

            int[,] nodeNumbers = new int[maxDiv.Height * 2 + 1, maxDiv.Width * 2 + 1]; // 座標 - 節点番号対応マップ
            IList<int[]> coords = new List<int[]>();  // 節点座標
            Dictionary<int, bool> forceBCNodeNumberDic = new Dictionary<int, bool>();
            int nodeCounter = 0; // 節点番号カウンター
            int elementCounter = 0; // 要素番号カウンター

            for (int yy = 0; yy < maxDiv.Height * 2 + 1; yy++)
            {
                for (int xx = 0; xx < maxDiv.Width * 2 + 1; xx++)
                {
                    nodeNumbers[yy, xx] = 0;
                }
            }
            for (int y = 0; y < maxDiv.Height; y++)
            {
                for (int x = 0; x < maxDiv.Width; x++)
                {
                    if (areaSelection[y, x])
                    {
                        // 媒質インデックス
                        int mediaIndex = areaToMediaIndex[y, x];

                        // 全体節点番号
                        //
                        //  1 +--2--+ 3
                        //  4 |     | 5
                        //  6 +--7--+ 8
                        //
                        int[][] pps = new int[8][]
                        {
                            new int[2]{ 2 * x    , 2 * y},
                            new int[2]{ 2 * x + 1, 2 * y},
                            new int[2]{ 2 * x + 2, 2 * y},
                            new int[2]{ 2 * x    , 2 * y + 1},
                            new int[2]{ 2 * x + 2, 2 * y + 1},
                            new int[2]{ 2 * x    , 2 * y + 2},
                            new int[2]{ 2 * x + 1, 2 * y + 2},
                            new int[2]{ 2 * x + 2, 2 * y + 2},
                        };

                        foreach (int[] pp in pps)
                        {
                            int xx = pp[0];
                            int yy = pp[1];
                            if (nodeNumbers[yy, xx] == 0)
                            {
                                nodeNumbers[yy, xx] = ++nodeCounter;
                                coords.Add(new int[2] { xx, yy });
                            }
                        }
                        // 強制境界判定
                        if (x == 0 || (x >= 1 && !areaSelection[y, x - 1]))
                        {
                            int nodeNumber;
                            int xx = 2 * x;
                            int[] yys = new int[3] { 2 * y, 2 * y + 1, 2 * y + 2 };
                            foreach (int yy in yys)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }
                        if (x == maxDiv.Width - 1 || (x <= maxDiv.Width - 2 && !areaSelection[y, x + 1]))
                        {
                            int nodeNumber;
                            int xx = 2 * x + 2;
                            int[] yys = new int[3] { 2 * y, 2 * y + 1, 2 * y + 2 };
                            foreach (int yy in yys)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }

                        if (y == 0 || (y >= 1 && !areaSelection[y - 1, x]))
                        {
                            int nodeNumber;
                            int yy = 2 * y;
                            int[] xxs = new int[3] { 2 * x, 2 * x + 1, 2 * x + 2 };
                            foreach (int xx in xxs)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }
                        if (y == maxDiv.Height - 1 || (y <= maxDiv.Height - 2 && !areaSelection[y + 1, x]))
                        {
                            int nodeNumber;
                            int yy = 2 * y + 2;
                            int[] xxs = new int[3] { 2 * x, 2 * x + 1, 2 * x + 2 };
                            foreach (int xx in xxs)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }

                        // ２次四角形要素（セレンディピティ族）
                        //
                        //    3+  6  +2      x
                        //    |       |
                        // -  7       5
                        //    |       |
                        //    0+  4  +1
                        //    
                        //    y
                        elements.Add(new int[]
                                {
                                    ++elementCounter,
                                    mediaIndex,
                                    nodeNumbers[2 * y + 2, 2 * x    ], //0
                                    nodeNumbers[2 * y + 2, 2 * x + 2],
                                    nodeNumbers[2 * y    , 2 * x + 2],
                                    nodeNumbers[2 * y    , 2 * x    ], 
                                    nodeNumbers[2 * y + 2, 2 * x + 1], //4
                                    nodeNumbers[2 * y + 1, 2 * x + 2],
                                    nodeNumbers[2 * y    , 2 * x + 1], 
                                    nodeNumbers[2 * y + 1, 2 * x    ]
                                });
                    }
                }
            }

            // ポート境界
            int portCounter = 0;
            portList = new List<IList<int>>();
            IList<int> portNodes;

            foreach (Edge edge in edgeList)
            {
                portCounter++;
                System.Diagnostics.Debug.Assert(edge.No == portCounter);
                portNodes = new List<int>();
                if (edge.Delta.Width == 0)
                {
                    // 2次線要素
                    int x = edge.Points[0].X;
                    int sty = edge.Points[0].Y;
                    int edy = edge.Points[1].Y;
                    int xx = 2 * x;
                    for (int y = sty; y < edy; y++)
                    {
                        int[] yys = { 2 * y, 2 * y + 1 };
                        foreach (int yy in yys)
                        {
                            portNodes.Add(nodeNumbers[yy, xx]);
                        }
                    }
                    portNodes.Add(nodeNumbers[2 * edy, xx]);
                }
                else if (edge.Delta.Height == 0)
                {
                    // 2次線要素
                    int y = edge.Points[0].Y;
                    int stx = edge.Points[0].X;
                    int edx = edge.Points[1].X;
                    int yy = 2 * y;
                    for (int x = stx; x < edx; x++)
                    {
                        int[] xxs = { 2 * x, 2 * x + 1 };
                        foreach (int xx in xxs)
                        {
                            portNodes.Add(nodeNumbers[yy, xx]);
                        }
                    }
                    portNodes.Add(nodeNumbers[yy, 2 * edx]);
                }
                else
                {
                    MessageBox.Show("Not implemented");
                }
                portList.Add(portNodes);
            }

            // 強制境界からポート境界の節点を取り除く
            // ただし、始点と終点は強制境界なので残す
            foreach (IList<int> nodes in portList)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (i != 0 && i != nodes.Count - 1)
                    {
                        int nodeNumber = nodes[i];
                        if (forceBCNodeNumberDic.ContainsKey(nodeNumber))
                        {
                            forceBCNodeNumberDic.Remove(nodeNumber);
                        }
                    }
                }
            }

            // 座標値に変換
            doubleCoords = new List<double[]>();
            for (int i = 0; i < coords.Count; i++)
            {
                int[] coord = coords[i];
                int xx = coord[0];
                int yy = coord[1];
                double[] doubleCoord = new double[] { xx * 0.5, maxDiv.Height - yy * 0.5 };
                doubleCoords.Add(doubleCoord);
            }
            forceBCNodeNumbers = forceBCNodeNumberDic.Keys.ToArray();

            return true;
        }


        /// <summary>
        /// 1次三角形要素メッシュを作成する
        /// </summary>
        /// <param name="maxDiv">図面の領域サイズ</param>
        /// <param name="areaSelection">マス目選択フラグ配列</param>
        /// <param name="areaToMediaIndex">マス目→媒質インデックスマップ配列</param>
        /// <param name="edgeList">ポート境界リスト</param>
        /// <param name="doubleCoords">[OUT]座標リスト（このリストのインデックス+1が節点番号として扱われます)</param>
        /// <param name="elements">[OUT]要素データリスト 要素データはint[] = { 要素番号, 媒質インデックス, 節点番号1, 節点番号2, ...}</param>
        /// <param name="portList">[OUT]ポート節点リストのリスト</param>
        /// <param name="forceBCNodeNumbers">強制境界節点配列</param>
        /// <returns></returns>
        public static bool MkTriMeshFirstOrder(
            Size maxDiv,
            bool[,] areaSelection, int[,] areaToMediaIndex,
            IList<Edge> edgeList,
            out IList<double[]> doubleCoords,
            out IList<int[]> elements,
            out IList<IList<int>> portList,
            out int[] forceBCNodeNumbers)
        {
            elements = new List<int[]>(); // 要素リスト

            int[,] nodeNumbers = new int[maxDiv.Height + 1, maxDiv.Width + 1]; // 座標 - 節点番号対応マップ
            IList<int[]> coords = new List<int[]>();  // 節点座標
            Dictionary<int, bool> forceBCNodeNumberDic = new Dictionary<int, bool>();
            int nodeCounter = 0; // 節点番号カウンター
            int elementCounter = 0; // 要素番号カウンター

            for (int yy = 0; yy < maxDiv.Height + 1; yy++)
            {
                for (int xx = 0; xx < maxDiv.Width + 1; xx++)
                {
                    nodeNumbers[yy, xx] = 0;
                }
            }
            for (int y = 0; y < maxDiv.Height; y++)
            {
                for (int x = 0; x < maxDiv.Width; x++)
                {
                    if (areaSelection[y, x])
                    {
                        // 媒質インデックス
                        int mediaIndex = areaToMediaIndex[y, x];

                        // 全体節点番号
                        //
                        //  1 +----+ 2
                        //    |    |
                        //  3 +----+ 4
                        //
                        int[][] pps = new int[4][]
                        {
                            new int[2]{ x    , y},
                            new int[2]{ x + 1, y},
                            new int[2]{ x    , y + 1},
                            new int[2]{ x + 1, y + 1},
                        };

                        foreach (int[] pp in pps)
                        {
                            int xx = pp[0];
                            int yy = pp[1];
                            if (nodeNumbers[yy, xx] == 0)
                            {
                                nodeNumbers[yy, xx] = ++nodeCounter;
                                coords.Add(new int[2] { xx, yy });
                            }
                        }
                        // 強制境界判定
                        if (x == 0 || (x >= 1 && !areaSelection[y, x - 1]))
                        {
                            int nodeNumber;
                            int xx = x;
                            int[] yys = new int[2] { y, y + 1 };
                            foreach (int yy in yys)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }
                        if (x == maxDiv.Width - 1 || (x <= maxDiv.Width - 2 && !areaSelection[y, x + 1]))
                        {
                            int nodeNumber;
                            int xx = x + 1;
                            int[] yys = new int[2] { y, y + 1 };
                            foreach (int yy in yys)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }

                        if (y == 0 || (y >= 1 && !areaSelection[y - 1, x]))
                        {
                            int nodeNumber;
                            int yy = y;
                            int[] xxs = new int[2] { x, x + 1 };
                            foreach (int xx in xxs)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }
                        if (y == maxDiv.Height - 1 || (y <= maxDiv.Height - 2 && !areaSelection[y + 1, x]))
                        {
                            int nodeNumber;
                            int yy = y + 1;
                            int[] xxs = new int[2] { x, x + 1 };
                            foreach (int xx in xxs)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }

                        // 長方形領域を2つの三角形要素に分割
                        //   要素番号、節点1,2,3の全体節点番号を格納
                        if ((x + y) % 2 == 0)
                        {
                            //
                            //  1 +----+ 3
                            //    |  +   
                            //  2 +
                            //
                            elements.Add(new int[] 
                                {
                                    ++elementCounter,
                                    mediaIndex, // 追加 ver1.1.0.0
                                    nodeNumbers[y, x], nodeNumbers[y + 1, x], nodeNumbers[y, x + 1]
                                });
                            //
                            //         + 2
                            //      +  |
                            //  3 +----+ 1
                            //
                            elements.Add(new int[]
                                {
                                    ++elementCounter,
                                    mediaIndex, // 追加 ver1.1.0.0
                                    nodeNumbers[y + 1, x + 1], nodeNumbers[y, x + 1], nodeNumbers[y + 1, x]
                                });
                        }
                        else
                        {
                            //
                            //  3 +
                            //    |  +
                            //  1 +----+ 2
                            //
                            elements.Add(new int[] 
                               {
                                   ++elementCounter,
                                    mediaIndex, // 追加 ver1.1.0.0
                                   nodeNumbers[y + 1, x], nodeNumbers[y + 1, x + 1], nodeNumbers[y, x]
                               });
                            //
                            //  2 +----+ 1
                            //      +  |
                            //         + 3
                            //
                            elements.Add(new int[] 
                                { 
                                    ++elementCounter,
                                    mediaIndex, // 追加 ver1.1.0.0
                                    nodeNumbers[y, x + 1], nodeNumbers[y, x], nodeNumbers[y + 1, x + 1]
                                });
                        }
                    }
                }
            }

            // ポート境界
            int portCounter = 0;
            portList = new List<IList<int>>();
            IList<int> portNodes;

            foreach (Edge edge in edgeList)
            {
                portCounter++;
                System.Diagnostics.Debug.Assert(edge.No == portCounter);
                portNodes = new List<int>();
                if (edge.Delta.Width == 0)
                {
                    // 1次線要素
                    int x = edge.Points[0].X;
                    int sty = edge.Points[0].Y;
                    int edy = edge.Points[1].Y;
                    int xx = x;
                    for (int y = sty; y < edy; y++)
                    {
                        int yy = y;
                        portNodes.Add(nodeNumbers[yy, xx]);
                    }
                    portNodes.Add(nodeNumbers[edy, xx]);
                }
                else if (edge.Delta.Height == 0)
                {
                    // 1次線要素
                    int y = edge.Points[0].Y;
                    int stx = edge.Points[0].X;
                    int edx = edge.Points[1].X;
                    int yy = y;
                    for (int x = stx; x < edx; x++)
                    {
                        int xx = x;
                        portNodes.Add(nodeNumbers[yy, xx]);
                    }
                    portNodes.Add(nodeNumbers[yy, edx]);
                }
                else
                {
                    MessageBox.Show("Not implemented");
                }
                portList.Add(portNodes);
            }

            // 強制境界からポート境界の節点を取り除く
            // ただし、始点と終点は強制境界なので残す
            foreach (IList<int> nodes in portList)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (i != 0 && i != nodes.Count - 1)
                    {
                        int nodeNumber = nodes[i];
                        if (forceBCNodeNumberDic.ContainsKey(nodeNumber))
                        {
                            forceBCNodeNumberDic.Remove(nodeNumber);
                        }
                    }
                }
            }

            // 座標値に変換
            doubleCoords = new List<double[]>();
            for (int i = 0; i < coords.Count; i++)
            {
                int[] coord = coords[i];
                int xx = coord[0];
                int yy = coord[1];
                double[] doubleCoord = new double[] { xx, maxDiv.Height - yy };
                doubleCoords.Add(doubleCoord);
            }
            forceBCNodeNumbers = forceBCNodeNumberDic.Keys.ToArray();

            return true;
        }
        
        /// <summary>
        /// １次四角形要素メッシュを作成する
        /// </summary>
        /// <param name="maxDiv">図面の領域サイズ</param>
        /// <param name="areaSelection">マス目選択フラグ配列</param>
        /// <param name="areaToMediaIndex">マス目→媒質インデックスマップ配列</param>
        /// <param name="edgeList">ポート境界リスト</param>
        /// <param name="doubleCoords">[OUT]座標リスト（このリストのインデックス+1が節点番号として扱われます)</param>
        /// <param name="elements">[OUT]要素データリスト 要素データはint[] = { 要素番号, 媒質インデックス, 節点番号1, 節点番号2, ...}</param>
        /// <param name="portList">[OUT]ポート節点リストのリスト</param>
        /// <param name="forceBCNodeNumbers">強制境界節点配列</param>
        /// <returns></returns>
        public static bool MkQuadMeshFirstOrder(
            Size maxDiv,
            bool[,] areaSelection, int[,] areaToMediaIndex,
            IList<Edge> edgeList,
            out IList<double[]> doubleCoords,
            out IList<int[]> elements,
            out IList<IList<int>> portList,
            out int[] forceBCNodeNumbers)
        {
            elements = new List<int[]>(); // 要素リスト

            int[,] nodeNumbers = new int[maxDiv.Height + 1, maxDiv.Width + 1]; // 座標 - 節点番号対応マップ
            IList<int[]> coords = new List<int[]>();  // 節点座標
            Dictionary<int, bool> forceBCNodeNumberDic = new Dictionary<int, bool>();
            int nodeCounter = 0; // 節点番号カウンター
            int elementCounter = 0; // 要素番号カウンター

            for (int yy = 0; yy < maxDiv.Height + 1; yy++)
            {
                for (int xx = 0; xx < maxDiv.Width + 1; xx++)
                {
                    nodeNumbers[yy, xx] = 0;
                }
            }
            for (int y = 0; y < maxDiv.Height; y++)
            {
                for (int x = 0; x < maxDiv.Width; x++)
                {
                    if (areaSelection[y, x])
                    {
                        // 媒質インデックス
                        int mediaIndex = areaToMediaIndex[y, x];

                        // 全体節点番号
                        //
                        //  1 +----+ 2
                        //    |    |
                        //  3 +----+ 4
                        //
                        int[][] pps = new int[4][]
                        {
                            new int[2]{ x    , y},
                            new int[2]{ x + 1, y},
                            new int[2]{ x    , y + 1},
                            new int[2]{ x + 1, y + 1},
                        };

                        foreach (int[] pp in pps)
                        {
                            int xx = pp[0];
                            int yy = pp[1];
                            if (nodeNumbers[yy, xx] == 0)
                            {
                                nodeNumbers[yy, xx] = ++nodeCounter;
                                coords.Add(new int[2] { xx, yy });
                            }
                        }
                        // 強制境界判定
                        if (x == 0 || (x >= 1 && !areaSelection[y, x - 1]))
                        {
                            int nodeNumber;
                            int xx = x;
                            int[] yys = new int[2] { y, y + 1 };
                            foreach (int yy in yys)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }
                        if (x == maxDiv.Width - 1 || (x <= maxDiv.Width - 2 && !areaSelection[y, x + 1]))
                        {
                            int nodeNumber;
                            int xx = x + 1;
                            int[] yys = new int[2] { y, y + 1 };
                            foreach (int yy in yys)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }

                        if (y == 0 || (y >= 1 && !areaSelection[y - 1, x]))
                        {
                            int nodeNumber;
                            int yy = y;
                            int[] xxs = new int[2] { x, x + 1 };
                            foreach (int xx in xxs)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }
                        if (y == maxDiv.Height - 1 || (y <= maxDiv.Height - 2 && !areaSelection[y + 1, x]))
                        {
                            int nodeNumber;
                            int yy = y + 1;
                            int[] xxs = new int[2] { x, x + 1 };
                            foreach (int xx in xxs)
                            {
                                nodeNumber = nodeNumbers[yy, xx];
                                if (!forceBCNodeNumberDic.ContainsKey(nodeNumber))
                                {
                                    forceBCNodeNumberDic.Add(nodeNumber, true);
                                }
                            }
                        }

                        // １次四角形要素
                        //
                        //    3+     +2      x
                        //     |     |
                        //     |     |
                        //    0+     +1
                        //    
                        //    y
                        elements.Add(new int[]
                                {
                                    ++elementCounter,
                                    mediaIndex,
                                    nodeNumbers[y + 1, x    ], //0
                                    nodeNumbers[y + 1, x + 1],
                                    nodeNumbers[y    , x + 1],
                                    nodeNumbers[y    , x    ], 
                                });
                    }
                }
            }

            // ポート境界
            int portCounter = 0;
            portList = new List<IList<int>>();
            IList<int> portNodes;

            foreach (Edge edge in edgeList)
            {
                portCounter++;
                System.Diagnostics.Debug.Assert(edge.No == portCounter);
                portNodes = new List<int>();
                if (edge.Delta.Width == 0)
                {
                    // 1次線要素
                    int x = edge.Points[0].X;
                    int sty = edge.Points[0].Y;
                    int edy = edge.Points[1].Y;
                    int xx = x;
                    for (int y = sty; y < edy; y++)
                    {
                        int yy = y;
                        portNodes.Add(nodeNumbers[yy, xx]);
                    }
                    portNodes.Add(nodeNumbers[edy, xx]);
                }
                else if (edge.Delta.Height == 0)
                {
                    // 1次線要素
                    int y = edge.Points[0].Y;
                    int stx = edge.Points[0].X;
                    int edx = edge.Points[1].X;
                    int yy = y;
                    for (int x = stx; x < edx; x++)
                    {
                        int xx = x;
                        portNodes.Add(nodeNumbers[yy, xx]);
                    }
                    portNodes.Add(nodeNumbers[yy, edx]);
                }
                else
                {
                    MessageBox.Show("Not implemented");
                }
                portList.Add(portNodes);
            }

            // 強制境界からポート境界の節点を取り除く
            // ただし、始点と終点は強制境界なので残す
            foreach (IList<int> nodes in portList)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (i != 0 && i != nodes.Count - 1)
                    {
                        int nodeNumber = nodes[i];
                        if (forceBCNodeNumberDic.ContainsKey(nodeNumber))
                        {
                            forceBCNodeNumberDic.Remove(nodeNumber);
                        }
                    }
                }
            }

            // 座標値に変換
            doubleCoords = new List<double[]>();
            for (int i = 0; i < coords.Count; i++)
            {
                int[] coord = coords[i];
                int xx = coord[0];
                int yy = coord[1];
                double[] doubleCoord = new double[] { xx, maxDiv.Height - yy };
                doubleCoords.Add(doubleCoord);
            }
            forceBCNodeNumbers = forceBCNodeNumberDic.Keys.ToArray();

            return true;
        }
        
        /// <summary>
        /// 要素の節点数から要素形状区分と補間次数を取得する
        /// </summary>
        /// <param name="eNodeCnt">要素の節点数</param>
        /// <param name="elemShapeDv">要素形状区分</param>
        /// <param name="order">補間次数</param>
        /// <param name="vertexCnt">頂点数</param>
        public static void GetElementShapeDvAndOrderByElemNodeCnt(int eNodeCnt, out Constants.FemElementShapeDV elemShapeDv, out int order, out int vertexCnt)
        {
            elemShapeDv = Constants.FemElementShapeDV.Triangle;
            order = Constants.SecondOrder;
            vertexCnt = Constants.TriVertexCnt;
            if (eNodeCnt == Constants.TriNodeCnt_SecondOrder)
            {
                // ２次三角形
                elemShapeDv = Constants.FemElementShapeDV.Triangle;
                order = Constants.SecondOrder;
                vertexCnt = Constants.TriVertexCnt;
            }
            else if (eNodeCnt == Constants.QuadNodeCnt_SecondOrder_Type2)
            {
                // ２次四角形
                elemShapeDv = Constants.FemElementShapeDV.QuadType2;
                order = Constants.SecondOrder;
                vertexCnt = Constants.QuadVertexCnt;
            }
            else if (eNodeCnt == Constants.TriNodeCnt_FirstOrder)
            {
                // １次三角形
                elemShapeDv = Constants.FemElementShapeDV.Triangle;
                order = Constants.FirstOrder;
                vertexCnt = Constants.TriVertexCnt;
            }
            else if (eNodeCnt == Constants.QuadNodeCnt_FirstOrder)
            {
                // １次四角形
                elemShapeDv = Constants.FemElementShapeDV.QuadType2;
                order = Constants.FirstOrder;
                vertexCnt = Constants.QuadVertexCnt;
            }
            else
            {
                // 未対応
                System.Diagnostics.Debug.Assert(false);
            }
        }

        /// <summary>
        /// 辺と要素の対応マップ作成
        /// </summary>
        /// <param name="in_Elements"></param>
        /// <param name="out_EdgeToElementNoH"></param>
        public static void MkEdgeToElementNoH(IList<FemElement> in_Elements, ref Dictionary<string, IList<int>> out_EdgeToElementNoH)
        {
            if (in_Elements.Count == 0)
            {
                return;
            }
            int eNodeCnt = in_Elements[0].NodeNumbers.Length;
            Constants.FemElementShapeDV elemShapeDv;
            int order;
            int vertexCnt;
            GetElementShapeDvAndOrderByElemNodeCnt(eNodeCnt, out elemShapeDv, out order, out vertexCnt);

            if (elemShapeDv == Constants.FemElementShapeDV.Triangle && order == Constants.SecondOrder)
            {
                // ２次三角形要素
                TriSecondOrderElements_MkEdgeToElementNoH(in_Elements, ref out_EdgeToElementNoH);
            }
            else if (elemShapeDv == Constants.FemElementShapeDV.QuadType2 && order == Constants.SecondOrder)
            {
                // ２次四角形要素（セレンディピティ族)
                QuadSecondOrderElements_MkEdgeToElementNoH(in_Elements, ref out_EdgeToElementNoH);
            }
            else if (elemShapeDv == Constants.FemElementShapeDV.Triangle && order == Constants.FirstOrder)
            {
                // １次三角形要素
                TriFirstOrderElements_MkEdgeToElementNoH(in_Elements, ref out_EdgeToElementNoH);
            }
            else if (elemShapeDv == Constants.FemElementShapeDV.QuadType2 && order == Constants.FirstOrder)
            {
                // 1次四角形要素（セレンディピティ族)
                QuadFirstOrderElements_MkEdgeToElementNoH(in_Elements, ref out_EdgeToElementNoH);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }

        /// <summary>
        /// ２次三角形要素：辺と要素の対応マップ作成
        /// </summary>
        /// <param name="in_Elements"></param>
        /// <param name="out_EdgeToElementNoH"></param>
        public static void TriSecondOrderElements_MkEdgeToElementNoH(IList<FemElement> in_Elements, ref Dictionary<string, IList<int>> out_EdgeToElementNoH)
        {
            // 辺と要素の対応マップ作成
            // 2次三角形要素の辺の始点、終点ローカル番号
            int[][] edgeStEdNoList = new int[Constants.TriEdgeCnt_SecondOrder][]
                        {
                            new int[]{1, 4},
                            new int[]{4, 2},
                            new int[]{2, 5},
                            new int[]{5, 3},
                            new int[]{3, 6},
                            new int[]{6, 1}
                        };
            out_EdgeToElementNoH.Clear();
            foreach (FemElement element in in_Elements)
            {
                foreach (int[] edgeStEdNo in edgeStEdNoList)
                {
                    int stNodeNumber = element.NodeNumbers[edgeStEdNo[0] - 1];
                    int edNodeNumber = element.NodeNumbers[edgeStEdNo[1] - 1];
                    string edgeKey = "";
                    if (stNodeNumber < edNodeNumber)
                    {
                        edgeKey = string.Format("{0}_{1}", stNodeNumber, edNodeNumber);
                    }
                    else
                    {
                        edgeKey = string.Format("{0}_{1}", edNodeNumber, stNodeNumber);
                    }
                    if (out_EdgeToElementNoH.ContainsKey(edgeKey))
                    {
                        out_EdgeToElementNoH[edgeKey].Add(element.No);
                    }
                    else
                    {
                        out_EdgeToElementNoH[edgeKey] = new List<int>();
                        out_EdgeToElementNoH[edgeKey].Add(element.No);
                    }
                }
            }
        }

        /// <summary>
        /// ２次四角形要素：辺と要素の対応マップ作成
        /// </summary>
        /// <param name="in_Elements"></param>
        /// <param name="out_EdgeToElementNoH"></param>
        public static void QuadSecondOrderElements_MkEdgeToElementNoH(IList<FemElement> in_Elements, ref Dictionary<string, IList<int>> out_EdgeToElementNoH)
        {
            // 辺と要素の対応マップ作成
            // ２次四角形要素（セレンディピティ族）
            //
            //    4+  7  +3      x
            //    |       |
            //    8       6
            //    |       |
            //    1+  5  +2
            //    
            //    y
            // 2次四角形要素の辺の始点、終点ローカル番号
            int[][] edgeStEdNoList = new int[Constants.QuadEdgeCnt_SecondOrder][]
                        {
                            new int[]{1, 5},
                            new int[]{5, 2},
                            new int[]{2, 6},
                            new int[]{6, 3},
                            new int[]{3, 7},
                            new int[]{7, 4},
                            new int[]{4, 8},
                            new int[]{8, 1},
                        };
            out_EdgeToElementNoH.Clear();
            foreach (FemElement element in in_Elements)
            {
                foreach (int[] edgeStEdNo in edgeStEdNoList)
                {
                    int stNodeNumber = element.NodeNumbers[edgeStEdNo[0] - 1];
                    int edNodeNumber = element.NodeNumbers[edgeStEdNo[1] - 1];
                    string edgeKey = "";
                    if (stNodeNumber < edNodeNumber)
                    {
                        edgeKey = string.Format("{0}_{1}", stNodeNumber, edNodeNumber);
                    }
                    else
                    {
                        edgeKey = string.Format("{0}_{1}", edNodeNumber, stNodeNumber);
                    }
                    if (out_EdgeToElementNoH.ContainsKey(edgeKey))
                    {
                        out_EdgeToElementNoH[edgeKey].Add(element.No);
                    }
                    else
                    {
                        out_EdgeToElementNoH[edgeKey] = new List<int>();
                        out_EdgeToElementNoH[edgeKey].Add(element.No);
                    }
                }
            }
        }

        /// <summary>
        /// １次三角形要素：辺と要素の対応マップ作成
        /// </summary>
        /// <param name="in_Elements"></param>
        /// <param name="out_EdgeToElementNoH"></param>
        public static void TriFirstOrderElements_MkEdgeToElementNoH(IList<FemElement> in_Elements, ref Dictionary<string, IList<int>> out_EdgeToElementNoH)
        {
            // 辺と要素の対応マップ作成
            // 1次三角形要素の辺の始点、終点ローカル番号
            int[][] edgeStEdNoList = new int[Constants.TriEdgeCnt_FirstOrder][]
                        {
                            new int[]{1, 2},
                            new int[]{2, 3},
                            new int[]{3, 1},
                        };
            out_EdgeToElementNoH.Clear();
            foreach (FemElement element in in_Elements)
            {
                foreach (int[] edgeStEdNo in edgeStEdNoList)
                {
                    int stNodeNumber = element.NodeNumbers[edgeStEdNo[0] - 1];
                    int edNodeNumber = element.NodeNumbers[edgeStEdNo[1] - 1];
                    string edgeKey = "";
                    if (stNodeNumber < edNodeNumber)
                    {
                        edgeKey = string.Format("{0}_{1}", stNodeNumber, edNodeNumber);
                    }
                    else
                    {
                        edgeKey = string.Format("{0}_{1}", edNodeNumber, stNodeNumber);
                    }
                    if (out_EdgeToElementNoH.ContainsKey(edgeKey))
                    {
                        out_EdgeToElementNoH[edgeKey].Add(element.No);
                    }
                    else
                    {
                        out_EdgeToElementNoH[edgeKey] = new List<int>();
                        out_EdgeToElementNoH[edgeKey].Add(element.No);
                    }
                }
            }
        }

        /// <summary>
        /// １次四角形要素：辺と要素の対応マップ作成
        /// </summary>
        /// <param name="in_Elements"></param>
        /// <param name="out_EdgeToElementNoH"></param>
        public static void QuadFirstOrderElements_MkEdgeToElementNoH(IList<FemElement> in_Elements, ref Dictionary<string, IList<int>> out_EdgeToElementNoH)
        {
            // 辺と要素の対応マップ作成
            // １次四角形要素
            //
            //    4+     +3      x
            //     |     |
            //     |     |
            //    1+     +2
            //    
            //    y
            // １次四角形要素の辺の始点、終点ローカル番号
            int[][] edgeStEdNoList = new int[Constants.QuadEdgeCnt_FirstOrder][]
                        {
                            new int[]{1, 2},
                            new int[]{2, 3},
                            new int[]{3, 4},
                            new int[]{4, 1},
                        };
            out_EdgeToElementNoH.Clear();
            foreach (FemElement element in in_Elements)
            {
                foreach (int[] edgeStEdNo in edgeStEdNoList)
                {
                    int stNodeNumber = element.NodeNumbers[edgeStEdNo[0] - 1];
                    int edNodeNumber = element.NodeNumbers[edgeStEdNo[1] - 1];
                    string edgeKey = "";
                    if (stNodeNumber < edNodeNumber)
                    {
                        edgeKey = string.Format("{0}_{1}", stNodeNumber, edNodeNumber);
                    }
                    else
                    {
                        edgeKey = string.Format("{0}_{1}", edNodeNumber, stNodeNumber);
                    }
                    if (out_EdgeToElementNoH.ContainsKey(edgeKey))
                    {
                        out_EdgeToElementNoH[edgeKey].Add(element.No);
                    }
                    else
                    {
                        out_EdgeToElementNoH[edgeKey] = new List<int>();
                        out_EdgeToElementNoH[edgeKey].Add(element.No);
                    }
                }
            }
        }


        /// <summary>
        /// 節点と要素番号のマップ作成
        /// </summary>
        /// <param name="in_Elements"></param>
        /// <param name="out_NodeToElementNoH"></param>
        public static void MkNodeToElementNoH(IList<FemElement> in_Elements, ref Dictionary<int, IList<int>> out_NodeToElementNoH)
        {
            //check
            for (int ieleno = 0; ieleno < in_Elements.Count; ieleno++)
            {
                System.Diagnostics.Debug.Assert(in_Elements[ieleno].No == ieleno + 1);
            }
            // 節点と要素番号のマップ作成
            foreach (FemElement element in in_Elements)
            {
                foreach (int nodeNumber in element.NodeNumbers)
                {
                    if (out_NodeToElementNoH.ContainsKey(nodeNumber))
                    {
                        out_NodeToElementNoH[nodeNumber].Add(element.No);
                    }
                    else
                    {
                        out_NodeToElementNoH[nodeNumber] = new List<int>();
                        out_NodeToElementNoH[nodeNumber].Add(element.No);
                    }
                }
            }
        }

        /// <summary>
        /// 点が要素内に含まれる？
        /// </summary>
        /// <param name="element">要素</param>
        /// <param name="test_pp">テストする点</param>
        /// <param name="Nodes">節点リスト（要素は座標を持たないので節点リストが必要）</param>
        /// <returns></returns>
        public static bool IsPointInElement(FemElement element, double[] test_pp, IList<FemNode> nodes)
        {
            bool hit = false;
            int eNodeCnt = element.NodeNumbers.Length;
            Constants.FemElementShapeDV elemShapeDv;
            int order;
            int vertexCnt;
            GetElementShapeDvAndOrderByElemNodeCnt(eNodeCnt, out elemShapeDv, out order, out vertexCnt);

            if (vertexCnt == Constants.TriVertexCnt)
            {
                // ２次/１次三角形要素
                hit = TriElement_IsPointInElement(element, test_pp, nodes);
            }
            else if (vertexCnt == Constants.QuadVertexCnt)
            {
                // ２次/１次四角形要素
                hit = QuadElement_IsPointInElement(element, test_pp, nodes);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
            return hit;
        }

        /// <summary>
        /// 三角形要素：点が要素内に含まれる？
        /// </summary>
        /// <param name="element">要素</param>
        /// <param name="test_pp">テストする点</param>
        /// <param name="Nodes">節点リスト（要素は座標を持たないので節点リストが必要）</param>
        /// <returns></returns>
        public static bool TriElement_IsPointInElement(FemElement element, double[] test_pp, IList<FemNode> nodes)
        {
            bool hit = false;

            // 三角形の頂点数
            const int vertexCnt = Constants.TriVertexCnt;
            double[][] pps = new double[vertexCnt][];
            // 2次三角形要素の最初の３点＝頂点の座標を取得
            for (int ino = 0; ino < vertexCnt; ino++)
            {
                int nodeNumber = element.NodeNumbers[ino];
                FemNode node = nodes[nodeNumber - 1];
                System.Diagnostics.Debug.Assert(node.No == nodeNumber);
                pps[ino] = node.Coord;
            }
            // 頂点？
            foreach (double[] pp in pps)
            {
                if (Math.Abs(pp[0] - test_pp[0]) < Constants.PrecisionLowerLimit && Math.Abs(pp[1] - test_pp[1]) < Constants.PrecisionLowerLimit)
                {
                    hit = true;
                    break;
                }
            }
            if (!hit)
            {
                // 面積から内部判定する
                double area = KerEMatTri.TriArea(pps[0], pps[1], pps[2]);
                double sumOfSubArea = 0.0;
                for (int ino = 0; ino < vertexCnt; ino++)
                {
                    double[][] subArea_pp = new double[vertexCnt][];
                    subArea_pp[0] = pps[ino];
                    subArea_pp[1] = pps[(ino + 1) % vertexCnt];
                    subArea_pp[2] = test_pp;
                    //foreach (double[] work_pp in subArea_pp)
                    //{
                    //    Console.Write("{0},{1}  ", work_pp[0], work_pp[1]);
                    //}
                    double subArea = KerEMatTri.TriArea(subArea_pp[0], subArea_pp[1], subArea_pp[2]);
                    //Console.Write("  subArea = {0}", subArea);
                    //Console.WriteLine();
                    //BUGFIX
                    //if (subArea <= 0.0)
                    // 丁度辺上の場合は、サブエリアの１つが０になるのでこれは許可しないといけない
                    if (subArea < -1.0 * Constants.PrecisionLowerLimit)  // 0未満
                    {
                        sumOfSubArea = 0.0;
                        break;
                        // 外側？
                    }
                    sumOfSubArea += Math.Abs(subArea);
                }
                if (Math.Abs(area - sumOfSubArea) < Constants.PrecisionLowerLimit)
                {
                    hit = true;
                }
            }
            return hit;
        }

        /// <summary>
        /// 四角形要素：点が要素内に含まれる？
        /// </summary>
        /// <param name="element">要素</param>
        /// <param name="test_pp">テストする点</param>
        /// <param name="Nodes">節点リスト（要素は座標を持たないので節点リストが必要）</param>
        /// <returns></returns>
        public static bool QuadElement_IsPointInElement(FemElement element, double[] test_pp, IList<FemNode> nodes)
        {
            bool hit = false;

            // 四角形の頂点数
            const int vertexCnt = Constants.QuadVertexCnt;
            double[][] pps = new double[vertexCnt][];
            int[] nodeNumbers = new int[vertexCnt];
            // 2次四角形要素の最初の４点＝頂点の座標を取得
            for (int ino = 0; ino < vertexCnt; ino++)
            {
                int nodeNumber = element.NodeNumbers[ino];
                FemNode node = nodes[nodeNumber - 1];
                System.Diagnostics.Debug.Assert(node.No == nodeNumber);
                pps[ino] = node.Coord;
                nodeNumbers[ino] = nodeNumber;
            }

            // ２つの三角形に分ける
            //        s
            //        |
            //    3+  +  +2
            //    |   |   |
            // ---|---+---|-->r
            //    |   |   |
            //    0+  +  +1
            //        |
            FemElement[] tris = new FemElement[2];
            tris[0] = new FemElement();
            tris[0].NodeNumbers = new int[] { nodeNumbers[0], nodeNumbers[1], nodeNumbers[3], 0, 0, 0 };
            tris[1] = new FemElement();
            tris[1].NodeNumbers = new int[] { nodeNumbers[2], nodeNumbers[3], nodeNumbers[1], 0, 0, 0 };
            foreach (FemElement tri in tris)
            {
                bool hitInsideTri = TriElement_IsPointInElement(tri, test_pp, nodes);
                if (hitInsideTri)
                {
                    hit = true;
                }
            }
            return hit;
        }

        /// <summary>
        /// 2点間距離の計算
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p0"></param>
        /// <returns></returns>
        public static double GetDistance(double[] p, double[] p0)
        {
            return Math.Sqrt((p[0] - p0[0]) * (p[0] - p0[0]) + (p[1] - p0[1]) * (p[1] - p0[1]));
        }

        /// <summary>
        /// 要素の節点数から該当するFemElementインスタンスを作成する
        /// </summary>
        /// <param name="eNodeCnt"></param>
        /// <returns></returns>
        public static FemElement CreateFemElementByElementNodeCnt(int eNodeCnt)
        {
            FemElement femElement = null;
            Constants.FemElementShapeDV elemShapeDv;
            int order;
            int vertexCnt;
            GetElementShapeDvAndOrderByElemNodeCnt(eNodeCnt, out elemShapeDv, out order, out vertexCnt);

            if (vertexCnt == Constants.TriVertexCnt)
            {
                femElement = new FemTriElement();
            }
            else if (vertexCnt == Constants.QuadVertexCnt)
            {
                femElement = new FemQuadElement();
            }
            else
            {
                femElement = new FemElement();
            }
            return femElement;
        }


    }
}
