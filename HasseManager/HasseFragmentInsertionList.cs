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
using System.Collections;
namespace HasseManager
{
    public class HasseFragmentInsertionList: Queue <FragmentToBeInserted > 
    {
       // private Queue<FragmentToBeInserted> FragmentList = new Queue<FragmentToBeInserted>();
        public void Add( HasseNode[] LowerNodes,HasseNode[] HigherNodes, string NewNodeContent,string Origin)
        {
            FragmentToBeInserted F = new FragmentToBeInserted();
            F.LowerNodes = LowerNodes;
            F.HigherNodes = HigherNodes;
            F.NewNodeContent = NewNodeContent;
            F.Origin = Origin;
            this.Enqueue (F);
        }
    }
    public class FragmentToBeInserted
    {
        public HasseNode[] LowerNodes;
        public String NewNodeContent;
        public HasseNode[] HigherNodes;
        public string Origin;
    }

}
