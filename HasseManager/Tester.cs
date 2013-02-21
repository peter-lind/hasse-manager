/*
HasseManager - a program for construction and mining of Hasse diagrams.
Copyright (C) 2012  Peter Lind

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.ggasoftware.indigo;

namespace HasseManager
{
    class Tester
    {
        public void test3()
        {
            Type type = Type.GetType("StringHasseNode");
            HasseDiagram HDM = new HasseDiagram(HasseNodeFactory.NodeType.STRING);
            System.Random rnd = new System.Random(1);
            HasseNodeCollection Elements1 = new HasseNodeCollection();
            HasseNodeCollection elements = new HasseNodeCollection();

            //StringHasseNode Node1 = new StringHasseNode("A*", HasseNode.ElementTypes.REAL, Elements1);
            //StringHasseNode Node2 = new StringHasseNode("*A*", HasseNode.ElementTypes.REAL, Elements1);

            //StringHasseNode Node1 = new StringHasseNode("*X", HasseNode.ElementTypes.REAL, Elements1);
            //StringHasseNode Node2 = new StringHasseNode("*X*", HasseNode.ElementTypes.REAL, Elements1);

            StringHasseNode Node1 = new StringHasseNode("CA*", HasseNode.HasseNodeTypes.REAL, Elements1);
            StringHasseNode Node2 = new StringHasseNode("DCCCA*", HasseNode.HasseNodeTypes.REAL, Elements1);


            bool test = Node2.IsLargerThan(Node1);
            System.Diagnostics.Debug.Assert(test);
            //Debug.Assert(Node3.IsLargerThan(Node2))
            //Debug.Assert(Not Node1.IsIdenticalTo(Node2))

            //Dim Node4 As New clsCharString("XABX", HasseElement.EnumType.REAL, Elements1)
            //Dim Node5 As New clsCharString("YABY", HasseElement.EnumType.REAL, Elements1)
            System.Collections.Queue q = new System.Collections.Queue();
            //Node1.makeMaxCommonSubStruc(Node4, Node5, False, q)
            //Dim mcs As HasseElement = CType(q.Dequeue, HasseElement)
            //Debug.Assert(mcs.UniqueString.Equals("AB"))

            for (int i = 1; i <= 1000; i++)
            {
                string buf = "";
                int strLen = Convert.ToInt32(rnd.Next(1, 10));
                //8
                //random string length
                for (int j = 1; j <= strLen; j++)
                {
                    float r = rnd.Next(0, 4);
                    //random character choice '4
                    switch ((Int32)Math.Truncate(r))
                    {
                        case 0:
                            buf += "A";
                            break;
                        case 1:
                            buf += "B";
                            break;
                        case 2:
                            buf += "C";
                            break;
                        case 3:
                            buf += "D";
                            break;
                        default:
                            throw new Exception("error in Test3");
                    }
                }
                HDM.AddNode(buf);
            }
            //HDM.BFGTopOrder.DebugReport()
            //TopologicalSort.topsort(HDM.AllNodes,true);
            //HDM.draw();
            System.Diagnostics.Debug.WriteLine(HasseDiagramStats.Report(HDM.HasseDiagramNodes));

        }

        public static void test4(string filename)
        {
            HasseDiagram HDM = new HasseDiagram(HasseNodeFactory.NodeType.STRING);
            HasseNodeCollection elements = new HasseNodeCollection();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            //        Dim fs As System.IO.FileStream = New IO.FileStream("C:\users\petlin\tjeckiska.txt", IO.FileMode.Open, IO.FileAccess.Read)
            //System.IO.FileStream fs = new System.IO.FileStream("C:\\users\\petlin\\svenska.txt", System.IO.FileMode.Open, System.IO.FileAccess.Read);
            // Dim fs As System.IO.FileStream = New IO.FileStream("C:\users\petlin\sverige.txt", IO.FileMode.Open, IO.FileAccess.Read)
            //System.IO.FileStream fs = new System.IO.FileStream("C:\\users\\petlin\\ryska.txt", System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            System.IO.TextReader rdr = new System.IO.StreamReader(fs);
            int maxCount = 1000;
            int count = 0;

            List<string> Words = new List<string>();


            do
            {
                if (count >= maxCount)
                    break;

                char[] delim = { ' ', ',', '.' };
                string line = rdr.ReadLine();
                if (line == null)
                    break;
                string[] words = line.Split(delim);
                for (int i = 0; i < words.Length; i++)
                {
                    count += 1;
                    string w = words[i];
                    w = w.Trim();
                    w = w.ToLower();
                    w = w.Replace("(", "");
                    w = w.Replace(")", "");
                    w = w.Replace("]", "");
                    w = w.Replace("[", "");
                    w = w.Replace("„", "");
                    w = w.Replace(":", "");
                    w = w.Replace("\"", "");
                    if (!string.IsNullOrEmpty(w))
                    {
                        Words.Add(w); // Add to list for insertion below
                    }
                    if (count >= maxCount)
                        break;

                }

            } while (true);

            Words.Sort();

            foreach (string Word in Words)
            {
                HDM.AddNode(Word);
            }

            fs.Close();
            sw.Stop();

            System.Diagnostics.Debug.WriteLine("Total time: " + ((double)sw.ElapsedMilliseconds / 1000).ToString() + " seconds");
            System.Diagnostics.Debug.WriteLine("Total time: " + ((double)sw.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency).ToString() + " seconds");

            HDM.ContractChains2();


            HDM.HasseDiagramNodes.Sort();
            foreach (HasseNode n in HDM.HasseDiagramNodes.Values)
            {
                //     System.Diagnostics.Debug.WriteLine (n.KeyString);  
            }
            DotFileWriter DW = new DotFileWriter(HDM.HasseDiagramNodes);
            DW.WriteDotFile();
            System.Diagnostics.Debug.WriteLine(HDM.RootNode.HashString());
            System.Diagnostics.Debug.WriteLine(HDM.HasseDiagramNodes.Count.ToString() + " Diagram objects");
            System.Diagnostics.Debug.WriteLine(HDM.DifferenceNodes.Count.ToString() + " Difference objects");
            //System.Diagnostics.Debug.WriteLine(HasseDiagramStats.Report(HDM.HasseDiagramNodes)); 

        }
        public void test5()
        {
            HasseDiagram HDM = new HasseDiagram(HasseNodeFactory.NodeType.STRING);
            System.Random rnd = new System.Random(1);
            HasseNodeCollection elements = new HasseNodeCollection();

            for (int i = 1; i <= 500; i++)
            {
                string buf = "";
                int strLen = Convert.ToInt32(rnd.Next(1, 500));
                //8
                //random string length
                for (int j = 1; j <= strLen; j++)
                {
                    buf += "A";
                }
                HDM.AddNode(buf);
            }
            //HDM.BFGTopOrder.DebugReport()
            //TopologicalSort.topsort(HDM.AllHasseNodes ,true);
            HDM.Draw();

        }
        public void test6()
        {
            HasseDiagram HDM = new HasseDiagram(HasseNodeFactory.NodeType.STRING);
            HasseNodeCollection elements = new HasseNodeCollection();

            HDM.AddNode("XA0");
            HDM.AddNode("XA1");
            HDM.AddNode("XB0");
            HDM.AddNode("XB1");

            HDM.AddNode("YA0");
            HDM.AddNode("YA1");
            HDM.AddNode("YB0");
            HDM.AddNode("YB1");
            // HDM.DeleteNodesWithOneCover();
            DotFileWriter DW = new DotFileWriter(HDM.HasseDiagramNodes);
            DW.WriteDotFile();
            //TopologicalSort.topsort(HDM.AllHasseNodes, true);
            HDM.Draw();
        }

        public static void chemtest()
        {
            Indigo indigo = new Indigo();

            //HasseNodeCollection allNodes = new HasseNodeCollection();

            //HasseFragmentInsertionQueue q = new HasseFragmentInsertionQueue(allNodes);

            /*
            ChemHasseNode L_ALA = new ChemHasseNode("N[C@@H](C)C(=O)O", HasseNode.HasseNodeTypes.REAL, allNodes);
           // ChemHasseNode D_ALA = new ChemHasseNode("N[C@H](C)C(=O)O", HasseNode.HasseNodeTypes.REAL, allNodes);
            ChemHasseNode D_ALA = new ChemHasseNode("NCC(=O)O", HasseNode.HasseNodeTypes.REAL, allNodes);


             * bool t;
            t=L_ALA.IsLargerThan(D_ALA);
            t=D_ALA.IsLargerThan(L_ALA);
            L_ALA.GetMaxCommonFragments(L_ALA, D_ALA,false,q,0);
            string[] d;
            d=L_ALA.GetDifferenceString (D_ALA);
            d = D_ALA.GetDifferenceString(L_ALA);

*/

            /*
            ChemHasseNode X0 = new ChemHasseNode("N", HasseNode.HasseNodeTypes.REAL, allNodes);
            ChemHasseNode X1 = new ChemHasseNode("NCOCCCS", HasseNode.HasseNodeTypes.REAL, allNodes);
            ChemHasseNode X2 = new ChemHasseNode("NCCOCCS", HasseNode.HasseNodeTypes.REAL, allNodes);
            X0.GetMaxCommonFragments(X1, X2, false, q, 1);
            */

            HasseDiagram HDM = new HasseDiagram(HasseNodeFactory.NodeType.CHEM );
            HasseNodeCollection elements = new HasseNodeCollection();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();


            foreach (IndigoObject item in indigo.iterateSDFile(@"C:\temp\benzodiazepines.sdf"))
            {
                //if (!allNodes.ContainsKey(item.canonicalSmiles()))
                //allNodes.Add(item.canonicalSmiles(), new ChemHasseNode(item.molfile(), HasseNode.HasseNodeTypes.REAL, elements));
                HDM.AddNode(item.molfile());
            }



            /*
            foreach (ChemHasseNode n1 in allNodes.Values)
            {
                foreach (ChemHasseNode n2 in allNodes.Values)
                {
                    System.Diagnostics.Debug.WriteLine(n1.IsLargerThan(n2));
                    System.Diagnostics.Debug.WriteLine(n2.IsLargerThan(n1));
                    System.Diagnostics.Debug.WriteLine("1)\t" + n1.KeyString);
                    System.Diagnostics.Debug.WriteLine("2)\t" + n2.KeyString);
                    n1.GetMaxCommonFragments(n1, n2, false, q, 1);
                    //string[]s=n1.GetDifferenceString(n2);
                    //foreach (string str in s)
                    // System.Diagnostics.Debug.WriteLine(str);

                    //s = n2.GetDifferenceString(n1);
                    //foreach (string str in s)
                    //  System.Diagnostics.Debug.WriteLine(str);

                }
            }
              */
            DotFileWriter DW = new DotFileWriter(HDM.HasseDiagramNodes);
            DW.WriteDotFile();

            sw.Stop();
            System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds.ToString());

        }

    }
}
