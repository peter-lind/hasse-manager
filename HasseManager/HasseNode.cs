﻿/*
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
using System.Security.Cryptography;
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
            MAX_COMMON_FRAGMENT = 4,
            DIFFERENCE_FRAGMENT = 8,
            REAL = 16
        }

        public List<HasseEdge> linkedEdges = new List<HasseEdge>();
        public List<HasseEdge> EdgesToCovering = new List<HasseEdge>();
        public List<HasseEdge> EdgesToCovered = new List<HasseEdge>();

        // elementary objects to be instantiated lazily when needed
        private Dictionary<string, HasseNode> m_elementarysubobjects;

        // passed in to constructor, needed to avoid creation of duplicate objects
        private HasseNodeCollection globalElementCollection;
        public bool AdditionCompleted = false;
        public int w = 0;
        public int x = 0;
        private int vCode = 0;

        private bool _debug_CheckedIsLarger = false;
        static internal int CountComparisons = 0;
        static internal int WasLargerThan = 0;
        static internal int id = 0;
        protected int MyId;
        protected Int64 _hash;

        internal HasseNodeTypes NodeType;

        public bool IsVisited(int c)
        {
            if (vCode == c) return true; else return false;
        }
        public int Weight()
        // return total count leafs above 
        {
            if (w != 0) return w;
            if (true == this.IsLeafNode()) return 1;
            int sum = 0;
            foreach (HasseEdge E in this.EdgesToCovering)
            {
                sum += E.UpperNode.Weight();
            }
            return sum;
        }
        public static int GetRandomInt()
        {
             System.Random R = new Random();
              return R.Next();
        }
        public void GetThisAndAllAbove(List<HasseNode> L, int visitCode)
        {
            if (visitCode == 0)
            {
                visitCode = GetRandomInt(); 
            }

            if (IsVisited(visitCode)) return;

            foreach (HasseEdge E in this.EdgesToCovering)
            {
                E.UpperNode.GetThisAndAllAbove(L, visitCode);
            }
            L.Add(this);
            vCode = visitCode; // mark as visited
        }
        public void GetThisAndAllBelow(List<HasseNode> L, int visitCode)
        {
            if (visitCode == 0)
            {
                System.Random R = new Random();
                visitCode = R.Next();
            }

            if (IsVisited(visitCode)) return;

            foreach (HasseEdge E in this.EdgesToCovered)
            {
                E.LowerNode.GetThisAndAllBelow(L, visitCode);
            }
            L.Add(this);
            vCode = visitCode; // mark as visited
        }

        public void AddLinkToEdge(HasseEdge E)
        {
            linkedEdges.Add(E); // to this node, add ref to edge
            E.LinkedNodes.Add(this); // to the edge, add ref to this node
        }
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

        public bool IsLeafNode()
        {
            if (this.EdgesToCovering.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
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

        public abstract int elementCount();
        //public abstract bool ContainsAllElementsIn(HasseNodeCollection col);
        //public abstract bool IsIdenticalTo(HasseNode elm);
        public abstract bool IsLargerThan(HasseNode smallobj);
        public abstract string KeyString { get; }
      //  public abstract Int64 HashInt64();
        public abstract bool GetMaxCommonFragments(HasseNode Node1, HasseNode Node2, bool dbg, HasseFragmentInsertionQueue NewFragmentList, int MinimumOverlap);
        public abstract string[] GetDifferenceString(HasseNode LargerNode);
        protected abstract Dictionary<string, HasseNode> makeElementarySubobjects(HasseNodeCollection GlobalHasseVertexObjectCollection);


        public  bool ContainsAllElementsIn(HasseNodeCollection col)
        {
            foreach (HasseNode n in col.Values)
            {
                if (!getElementarySubobjects().Values.Contains(n))
                    return false;
            }
            return true;
        }

        public Dictionary<string, HasseNode> getElementarySubobjects()
        {
            if (m_elementarysubobjects == null)
            {
                m_elementarysubobjects = makeElementarySubobjects(globalElementCollection);
            }
            return m_elementarysubobjects;
        }

        public bool HasElement(HasseNode N)
        {
            if (m_elementarysubobjects.ContainsKey(N.KeyString)) { return true; } else { return false; }
        }
        // Implement IComparable CompareTo method - provide default sort order.
        int IComparable.CompareTo(object node)
        {
            HasseNode Node = (HasseNode)node;
            return String.Compare(this.KeyString, Node.KeyString);
        }

        public bool HasElements(HasseNode[] Nodes)
        {
            foreach (HasseNode N in Nodes) // do this have all of these?
            {
                if (!this.HasElement(N))
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasOneOfElements(HasseNode[] Nodes)
        {
            foreach (HasseNode N in Nodes)
            {
                if (this.HasElement(N))
                {
                    return true;
                }
            }
            return false;
        }



        public HasseNode(HasseNodeTypes Type, HasseNodeCollection Elements)
        {
            id += 1;
            MyId = id;
            this.NodeType = Type;
            this.globalElementCollection = Elements;
        }

        public string HashString()
        {
            //System.Text.Encoding enc = System.Text.Encoding.UTF8 ;
            //string myString = enc.GetString(this.Hash());
            //return myString;
            return HashInt64().ToString();
        }

        public List<HasseNode> GetSiblings()
        {
            List<HasseNode> L = new List<HasseNode>();
            foreach (HasseEdge EdgeDown in this.EdgesToCovered)
            {
                HasseNode LowerNode = EdgeDown.LowerNode;
                foreach (HasseEdge EdgeUp in LowerNode.EdgesToCovering)
                {
                    if (this != EdgeUp.UpperNode)
                        L.Add(EdgeUp.UpperNode);
                }
            }
            return L;
        }

        public Int64 HashInt64()
        {
            if (_hash != 0) return _hash;
            int CountNodesAbove = this.EdgesToCovering.Count();
            Int64 sum = 0;
            if (CountNodesAbove > 0) // make a sum of hashes from nodes above
            {
                foreach (HasseEdge E in this.EdgesToCovering)
                {
                    Int64 b = E.UpperNode.HashInt64();
                    sum += (b / 10000);
                }

                // IMPORTANT - now shuffle bits in sum
                // otherwise hash from top nodes will eventually be divided away
                byte[] SumBytes = BitConverter.GetBytes(sum); // bytes from the Int64
                byte[] SumHash = new MD5CryptoServiceProvider().ComputeHash(SumBytes); // hash the hash
                sum = BitConverter.ToInt64(SumHash, 1); // back to Int64
            }

            // so far hash is only based on nodes above. NOw add part from this
            byte[] KeyBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(this.KeyString);
            byte[] KeyHash = new MD5CryptoServiceProvider().ComputeHash(KeyBytes);
            Int64 thisInt64 = BitConverter.ToInt64(KeyHash, 1);
            //SystemDiagnostics.Debug(thisInt64.ToString() + " " + this.KeyString );
            sum += (thisInt64 / 10000);
            _hash = sum; // cache this and do not reevaluate
            return sum;
        }

    }

}
