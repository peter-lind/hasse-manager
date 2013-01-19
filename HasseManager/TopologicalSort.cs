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

namespace HasseManager
{
    static class  TopologicalSort
    {

        /*
        public static void topsort(HasseNodeCollection Nodes ,bool forward)
        {
            //L <- Empty list were we put the sorted elements
            //Q <- Set of all nodes with no incoming edges
            //while Q is non-empty do
            //    remove a node n from Q
            //    insert n into L
            //    for each node m with an edge e from n to m do
            //        remove edge e from the graph
            //        if m has no other incoming edges then
            //            insert m into Q
            //if graph has edges then
            //    output error message (graph has a cycle)
            //else 
            //    output message (proposed topologically sorted order: L)
            List<HasseNode> L = new List<HasseNode>();
            Queue<HasseNode> Q = new Queue<HasseNode>();

            AddEdges( Nodes, forward);
            foreach (HasseNode Node in Nodes.Values)
            {
                if (Node.EdgesToCovered.Count == 0)
                    Q.Enqueue(Node);
            }
            while (Q.Count > 0)
            {
                HasseNode n = Q.Dequeue();
                L.Add(n);

                //make copy of list
                List<HasseEdge> OutGoingEdgesFrom_n = new List<HasseEdge>(n.EdgesToCovering);

                foreach (HasseEdge e in OutGoingEdgesFrom_n)
                {
                    n.OutgoingNodesTo().Remove(m.KeyString);
                    e.UpperNode.EdgesToCovered.Remove (    .IncomingNodesFrom().Remove(n.KeyString);
                    if (e.UpperNode.EdgesToCovered.Count == 0)
                    {
                        Q.Enqueue(e.UpperNode );
                    }
                }

            }


            foreach (HasseNode Node in L)
            {
                System.Diagnostics.Debug.WriteLine(Node.KeyString);
            }
        }


        */

        /*
        private static void AddEdges(HasseNodeCollection col, bool forward)
        {
            //this sub is used by Topsort
            foreach (HasseNode Node1 in col.Values)
            {
                foreach (HasseNode Node2 in col.Values)
                {
                    if ((!Object.ReferenceEquals(Node1, Node2)))
                    {
                        if (forward)
                        {
                            if (Node2.IsLargerThan(Node1))
                            {
                                Node2.IncomingNodesFrom().Add(Node1.KeyString, Node1);
                                Node1.OutgoingNodesTo().Add(Node2.KeyString, Node2);
                            }
                            //reverse 
                        }
                        else
                        {
                            if (Node1.IsLargerThan(Node2))
                            {
                                Node2.IncomingNodesFrom().Add(Node1.KeyString, Node1);
                                Node1.OutgoingNodesTo().Add(Node2.KeyString, Node2);
                            }
                        }
                    }
                }
            }
        }

        */

    }
}
