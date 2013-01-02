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
using System.Diagnostics;

namespace HasseManager
{
    public class HasseDiagram
    {
        public HasseNodeCollection AllHasseNodes = new HasseNodeCollection();
        HasseNodeCollection ElementaryHasseNodes = new HasseNodeCollection();
        System.Collections.Queue ProcessingQueue = new System.Collections.Queue();
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        int CountAdditions = 0;

        public const int LOG_NOTHING = 0;
        public const int LOG_DEBUG_MAKE_LUB = 1;
        public const int LOG_DEBUG_MAKE_GLB = 2;
        public const int LOG_DEBUG_INSERT_NEW = 4;
        public const int LOG_ADDITIONS = 8;
        public const int LOG_ALL_COMPARISON_COUNTS = 16;
        public const int LOG_DEBUG_ALL = 1 + 2 + 4 + 8;

        public const int LOG_ALL_TIMINGS = 32;

        public const int CHECK_NOTHING = 0;
        public const int CHECK_LUB_AND_GLB = 1;
        public const int CHECK_ALL = 1;


        // ==========================================
        // edit those settings
        public const int DEBUGLEVEL = LOG_NOTHING;
        public const int CHECK_LEVEL = CHECK_NOTHING;
        public bool MAKE_MCS_AT_ONCE = true;

        // ==========================================




        public void ContractChains()
        {
            HasseNodeCollection VisitedNodes = new HasseNodeCollection();
            // identify vertices with one edge in and one edge out, put them on remove list
            List<HasseNode> ToBeRemoved = new List<HasseNode>();
            foreach (HasseNode Node in AllHasseNodes.Values)
            {
                if (!VisitedNodes.ContainsKey(Node.UniqueString))
                {
                    if (Node.NodesCovered().Count == 1 && Node.NodesCovering().Count == 1)
                    {
                        ToBeRemoved.Add(Node);
                    }
                    VisitedNodes.Add(Node.UniqueString, Node);
                }
            }

            //now contract nodes A-B-C to form A-C, then throw away B
            foreach (HasseNode Node in ToBeRemoved)
            {
                HasseNode Node1 = null;
                HasseNode Node2 = null;
                // it is only one!
                foreach (HasseNode o1 in Node.NodesCovered().Values) { Node1 = o1; }
                // it is only one!
                foreach (HasseNode o2 in Node.NodesCovering().Values) { Node2 = o2; }

                this.BreakCoverRelation(Node1, Node);
                this.BreakCoverRelation(Node, Node2);
                this.MakeCoverRelation(Node1, Node2, false);
                System.Diagnostics.Debug.WriteLine("contract  [" + Node1.UniqueString + "]-[" + Node.UniqueString + "]-[" + Node2.UniqueString + "]");
                AllHasseNodes.Remove(Node.UniqueString);
            }
        }


        public void ContractChains2()
        {
            HasseNodeCollection VisitedNodes = new HasseNodeCollection();
            // identify vertices with one edge out, put them on remove list
            List<HasseNode> ToBeRemoved = new List<HasseNode>();
            foreach (HasseNode Node in AllHasseNodes.Values)
            {
                if (!VisitedNodes.ContainsKey(Node.UniqueString))
                {
                    if (Node.NodesCovering().Count == 1)
                    {
                        foreach (HasseNode TheCoveringNode in Node.NodesCovering().Values){
                        if (TheCoveringNode.NodesCovered().Count==1 )
                        ToBeRemoved.Add(Node);
                        }
                    }
                    VisitedNodes.Add(Node.UniqueString, Node);
                }
            }

            //now contract nodes A-B to form B
            // B must inherit innodes as into A, then throw away A
            foreach (HasseNode Node in ToBeRemoved)
            {
                HasseNode NodeCovering = null;
                HasseNode Node2 = null;
                // it is only one!
                foreach (HasseNode o1 in Node.NodesCovering().Values)
                { NodeCovering = o1;}
                
                    
                    System.Diagnostics.Debug.WriteLine("contract  [" + Node.UniqueString + "]-[" + NodeCovering.UniqueString + "]");
                    this.BreakCoverRelation(Node, NodeCovering);

                    // make list of covered nodes
                    List<HasseNode> CoveredNodes = new List<HasseNode>();
                    foreach (HasseNode o2 in Node.NodesCovered().Values)
                        CoveredNodes.Add(o2);

                    foreach (HasseNode o2 in CoveredNodes)
                    {
                        Node2 = o2;
                        // inherit edges from those that Node covers
                        this.MakeCoverRelation(Node2, NodeCovering, false);
                        this.BreakCoverRelation(Node, Node2);

                    }

                
                AllHasseNodes.Remove(Node.UniqueString);
            }
        }




