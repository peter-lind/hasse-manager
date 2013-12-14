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
    public static class HasseDiagramStats
    {
        public static String Report(HasseNodeCollection nodes, HasseNode rootNode)
        {
            int countLeaves = 0;
            int countReals = 0;
            foreach (HasseNode node in nodes.Values)
            {
                if (node.IsLeafNode()) countLeaves++;
                if (node.HasNodeType (HasseNode.HasseNodeTypes.REAL)) countReals ++;
            }
  
            StringBuilder sb = new StringBuilder();
  
            sb.AppendLine("Leaf count: " + countLeaves.ToString () );
            sb.AppendLine("REAL count: " + countReals.ToString () );

            // how selective are the root nodes?
            // the average root node, how many parents, of all?


            List<HasseNodeCollection> NodesInLevel = new List<HasseNodeCollection>();
            int CountUpwardEdges = 0;
            int CountLeafNodes = 0;
            int countAllLeaves = 0;
            HasseNodeCollection NodesOnThisLevel = new HasseNodeCollection();
            NodesOnThisLevel.Add(rootNode.KeyString , rootNode); 

            HasseNodeCollection NodesOnThisLevelPlusOne = new HasseNodeCollection();

            for (int level = 1; ;level++ )
            {
                CountUpwardEdges = 0;
                CountLeafNodes = 0;
                foreach (HasseNode node in NodesOnThisLevel.Values)
                {
                    node.LevelFromRoot = level;
                    foreach (HasseEdge EdgeUpToParent in node.EdgesToCovering )
                    {
                        HasseNode Parent = EdgeUpToParent.UpperNode;
                        CountUpwardEdges++;
                        if (!NodesOnThisLevelPlusOne.ContainsKey(Parent.KeyString))
                        {
                            NodesOnThisLevelPlusOne.Add(Parent.KeyString, Parent);
                        }
                    }
                    if (node.EdgesToCovering.Count == 0)
                    { CountLeafNodes++; countAllLeaves++; }
                }

                sb.AppendLine("at level " + level.ToString() + ";\tnodes: " + NodesOnThisLevel.Count.ToString() + "\tsum upward edges: " +  CountUpwardEdges.ToString() + "\t count leaf nodes: " +  CountLeafNodes.ToString());

                NodesInLevel.Add(NodesOnThisLevel);
               if (NodesOnThisLevelPlusOne.Count ==0) {break;} 
                NodesOnThisLevel = NodesOnThisLevelPlusOne;
                NodesOnThisLevelPlusOne = new  HasseNodeCollection();
            }
            sb.AppendLine("total node count: " + nodes.Count.ToString());
            sb.AppendLine("total leaves count: " + countAllLeaves.ToString());

            return sb.ToString ();
        }
    }
}
