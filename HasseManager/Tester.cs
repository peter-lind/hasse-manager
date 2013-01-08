﻿/*
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

namespace HasseManager
{
    class Tester
    {
        public void test3()
        {
            HasseDiagram HDM = new HasseDiagram();
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
                    switch ((Int32 )Math.Truncate(r))
                    {
                        case   0:
                            buf += "A";
                            break;
                        case  1:
                            buf += "B";
                            break;
                        case  2:
                            buf += "C";
                            break;
                        case  3:
                            buf += "D";
                            break;
                        default   :
                            throw new Exception ("error in Test3");
                    }
                }
                HDM.InsertElementIntoHasseDiagram(new StringHasseNode (buf, HasseNode.HasseNodeTypes.REAL, elements));
            }
            //HDM.BFGTopOrder.DebugReport()
            //TopologicalSort.topsort(HDM.AllNodes,true);
            //HDM.draw();
            System.Diagnostics.Debug.WriteLine(HasseDiagramStats.Report(HDM.AllHasseNodes));    

        }

        public void test4()
        {
            HasseDiagram HDM = new HasseDiagram();
            HasseNodeCollection elements = new HasseNodeCollection();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            //        Dim fs As System.IO.FileStream = New IO.FileStream("C:\users\petlin\tjeckiska.txt", IO.FileMode.Open, IO.FileAccess.Read)
            System.IO.FileStream fs = new System.IO.FileStream("C:\\users\\petlin\\svenska.txt", System.IO.FileMode.Open, System.IO.FileAccess.Read);
            // Dim fs As System.IO.FileStream = New IO.FileStream("C:\users\petlin\sverige.txt", IO.FileMode.Open, IO.FileAccess.Read)
            //System.IO.FileStream fs = new System.IO.FileStream("C:\\users\\petlin\\ryska.txt", System.IO.FileMode.Open, System.IO.FileAccess.Read);

            System.IO.TextReader rdr = new System.IO.StreamReader(fs);
            int maxCount = 10000;
            int count = 0;
            do
            {
                count += 1;
                char[] delim = {' ',',','.'};
                string line = rdr.ReadLine();
                if (line == null)
                    break; 
                string[] words = line.Split(delim);
                for (int i = 0; i< words.Length;i++)
                {
                    string w = words[i];
                    w = w.Trim();
                    w = w.ToLower();
                    w = w.Replace( "(", "");
                    w = w.Replace( ")", "");
                    w = w.Replace( "]", "");
                    w = w.Replace( "[", "");
                    w = w.Replace( "„", "");
                    w = w.Replace( ":", "");
                    if (!string.IsNullOrEmpty(w))
                    {
                        HasseNode NewNode = new StringHasseNode(w, HasseNode.HasseNodeTypes.REAL, elements);
                        HDM.InsertElementIntoHasseDiagram(NewNode);
                        if (w.Contains ("s"))
                        HDM.RemoveElementFromHasseDiagram(NewNode); 
                    }
                }

                if (count >= maxCount)
                    break; 
            } while (true);
            fs.Close();
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("Total time: " + (sw.ElapsedMilliseconds / 1000).ToString() + " seconds");
            HDM.ContractChains2();
            //HDM.topsort(forward:=False)

            // 20 microseconds to loop 1000 objects and assign
            // 40 microseconds to add 1000 elements to new list
            //HDM.Draw();
            sw.Reset()  ;
            sw.Start();
            string Report = HasseDiagramStats.Report(HDM.AllHasseNodes);
            System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds.ToString()); 
            System.Diagnostics.Debug.WriteLine(Report);
            System.Diagnostics.Debug.WriteLine(HDM.AllHasseNodes.Count.ToString() + " objects");


        }
        public void test5()
        {
            HasseDiagram HDM = new HasseDiagram();
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
                HDM.InsertElementIntoHasseDiagram(new StringHasseNode(buf, HasseNode.HasseNodeTypes.REAL, elements));
            }
            //HDM.BFGTopOrder.DebugReport()
            TopologicalSort.topsort(HDM.AllHasseNodes ,true);
            HDM.Draw();

        }



    }
}