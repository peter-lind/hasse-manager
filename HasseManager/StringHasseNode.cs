
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

        private string str = "";
        //private int m_elementcount;
        // private HasseVertexNodeCollection m_elementarysubobjects;
        const bool DEBUG_LABELLED_OBJECTS = true;
        const bool DEBUG_NEW = true;

        public StringHasseNode(string s, HasseNodeTypes ElementType, HasseNodeCollection globalElementCollection)
            : base(ElementType, globalElementCollection)
        {


   
            // TODO remove this check
            if (s.Contains("**")) { throw new Exception("double stars"); }
                

            if (string.IsNullOrEmpty(s))
                throw new Exception("new empty string");
            Debug.WriteLineIf(DEBUG_NEW, "                                          created  " + s);

            str = s;
        }



        public override int elementCount
        {
            get { return getElementarySubobjects().Values.Count; }
        }


        public override void makeMaxCommonSubStruc(HasseNode Node1, HasseNode Node2, bool dbg, ref System.Collections.Queue q, HasseNodeCollection GlobalHasseVertexNodeCollection)
        {
            // this Node is known to be subNode on both Node1 and Node2
            // it can match several places

            string strSeed = this.UniqueString;
            string str1 = Node1.UniqueString;
            string str2 = Node2.UniqueString;
            int pos1 = -1;
            do
            {
                pos1 = str1.IndexOf(strSeed, pos1 + 1);
                if (pos1 > 0)
                {
                    int pos2 = -1;
                    do
                    {
                        pos2 = str2.IndexOf(strSeed, pos2 + 1);
                        if (pos2 > 0)
                        {
                            int leftoffs = 0;
                            do
                            {
                                leftoffs += 1;
                                if (pos1 - leftoffs < 0)
                                    break; // TODO: might not be correct. Was : Exit Do
                                if (pos2 - leftoffs < 0)
                                    break; // TODO: might not be correct. Was : Exit Do
                            } while (!(!str1.Substring(pos1 - leftoffs, 1).Equals(str2.Substring(pos2 - leftoffs, 1))));
                            leftoffs -= 1;


                            int rightoffs = 0;
                            do
                            {
                                rightoffs += 1;
                                if (pos1 + rightoffs >= str1.Length)
                                    break; // TODO: might not be correct. Was : Exit Do
                                if (pos2 + rightoffs >= str2.Length)
                                    break; // TODO: might not be correct. Was : Exit Do
                            } while (!(!str1.Substring(pos1 + rightoffs, 1).Equals(str2.Substring(pos2 + rightoffs, 1))));
                            rightoffs -= 1;

                            string s = str1.Substring(pos1 - leftoffs, 1 + leftoffs + rightoffs);

                            //exit on this trivial case
                            if (s.Equals(this.UniqueString))
                                continue;


                            StringHasseNode newNode = new StringHasseNode(s, HasseNodeTypes.FRAGMENT, GlobalHasseVertexNodeCollection);
                            Debug.WriteLineIf(dbg, " created MCS of " + str1 + " and " + str2 + ": " + s);
                            //arrlistLabelledNodes.Add(newNode)
                            if (newNode.UniqueString.Equals ("*")){System.Diagnostics.Debugger.Break();} 
                            q.Enqueue(newNode);


                        }
                        else
                        {
                            break; // TODO: might not be correct. Was : Exit Do
                        }
                    } while (true);

                }
                else
                {
                    break; // TODO: might not be correct. Was : Exit Do
                }
            } while (true);

        }

       


        public override void makeLabelledNodes(HasseNode Node2, ref System.Collections.Queue q, ref HasseNodeCollection existingNodes)
        {
            //the Node of the argument should be the larger object 
            //can result in several objects, like both *A and A*

            String ShorterString = this.str;        //for readability
            StringHasseNode chrNode2 = (StringHasseNode)Node2;
            string LongerString = chrNode2.str;     //for readability
            String TrimmedShortString = ShorterString.Replace("*", "");


           // if (ShorterString.Contains("*"))
             //   System.Diagnostics.Debugger.Break();    
            
            
            //find position(s) of substring in string og larger Node
            int pos = LongerString.IndexOf(TrimmedShortString);
            //Dim arrlistLabelledObjects As ArrayList = New ArrayList
            Debug.WriteLineIf(DEBUG_LABELLED_OBJECTS, "make labelled objects between " + this.UniqueString + " and " + Node2.UniqueString + " ...");

            while (pos > -1)
            {
                Debug.WriteLineIf(DEBUG_LABELLED_OBJECTS, " pos: " + pos.ToString());

                //str:        X
                //largestr:  AXB
                //pos:       012

                //substring based on ShorterString - add stars as appropriate
                string newstring = TrimmedShortString;

                if (pos > 0)  // star to left?
                    newstring = "*" + newstring;

                if (ShorterString.Length + pos < LongerString.Length) // star to right?
                    newstring = newstring + "*";


                if (!newstring.Equals(ShorterString))  // is it not what we started with?
                {
                    if (!existingNodes.ContainsKey(newstring))
                    {
                        StringHasseNode newNode = new StringHasseNode(newstring, HasseNodeTypes.FRAGMENT, existingNodes);
                        Debug.WriteLineIf(DEBUG_LABELLED_OBJECTS, " created labelled object: " + newstring);
                       if (newNode.UniqueString.Equals ("*")){System.Diagnostics.Debugger.Break();} 
                        q.Enqueue(newNode);
                    }
                }

                // something on left side?
                //substring based on larger string minus smaller - left side
                if (pos > 0)
                {
                    string leftStr = LongerString.Substring(0, pos);
                    if ((leftStr.Length >0) && (!leftStr.Equals("*"))) /*if not empty or a star only*/
                    {
                       // if (!leftStr.StartsWith ("*") )leftStr += "*"; /*then add a star if not already at start*/
                         leftStr += "*"; /*then add a star if not already at start*/
                        if (!existingNodes.ContainsKey(leftStr))
                        {  /*create new node, put this in queue for insert into Hasse diagram*/
                            StringHasseNode newNodeLeft = new StringHasseNode(leftStr, HasseNodeTypes.FRAGMENT, existingNodes);
                            Debug.WriteLineIf(DEBUG_LABELLED_OBJECTS, " created left labelled object: " + leftStr);
                            if (newNodeLeft.UniqueString.Equals ("*")){System.Diagnostics.Debugger.Break();} 
                            q.Enqueue(newNodeLeft);
                        }
                    }
                }


                if (pos + ShorterString.Length   <= (LongerString.Length - 1))  // there are one or more chars on right side
                {
                    string rightStr = LongerString.Substring(pos + ShorterString.Length);
                    if ((rightStr.Length > 0) && (!rightStr.Equals("*"))) /*if not empty or a star only*/
                    {
                       //if (!rightStr.EndsWith ("*") ) rightStr += "*"; /*then add a star if already in end*/
                         rightStr = "*" + rightStr ; /*then add a star if already in end*/
                        if (!existingNodes.ContainsKey(rightStr))
                        {
                            StringHasseNode newNodeRight = new StringHasseNode(rightStr, HasseNodeTypes.FRAGMENT, existingNodes);
                            Debug.WriteLineIf(DEBUG_LABELLED_OBJECTS, " created right labelled object: " + rightStr);
                            if (newNodeRight.UniqueString.Equals("*")) { System.Diagnostics.Debugger.Break(); }
                            q.Enqueue(newNodeRight);
                        }
                    }
                }
                pos = LongerString.IndexOf(TrimmedShortString, pos + 1);
            }
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


        protected override HasseNodeCollection makeElementarySubobjects(HasseNodeCollection GlobalHasseNodeCollection)
        {
            //watch out for infinite recursion as we are called from New and also may call New

            HasseNodeCollection elmobjects = new HasseNodeCollection();
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

            return elmobjects;
        }


        public override bool IsIdenticalTo(HasseNode Node)
        {
            if (this.str.Equals(Node.UniqueString))
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
#if DEBUG
            if (this.UniqueString.Equals("DB") && smallHasseElement.UniqueString.Equals("D"))
            {
                //System.Diagnostics.Debugger.Break();
            }
#endif


            // we require that this is strictly larger than Node2
            const bool DBG = false;
            CountComparisons++;
            if (CountComparisons % 100000 == 0)
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

            if (smallHasseElement.elementCount > this.elementCount)
            {
                // must be smaller
                Debug.WriteLineIf(DBG, this.UniqueString + " is not larger than " + smallHasseElement.UniqueString + " (0)");
                return false;
            }
            if (!this.ContainsAllElementsIn(smallHasseElement.getElementarySubobjects()))
            {
                Debug.WriteLineIf(DBG, this.UniqueString + " is incomparable to, or smaller than " + smallHasseElement.UniqueString + " (I)");
                return false;
            }

            StringHasseNode smallStringHasseNode = (StringHasseNode)smallHasseElement;
            //remove stars    
            string SmallString=smallStringHasseNode.str;
            string StrippedSmallString = SmallString.Replace("*", "");


            bool SmallStringHasStars = true;
            if (StrippedSmallString.Equals(smallStringHasseNode.str))
                SmallStringHasStars = false;


            if (SmallStringHasStars == false)
            {
                if (str.Contains(StrippedSmallString) && StrippedSmallString.Length < str.Length)
                {
                    Debug.WriteLineIf(DBG, this.UniqueString + " is larger than " + smallHasseElement.UniqueString + " (II)");
                    WasLargerThan += 1;
                    return true;
                }
                else
                {
                    Debug.WriteLineIf(DBG, this.UniqueString + " is not larger than " + smallHasseElement.UniqueString + " (III)");
                    return false;
                }
            }


            // small string has stars, different rules
            // find occurrences of small string in large string.
            int offset = 0;
            offset=str.IndexOf(StrippedSmallString);
            while (offset != -1 && (offset <= str.Length)  )
            {

                Boolean IsMatch = TestStringMappingForMatch(str,SmallString, StrippedSmallString, offset);
                if (IsMatch) {
                    Debug.WriteLineIf(DBG, this.UniqueString + " is larger than " + smallHasseElement.UniqueString + " (IV)");
                    WasLargerThan += 1;
                    return true;
                }
                offset = str.IndexOf(StrippedSmallString, offset +1 );
            } 


            Debug.WriteLineIf(DBG, this.UniqueString + " is not larger than " + smallHasseElement.UniqueString + " (V)");
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
            Boolean StarToLeftAndRight = StarToLeft && StarToRight ;
            
                if (StarToLeftAndRight){
                    if (offset != 0) /* something for left star*/
                        if (str.Length > (offset + StrippedSmallString.Length)) /*something for right star*/

                        if (!((str[offset-1].Equals ('*'))&&(str[offset + StrippedSmallString.Length].Equals('*'))))
                        return true;    
                
                }
                else if (StarToLeft) {
                    if (offset != 0)/* something for left star..*/
                    { 
                        if (!str[offset - 1].Equals('*')) /* something other than also star */ return true;
                        /* at this point we know we have a match, but do we have larger than? */
                        if (str.Length >= (offset + SmallString.Length))return true;
                    }
                }
                else if (StarToRight) {
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


        public override string UniqueString
        {
            get { return str; }
        }

        public new string ID
        {
            get { return base.MyId.ToString(); }
        }

    }

}
