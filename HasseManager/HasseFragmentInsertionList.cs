using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
namespace HasseManager
{
    public class HasseFragmentInsertionList: IEnumerable 
    {
        private List<FragmentToBeInserted> FragmentList = new List<FragmentToBeInserted>();
        public void Add( HasseNode[] LowerNodes,HasseNode[] HigherNodes, string NewNodeContent,string Origin)
        {
            FragmentToBeInserted F = new FragmentToBeInserted();
            F.LowerNodes = LowerNodes;
            F.HigherNodes = HigherNodes;
            F.NewNodeContent = NewNodeContent;
            F.Origin = Origin;
            FragmentList.Add(F);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return  FragmentList.GetEnumerator();

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
