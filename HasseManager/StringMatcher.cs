using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HasseManager
{

    public class StringMatcher
    {
        string StrA = "";
        string StrB = "";
        int offs = 0;
        int pos = 0;
        Match M;

        public void Initialise(string SearchString, string StringA, string StringB)
        {
            M = new Match(SearchString, 0, 0, StringA, 0, 0, StringB);
            M.LastPosInA = M.StrA.Length - 1;
        }

        public void Initialise(string StringA, string StringB)
        {
            StrA = StringA;
            StrB = StringB;
            offs = 1 - StrB.Length-1;
        }

        public Match nextMatch()
        {
            //move last pos if possible, then do. else move offs
            //now note if match. if so, can possibly expand.

            //------0123456789       positions relative beginning strA
            //------STRING1--------
            //------------STRING2--  offs here = (StrA.Length - 1) , in this example +6
            //  String 2 moved by offs, with range (1 -StrB.Length) to (StrA.Length - 1)
            // if positive offset, strings can be compared from offs to (lowest of length(A)-1 and (offset + length(B)-1))
            // if negative offset, strings can be compared from 0 to (lowset of length(A)-1 and length(B)+offset-1)

           // if (StrB.Equals("about")) System.Diagnostics.Debugger.Break(); 

            //if (StrA.Contains("*")) System.Diagnostics.Debugger.Break();   
            do
            {
                offs++;
                bool haveMatch = false;
                int firstpos = 0;
                int lastpos = 0;
                pos = (offs >= 0) ? offs-1 : -1;
                // search for first matching character
                do
                {
                    pos++;
                    if (pos > Math.Min(StrA.Length-1, offs + StrB.Length - 1)) break;
                    //if (StrA[pos].Equals('*')) System.Diagnostics.Debugger.Break ();
                    haveMatch = (StrA[pos].Equals(StrB[pos - offs]) && (!StrA[pos].Equals('*')));
                }
                while (!haveMatch);
                if (!haveMatch) continue;
                firstpos = pos;

                // we have a match. search for first non-matching character
                do
                {
                    pos++;
                    if (pos > Math.Min(StrA.Length-1, offs + StrB.Length - 1)) break;
                    //if (StrA[pos].Equals('*')) System.Diagnostics.Debugger.Break();
                    haveMatch = (StrA[pos].Equals(StrB[pos - offs]) && (!StrA[pos].Equals('*')));
                } while (haveMatch);
                lastpos = pos - 1;
                return new Match("",firstpos, lastpos, StrA, firstpos - offs, lastpos - offs, StrB);
            } while (offs < StrA.Length - 1);
            return null;
        }


        public Match CurrentMatch()
        {
            return M;
        }
        public bool FindNextSubstringMatchNotUsed()
        {
            // finds StrA in B, start look from FirstPosInB
            M.FirstPosInB = M.StrB.IndexOf(M.StrA, M.FirstPosInB);
            if (M.FirstPosInB > -1) { return true; };
            return false;
        }

    }

    public class Match
    {
        public string SearchString;
        public int FirstPosInA;
        public int FirstPosInB;
        public int LastPosInA;
        public int LastPosInB;
        public string StrA;
        public string StrB;

        public Match(string SearchString, int FirstPosInA, int LastPosInA, string StrA, int FirstPosInB, int LastPosInB, string StrB)
        {

            this.FirstPosInA = FirstPosInA;
            this.FirstPosInB = FirstPosInB;
            this.LastPosInA = LastPosInA;
            this.LastPosInB = LastPosInB;
            this.StrA = StrA;
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
            List<string> DiffStringList = new List<string>();
            //+ strSeed.Length
            //A to left - do we have one or more chars to left? last is not star?
            if (FirstPosInA > 0 && (!StrA.Substring(FirstPosInA - 1, 1).Equals("*")))
            {
                string sl1 = StrA.Substring(0, FirstPosInA) + "*";
                DiffStringList.Add(sl1);
            }

            // B to left
            if (FirstPosInB > 0 && (!StrB.Substring(FirstPosInB - 1, 1).Equals("*")))
            {
                string sl2 = StrB.Substring(0, FirstPosInB) + "*";
                DiffStringList.Add(sl2);
            }

            //pos1 right
            if (LastPosInA + 1 < StrA.Length && (!StrA.Substring(LastPosInA + 1, 1).Equals("*")))
            {
                string sr1 = "*" + StrA.Substring(LastPosInA + 1);
                DiffStringList.Add(sr1);
            }
            //pos2 right
            if (LastPosInB + 1 < StrB.Length && (!StrB.Substring(LastPosInB + 1, 1).Equals("*")))
            {
                string sr2 = "*" + StrB.Substring(LastPosInB + 1);
                DiffStringList.Add(sr2);
            }

            return DiffStringList;
        }
    }
}
