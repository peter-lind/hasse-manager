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
    public class HasseEdge
        // Edge with upper and lower nodes, plus a label
    {
        private int visitCode;
        private HasseNode upperNode;
        private HasseNode lowerNode;
        private HasseNode[] label;
        private string labelText;
        private List<HasseNode > linkedNodes = new List<HasseNode> ();
        public bool Removed = false; // for debug

        public void Visit(int vCode)
        {
            visitCode = vCode;
        }
        public bool IsVisited(int vCode)
        {
            if (visitCode == vCode) return true; else return false;
        }

        public HasseNode UpperNode
        {
            get
            {
                return upperNode;
            }
            set
            {
                upperNode = value;
                if (value == null)
                {
                 //   System.Diagnostics.Debugger.Break();   
                }
            }
        }

        public HasseNode LowerNode
        {
            get
            {
                return lowerNode;
            }
            set
            {
                lowerNode = value;
                if (value == null)
                {
                  //  System.Diagnostics.Debugger.Break();
                }

            }
        }

        public List<HasseNode> LinkedNodes
        {
            get
            {
                return linkedNodes;
            }
        }
        public void  AddLinkedNode(HasseNode LinkedNode)
            {
                linkedNodes.Add(LinkedNode);
            }
       


        public string LabelText
        {
            get
            {
                return labelText;
            }
            set
            {
                labelText = value;
            }
        }


        public HasseEdge() 
        {
            //System.Diagnostics.Debugger.Break();   
        }

    }
    
}