        public void FindLUBAndGLB(HasseNode newNode, ref HasseNodeCollection lub, ref HasseNodeCollection glb)
        {
            bool dbgTimings = Convert.ToBoolean(DEBUGLEVEL & LOG_ALL_TIMINGS);
            bool dbgComparisons = Convert.ToBoolean(DEBUGLEVEL & LOG_ALL_COMPARISON_COUNTS);
            HasseNodeCollection correctGLB = BruteForceFindGlb(newNode, AllHasseNodes);
            HasseNodeCollection correctLub = BruteForceFindLub(newNode, AllHasseNodes);
            glb = correctGLB;
            lub = correctLub;
        }






        public void InsertElementIntoHasseDiagram(HasseNode newNode)
        {
            bool debug = Convert.ToBoolean(DEBUGLEVEL & LOG_ADDITIONS);
            bool dbgTimings = Convert.ToBoolean(DEBUGLEVEL & LOG_ALL_TIMINGS);
            sw.Reset();
            sw.Start();
            //caution - make sure we avoid try to add identical objects

            if (newNode.UniqueString.Equals(""))
            {
                System.Diagnostics.Debugger.Break();
            }

            if (AllHasseNodes.ContainsKey(newNode.UniqueString))
            {
                AllHasseNodes[newNode.UniqueString].AddNodeType(newNode.NodeType);
                System.Diagnostics.Debug.WriteLineIf(debug, " Skipping add of " + newNode.UniqueString);
                return;
            }
            CountAdditions += 1;
            if (DEBUGLEVEL > 0)
                System.Diagnostics.Debug.WriteLine("Add Node " + newNode.UniqueString + " " + CountAdditions.ToString());


            foreach (HasseNode newObjectElement in newNode.getElementarySubobjects().Values)
            {
                if (!AllHasseNodes.ContainsKey(newObjectElement.UniqueString))
                    AllHasseNodes.Add(newObjectElement.UniqueString, newObjectElement);

                if (!ElementaryHasseNodes.ContainsKey(newObjectElement.UniqueString))
                    ElementaryHasseNodes.Add(newObjectElement.UniqueString, newObjectElement);

            }
            // is this node an elementary node and also real or frag? 
            if (ElementaryHasseNodes.ContainsKey(newNode.UniqueString))
            // perhaps just added
            {
                // get type of new node
                HasseNode.HasseNodeTypes t = newNode.NodeType;
                // from now on, refer to the existing node:
                newNode = AllHasseNodes[newNode.UniqueString];
                // add type(s) for this node to, for example add  
                newNode.AddNodeType(t);
            }


            System.Diagnostics.Debug.WriteLineIf(dbgTimings, " ticks add 1 (init) " + sw.ElapsedTicks.ToString());
            sw.Reset();
            sw.Start();


            System.Diagnostics.Debug.WriteLineIf(debug, "=== Start LUB and GLB for " + newNode.UniqueString + " ===");
            HasseNodeCollection glb_x = new HasseNodeCollection();
            HasseNodeCollection lub_x = new HasseNodeCollection();

            if (CountAdditions == -1)
                System.Diagnostics.Debugger.Break();

            FindLUBAndGLB(newNode, ref lub_x, ref glb_x);

            System.Diagnostics.Debug.WriteLineIf(dbgTimings, " ticks 2 (got lub and glb) " + sw.ElapsedTicks.ToString());
            sw.Reset();
            sw.Start();
            System.Diagnostics.Debug.WriteLineIf(debug, "=== Done LUB and GLB =======");

            InsertElementBetweenGlbAndLub(newNode, lub_x, glb_x, ProcessingQueue, debug);
            System.Diagnostics.Debug.WriteLineIf(dbgTimings, " ticks 3 (inserted new) " + sw.ElapsedTicks.ToString());
            sw.Reset();
            sw.Start();

            if (Convert.ToBoolean(CHECK_LEVEL & CHECK_ALL))
                newNode.validate();


            if (MAKE_MCS_AT_ONCE)
            {
                if (newNode.NodesCovering().Count > 1)
                {
                    int i = 1;

                    //For i As Integer = 1 To newNode.NodesCovering.Count
                    foreach (HasseNode covering1 in newNode.NodesCovering().Values)
                    {
                        int j = 1;
                        foreach (HasseNode covering2 in newNode.NodesCovering().Values)
                        {
                            if (j > i)
                            {
                                newNode.makeMaxCommonSubStruc(covering1, covering2, true, ref ProcessingQueue, AllHasseNodes);
                            }
                            j += 1;
                        }
                        i += 1;
                    }
                }
            }


            System.Diagnostics.Debug.WriteLineIf(dbgTimings, " ticks 4 (made MCS)" + sw.ElapsedTicks.ToString());

            //start search upward from elementary subobjects of new object
            if (Convert.ToBoolean(CHECK_LEVEL & CHECK_ALL))
            {
                foreach (HasseNode Node in newNode.getElementarySubobjects().Values)
                {
                    Node.validate();
                }
            }

            // The node may be there already, just added, as element if it is both element and real
            if (!newNode.HasNodeType(HasseNode.HasseNodeTypes.ELEMENT))
                AllHasseNodes.Add(newNode.UniqueString, newNode);


            while (ProcessingQueue.Count > 0)
            {
                HasseNode QueuedNode = (HasseNode)ProcessingQueue.Dequeue();
                InsertElementIntoHasseDiagram(QueuedNode);
            }
        }



