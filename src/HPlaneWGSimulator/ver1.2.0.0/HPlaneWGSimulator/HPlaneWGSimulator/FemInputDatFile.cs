using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Numerics; // Complex
using System.Text.RegularExpressions;

namespace HPlaneWGSimulator
{
    /// <summary>
    /// Fem入力データファイルの読み書き
    /// </summary>
    class FemInputDatFile
    {
        /// <summary>
        ///  Fem入力データをファイルから読み込み
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="nodes"></param>
        /// <param name="elements"></param>
        /// <param name="ports"></param>
        /// <param name="forceBCNodes"></param>
        /// <param name="incidentPortNo"></param>
        /// <param name="medias"></param>
        /// <returns></returns>
        public static bool LoadFromFile(
            string filename,
            out IList<FemNode> nodes,
            out IList<FemElement> elements,
            out IList<IList<int>> ports,
            out IList<int> forceBCNodes,
            out int incidentPortNo,
            out MediaInfo[] medias
            )
        {
            // 要素内節点数(2次三角形要素)
            const int elementNodeCnt = 6;

            nodes = new List<FemNode>();
            elements = new List<FemElement>();
            ports = new List<IList<int>>();
            forceBCNodes = new List<int>();
            incidentPortNo = 1;
            medias = new MediaInfo[Constants.MaxMediaCount];
            for (int i = 0; i < medias.Length; i++)
            {
                MediaInfo media = new MediaInfo();
                media.BackColor = CadLogic.MediaBackColors[i];
                medias[i] = media;
            }

            if (!File.Exists(filename))
            {
                return false;
            }

            // 入力データ読み込み
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    const char delimiter = ',';
                    string line;
                    string[] tokens;

                    line = sr.ReadLine();
                    tokens = line.Split(delimiter);
                    if (tokens.Length != 2 || tokens[0] != "Nodes")
                    {
                        MessageBox.Show("節点情報がありません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    int nodeCnt = int.Parse(tokens[1]);
                    for (int i = 0; i < nodeCnt; i++)
                    {
                        line = sr.ReadLine();
                        tokens = line.Split(delimiter);
                        if (tokens.Length != 3)
                        {
                            MessageBox.Show("節点情報が不正です", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                        int no = int.Parse(tokens[0]);
                        if (no != i + 1)
                        {
                            MessageBox.Show("節点番号が不正です", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                        FemNode femNode = new FemNode();
                        femNode.No = no;
                        femNode.Coord = new double[2];
                        femNode.Coord[0] = double.Parse(tokens[1]);
                        femNode.Coord[1] = double.Parse(tokens[2]);
                        nodes.Add(femNode);
                    }

                    line = sr.ReadLine();
                    tokens = line.Split(delimiter);
                    if (tokens.Length != 2 || tokens[0] != "Elements")
                    {
                        MessageBox.Show("要素情報がありません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    int elementCnt = int.Parse(tokens[1]);
                    for (int i = 0; i < elementCnt; i++)
                    {
                        line = sr.ReadLine();
                        tokens = line.Split(delimiter);
                        if ((tokens.Length != 1 + elementNodeCnt) && (tokens.Length != 2 + elementNodeCnt))  // ver1.1.0.0で媒質インデックスを番号の後に挿入
                        {
                            MessageBox.Show("要素情報が不正です", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                        FemElement femElement = new FemElement();
                        femElement.No = int.Parse(tokens[0]);
                        int mediaIndex = 0;
                        int indexOffset = 1; // ver1.0.0.0
                        if (tokens.Length == 2 + elementNodeCnt)
                        {
                            // ver1.1.0.0で媒質インデックスを追加
                            mediaIndex = int.Parse(tokens[1]);
                            indexOffset = 2;
                        }
                        femElement.MediaIndex = mediaIndex;
                        femElement.NodeNumbers = new int[elementNodeCnt];
                        for (int n = 0; n < femElement.NodeNumbers.Length; n++)
                        {
                            femElement.NodeNumbers[n] = int.Parse(tokens[n + indexOffset]);
                        }
                        elements.Add(femElement);
                    }

                    line = sr.ReadLine();
                    tokens = line.Split(delimiter);
                    if (tokens.Length != 2 || tokens[0] != "Ports")
                    {
                        MessageBox.Show("入出力ポート情報がありません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    int portCnt = int.Parse(tokens[1]);
                    for (int i = 0; i < portCnt; i++)
                    {
                        line = sr.ReadLine();
                        tokens = line.Split(delimiter);
                        if (tokens.Length != 2)
                        {
                            MessageBox.Show("入出力ポート情報が不正です", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                        int portNo = int.Parse(tokens[0]);
                        int portNodeCnt = int.Parse(tokens[1]);
                        if (portNo != i + 1)
                        {
                            MessageBox.Show("ポート番号が不正です", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                        IList<int> portNodes = new List<int>();
                        for (int n = 0; n < portNodeCnt; n++)
                        {
                            line = sr.ReadLine();
                            tokens = line.Split(delimiter);
                            if (tokens.Length != 2)
                            {
                                MessageBox.Show("ポートの節点情報が不正です", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }
                            int portNodeNumber = int.Parse(tokens[0]);
                            int nodeNumber = int.Parse(tokens[1]);
                            if (portNodeNumber != n + 1)
                            {
                                MessageBox.Show("ポートの節点番号が不正です", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }
                            portNodes.Add(nodeNumber);
                        }
                        ports.Add(portNodes);
                    }

                    line = sr.ReadLine();
                    tokens = line.Split(delimiter);
                    if (tokens.Length != 2 || tokens[0] != "Force")
                    {
                        MessageBox.Show("強制境界情報がありません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    int forceNodeCnt = int.Parse(tokens[1]);
                    for (int i = 0; i < forceNodeCnt; i++)
                    {
                        line = sr.ReadLine();
                        int nodeNumber = int.Parse(line);
                        forceBCNodes.Add(nodeNumber);
                    }

                    line = sr.ReadLine();
                    tokens = line.Split(delimiter);
                    if (tokens.Length != 2 || tokens[0] != "IncidentPortNo")
                    {
                        MessageBox.Show("入射ポート番号がありません", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    incidentPortNo = int.Parse(tokens[1]);

                    //////////////////////////////////////////
                    //// Ver1.1.0.0からの追加情報
                    //////////////////////////////////////////
                    line = sr.ReadLine();
                    if (line == null || line.Length == 0)
                    {
                        // 媒質情報なし
                        // ver1.0.0.0
                    }
                    else
                    {
                        // 媒質情報？
                        // ver1.1.0.0
                        tokens = line.Split(delimiter);
                        if (tokens[0] != "Medias")
                        {
                            MessageBox.Show("媒質情報がありません");
                            return false;
                        }
                        int cnt = int.Parse(tokens[1]);
                        if (cnt > Constants.MaxMediaCount)
                        {
                            MessageBox.Show("媒質情報の個数が不正です");
                            return false;
                        }
                        for (int i = 0; i < cnt; i++)
                        {
                            line = sr.ReadLine();
                            if (line.Length == 0)
                            {
                                MessageBox.Show("媒質情報が不正です");
                                return false;
                            }
                            tokens = line.Split(delimiter);
                            if (tokens.Length != 1 + 9 + 9)
                            {
                                MessageBox.Show("媒質情報が不正です");
                                return false;
                            }
                            int mediaIndex = int.Parse(tokens[0]);
                            System.Diagnostics.Debug.Assert(mediaIndex == i);

                            double[,] p = new double[3, 3];
                            for (int m = 0; m < p.GetLength(0); m++)
                            {
                                for (int n = 0; n < p.GetLength(1); n++)
                                {
                                    p[m, n] = double.Parse(tokens[1 + m * p.GetLength(1) + n]);
                                }
                            }
                            medias[i].SetP(p);

                            double[,] q = new double[3, 3];
                            for (int m = 0; m < q.GetLength(0); m++)
                            {
                                for (int n = 0; n < q.GetLength(1); n++)
                                {
                                    q[m, n] = double.Parse(tokens[1 + 9 + m * q.GetLength(1) + n]);
                                }
                            }
                            medias[i].SetQ(q);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Fem入力データファイルへ保存
        ///   I/FがCadの内部データ寄りになっているので、変更したいが後回し
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="nodeCnt"></param>
        /// <param name="doubleCoords"></param>
        /// <param name="elementCnt"></param>
        /// <param name="elements"></param>
        /// <param name="portCnt"></param>
        /// <param name="portList"></param>
        /// <param name="forceBCNodeNumbers"></param>
        /// <param name="incidentPortNo"></param>
        /// <param name="medias"></param>
        public static void SaveToFileFromCad
            (string filename,
            int nodeCnt, IList<double[]> doubleCoords,
            int elementCnt, IList<int[]> elements,
            int portCnt, IList<IList<int>> portList,
            int[] forceBCNodeNumbers,
            int incidentPortNo,
            MediaInfo[] medias)
        {
            //////////////////////////////////////////
            // ファイル出力
            //////////////////////////////////////////
            try
            {
                using (StreamWriter sw = new StreamWriter(filename))
                {
                    string line;

                    // 節点番号と座標の出力
                    line = string.Format("Nodes,{0}", nodeCnt);
                    sw.WriteLine(line);
                    for (int i = 0; i < doubleCoords.Count; i++)
                    {
                        double[] doubleCoord = doubleCoords[i];
                        int nodeNumber = i + 1;
                        line = string.Format("{0},{1},{2}", nodeNumber, doubleCoord[0], doubleCoord[1]);
                        sw.WriteLine(line);
                    }
                    // 要素番号と要素を構成する節点の全体節点番号の出力
                    line = string.Format("Elements,{0}", elementCnt);
                    sw.WriteLine(line);
                    foreach (int[] element in elements)
                    {
                        line = "";
                        foreach (int k in element)
                        {
                            line += string.Format("{0},", k);
                        }
                        line = line.Substring(0, line.Length - 1); // 最後の,を削除
                        sw.WriteLine(line);
                    }
                    // ポート境界条件節点
                    int portCounter = 0;
                    line = string.Format("Ports,{0}", portList.Count);
                    sw.WriteLine(line);
                    foreach (IList<int> nodes in portList)
                    {
                        line = string.Format("{0},{1}", ++portCounter, nodes.Count);
                        sw.WriteLine(line);
                        int portNodeNumber = 0;
                        foreach (int nodeNumber in nodes)
                        {
                            line = string.Format("{0},{1}", ++portNodeNumber, nodeNumber);
                            sw.WriteLine(line);
                        }
                    }
                    // 強制境界節点
                    line = string.Format("Force,{0}", forceBCNodeNumbers.Length);
                    sw.WriteLine(line);
                    foreach (int nodeNumber in forceBCNodeNumbers)
                    {
                        line = string.Format("{0}", nodeNumber);
                        sw.WriteLine(line);
                    }
                    // 入射ポート番号
                    line = string.Format("IncidentPortNo,{0}", incidentPortNo);
                    sw.WriteLine(line);
                    //////////////////////////////////////////
                    //// Ver1.1.0.0からの追加情報
                    //////////////////////////////////////////
                    // 媒質情報の個数
                    sw.WriteLine("Medias,{0}", medias.Length);
                    // 媒質情報の書き込み
                    for (int i = 0; i < medias.Length; i++)
                    {
                        MediaInfo media = medias[i];
                        line = string.Format("{0},", i);
                        double[,] p = media.P;
                        for (int m = 0; m < p.GetLength(0); m++)
                        {
                            for (int n = 0; n < p.GetLength(1); n++)
                            {
                                line += string.Format("{0},", p[m, n]);
                            }
                        }
                        double[,] q = media.Q;
                        for (int m = 0; m < q.GetLength(0); m++)
                        {
                            for (int n = 0; n < q.GetLength(1); n++)
                            {
                                line += string.Format("{0},", q[m, n]);
                            }
                        }
                        line = line.Remove(line.Length - 1); // 最後の,を削除
                        sw.WriteLine(line);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }
    }
}
