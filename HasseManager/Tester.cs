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
    class StringHashComparer : IComparer<string>
    {
        int IComparer<string>.Compare(string x, string y)
        {
            if (x.GetHashCode()  < y.GetHashCode()) return -1; else return 1;
        }
    }


    class Tester
    {
        public void test3()
        {
//            Type type = Type.GetType("StringHasseNode");
            HasseDiagram HDM = new HasseDiagram(HasseNodeFactory.NodeType.STRING);
            System.Random rnd = new System.Random(1);
            //HasseNodeCollection Elements1 = new HasseNodeCollection();
            //HasseNodeCollection elements = new HasseNodeCollection();


            StringHasseNode Node1 = new StringHasseNode("CA*", HasseNode.HasseNodeTypes.REAL, "");
            StringHasseNode Node2 = new StringHasseNode("DCCCA*", HasseNode.HasseNodeTypes.REAL, "");


            bool test = Node2.IsLargerThan(Node1);
            System.Diagnostics.Debug.Assert(test);

            System.Collections.Queue q = new System.Collections.Queue();

            for (int i = 1; i <= 1000; i++)
            {
                string buf = "";
                int strLen = Convert.ToInt32(rnd.Next(1, 10));
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

            System.Diagnostics.Debug.WriteLine(HasseDiagramStats.Report(HDM.HasseDiagramNodes,HDM.RootNode ));

        }

        public static void test4(string filename)
        {
            HasseDiagram HDM = new HasseDiagram(HasseNodeFactory.NodeType.STRING);
            HasseNodeCollection elements = new HasseNodeCollection();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            System.IO.TextReader rdr = new System.IO.StreamReader(fs);
            int maxCount = 2000;
            int count = 0;

            List<string> Words = new List<string>();


            do
            {
                //if (count >= maxCount)   break;

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
                    w = w.Replace("*", "");
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

                }

            } while (true);

            // 'randomise' order
           StringHashComparer shc = new StringHashComparer();
           Words.Sort(shc);

            //65533
           count = 0;
            foreach (string Word in Words)
            {
                count += 1;

                System.Diagnostics.Debug.WriteLine(Word);  
                foreach (char c in Word.ToCharArray())
                {
                //    System.Diagnostics.Debug.WriteLine(((int)c).ToString ());
                }

                HDM.AddNode(Word);
                if (count >= maxCount)
                    break;
            }

            fs.Close();
            sw.Stop();

            System.Diagnostics.Debug.WriteLine("Total time: " + ((double)sw.ElapsedMilliseconds / 1000).ToString() + " seconds");
            System.Diagnostics.Debug.WriteLine("Total time: " + ((double)sw.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency).ToString() + " seconds");

            HDM.ContractChains2();

            HDM.HasseDiagramNodes.Sort();
            foreach (HasseNode n in HDM.HasseDiagramNodes.Values)
            {
                    System.Diagnostics.Debug.WriteLine (n.KeyString);  
            }

            DotFileWriter DW = new DotFileWriter(HDM.HasseDiagramNodes, "C:\\temp\\testdotfile.dot");
            DW.LabelMode = labelMode.USE_NODE_KEY;
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
            DotFileWriter DW = new DotFileWriter(HDM.HasseDiagramNodes, "C:\\temp\\testdotfile.dot");
            DW.WriteDotFile();
            //TopologicalSort.topsort(HDM.AllHasseNodes, true);
            HDM.Draw();
        }

        public static void chemtest()
        {

            //IndigoTests T = new IndigoTests();
            //T.test1();
            
            
            Indigo indigo = new Indigo();

            /*
            ChemHasseNode L_ALA = new ChemHasseNode("N[C@@H](C)C(=O)O", HasseNode.HasseNodeTypes.REAL, allNodes);
           ChemHasseNode D_ALA = new ChemHasseNode("N[C@H](C)C(=O)O", HasseNode.HasseNodeTypes.REAL, allNodes);
            d=L_ALA.GetDifferenceString (D_ALA);
            d = D_ALA.GetDifferenceString(L_ALA);
*/
//            HasseDiagram HDM = new HasseDiagram(HasseNodeFactory.NodeType.CHEM );
            HasseDiagram HDM = new HasseDiagram(HasseNodeFactory.NodeType.FINGERPRINTCHEM );
            HasseNodeCollection elements = new HasseNodeCollection();

            //ChemHasseNode A = (ChemHasseNode)HDM.AddNode("c1ccccccc[nH]1"); // azonine
            //ChemHasseNode B = (ChemHasseNode)HDM.AddNode("C12=C(C=NC=C1)NC1=C2C=CC=C1");  // pyrido indol
            //ChemHasseNode C = (ChemHasseNode)HDM.AddNode("c1cccc2[nH]ccc21"); // indol

            //FingerprintChemHasseNode A = (FingerprintChemHasseNode)HDM.AddNode("C"); // 
            //FingerprintChemHasseNode B = (FingerprintChemHasseNode)HDM.AddNode("CC");  // 
            //FingerprintChemHasseNode C = (FingerprintChemHasseNode)HDM.AddNode("CN"); // 


           // ChemHasseNode A = (ChemHasseNode)HDM.AddNode(@"[NH3+]C");
            //ChemHasseNode B = (ChemHasseNode)HDM.AddNode(@"[NH2]CC");
                
            //bool tst = A.IsLargerThan(B);
            //tst = B.IsLargerThan(A);
            //A.GetMaxCommonFragments(A, B);
            //tst = A.IsLargerThan(C);
            //tst = C.IsLargerThan(A);

            // pyridoindol larger than azonine

            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.BelowNormal;     
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            int count = 0;
            //foreach (IndigoObject item in indigo.iterateSDFile(@"C:\temp\benzodiazepines.sdf"))
            //foreach (IndigoObject item in indigo.iterateSDFile(@"C:\temp\monoamtrain_x.sdf"))
            //foreach (IndigoObject item in indigo.iterateSDFile(@"C:\HassePub\Datasets\Jorissen\compounds_1st.sdf"))
        //foreach (IndigoObject item in indigo.iterateSDFile(@"C:\HassePub\Datasets\benzodiazepines_v4.sdf"))
            //    foreach (IndigoObject item in indigo.iterateSDFile(@"C:\HassePub\Datasets\small_set.sdf"))
            // foreach (IndigoObject item in indigo.iterateSDFile(@"C:\HassePub\Datasets\chembl_pyridines.sdf"))
            //foreach (IndigoObject item in indigo.iterateSDFile(@"C:\HassePub\Datasets\ChEBI_anilines_480-500.sdf"))
            //foreach (IndigoObject item in indigo.iterateSDFile(@"C:\HassePub\Datasets\ChEBI_valines.sdf"))
            foreach (IndigoObject item in indigo.iterateSDFile(@"C:\HassePub\Datasets\ChEBI_valines_v21.sdf")) // in pub
            {
              
               if (item.isChiral() == false) continue;
                count++;
                    //System.Diagnostics.Debugger.Break();   
                if (count >24) break; //24 for valines in pub

       //         HasseNode N = HDM.CreateNewNode(item);
                //if (N.IsValid())
                //{
                 System.Diagnostics.Stopwatch sw2 = new System.Diagnostics.Stopwatch ();
                 sw2.Start();
                 try
                {
                    HasseNode N = HDM.CreateNewNode(item.molfile());
                    if (item.hasProperty("CHEMBL ID"))
                    {
                        string ChemblID = item.getProperty("CHEMBL ID");
                        N.SetName(ChemblID);
                    } else if (item.hasProperty("ChEBI ID")){
                        string ChebiID = item.getProperty("ChEBI ID");
                        N.SetName(ChebiID);
                    }
                    else
                    {
                        N.SetName(count.ToString());
                    }  

                    HDM.AddNode(N);
                    System.Diagnostics.Debug.WriteLine("---   " + N.KeyString);

                }
                 catch (Exception ex)
                 {
                     System.Diagnostics.Debug.WriteLine ("WARNING: could not add node: " + ex.ToString ()) ;  
                 }
             }

        List<HasseNode> subset = new List<HasseNode>();

            /*
        foreach (HasseNode N in HDM.HasseDiagramNodes.Values)
        {
            if (N.NodeType != HasseNode.HasseNodeTypes.ROOT)
            {
                float weight = N.Weight();
                N.SetName("w=" + weight.ToString());
                float w = 5F * weight / 29F;
                if (w < 1) w = 1F;
                if (w > 3.5) w = 3.5F;
         //       N.CreateImage(w);
                N.CreateImage();
                //if (weight > 20) subset.Add(N);
            }
            
        }
         */
             //DotFileWriter DW = new DotFileWriter(HDM.HasseDiagramNodes, subset, "C:\\temp\\testdotfile.dot");
            DotFileWriter DW = new DotFileWriter(HDM.HasseDiagramNodes, "C:\\temp\\testdotfile.dot");

            int cnt = 0;
            
            
            foreach (HasseNode N in HDM.HasseDiagramNodes.Values)
            {

                if ((N.GetType() == typeof(ChemHasseNode) || N.GetType() == typeof(FingerprintChemHasseNode )) && N.NodeType != HasseNode.HasseNodeTypes.ROOT)
                {
                    cnt++;
                    N.SetName(cnt.ToString());
                   if (N.GetType() == typeof(ChemHasseNode))  ((ChemHasseNode ) N).CreateImage();
                   if (N.GetType() == typeof(FingerprintChemHasseNode)) ((FingerprintChemHasseNode)N).CreateImage();
                }
            }
            
            DW.SetLabelsToNumericSequence();
            DW.SetDrawingColors();
            DW.LabelMode = labelMode.NO_LABELS; // for figure 5
            DW.directionMode = graphDirection.RIGHT   ;

            //DW.LabelMode = labelMode.USE_NODE_LABEL;
            //DW.LabelMode = labelMode.USE_NODE_ID;
            
            DW.UseImage = true; 
            DW.WriteEdgeLabels = false;

            sw.Stop();
            System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds.ToString());

            DW.WriteDotFile();

            System.Diagnostics.Debug.WriteLine("Nodes: " + HDM.HasseDiagramNodes.Count());
            System.Diagnostics.Debug.WriteLine("Diffs: " + HDM.DifferenceNodes.Count());
            System.Diagnostics.Debug.WriteLine("Hash: " + HDM.RootNode.HashString());

            foreach (HasseNode N in HDM.HasseDiagramNodes.Values )
            {
                if (N.HasNodeType(HasseNode.HasseNodeTypes.FRAGMENT))
                    ;
              //  System.Diagnostics.Debug.WriteLine(N.KeyString + " " + N.Weight().ToString() );   
            }
             System.Diagnostics.Debug.WriteLine (     HasseDiagramStats.Report(HDM.HasseDiagramNodes, HDM.RootNode )); 
        }

    }
}
