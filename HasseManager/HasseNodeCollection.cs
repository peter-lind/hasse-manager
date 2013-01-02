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
        public HasseNodeCollection() : base() { }

        /*
        protected override string GetKeyForItem(HasseElement item)
        {
            return item.UniqueString ;
        }
        */


        public HasseNodeCollection Clone
        {
            get { return (HasseNodeCollection)this.MemberwiseClone(); }
        }

        public List<HasseNode> AllSiblingsTo(HasseNode ReferenceVertex)
        {
            // return the objects which are covered by the covers of this
            
            Debug.WriteLine("ref: " + ReferenceVertex.UniqueString);
            List<HasseNode> l = new List<HasseNode>();
            //loop covering
            foreach (HasseNode covering in ReferenceVertex.NodesCovering().Values )
            {
                Debug.WriteLine("covering: " + covering.UniqueString);
                foreach (HasseNode Sibling in covering.NodesCovered().Values )
                {
                    if (!l.Contains(Sibling))
                    {
                        Debug.WriteLine("Sibling: " + Sibling.UniqueString);
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
