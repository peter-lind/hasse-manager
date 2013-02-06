
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
using System.Security.Cryptography;

namespace HasseManager
{

    public class StringHasseNode : HasseNode
    {

        //***********************************
        // for testing logic of HasseElement
        // partially orders character strings by substring inclusion, so 'ABC' is larger than 'BC' 

        //***********************************

        static StringMatcher Matcher = new StringMatcher(); 
        static int CountMCS=0;

        private string str = "";
        const bool DEBUG_LABELLED_OBJECTS = false;
        const bool DEBUG_NEW = false;



        public override Int64 HashInt64()
        {
            if (_hash!=0) return _hash;

            int CountNodesAbove = this.EdgesToCovering.Count();
            //int CountAllBits = 0;
            //byte[][] BytesFromAll = new byte[CountNodesAbove+1][];
            //int i = 0;
            Int64 sum = 0;
            if (CountNodesAbove > 0)
            {
                foreach (HasseEdge E in this.EdgesToCovering)
                {
                    Int64 b = E.UpperNode.HashInt64();
                    sum += (b / 10000);
                }

                // important - now shuffle bits in sum
                // otherwise hash from top nodes will eventually be divided away
                byte[] SumBytes = BitConverter.GetBytes(sum);
                byte[] SumHash = new MD5CryptoServiceProvider().ComputeHash(SumBytes);
                sum = BitConverter.ToInt64(SumHash, 1);
            }

            byte[] KeyBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(this.KeyString);
            byte[] KeyHash =new MD5CryptoServiceProvider().ComputeHash(KeyBytes );
            Int64 thisInt64 = BitConverter.ToInt64(KeyHash,1);
            //SystemDiagnostics.Debug(thisInt64.ToString() + " " + this.KeyString );
            sum += (thisInt64 / 10000);
            _hash = sum; // cache this and do not reevaluate
            return sum;
        }

        public StringHasseNode(string s, HasseNodeTypes ElementType, HasseNodeCollection globalElementCollection)
            : base(ElementType, globalElementCollection )
        {

            // TODO remove this check
            if (s.Contains("**")) { throw new Exception("double stars"); }
            if (s.Equals ("~~~~~~"))
            {
                System.Diagnostics.Debugger.Break();
            }


            if (string.IsNullOrEmpty(s) && (this.NodeType != HasseNode.HasseNodeTypes.ROOT )   )
                throw new Exception("new empty string");
            Debug.WriteLineIf(DEBUG_NEW, "                                          created  " + s);

            str = s;
        }

        public override int elementCount()
        {
             return getElementarySubobjects().Count; 
        }

        private int GetNextMatch(int StartFrom,string SearchFor, string SearchIn)
        {
           int HitPosition= SearchIn.IndexOf(SearchFor, StartFrom);
           return HitPosition;
        }

        public override bool GetMaxCommonFragments(HasseNode Node1, HasseNode Node2, bool dbg,
            HasseFragmentInsertionQueue NewFragmentList, int MinimumOverlap)
        {
            
            // this Node is directly below both Node1 and Node2
            // it can match several places
            CountMCS++;    
            string strSeed = this.KeyString.Replace("*", "");
            string str1 = Node1.KeyString;
            string str2 = Node2.KeyString;
            bool FoundMCS = false;    
            // we are only interested in matches strictly larger than seed
            if (strSeed.Length + 1 > MinimumOverlap) { MinimumOverlap = strSeed.Length + 1; }

            int MatchPosA = GetNextMatch(0,strSeed,str1);
            while(MatchPosA>-1){
                int MatchPosB = GetNextMatch(0,strSeed,str2);
                while(MatchPosB>-1){
                    
                    Match M = new Match( strSeed,MatchPosA, 0, str1, MatchPosB, 0, str2);
                    M.ExpandMCSMatch();
                    //MatchPosA= M.LastPosInA;
                    //MatchPosB = M.LastPosInB; 
                    if (true == ProcessMatch(M, MinimumOverlap, NewFragmentList, new HasseNode[2] { Node1, Node2 }))
                    { FoundMCS = true; }
                    MatchPosB = GetNextMatch(MatchPosB +1,strSeed,str2);
                }
                MatchPosA = GetNextMatch(MatchPosA +1,strSeed,str1);
            }
           
            return FoundMCS;
        }



