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
    public class HasseNodeCollection : Dictionary <string, HasseNode>
    {
        public HasseNodeCollection()
            : base()
        { 
            
        }

        public void Sort()
        {
            // Todo: this can probably be done without copying all
            List<HasseNode> Nodes;
            Nodes = this.Values.ToList();
            Nodes.Sort();
            this.Clear();
            foreach (HasseNode N in Nodes)
            {
                this.Add(N.KeyString, N);
            }

        }

        public HasseNodeCollection Clone
        {
            get { return (HasseNodeCollection)this.MemberwiseClone(); }
        }

        /*
        public List<HasseNode> NodesHavingElements(HasseNode[] Elements)
        {
            // TODO sort elements with least common first
            List<HasseNode> L = new List<HasseNode>();
            foreach (HasseNode C in this.Values ){
                if (C.HasElements(Elements))
                {
                    L.Add(C);
                }
            }
            return L; 
        }
        */

        /*
        public List<HasseNode> NodesWithOneOfElements(HasseNode[] Elements)
        {
            // must test all of elements before give up
            // TODO sort elements with most common first
            List<HasseNode> L = new List<HasseNode>();
            foreach (HasseNode C in this.Values)
            {
                if (C.HasOneOfElements(Elements))
                {
                    L.Add(C);
                }
            }
            return L;
        }

        */
        public HasseNode NodeWithName(string name)
        {
            foreach (HasseNode N in this.Values)
            {
                if (N.GetName ().Equals (name))
                    return N;
            }
            return null;
        }

        public List<HasseNode> Siblings(HasseNode N, int levels)
        {
            List<HasseNode> L = new List <HasseNode>();
            L.Add(N);
            List<HasseNode> Lowers = LowerNodes(L, levels);
            List<HasseNode> Siblings = HigherNodes(Lowers, levels);
            return Siblings;
        }


        public List<HasseNode> HigherNodes(List<HasseNode> L, int levels)
        // return all nodes from all generations down to Levels depth
        // levels 1 will mean one layer down
        {
            List<HasseNode> TheLayerAbove = new List<HasseNode>();
            foreach (HasseNode N in L)
            {
                foreach (HasseEdge E in N.EdgesToCovering)
                {
                    if (!TheLayerAbove.Contains(E.UpperNode))
                        TheLayerAbove.Add(E.UpperNode);
                }
            }
            if (levels > 1) // have not reached bottom, go deeper
            {
                List<HasseNode> AllLayersAbove = LowerNodes(TheLayerAbove, levels - 1);
                foreach (HasseNode N_ in AllLayersAbove)
                {
                    if (!L.Contains(N_))
                        L.Add(N_);
                }
                return L;
            }
            else
            {  // have reached bottom, return the lower layer together with this layer

                foreach (HasseNode N in TheLayerAbove)
                {
                    if (!L.Contains(N))
                        L.Add(N);
                }
                return L;
            }
        }




        public  List<HasseNode>  LowerNodes( List<HasseNode> L, int levels)
            // return all nodes from all generations down to Levels depth
            // levels 1 will mean one layer down
        {
            List<HasseNode> TheLayerUnder = new List<HasseNode>();
            foreach (HasseNode N in L)
            {
                foreach (HasseEdge E in N.EdgesToCovered)
                {
                    if (!TheLayerUnder.Contains (E.LowerNode ))
                    TheLayerUnder.Add(E.LowerNode);
                }
            }
            if (levels > 1) // have not reached bottom, go deeper
            {
                List<HasseNode> AllLayersUnder = LowerNodes(TheLayerUnder, levels - 1);
                foreach (HasseNode N_ in AllLayersUnder)
                {
                    if (!L.Contains (N_))
                    L.Add(N_);
                }
                return L;
            }
            else
            {  // have reached bottom, return the lower layer together with this layer
                
                foreach (HasseNode N in TheLayerUnder)
                {
                    if (!L.Contains(N))
                        L.Add(N);
                }
                return L;
            }
        }
        
        public List<HasseNode> AllSiblingsTo(HasseNode ReferenceNode)
        {
            // return the objects which are covered by the covers of this
            
            Debug.WriteLine("ref: " + ReferenceNode.KeyString);
            List<HasseNode> l = new List<HasseNode>();
            //loop covering
            foreach (HasseEdge EdgeToCovering in ReferenceNode.EdgesToCovering  )
            {
                HasseNode CoveringNode = EdgeToCovering.UpperNode; 
                Debug.WriteLine("covering: " + CoveringNode.KeyString);
                foreach (HasseEdge EdgeDownToSibling in CoveringNode.EdgesToCovered  )
                {
                    HasseNode Sibling = EdgeDownToSibling.LowerNode; 
                    if (!l.Contains(Sibling))
                    {
                        Debug.WriteLine("Sibling: " + Sibling.KeyString);
                        l.Add(Sibling);
                    }
                }
            }
            return l;

        }



        public void ResetDebugInfo()
        {
            foreach (HasseNode o in this.Values )
            {
                o.debug_CheckedIsLarger = false;
            }
        }


        public bool IsSynchronized
        {
            get { return false; }
        }

        public HasseNode getIdenticalNodeIfExists(HasseNode elm1)
        {

            foreach (HasseNode existingelement in this.Values )
            {
                 if (elm1.KeyString.Equals (existingelement.KeyString  ) )
                {
                    return existingelement;
                }
            }
            return null;
        }

    }

}