        private void InsertElementBetweenGlbAndLub(HasseNode newNode, HasseNodeCollection collectionLUB, HasseNodeCollection collectionGLB, System.Collections.Queue processingQueue, bool debug)
        {
            bool dbg = Convert.ToBoolean(DEBUGLEVEL & LOG_DEBUG_INSERT_NEW);

            //break any existing relations between LUBs and GLBs
            foreach (HasseNode v_low in collectionGLB.Values)
            {
                foreach (HasseNode v_high in collectionLUB.Values)
                {
                    System.Diagnostics.Debug.WriteLineIf(dbg, "cut " + v_low.UniqueString + " - " + v_high.UniqueString);
                    breakCoverRelationIfExists(v_low, v_high);
                }
            }

            //make relations between GLBs and new
            foreach (HasseNode v_low in collectionGLB.Values)
            {
                System.Diagnostics.Debug.WriteLineIf(dbg, "cover (I)  " + v_low.UniqueString + " with " + newNode.UniqueString);
                MakeCoverRelation(v_low, newNode, dbg = debug);
                v_low.makeLabelledObjects(newNode, ref processingQueue, ref AllHasseNodes);
            }

            //make relations between new and LUBs
            foreach (HasseNode v_high in collectionLUB.Values)
            {
                System.Diagnostics.Debug.WriteLineIf(dbg, "cover (II) " + newNode.UniqueString + " with " + v_high.UniqueString);
                MakeCoverRelation(newNode, v_high, dbg = debug);
                newNode.makeLabelledObjects(v_high, ref processingQueue, ref AllHasseNodes);
            }

        }