        private bool ProcessMatch(Match M, int MinimumOverlap, HasseFragmentInsertionQueue NewFragmentList ,HasseNode [] PartlyMatchingNodes)
            {
            string StringMCS = M.GetMatchString();
            bool matchWasNew = false;          
            if (StringMCS.Length >= MinimumOverlap)
            {
                // deal with the max common substructure:
                // star to left? Both strings must then have one pos matching star
                if (M.FirstPosInA  > 0 && M.FirstPosInB  > 0)
                { StringMCS = "*" + StringMCS; }
                // star to right? Both strings must then have one pos matching star
                if ((M.LastPosInA < M.StrA.Length - 1) && (M.LastPosInB < M.StrB.Length - 1))
                {
                    StringMCS = StringMCS + "*";
                }

                // Do not return back what was started from:
                if (StringMCS.Equals(PartlyMatchingNodes[0].KeyString) | StringMCS.Equals(PartlyMatchingNodes[1].KeyString ))
                {
                    return false;
                }
 
                if (StringMCS.Equals("~~~~")) { System.Diagnostics.Debugger.Break(); } // TODO remove
                if (true == NewFragmentList.Add(new HasseNode[1] { this }, PartlyMatchingNodes,
                    StringMCS, "MCS",
                    HasseNodeTypes.FRAGMENT | HasseNodeTypes.MAX_COMMON_FRAGMENT, null))
                    matchWasNew = true;
            }
            //if (matchWasNew) { System.Diagnostics.Debugger.Break(); }
            return matchWasNew;
        }


        public override string[] GetDifferenceString(HasseNode LargerNode){
            List<string> DiffList = new List<string>() ;
            string SmallString = this.KeyString.Replace("*", "");
            int MatchPosFirst = GetNextMatch(0, SmallString, LargerNode.KeyString);
            int MatchposLast = MatchPosFirst + SmallString.Length-1; 
                        while (MatchPosFirst > -1)
                        {
                            string strL = LargerNode.KeyString.Substring(0, MatchPosFirst );
                            if (strL.Length > 0 && (!strL.Equals("*")))
                            {
                                // last char in small string matches (non-star) character ?
                                if (LargerNode.KeyString.Length > strL.Length && (!LargerNode.KeyString.Substring(strL.Length, 1).Equals("*")))
                                    strL = strL + "*" ;
                                    DiffList.Add(strL);
                            }
                            string strR = LargerNode.KeyString.Substring(MatchPosFirst + SmallString.Length);
                            if (strR.Length > 0 && (!strR.Equals("*")))
                            {
                                if ((MatchPosFirst + SmallString.Length) > 0 && (!LargerNode.KeyString.Substring(MatchPosFirst + SmallString.Length - 1, 1).Equals("*")))
                                    strR = "*" + strR;
                                DiffList.Add(strR);
                            }
                            MatchPosFirst = GetNextMatch(MatchPosFirst + 1, SmallString, LargerNode.KeyString);
                        }
            //return new string[]{};
            string[] s = DiffList.ToArray();
            return s;
        }




        public override bool ContainsAllElementsIn(HasseNodeCollection col)
        {
            foreach (StringHasseNode n in col.Values)
            {
                if (!this.getElementarySubobjects().Values.Contains(n))
                    return false;
            }
            return true;
        }


        protected override Dictionary<string, HasseNode> makeElementarySubobjects(HasseNodeCollection GlobalHasseNodeCollection)
        {
            //watch out for infinite recursion as we are called from New and also may call New

            Dictionary<string, HasseNode> elmobjects = new Dictionary<string, HasseNode>();
            if (this.NodeType == HasseNodeTypes.ROOT  ) return (elmobjects);
            int i = 0;
            string str2 = str.Replace("*", "");
            for (i = 0; i <= str2.Length - 1; i++)
            {
                string buf = str2.Substring(i, 1);

                //get existing reference to this elm or create new if not exist 
                StringHasseNode element = null;
                if (GlobalHasseNodeCollection.ContainsKey(buf))
                {
                    element = (StringHasseNode)GlobalHasseNodeCollection[buf];
                }
                else
                {
                    element = new StringHasseNode(buf, HasseNodeTypes.ELEMENT, GlobalHasseNodeCollection);
                    GlobalHasseNodeCollection.Add(buf, element);
                }

                //todo change to better names for elements collections
                if (!elmobjects.ContainsKey(buf))
                    elmobjects.Add(buf, element);
            }
            // if only one elementary object, then use same object for this (as item) and that (as element) 
            if (elmobjects.Count == 1 && str.Length ==1 && str[0]!='*') {
                elmobjects.Clear();
                elmobjects.Add(this.KeyString , this);
                this.AddNodeType(HasseNodeTypes.ELEMENT);
            }

            return elmobjects;
        }


