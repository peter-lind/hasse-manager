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
    public abstract class HasseNode
    {
        public enum HasseNodeTypes
        {
            // A node can have more than one type, use bitwise AND or OR.
            ELEMENT = 1,
            FRAGMENT = 2,
            REAL = 4
        }

        //private HasseVertexObjectCollection allElements;
        private HasseNodeCollection m_NodesCovering = new HasseNodeCollection();
        private HasseNodeCollection m_NodesCovered = new HasseNodeCollection();
        private HasseNodeCollection m_incomingNodesFrom = new HasseNodeCollection();
        private HasseNodeCollection m_outgoingNodesTo = new HasseNodeCollection();

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
            int tst= (int )(this.NodeType & NodeTypeToTestFor);
            return (tst>0);
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
            foreach (HasseNode cover in m_NodesCovering.Values)
            {
                foreach (HasseNode _cover in m_NodesCovering.Values)
                {
                    if ((!object.ReferenceEquals(cover, _cover)))
                    {
                        if (cover.IsLargerThan(_cover))
                        {
                            throw new Exception(this.UniqueString + " has two comparable covers : " + cover.UniqueString + " and " + _cover.UniqueString);
                        }
                    }
                }
            }
            foreach (HasseNode covered in m_NodesCovered.Values)
            {
                foreach (HasseNode _covered in m_NodesCovering.Values)
                {
                    if (covered.IsLargerThan(_covered))
                    {
                        throw new Exception(this.UniqueString + " is covering two comparable objects: " + covered.UniqueString + " " + _covered.UniqueString);
                    }
                }
            }

            return true;
        }

        public HasseNodeCollection NodesCovering()
        {
            return (m_NodesCovering);
        }

        public HasseNodeCollection NodesCovered()
        {
            return (m_NodesCovered);
        }

        public HasseNodeCollection IncomingNodesFrom()
        {
            return (m_incomingNodesFrom);
        }

        public HasseNodeCollection OutgoingNodesTo()
        {
            return (m_outgoingNodesTo);
        }

        public abstract int elementCount { get; }
        public abstract bool ContainsAllElementsIn(HasseNodeCollection col);

        //Public MustOverride Sub makeElementarySubobjects(ByVal existingelements As HasseVertexNodeCollection)

        // distinct objects, no duplicates 
        //cannot rely absolutely on keys for chemistry using accord

        public abstract bool IsIdenticalTo(HasseNode elm);
        public abstract bool IsLargerThan(HasseNode smallobj);
        public abstract string UniqueString { get; }
        public abstract void makeLabelledObjects(HasseNode Node2, ref System.Collections.Queue q, ref HasseNodeCollection existingNodes);
        public abstract void makeMaxCommonSubStruc(HasseNode Node1, HasseNode Node2, bool dbg, ref System.Collections.Queue q, HasseNodeCollection GlobalHasseVertexNodeCollection);
        protected abstract HasseNodeCollection makeElementarySubobjects(HasseNodeCollection GlobalHasseVertexObjectCollection);

        public HasseNodeCollection getElementarySubobjects()
        {
            if (m_elementarysubobjects == null)
            {
                m_elementarysubobjects = makeElementarySubobjects(globalElementCollection);
            }
            return m_elementarysubobjects;
        }



        public HasseNode(HasseNodeTypes Type, HasseNodeCollection globalElementCollection)
        {
            id += 1;
            MyId = id;
            this.globalElementCollection = globalElementCollection;
            this.NodeType = Type;
        }
    }

}
