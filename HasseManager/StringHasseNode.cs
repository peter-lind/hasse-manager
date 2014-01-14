
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




        public StringHasseNode(string s, HasseNodeTypes ElementType, string debugInfo)
            : base(ElementType, debugInfo)
        {

            // TODO remove this check
            if (s.Contains("**")) { throw new Exception("double stars"); }


            if (string.IsNullOrEmpty(s) && (this.NodeType != HasseNode.HasseNodeTypes.ROOT )   )
                throw new Exception("new empty string");
            Debug.WriteLineIf(DEBUG_NEW, "                                          created  " + s);

            // todo remove one of below
            str = s;
            keystring = s;
            base.size = s.Length; 
        }


        private int GetNextMatch(int StartFrom,string SearchFor, string SearchIn)
        {
           int HitPosition= SearchIn.IndexOf(SearchFor, StartFrom);
           return HitPosition;
        }

        public  bool GetMaxCommonFragmentsNotUsed(HasseNode Node1, HasseNode Node2, bool dbg,
            HasseFragmentInsertionQueue NewFragmentList, int MinimumOverlap)
        {
            
            // this Node is directly below both Node1 and Node2
            // it can match several places
            CountMCS ++;    
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
                    string debugInfo = "MCS " + Node1.GetID().ToString () + " " + Node2.GetID().ToString ();  
                    if (true == ProcessMatch(M, MinimumOverlap, NewFragmentList, new HasseNode[2] { Node1, Node2 }, debugInfo))
                    { FoundMCS = true; }
                    MatchPosB = GetNextMatch(MatchPosB +1,strSeed,str2);
                }
                MatchPosA = GetNextMatch(MatchPosA +1,strSeed,str1);
            }
           
            return FoundMCS;
        }
        public override bool GetMaxCommonFragments(HasseNode Node1, HasseNode Node2, bool dbg,
            HasseFragmentInsertionQueue NewFragmentList, int MinimumOverlap)
        {
            CountMCS++;
            string str1 = Node1.KeyString;
            string str2 = Node2.KeyString;
            bool FoundMCS = false;

            StringMatcher sm = new StringMatcher();
            sm.Initialise(str1, str2);
            Match m = null;
            do
            {      
                m = sm.nextMatch();
                if (m == null) break;
                if (m.LastPosInA - m.FirstPosInA < MinimumOverlap-1) continue;
                //System.Diagnostics.Debug.WriteLine(m.StrA.Substring(m.FirstPosInA, m.LastPosInA - m.FirstPosInA + 1));
                //System.Diagnostics.Debug.WriteLine(m.StrB.Substring(m.FirstPosInB, m.LastPosInB - m.FirstPosInB + 1));
                string debugInfo = "MCS " + Node1.GetID().ToString() + " " + Node2.GetID().ToString();
                if (true == ProcessMatch(m, MinimumOverlap, NewFragmentList, new HasseNode[2] { Node1, Node2 }, debugInfo))
                { FoundMCS = true; }

            } while (true);
            return FoundMCS;
        }

        private bool ProcessMatch(Match M, int MinimumOverlap, HasseFragmentInsertionQueue NewFragmentList ,HasseNode [] PartlyMatchingNodes, string debugInfo)
            {
            string StringMCS = M.GetMatchString();
            bool matchWasNew = false;          
            if (StringMCS.Length >= MinimumOverlap)
            {
              //  if (M.StrA.Equals("*t")) System.Diagnostics.Debugger.Break(); 

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
                if (StringMCS.Equals("**")) System.Diagnostics.Debugger.Break();   
                if (!StringMCS.Equals("*"))
                {
                    if (true == NewFragmentList.Add(new HasseNode[1] { this }, PartlyMatchingNodes,
                        StringMCS, debugInfo,
                        HasseNodeTypes.FRAGMENT | HasseNodeTypes.MAX_COMMON_FRAGMENT, null))
                        matchWasNew = true;
                }
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

            /*
            if (smallHasseElement.elementCount() > this.elementCount())
            {
                // must be smaller
                Debug.WriteLineIf(DBG, this.KeyString + " is not larger than " + smallHasseElement.KeyString + " (0)");
                return false;
            }
            */


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

        public  string ID
        {
            get { return base.ID.ToString(); }
        }
    }

}
