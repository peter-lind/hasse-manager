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
    public abstract class HasseNode : IComparable
    {
        public enum HasseNodeTypes
        {
            // A node can have more than one type, use bitwise AND or OR.
            ROOT = 0,
            ELEMENT = 1,
            FRAGMENT = 2,
            REAL = 4
        }


        public List<HasseEdge> EdgesToCovering = new List<HasseEdge>();
        public List<HasseEdge> EdgesToCovered = new List<HasseEdge>();


        //private HasseNodeCollection m_NodesCovering = new HasseNodeCollection();
        //private HasseNodeCollection m_NodesCovered = new HasseNodeCollection();
        //private HasseNodeCollection m_incomingNodesFrom = new HasseNodeCollection();
        //private HasseNodeCollection m_outgoingNodesTo = new HasseNodeCollection();

        // elementary objects to be instantiated lazily when needed
        private HasseNodeCollection m_elementarysubobjects;

        // passed in to constructor, needed to avoid creation of duplicate objects
        private HasseNodeCollection globalElementCollection;
        public int weight = 0;
        public int x = 0;

        private bool _debug_CheckedIsLarger = false;
        static internal int CountComparisons = 0;
        static internal int WasLargerThan = 0;
        static internal int id = 0;
        internal int MyId;

        internal HasseNodeTypes NodeType;

        public void AddNodeType(HasseNodeTypes NewNodeType)
        { /*set the bit corresponding to NewNodeType */
            this.NodeType = this.NodeType | NewNodeType;
        }

        public bool HasNodeType(HasseNodeTypes NodeTypeToTestFor)
        {
            // use bitwise AND to see if bit of type in question is set
            int tst = (int)(this.NodeType & NodeTypeToTestFor);
            return (tst > 0);
        }

        public bool debug_CheckedIsLarger
        {
            set
            {
                if (value == true & _debug_CheckedIsLarger == true)
                {
                    throw new Exception("checking for > twice");
                }
                _debug_CheckedIsLarger = value;
            }
        }

        public bool validate()
        {
            foreach (HasseEdge cover in EdgesToCovering)
            {
                foreach (HasseEdge _cover in EdgesToCovered)
                {
                    if ((!object.ReferenceEquals(cover.UpperNode, _cover.UpperNode)))
                    {
                        if (cover.UpperNode.IsLargerThan(_cover.UpperNode))
                        {
                            throw new Exception(this.KeyString + " has two comparable covers : " + cover.UpperNode.KeyString + " and " + _cover.UpperNode.KeyString);
                        }
                    }
                }
            }
            foreach (HasseEdge covered in EdgesToCovered)
            {
                foreach (HasseEdge _covered in EdgesToCovered)
                {
                    if (covered.LowerNode.IsLargerThan(_covered.LowerNode))
                    {
                        throw new Exception(this.KeyString + " is covering two comparable objects: " + covered.LowerNode.KeyString + " " + _covered.LowerNode.KeyString);
                    }
                }
            }

            return true;
        }

        /*
        public List<HasseEdge> EdgesUp()
        {
            return _EdgesToCovering;
        }

        public List<HasseEdge> EdgesDown()
        {
            return _EdgesToCovered;
        }
        *)
        /*
        public HasseNodeCollection NodesCovering()
        {
            return (m_NodesCovering);
        }

        public HasseNodeCollection NodesCovered()
        {
            return (m_NodesCovered);
        }
        */
        /*
        public HasseNodeCollection IncomingNodesFrom()
        {
            return (m_incomingNodesFrom);
        }

        public HasseNodeCollection OutgoingNodesTo()
        {
            return (m_outgoingNodesTo);
        }
        */

        public abstract int elementCount();
        public abstract bool ContainsAllElementsIn(HasseNodeCollection col);
        public abstract bool IsIdenticalTo(HasseNode elm);
        public abstract bool IsLargerThan(HasseNode smallobj);
        public abstract string KeyString { get; }
        public abstract void GetMaxCommonFragments(HasseNode Node1, HasseNode Node2, bool dbg, HasseFragmentInsertionList NewEdgeList, HasseNodeCollection GlobalElementCollection, int MinimumOverlap);
        public abstract string[] GetDifferenceString(HasseNode LargerNode);
        protected abstract HasseNodeCollection makeElementarySubobjects(HasseNodeCollection GlobalHasseVertexObjectCollection);

        public HasseNodeCollection getElementarySubobjects()
        {
            if (m_elementarysubobjects == null)
            {
                m_elementarysubobjects = makeElementarySubobjects(globalElementCollection);
            }
            return m_elementarysubobjects;
        }

        // Implement IComparable CompareTo method - provide default sort order.
        int IComparable.CompareTo(object node)
        {
            HasseNode Node = (HasseNode)node;
            return String.Compare(this.KeyString, Node.KeyString);

        }

        public HasseNode(HasseNodeTypes Type, HasseNodeCollection Elements)
        {
            id += 1;
            MyId = id;
            this.NodeType = Type;
            this.globalElementCollection = Elements;
        }
    }

}
