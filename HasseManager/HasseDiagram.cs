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
        public HasseNodeCollection HasseDiagramNodes = new HasseNodeCollection();
        public HasseNodeCollection DifferenceNodes = new HasseNodeCollection();
        HasseNodeCollection ElementaryHasseNodes = new HasseNodeCollection();
        HasseFragmentInsertionQueue FragmentInsertionQueue = new HasseFragmentInsertionQueue();

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
        public const int DEBUGLEVEL = 4 + 8;
        public const int CHECK_LEVEL = CHECK_NOTHING;
        public bool MAKE_MCS_AT_ONCE = true;
        public bool MAKE_LABELLED_NODES = true;
        public const int MINIMUM_FRAGMENT_SIZE = 5;
        // ==========================================


        public HasseDiagram(HasseNode Root)
        {
            RootNode = Root;
            this.InsertNodeIntoDiagram(RootNode);
        }


        public int DeleteNodesWithOneCover()
        {
            List<HasseNode> ToBeRemoved = new List<HasseNode>();
            foreach (HasseNode N in HasseDiagramNodes.Values)
            {
                if (N.EdgesToCovering.Count == 1) { ToBeRemoved.Add(N); }
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


            HasseEdge[] EdgesDownToCovered = Node.EdgesToCovered.ToArray();
            HasseNode TopNode = Node.EdgesToCovering[0].UpperNode;

            System.Diagnostics.Debug.WriteLine("ContractNodeWithCover: " + Node.KeyString);
            System.Diagnostics.Debug.WriteLine("Covered by: " + TopNode.KeyString);

            foreach (HasseEdge EdgeToCovered in EdgesDownToCovered)
            {
                System.Diagnostics.Debug.WriteLine("Moving edge from : " + EdgeToCovered.LowerNode.KeyString);
                MakeEdgeIfNotExists(EdgeToCovered.LowerNode, TopNode);
                RemoveEdge(EdgeToCovered);
            }
            HasseDiagramNodes.Remove(Node.KeyString);
        }


        public void ContractChains()
        {
            HasseNodeCollection VisitedNodes = new HasseNodeCollection();
            // identify vertices with one edge in and one edge out, put them on remove list
            List<HasseNode> ToBeRemoved = new List<HasseNode>();
            foreach (HasseNode Node in HasseDiagramNodes.Values)
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
                HasseDiagramNodes.Remove(Node.KeyString);
            }
        }


        public void ContractChains2()
        {
            // identify vertices with one edge out, put them on remove list
            HasseNodeCollection VisitedNodes = new HasseNodeCollection();
            List<HasseNode> ToBeRemoved = new List<HasseNode>();
            foreach (HasseNode Node in HasseDiagramNodes.Values)
            {
                if (!VisitedNodes.ContainsKey(Node.KeyString))
                {
                    if (Node.EdgesToCovering.Count == 1)
                    {
                        foreach (HasseEdge TheCoveringEdge in Node.EdgesToCovering)
                        {
                            if (TheCoveringEdge.UpperNode.EdgesToCovered.Count == 1)
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

                // make list of edges to covered 
                HasseEdge[] EdgesToCovered = Node.EdgesToCovered.ToArray();

                foreach (HasseEdge E2 in EdgesToCovered)
                {
                    HasseNode Node2 = E2.LowerNode;
                    // inherit edges from those that Node covers
                    this.MakeEdge(Node2, NodeCovering);
                    this.RemoveEdge(E2);
                }
                HasseDiagramNodes.Remove(Node.KeyString);
            }
        }


        public void FindLUBAndGLB(HasseNode newNode, ref HasseNodeCollection lub, ref HasseNodeCollection glb)
        {
            bool dbgTimings = Convert.ToBoolean(DEBUGLEVEL & LOG_ALL_TIMINGS);
            bool dbgComparisons = Convert.ToBoolean(DEBUGLEVEL & LOG_ALL_COMPARISON_COUNTS);
            glb = BruteForceFindGlb(newNode, HasseDiagramNodes);
            lub = BruteForceFindLub(newNode, HasseDiagramNodes);
        }


        public void RemoveNodeFromHasseDiagram(HasseNode Node)
        {
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
            HasseDiagramNodes.Remove(Node.KeyString);
        }


        public void InsertNodeIntoDiagram(HasseNode newNode)
        {
            InsertNodeIntoDiagram(newNode, null, null);

            while (FragmentInsertionQueue.Count > 0)
            {
                FragmentToBeInserted F = FragmentInsertionQueue.Dequeue();
                HasseNode Frag = new StringHasseNode(F.NewNodeContent, HasseNode.HasseNodeTypes.FRAGMENT, ElementaryHasseNodes);

                bool NodeWasInserted = InsertNodeIntoDiagram(Frag, null, null);
            }
        }

        private bool InsertNodeIntoDiagram(HasseNode newNode, HasseNode[] AboveTheseNodes, HasseNode[] BelowTheseNodes)
        {
            // should return true if node was inserted 
            // or false if corresponding node already exist 

            bool debug = Convert.ToBoolean(DEBUGLEVEL & LOG_INSERTIONS);
            bool dbgTimings = Convert.ToBoolean(DEBUGLEVEL & LOG_ALL_TIMINGS);
            sw.Reset();
            sw.Start();
            // do not add identical objects
            if (newNode.KeyString.Equals("*rdiskt"))
            {
                System.Diagnostics.Debugger.Break();
            }

            if (HasseDiagramNodes.ContainsKey(newNode.KeyString))
            {
                HasseDiagramNodes[newNode.KeyString].AddNodeType(newNode.NodeType);
                System.Diagnostics.Debug.WriteLineIf(debug, " Skipping add of " + newNode.KeyString);
                return false;
            }
            CountAdditions += 1;
            if (DEBUGLEVEL > 0)
            {
                System.Diagnostics.Debug.WriteLine("Add Node " + newNode.KeyString + " " + CountAdditions.ToString());
            }

            foreach (HasseNode newObjectElement in newNode.getElementarySubobjects().Values)
            {
                if (ALWAYS_ADD_ELEMENTS_TO_DIAGRAM)
                {
                    if (!HasseDiagramNodes.ContainsKey(newObjectElement.KeyString))
                        HasseDiagramNodes.Add(newObjectElement.KeyString, newObjectElement);
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

            HasseNodeCollection NodesInGreatestLowerBound = null;
            HasseNodeCollection NodesInLeastUpperBound = null;
            FindLUBAndGLB(newNode, ref NodesInLeastUpperBound, ref NodesInGreatestLowerBound);

            System.Diagnostics.Debug.WriteLineIf(dbgTimings, " ticks 2 (got lub and glb) " + sw.ElapsedTicks.ToString());
            sw.Reset();
            sw.Start();
            System.Diagnostics.Debug.WriteLineIf(debug, "=== Done LUB and GLB =======");

            List<HasseEdge> NewlyFormedEdges = InsertNodeBetweenGlbAndLub(
                newNode, NodesInLeastUpperBound, NodesInGreatestLowerBound, debug);

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
                        if (!newNode.Equals(Sibling))
                        {
                            if (newNode.KeyString.Equals(Sibling.KeyString))
                            {
                                throw new Exception("Make MCS: different objects with same key");
                            }
                            if (Parent.NodeType == HasseNode.HasseNodeTypes.ROOT)
                            {
                                foreach (HasseNode Element in newNode.getElementarySubobjects().Values)
                                {
                                    // TODO better efficiency, use with least count elements
                                    // TODO make debug level for this
                                    Element.GetMaxCommonFragments(newNode, Sibling, false, FragmentInsertionQueue, ElementaryHasseNodes, MINIMUM_FRAGMENT_SIZE);
                                }
                            }
                            else
                            {
                                Parent.GetMaxCommonFragments(newNode, Sibling, false, FragmentInsertionQueue, ElementaryHasseNodes, MINIMUM_FRAGMENT_SIZE);
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
                HasseDiagramNodes.Add(newNode.KeyString, newNode);
            return true;
        }



        private List<HasseEdge> InsertNodeBetweenGlbAndLub(
            HasseNode newNode,
            HasseNodeCollection collectionLUB,
            HasseNodeCollection collectionGLB, bool debug)
        {
            bool dbg = Convert.ToBoolean(DEBUGLEVEL & LOG_INSERTIONS);
            List<HasseEdge> AddedEdges = new List<HasseEdge>();
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
            foreach (HasseEdge E in EdgesToBeDeleted)
            {
                RemoveEdge(E);
            }
            //make differences between elements and new

            foreach (HasseNode v_low in collectionGLB.Values)
            {
                System.Diagnostics.Debug.WriteLineIf(dbg, "cover (I)  " + v_low.KeyString + " with " + newNode.KeyString);
                HasseEdge NewEdge = MakeEdge(v_low, newNode);
                AddedEdges.Add(NewEdge);
            }
            //make relations between new and LUBs
            foreach (HasseNode v_high in collectionLUB.Values)
            {
                System.Diagnostics.Debug.WriteLineIf(dbg, "cover (II) " + newNode.KeyString + " with " + v_high.KeyString);
                MakeEdge(newNode, v_high);
            }
            return AddedEdges;
        }



        private void RemoveEdge(HasseEdge E)
        {
            //disconnect upper node
            E.UpperNode.EdgesToCovered.Remove(E);
            E.UpperNode = null;

            // disconnect lower node
            E.LowerNode.EdgesToCovering.Remove(E);
            E.LowerNode = null;


            // --------- deal with linked node(s) ------------------------------------------
            HasseNode[] LinkedNodes = E.LinkedNodes.ToArray(); // make copy to avoid changed collection error
            foreach (HasseNode LinkedNode in LinkedNodes)
            {

                // remove reference to this (E) from the linked node 
                LinkedNode.linkedEdges.Remove(E);

                if (LinkedNode.linkedEdges.Count == 0) // possibly remove node completely..
                {
                    // ... if not MCS
                    if (false == Convert.ToBoolean(LinkedNode.NodeType &
                        HasseNode.HasseNodeTypes.MAX_COMMON_FRAGMENT))
                    {
                        // ... and not non-fragment
                        if (false == Convert.ToBoolean(LinkedNode.NodeType &
                        HasseNode.HasseNodeTypes.REAL))
                        {
                            // Remove node from list catalog of difference nodes 
                            DifferenceNodes.Remove(LinkedNode.KeyString);
                            // Remove reference from this edge to linked node
                            E.LinkedNodes.Remove(LinkedNode); // disconnect to linked node

                            // by now E should point to nothing and nothing point to E
                        }
                    }
                }
            }// -----------------    loop until dealt with linked nodes --------------------
            E.Removed = true; // For debugging
        }


        private HasseEdge MakeEdge(HasseNode N1, HasseNode N2)
        {
            // returns new edge. If already exists, returns null
            if ((N1 == null) || (N2 == null)) { throw new Exception("MakeEdge with null argument"); }
            if (ExistsEdge(N1, N2)) return null;

            HasseEdge E = new HasseEdge();
            E.LowerNode = N1;
            E.UpperNode = N2;
            N1.EdgesToCovering.Add(E);
            N2.EdgesToCovered.Add(E);

            string[] EdgeLabels = N1.GetDifferenceString(N2);
            E.LabelText = String.Join(", ", EdgeLabels);


            foreach (string s in EdgeLabels)
            {
                if (E.LabelText.Contains("~~~")) //for debug
                {
                    System.Diagnostics.Debugger.Break();
                }


                // For difference fragments, add link between fragment and those
                // edges giving this Node as difference.  
                // The link is for efficiency of updates of difference items, it is not an edge in hasse diagram

                if (N1 != RootNode) // do not make difference here
                {
                    HasseNode DifferenceNode = null;
                    if (DifferenceNodes.ContainsKey(s))  // if such node already exists in differenc nodes
                    {
                        DifferenceNode = DifferenceNodes[s]; //  get that node
                    }
                    else
                        if (HasseDiagramNodes.ContainsKey(s))  // if already exists in diagram nodes
                        {
                            DifferenceNode = HasseDiagramNodes[s]; // get reference
                            // add difference type
                            DifferenceNode.AddNodeType(HasseNode.HasseNodeTypes.DIFFERENCE_FRAGMENT);
                            // and add to difference catalog
                            DifferenceNodes.Add(DifferenceNode.KeyString    , DifferenceNode); 
                        }
                        else // not exist anywhere
                        {
                            // create and add to differene node catalog
                            DifferenceNode = new StringHasseNode(
                                s, HasseNode.HasseNodeTypes.FRAGMENT | HasseNode.HasseNodeTypes.DIFFERENCE_FRAGMENT,
                                this.ElementaryHasseNodes);
                            DifferenceNodes.Add(DifferenceNode.KeyString, DifferenceNode);
                        }

                    DifferenceNode.AddLinkToEdge(E); // create the link

                }
            }
            return E;
        }


        private bool ExistsEdge(HasseNode N1, HasseNode N2)
        {
            foreach (HasseEdge E in N1.EdgesToCovering)
            {
                if (E.UpperNode == N2)
                {
                    return true;
                }
            }
            return false;
        }

        private void MakeEdgeIfNotExists(HasseNode N1, HasseNode N2)
        {
            // Todo for max efficiency check if count covered from N2 is lower
            foreach (HasseEdge E in N1.EdgesToCovering)
            {
                if (E.UpperNode == N2) return;
            }
            MakeEdge(N1, N2);
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
            System.Diagnostics.Debug.WriteLine(Node.weight.ToString() + "\t" + spaces + Node.KeyString);

            foreach (HasseEdge e in Node.EdgesToCovering)
            {
                DrawCoveringNodes(level + 1, e.UpperNode);
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
                    System.Diagnostics.Debug.WriteLineIf(dbg, "test if " + ReferenceNode.KeyString + " is larger than " + Node.KeyString);
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
                    else ToBeRemoved.Add(Node.KeyString);
                }

            }

            foreach (string key in ToBeRemoved)
            {
                glb.Remove(key);
            }

            return (glb);
        }

        private static void DeleteNodesBelow(HasseNode Node, List<string> ToBeRemoved, int level)
        {
            foreach (HasseEdge E in Node.EdgesToCovered)
            {
                if (!ToBeRemoved.Contains(E.LowerNode.KeyString))
                    DeleteNodesBelow(E.LowerNode, ToBeRemoved, level + 1);
            }
            if (level > 0) ToBeRemoved.Add(Node.KeyString);
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
                if (!ToBeRemoved.Contains(EdgeToCovering.UpperNode.KeyString))
                    DeleteNodesAbove(EdgeToCovering.UpperNode, ToBeRemoved, level + 1);
            }
            if (level > 0) ToBeRemoved.Add(Node.KeyString);
        }


    }
}
