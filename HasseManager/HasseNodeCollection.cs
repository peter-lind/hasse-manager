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
                if (elm1.IsIdenticalTo(existingelement))
                {
                    return existingelement;
                }
            }
            return null;
        }

    }

}
