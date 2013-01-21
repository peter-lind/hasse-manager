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
        public HasseNode RootNode = null;
        public HasseNodeCollection AllHasseNodes = new HasseNodeCollection();
        HasseNodeCollection ElementaryHasseNodes = new HasseNodeCollection();
 //       System.Collections.Queue ProcessingQueue = new System.Collections.Queue();
        HasseFragmentInsertionList FragmentInsertionList = new HasseFragmentInsertionList();
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        int CountAdditions = 0;


        public const int LOG_NOTHING = 0;
        public const int LOG_DEBUG_MAKE_LUB = 1;
        public const int LOG_DEBUG_MAKE_GLB = 2;
        public const int LOG_DEBUG_ADD_NEW_ELEMENT = 4;
        public const int LOG_INSERTIONS = 8;
        public const int LOG_ALL_COMPARISON_COUNTS = 16;
        public const int LOG_DEBUG_ALL = 1 + 2 + 4 + 8;

        public const int LOG_ALL_TIMINGS = 32;

        public const int CHECK_NOTHING = 0;
        public const int CHECK_LUB_AND_GLB = 1;
        public const int CHECK_ALL = 1;


        // ==========================================
        // edit those settings
        public bool ALWAYS_ADD_ELEMENTS_TO_DIAGRAM = false;
        public const int DEBUGLEVEL = 4+8;
        public const int CHECK_LEVEL = CHECK_NOTHING;
        public bool MAKE_MCS_AT_ONCE = true;
       public bool MAKE_LABELLED_NODES = true;

        // ==========================================


       public HasseDiagram(HasseNode Root)
       {
           RootNode = Root;
           this.InsertNodeIntoDiagram(RootNode);
       }

       
       public int DeleteNodesWithOneCover()
       {
           List<HasseNode> ToBeRemoved = new List<HasseNode>();
           foreach (HasseNode N in AllHasseNodes.Values)
           {
              if (N.EdgesToCovering.Count ==1 ) { ToBeRemoved.Add(N); }
           }
           int Count = ToBeRemoved.Count(); 
           foreach (HasseNode N in ToBeRemoved)
           {
               ContractNodeWithCover(N);
           }
           return ToBeRemoved.Count(); 
       }
 

       public void ContractNodeWithCover(HasseNode Node)
       {
           // Node should have exactly one cover. 
           // Edges from covered are moved to go to the cover = (break and form)
           // Finally Node is deleted
           

          // make list of currently covered nodes
           //List<HasseNode> CoveredNodes = new List<HasseNode>();
             // foreach(HasseEdge E in Node.EdgesToCovered ){
             // CoveredNodes.Add (E.LowerNode ); 
             // }
              
           HasseEdge[] EdgesDownToCovered = Node.EdgesToCovered.ToArray();  
                    
           HasseNode TopNode = Node.EdgesToCovering[0].UpperNode;  

           System.Diagnostics.Debug.WriteLine ("ContractNodeWithCover: " + Node.KeyString);
           System.Diagnostics.Debug.WriteLine("Covered by: " + TopNode.KeyString);

           foreach (HasseEdge EdgeToCovered in EdgesDownToCovered)
          {
              System.Diagnostics.Debug.WriteLine("Moving edge from : " + EdgeToCovered.LowerNode.KeyString);
              MakeEdgeIfNotExists( EdgeToCovered.LowerNode  , TopNode);              
               RemoveEdge(EdgeToCovered);
              //BreakCoverRelation(CoveredNode, Node);
              //MakeCoverRelationIfNotExists  (CoveredNode, TopNode);
          }
          AllHasseNodes.Remove(Node.KeyString); 
       }


        public void ContractChains()
        {
            HasseNodeCollection VisitedNodes = new HasseNodeCollection();
            // identify vertices with one edge in and one edge out, put them on remove list
            List<HasseNode> ToBeRemoved = new List<HasseNode>();
            foreach (HasseNode Node in AllHasseNodes.Values)
            {
                if (!VisitedNodes.ContainsKey(Node.KeyString))
                {
                    if (Node.EdgesToCovered.Count == 1 && Node.EdgesToCovering.Count == 1)
                    {
                        ToBeRemoved.Add(Node);
                    }
                    VisitedNodes.Add(Node.KeyString, Node);
                }
            }

            //now contract nodes A-B-C to form A-C, then throw away B
            foreach (HasseNode Node in ToBeRemoved)
            {
                HasseNode Node1 = Node.EdgesToCovered[0].LowerNode;
                HasseNode Node2 = Node.EdgesToCovering[0].UpperNode;
                this.MakeEdge(Node1, Node2);
                this.RemoveEdge(Node.EdgesToCovered[0]);
                this.RemoveEdge(Node.EdgesToCovering[0]);

                //this.BreakCoverRelation(Node1, Node);
                //this.BreakCoverRelation(Node, Node2);
                //this.MakeCoverRelation(Node1, Node2, false);
              
                //System.Diagnostics.Debug.WriteLine("contract  [" + Node1.KeyString + "]-[" + Node.KeyString + "]-[" + Node2.KeyString + "]");
                AllHasseNodes.Remove(Node.KeyString);
            }
        }


        public void ContractChains2()
        {
            // identify vertices with one edge out, put them on remove list
            HasseNodeCollection VisitedNodes = new HasseNodeCollection();
            List<HasseNode> ToBeRemoved = new List<HasseNode>();
            foreach (HasseNode Node in AllHasseNodes.Values)
            {
                if (!VisitedNodes.ContainsKey(Node.KeyString))
                {
                    if (Node.EdgesToCovering.Count == 1)
                    {
                        foreach (HasseEdge TheCoveringEdge in Node.EdgesToCovering){
                        if (TheCoveringEdge.UpperNode.EdgesToCovered.Count==1 )
                        ToBeRemoved.Add(Node);
                        }
                    }
                    VisitedNodes.Add(Node.KeyString, Node);
                }
            }

            //now contract nodes A-B to form B
            // B must inherit innodes as into A, then throw away A
            foreach (HasseNode Node in ToBeRemoved)
            {
                HasseEdge EdgeUpToCovering = Node.EdgesToCovering[0];
                HasseNode NodeCovering = EdgeUpToCovering.UpperNode;
                //HasseNode Node2 = null;
                                   
                    System.Diagnostics.Debug.WriteLine("contract  [" + Node.KeyString + "]-[" + NodeCovering.KeyString + "]");
                    RemoveEdge(EdgeUpToCovering);  
                //this.BreakCoverRelation(Node, NodeCovering);

                    // make list of edges to covered 
                    HasseEdge [] EdgesToCovered = Node.EdgesToCovered.ToArray (); 

                    foreach (HasseEdge E2 in EdgesToCovered)
                    {
                        HasseNode Node2 = E2.LowerNode ;
                       // Node2 = o2;
                        // inherit edges from those that Node covers
                        this.MakeEdge(Node2, NodeCovering);
                        //this.MakeCoverRelation(Node2, NodeCovering, false);
                        this.RemoveEdge(E2);
                        //this.BreakCoverRelation(Node, Node2);
                    }
                AllHasseNodes.Remove(Node.KeyString);
            }
        }




        public void FindLUBAndGLB(HasseNode newNode, ref HasseNodeCollection lub,ref HasseNodeCollection glb)
        {
            bool dbgTimings = Convert.ToBoolean(DEBUGLEVEL & LOG_ALL_TIMINGS);
            bool dbgComparisons = Convert.ToBoolean(DEBUGLEVEL & LOG_ALL_COMPARISON_COUNTS);
            glb = BruteForceFindGlb(newNode, AllHasseNodes);
            lub = BruteForceFindLub(newNode, AllHasseNodes);
        }


        public void RemoveElementFromHasseDiagram(HasseNode Node)
        {
            //HasseNodeCollection glb_x = new HasseNodeCollection();
            HasseEdge[] CoveredEdges = Node.EdgesToCovered.ToArray();
            HasseEdge[] CoveringEdges = Node.EdgesToCovering.ToArray();

            // form the new edges;
            foreach (HasseEdge CoveredEdge in CoveredEdges)
            {
                foreach (HasseEdge CoveringEdge in CoveringEdges)
                {
                    MakeEdge(CoveredEdge.LowerNode, CoveringEdge.UpperNode);
                }
            }
            // remove all edges involving Node
            foreach (HasseEdge CoveredEdge in CoveredEdges)
                RemoveEdge(CoveredEdge);
            foreach (HasseEdge CoveringEdge in CoveringEdges)
                RemoveEdge(CoveringEdge);
            // remove Node from Node dictionary
            AllHasseNodes.Remove(Node.KeyString ); 
        }


        public void InsertNodeIntoDiagram(HasseNode newNode)
        {
            InsertNodeIntoDiagram (newNode,null,null);
            foreach (FragmentToBeInserted F in FragmentInsertionList)
            {
                HasseNode Frag = new StringHasseNode(F.NewNodeContent, HasseNode.HasseNodeTypes.FRAGMENT, ElementaryHasseNodes);
                InsertNodeIntoDiagram(Frag,null,null);
            }
        }

        private void InsertNodeIntoDiagram(HasseNode newNode, HasseNode[] AboveTheseNodes,HasseNode[] BelowTheseNodes )
        {
            bool debug = Convert.ToBoolean(DEBUGLEVEL & LOG_INSERTIONS);
            bool dbgTimings = Convert.ToBoolean(DEBUGLEVEL & LOG_ALL_TIMINGS);
            sw.Reset();
            sw.Start();
            // do not add identical objects
            if (newNode.KeyString.Equals("~~~~"))
            {
                System.Diagnostics.Debugger.Break();
            }

            if (AllHasseNodes.ContainsKey(newNode.KeyString))
            {
                AllHasseNodes[newNode.KeyString].AddNodeType(newNode.NodeType);
                System.Diagnostics.Debug.WriteLineIf(debug, " Skipping add of " + newNode.KeyString);
                return;
            }
            CountAdditions += 1;
            if (DEBUGLEVEL > 0)
                System.Diagnostics.Debug.WriteLine("Add Node " + newNode.KeyString + " " + CountAdditions.ToString());

                foreach (HasseNode newObjectElement in newNode.getElementarySubobjects().Values)
                {
                    if (ALWAYS_ADD_ELEMENTS_TO_DIAGRAM)
                    {
                        if (!AllHasseNodes.ContainsKey(newObjectElement.KeyString))
                            AllHasseNodes.Add(newObjectElement.KeyString, newObjectElement);
                    }

                    if (!ElementaryHasseNodes.ContainsKey(newObjectElement.KeyString))
                        ElementaryHasseNodes.Add(newObjectElement.KeyString, newObjectElement);
                }

            System.Diagnostics.Debug.WriteLineIf(dbgTimings, " ticks add 1 (init) " + sw.ElapsedTicks.ToString());
            sw.Reset();
            sw.Start();

            System.Diagnostics.Debug.WriteLineIf(debug, "=== Start LUB and GLB for " + newNode.KeyString + " ===");

            if (CountAdditions == -1)
                System.Diagnostics.Debugger.Break();

            HasseNodeCollection NodesInGreatestLowerBound=null ;
            HasseNodeCollection NodesInLeastUpperBound=null;
            FindLUBAndGLB(newNode, ref NodesInLeastUpperBound, ref NodesInGreatestLowerBound);

            System.Diagnostics.Debug.WriteLineIf(dbgTimings, " ticks 2 (got lub and glb) " + sw.ElapsedTicks.ToString());
            sw.Reset();
            sw.Start();
            System.Diagnostics.Debug.WriteLineIf(debug, "=== Done LUB and GLB =======");

            InsertElementBetweenGlbAndLub(newNode, NodesInLeastUpperBound, NodesInGreatestLowerBound,  debug);
            System.Diagnostics.Debug.WriteLineIf(dbgTimings, " ticks 3 (inserted new) " + sw.ElapsedTicks.ToString());
            sw.Reset();
            sw.Start();

            if (Convert.ToBoolean(CHECK_LEVEL & CHECK_ALL))
                newNode.validate();


            if (MAKE_MCS_AT_ONCE)
            {
                // can have several lowers, loop them
                    foreach (HasseEdge EdgeDownToParent in newNode.EdgesToCovered)
                    {
                        HasseNode Parent = EdgeDownToParent.LowerNode;
                        // get sibling of new
                        foreach (HasseEdge EdgeUpToSibling in EdgeDownToParent.LowerNode.EdgesToCovering)
                        {
                            HasseNode Sibling = EdgeUpToSibling.UpperNode;
                            if (! newNode.Equals (Sibling   )) 
                            {
                                if (Parent.NodeType == HasseNode.HasseNodeTypes.ROOT)
                                {
                                    foreach (HasseNode Element in newNode.getElementarySubobjects().Values  )   {                                 
                                        // TODO better efficiency, use with least count elements
                                        // TODO make debug level for this
                                        Element.GetMaxCommonFragments(newNode,Sibling , false, FragmentInsertionList, ElementaryHasseNodes);
                                    }
                                }
                                else
                                {
                                    Parent.GetMaxCommonFragments(newNode, Sibling , false, FragmentInsertionList, ElementaryHasseNodes);
                                }
                            }
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
                AllHasseNodes.Add(newNode.KeyString, newNode);
        }



        private void InsertElementBetweenGlbAndLub(HasseNode newNode, HasseNodeCollection collectionLUB, HasseNodeCollection collectionGLB,  bool debug)
        {
            bool dbg = Convert.ToBoolean(DEBUGLEVEL & LOG_INSERTIONS);

            // break any existing relations between LUBs and GLBs
            // first make list of edges to delete
            List<HasseEdge> EdgesToBeDeleted = new List<HasseEdge>();
            foreach (HasseNode v_low in collectionGLB.Values)
            {
                foreach (HasseNode v_high in collectionLUB.Values)
                {
                    System.Diagnostics.Debug.WriteLineIf(dbg, "cut " + v_low.KeyString + " - " + v_high.KeyString);
                    foreach (HasseEdge E in v_low.EdgesToCovering)
                    {
                        if (E.UpperNode == v_high)
                            EdgesToBeDeleted.Add(E);
                    }
                }
            }
            foreach (HasseEdge E in EdgesToBeDeleted ){                            
                RemoveEdge(E);}
            //make differences between elements and new
           
            foreach (HasseNode v_low in collectionGLB.Values)
            {
                System.Diagnostics.Debug.WriteLineIf(dbg, "cover (I)  " + v_low.KeyString + " with " + newNode.KeyString);
                MakeEdge(v_low, newNode);

                //if (MAKE_LABELLED_NODES == true)
                //{
                  //  v_low.GetDifferenceFragments(newNode, ref processingQueue, ref AllHasseNodes);
                }
            

            //make relations between new and LUBs
            foreach (HasseNode v_high in collectionLUB.Values)
            {
                System.Diagnostics.Debug.WriteLineIf(dbg, "cover (II) " + newNode.KeyString + " with " + v_high.KeyString);
                MakeEdge(newNode, v_high);
              /*
                if (MAKE_LABELLED_NODES == true)
                {
                    newNode.GetDifferenceFragments(v_high, ref processingQueue, ref AllHasseNodes);
                }
               * */
            }

        }

        /*

        private void MakeCoverRelation(HasseNode Node1, HasseNode Node2, bool dbg)
        {
            try
            {
                Node1.NodesCovering().Add(Node2.KeyString, Node2);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(Node2.KeyString + " already covering " + Node1.KeyString);
                throw new Exception(Node2.KeyString + " already covering " + Node1.KeyString);
            }
            try
            {
                Node2.NodesCovered().Add(Node1.KeyString, Node1);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(Node2.KeyString + " already covered by " + Node1.KeyString);
                throw new Exception(Node2.KeyString + " already covered by " + Node1.KeyString);
            }
        }

        */
        /*
        private void MakeCoverRelationIfNotExists(HasseNode Node1, HasseNode Node2)
        {
            if (!Node1.NodesCovering().ContainsKey(Node2.KeyString ) )
            {
                Node1.NodesCovering().Add(Node2.KeyString, Node2);
            }
            if (!Node2.NodesCovered().ContainsKey(Node1.KeyString))
            {
                Node2.NodesCovered().Add(Node1.KeyString, Node1);
            }
        }
        */

        private void RemoveEdge(HasseEdge E)
        {
            //remove this edge from upper nodes list of edges down
            E.UpperNode.EdgesToCovered.Remove(E); 
            // remove this edge from lower nodes list of edges up
            E.LowerNode.EdgesToCovering.Remove(E);  
            E.LowerNode = null;
            E.UpperNode = null;
        }
        private void MakeEdge(HasseNode N1, HasseNode N2)
        {
            HasseEdge E = new HasseEdge(); 
            E.LowerNode =N1;
            E.UpperNode =N2;
            N1.EdgesToCovering.Add (E);
            N2.EdgesToCovered.Add (E); 
        }

        private void MakeEdgeIfNotExists(HasseNode N1, HasseNode N2)
        {
            // Todo for max efficiency check if count covered from N2 is lower
            foreach (HasseEdge E in N1.EdgesToCovering)
            {
                if (E.UpperNode ==N2) return;
            }
            MakeEdge(N1,N2);
        }

        /*
        private void BreakCoverRelation(HasseNode Node1, HasseNode Node2)
        {
            Node1.NodesCovering().Remove(Node2.KeyString);
            Node2.NodesCovered().Remove(Node1.KeyString);
        }
         
        private void breakCoverRelationIfExists(HasseNode Node1, HasseNode Node2)
        {
            if (Node1.NodesCovering().ContainsKey(Node2.KeyString))
                Node1.NodesCovering().Remove(Node2.KeyString);
            if (Node2.NodesCovered().ContainsKey(Node1.KeyString))
                Node2.NodesCovered().Remove(Node1.KeyString);
        }
        */


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
            System.Diagnostics.Debug.WriteLine(Node.weight.ToString() + "\t" + spaces + Node.KeyString);
            
            foreach (HasseEdge e in Node.EdgesToCovering)
            {
                DrawCoveringNodes(level + 1, e.UpperNode );
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
                    lub.Add(Node.KeyString, Node);
                }
            }

            foreach (HasseNode Node in lub.Values)
            {
                foreach (HasseNode Node2 in lub.Values)
                {
                    if (Node2.IsLargerThan(Node))
                    {
                        ToBeRemoved.Add(Node2.KeyString);
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
            if (@ref.KeyString.Equals("x")) { System.Diagnostics.Debugger.Break(); }

            HasseNodeCollection glb = new HasseNodeCollection();
            List<string> ToBeRemoved = new List<string>();
            foreach (HasseNode Node in AllNodes.Values)
            {
                if (@ref.IsLargerThan(Node))
                {
                    glb.Add(Node.KeyString, Node);
                }
            }


            foreach (HasseNode Node in glb.Values)
            {
                foreach (HasseNode Node2 in glb.Values)
                {
                    if (Node != Node2)
                    {
                    if (Node.IsLargerThan(Node2))
                        { ToBeRemoved.Add(Node2.KeyString); }
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

        public static HasseNodeCollection FindGlb(HasseNode ReferenceNode, HasseNodeCollection AllNodes)
        {
            bool dbg = Convert.ToBoolean(DEBUGLEVEL & LOG_DEBUG_MAKE_GLB);
            HasseNodeCollection glb = new HasseNodeCollection();
            List<string> ToBeRemoved = new List<string>();
            foreach (HasseNode Node in AllNodes.Values)
            {
                if (!ToBeRemoved.Contains(Node.KeyString))
                {
                    System.Diagnostics.Debug.WriteLineIf(dbg, "test if " + ReferenceNode.KeyString + " is larger than " + Node.KeyString   );
                    if (ReferenceNode.IsLargerThan(Node))
                    {
                        System.Diagnostics.Debug.WriteLineIf(dbg, " yes - is glb candidate, delete below...");
                        glb.Add(Node.KeyString, Node);
                        DeleteNodesBelow(Node, ToBeRemoved, 0);
                    }
                 /*   else if (Node.IsLargerThan(ReferenceNode))
                    {
                        DeleteNodesAbove(Node, ToBeRemoved, 0);
                    }
                  */
                    else ToBeRemoved.Add(Node.KeyString ); 
                }

            }

            foreach (string key in ToBeRemoved)
            {
                glb.Remove(key);
            }

            return (glb);
        }

        private static void DeleteNodesBelow(HasseNode Node, List<string> ToBeRemoved, int level) {
        foreach (HasseEdge E in Node.EdgesToCovered ){
           if (!ToBeRemoved.Contains (E.LowerNode.KeyString ))
               DeleteNodesBelow(E.LowerNode, ToBeRemoved, level + 1);
        }
        if (level>0)ToBeRemoved.Add(Node.KeyString ); 
        }





        public static HasseNodeCollection FindLub(HasseNode ReferenceNode, HasseNodeCollection AllNodes)
        {
            bool dbg = Convert.ToBoolean(DEBUGLEVEL & LOG_DEBUG_MAKE_LUB);
            HasseNodeCollection lub = new HasseNodeCollection();
            List<string> ToBeRemoved = new List<string>();
            foreach (HasseNode Node in AllNodes.Values)
            {
                if (!ToBeRemoved.Contains(Node.KeyString))
                {
                    System.Diagnostics.Debug.WriteLineIf(dbg, "test if " + Node.KeyString + " is larger than " + ReferenceNode.KeyString);
                    if (Node.IsLargerThan(ReferenceNode))
                    {
                        System.Diagnostics.Debug.WriteLineIf(dbg, " yes - is lub candidate, delete above...");
                        lub.Add(Node.KeyString, Node);
                        DeleteNodesAbove(Node, ToBeRemoved, 0);
                    }
                    /*
                     else if (ReferenceNode.IsLargerThan(Node))
                    {
                        DeleteNodesBelow(Node, ToBeRemoved, 0);
                    }
                     */
                    else ToBeRemoved.Add(Node.KeyString); 

                }
            }

            foreach (string key in ToBeRemoved)
            {
                lub.Remove(key);
            }

            return (lub);
        }

        private static void DeleteNodesAbove(HasseNode Node, List<string> ToBeRemoved, int level)
        {
            foreach (HasseEdge EdgeToCovering in Node.EdgesToCovering)
            {
                if (!ToBeRemoved.Contains(EdgeToCovering.UpperNode  .KeyString))
                    DeleteNodesAbove(EdgeToCovering.UpperNode, ToBeRemoved, level + 1);
            }
            if (level > 0) ToBeRemoved.Add(Node.KeyString);
        }






    }
}
