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
using System.Security.Cryptography;
namespace HasseManager
{
    public abstract class HasseNode : IComparable
    {
        public enum HasseNodeTypes
        {
            // A node can have more than one type, use bitwise AND or OR.
            ROOT = 1,
            //ELEMENT = 2,
            FRAGMENT = 4,
            MAX_COMMON_FRAGMENT = 8,
            DIFFERENCE_FRAGMENT = 16,
            REAL = 32
        }

        public List<HasseEdge> linkedEdges = new List<HasseEdge>();
        public List<HasseEdge> EdgesToCovering = new List<HasseEdge>();
        public List<HasseEdge> EdgesToCovered = new List<HasseEdge>();

        //public HasseFingerprint BitArrayFingerPrint;

        // passed in to constructor, needed to avoid creation of duplicate objects
        public string DrawingColor = "";
        public int w = 0;
        public int x = 0;
        private int vCode = 0;
        protected bool _isvalid = false;
        private bool _debug_CheckedIsLarger = false;
        static internal int CountComparisons = 0;
        static internal int WasLargerThan = 0;
        static internal int counterForID = 0;
        static internal System.Random R = new Random() ;
        protected int ID;
        protected int size;
        protected string Name;
        protected string keystring;
        protected Int64 _hash;
        public string ImageFileName = "";
        public string LabelText;

        internal HasseNodeTypes NodeType;

        public HasseNode(HasseNodeTypes Type,  string debugInfo)
        {
            counterForID += 1;
            ID = counterForID;
            this.NodeType = Type;
            if (debugInfo.Length == 0)
                debugInfo = ID.ToString();
            this.LabelText = debugInfo;
        }


        public int LevelFromRoot=0;

        public int GetID()
        {
            return ID;
        }

        public bool IsValid()
        {
            return _isvalid;
        }

        public virtual int Size()
        {
            return size;
        }

        public virtual string GetName()
        {
            return Name;
        }
        public virtual void SetName(string n)
        {
            Name = n;
        }
        

        public bool IsVisited(int c)
        {
            if (vCode == c) return true; else return false;
        }


        public int Weight()
        {
            int visitCode = GetRandomInt();
            return Weight(visitCode);
        }

        public int Weight(int visitCode)
        // return total count leafs above 
        {
            if (IsVisited(visitCode)) {return 0; }// I have been counted already - do not count me again
            vCode = visitCode; // mark as visited
            int sum = 0;
            //if (true == this.IsLeafNode()) sum++;           // count me now
            if (this.HasNodeType ( HasseNodeTypes.REAL ) )sum++;           // count me now
            foreach (HasseEdge E in this.EdgesToCovering)
            {
                sum += E.UpperNode.Weight(visitCode );
            }
            return sum;
        }

        public static int GetRandomInt()
        {
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
                foreach (HasseEdge _cover in EdgesToCovering)
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

       // public abstract int elementCount();
        public abstract bool IsLargerThan(HasseNode smallobj);
        public abstract bool GetMaxCommonFragments(HasseNode Node1, HasseNode Node2, bool dbg, HasseFragmentInsertionQueue NewFragmentList, int MinimumOverlap);
        public abstract string[] GetDifferenceString(HasseNode LargerNode);


        public string KeyString
        {
            get { return keystring; }
        }

        // Implement IComparable CompareTo method - provide default sort order.
        int IComparable.CompareTo(object node)
        {
            HasseNode Node = (HasseNode)node;
            return String.Compare(this.KeyString, Node.KeyString);
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

        // require derived classes can create empty fingerprints
        //public abstract HasseFingerprint CreateFingerprint();
    }

}