        public override bool IsIdenticalTo(HasseNode Node)
        {
            if (this.str.Equals(Node.KeyString))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public override bool IsLargerThan(HasseNode smallHasseElement)

        {
            if (this.NodeType == HasseNodeTypes.ROOT) return false;           
            if (smallHasseElement.NodeType == HasseNodeTypes.ROOT) return true;
 
#if DEBUG
            if (this.KeyString.Equals("*en*") && smallHasseElement.KeyString.Contains("en"))
            {
              //  System.Diagnostics.Debugger.Break();
            }
#endif


            // we require that this is strictly larger than Node2
            const bool DBG = false;
            CountComparisons++;
            if (CountComparisons % 10000 == 0)
            {
                System.Diagnostics.Debug.WriteLine("comparisons:     \t" + CountComparisons.ToString() + "\t of which where '>';\t" + WasLargerThan.ToString());
            }

            // rules:
            // B  <  ABC
            // BC <  *BC 
            // BC <> *B* 
            // A !> A
            // *A < BA
            // *A !> *A

            //with stars taken away Node must be a substring or identical
            //also something must correspond to the stars
            //we test all matchings 

            if (smallHasseElement.elementCount() > this.elementCount())
            {
                // must be smaller
                Debug.WriteLineIf(DBG, this.KeyString + " is not larger than " + smallHasseElement.KeyString + " (0)");
                return false;
            }


            StringHasseNode smallStringHasseNode = (StringHasseNode)smallHasseElement;
            //remove stars    
            string SmallString = smallStringHasseNode.str;
            string StrippedSmallString = SmallString.Replace("*", "");


            bool SmallStringHasStars = true;
            if (StrippedSmallString.Equals(smallStringHasseNode.str))
                SmallStringHasStars = false;


            if (SmallStringHasStars == false)
            {
                if (str.Contains(StrippedSmallString) && StrippedSmallString.Length < str.Length)
                {
                    Debug.WriteLineIf(DBG, this.KeyString + " is larger than " + smallHasseElement.KeyString + " (II)");
                    WasLargerThan += 1;
                    return true;
                }
                else
                {
                    Debug.WriteLineIf(DBG, this.KeyString + " is not larger than " + smallHasseElement.KeyString + " (III)");
                    return false;
                }
            }


            // small string has stars, different rules
            // find occurrences of small string in large string.
            int offset = 0;
            offset = str.IndexOf(StrippedSmallString);
            while (offset != -1 && (offset <= str.Length))
            {

                Boolean IsMatch = TestStringMappingForMatch(str, SmallString, StrippedSmallString, offset);
                if (IsMatch)
                {
                    Debug.WriteLineIf(DBG, this.KeyString + " is larger than " + smallHasseElement.KeyString + " (IV)");
                    WasLargerThan += 1;
                    return true;
                }
                offset = str.IndexOf(StrippedSmallString, offset + 1);
            }


            Debug.WriteLineIf(DBG, this.KeyString + " is not larger than " + smallHasseElement.KeyString + " (V)");
            return false;
        }



        private bool TestStringMappingForMatch(string str, string SmallString, string StrippedSmallString, int offset)
        {
#if DEBUG
            if (str.Equals("BBDA*") && SmallString.Equals("AB*"))
            {
                System.Diagnostics.Debugger.Break();
            }
#endif


            Boolean StarToLeft = SmallString.StartsWith("*");
            Boolean StarToRight = SmallString.EndsWith("*");
            Boolean StarToLeftAndRight = StarToLeft && StarToRight;

            if (StarToLeftAndRight)
            {
                if (offset != 0) /* something for left star*/
                    if (str.Length > (offset + StrippedSmallString.Length)) /*something for right star*/

                        if (!((str[offset - 1].Equals('*')) && (str[offset + StrippedSmallString.Length].Equals('*'))))
                            return true;

            }
            else if (StarToLeft)
            {
                if (offset != 0)/* something for left star..*/
                {
                    if (!str[offset - 1].Equals('*')) /* something other than also star */ return true;
                    /* at this point we know we have a match, but do we have larger than? */
                    if (str.Length >= (offset + SmallString.Length)) return true;
                }
            }
            else if (StarToRight)
            {
                if (str.Length > (offset + StrippedSmallString.Length))/*something for right star.. */
                {
                    if (!str[offset + StrippedSmallString.Length].Equals('*')) /* something other than also star*/
                        return true;
                    /* at this point we know we have a match, but do we have larger than? */
                    if (offset > 0) return true;
                }
            }

            return false;
        }


        public override string KeyString
        {
            get { return str; }
        }

        public  string ID
        {
            get { return base.MyId.ToString(); }
        }


        public class StringMatcher
        {
            Match M;
            public void Initialise(string SearchString,  string StringA, string StringB)
            {
                M = new Match(SearchString , 0 ,0,StringA ,0,0 ,StringB   );
                M.LastPosInA = M.StrA.Length - 1; 
            }

            public Match CurrentMatch()
            {
                return M;
            }
            public bool  FindNextSubstringMatch()
            {
                M.FirstPosInB = M.StrB.IndexOf(M.StrA, M.FirstPosInB);
                if (M.FirstPosInB > -1) { return true; };
                return false;
            }

        }


        public class Match {
            public string SearchString;
            public int FirstPosInA;
            public int FirstPosInB;
            public int LastPosInA;
            public int LastPosInB;
            public string StrA;
            public string StrB;

            public Match(string SearchString, int FirstPosInA, int LastPosInA, string StrA,  int FirstPosInB, int LastPosInB, string StrB)
            {
                
                this.FirstPosInA = FirstPosInA;
                this.FirstPosInB = FirstPosInB;
                this.LastPosInA = LastPosInA;
                this.LastPosInB = LastPosInB;
                this.StrA =StrA ;
                this.StrB = StrB;
                this.SearchString = SearchString;
        }
           


            public void ExpandMCSMatch()
            {
                // we already know this describes a match, but we started with a seed possibly
                // smaller than full matching region, try to expand to left and right...
                int leftoffs = 0;
                do
                { // perhaps there is match also to the left of where seed and str1/str2 match ?
                    leftoffs += 1;
                    if (FirstPosInA - leftoffs < 0) //before start of string 1
                        break;
                    if (FirstPosInB - leftoffs < 0) //before start of string 2
                        break;
                } while (StrA.Substring(FirstPosInA - leftoffs, 1).Equals(StrB.Substring(FirstPosInB - leftoffs, 1)));
                leftoffs -= 1; //move back one step to where it last was ok


                
                int rightoffs = 0;
                do
                { // perhaps there is match also to the right of where seed and str1/str2 match ?
                    rightoffs += 1;
                    if (FirstPosInA + rightoffs >= StrA.Length) // after end of string 1
                        break;
                    if (FirstPosInB + rightoffs >= StrB.Length) // after end of string 2
                        break;
                } while (StrA.Substring(FirstPosInA + rightoffs, 1).Equals(StrB.Substring(FirstPosInB + rightoffs, 1)));
                rightoffs -= 1; //move back one step to where it last was ok
           //     if (StrA.StartsWith("beg") && StrB.StartsWith("beg"))
             //       System.Diagnostics.Debugger.Break();   

                // adjust match object
                LastPosInA = FirstPosInA + rightoffs;
                LastPosInB = FirstPosInB + rightoffs;

                FirstPosInA -= leftoffs;
                FirstPosInB -= leftoffs;
            }

            public string GetMatchString()
            {
                return StrA.Substring(FirstPosInA, MatchStringLength());
            }
         
            public int MatchStringLength()
            {
                return LastPosInA - FirstPosInA + 1; 
            }

            public List<string> DifferenceStrings()
            {
                List<string> DiffStringList = new List<string >();
                //+ strSeed.Length
                //A to left - do we have one or more chars to left? last is not star?
                if (FirstPosInA >0  && (! StrA.Substring(FirstPosInA  - 1, 1).Equals("*")))
                {
                    string sl1 = StrA.Substring(0, FirstPosInA ) + "*";
                    DiffStringList.Add(sl1);
                }

                // B to left
                if (FirstPosInB > 0 && (!StrB.Substring(FirstPosInB - 1, 1).Equals("*")))
                {
                    string sl2 = StrB.Substring(0, FirstPosInB) + "*";
                    DiffStringList.Add(sl2);
                }

                //pos1 right
                if (LastPosInA +1 < StrA.Length && (!StrA.Substring(LastPosInA+1,1  ).Equals ("*")    ))
                {
                    string sr1 = "*" + StrA.Substring(LastPosInA + 1);
                    DiffStringList.Add(sr1);
                }
                //pos2 right
                if (LastPosInB +1 < StrB.Length && (!StrB.Substring(LastPosInB+1,1  ).Equals ("*")    ) )
                {
                    string sr2 = "*" + StrB.Substring(LastPosInB + 1);
                    DiffStringList.Add(sr2);
                }

                return DiffStringList; 
            }
        }

    }

}