        private void MakeCoverRelation(HasseNode Node1, HasseNode Node2, bool dbg)
        {
            try
            {
                Node1.NodesCovering().Add(Node2.UniqueString, Node2);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(Node2.UniqueString + " already covering " + Node1.UniqueString);
                throw new Exception(Node2.UniqueString + " already covering " + Node1.UniqueString);
            }
            try
            {
                Node2.NodesCovered().Add(Node1.UniqueString, Node1);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(Node2.UniqueString + " already covered by " + Node1.UniqueString);
                throw new Exception(Node2.UniqueString + " already covered by " + Node1.UniqueString);
            }
        }

        private void BreakCoverRelation(HasseNode Node1, HasseNode Node2)
        {
            Node1.NodesCovering().Remove(Node2.UniqueString);
            Node2.NodesCovered().Remove(Node1.UniqueString);
        }
        private void breakCoverRelationIfExists(HasseNode Node1, HasseNode Node2)
        {
            if (Node1.NodesCovering().ContainsKey(Node2.UniqueString))
                Node1.NodesCovering().Remove(Node2.UniqueString);
            if (Node2.NodesCovered().ContainsKey(Node1.UniqueString))
                Node2.NodesCovered().Remove(Node1.UniqueString);
        }



        public void Draw()
        {
            foreach (HasseNode Node in this.ElementaryHasseNodes.Values)
            {
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("====================================================");
                DrawCoveringNodes(0, Node);
            }

        }

        private void DrawCoveringNodes(int level, HasseNode Node)
        {
            string spaces = new string('-', level * 2);
            //if (Node.NodesCovering().Values.Count >5  )
            System.Diagnostics.Debug.WriteLine(Node.weight.ToString() + "\t" + spaces + Node.UniqueString);
            foreach (HasseNode o in Node.NodesCovering().Values)
            {
                DrawCoveringNodes(level + 1, o);
            }
        }

        public static HasseNodeCollection BruteForceFindLub(HasseNode @ref, HasseNodeCollection AllNodes)
        {
#if DEBUG
            // if (@ref.UniqueString.Equals("*B*")) {
            //    System.Diagnostics.Debugger.Break();
            // }
#endif

            HasseNodeCollection lub = new HasseNodeCollection();
            List<string> ToBeRemoved = new List<string>();
            foreach (HasseNode Node in AllNodes.Values)
            {
                if (Node.IsLargerThan(@ref))
                {
                    lub.Add(Node.UniqueString, Node);
                }
            }

            foreach (HasseNode Node in lub.Values)
            {
                foreach (HasseNode Node2 in lub.Values)
                {
                    if (Node2.IsLargerThan(Node))
                    {
                        ToBeRemoved.Add(Node2.UniqueString);
                        //break;
                    }
                }
            }
            foreach (string key in ToBeRemoved)
            {
                lub.Remove(key);
            }

            return (lub);
        }


        public static HasseNodeCollection BruteForceFindGlb(HasseNode @ref, HasseNodeCollection AllNodes)
        {
            HasseNodeCollection glb = new HasseNodeCollection();
            List<string> ToBeRemoved = new List<string>();
            foreach (HasseNode Node in AllNodes.Values)
            {
                if (@ref.IsLargerThan(Node))
                {
                    glb.Add(Node.UniqueString, Node);
                }
            }


            foreach (HasseNode Node in glb.Values)
            {
                foreach (HasseNode Node2 in glb.Values)
                {
                    if (Node.IsLargerThan(Node2))
                    {
                        ToBeRemoved.Add(Node2.UniqueString);
                        //           break;
                    }
                }
            }
            foreach (string key in ToBeRemoved)
            {
                glb.Remove(key);
            }

            return (glb);
        }





    }
}
