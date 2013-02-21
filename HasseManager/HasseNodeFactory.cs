using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HasseManager
{
   public  class HasseNodeFactory
    {
        NodeType nType;
        public enum NodeType
        {
            STRING,
            CHEM
        }
        HasseNodeCollection elementsCollection;

        public HasseNodeFactory(NodeType t, HasseNodeCollection elements)
        {
            nType = t;
            elementsCollection = elements;
        }

        public HasseNode NewNode(string s, HasseNode.HasseNodeTypes e)
        {
            
            switch (nType)
            {
                case NodeType.STRING  :
                    StringHasseNode SN = new StringHasseNode(s, e, elementsCollection);
                    return SN;
                case NodeType.CHEM  :
                    ChemHasseNode CN = new ChemHasseNode(s, e, elementsCollection);
                    return CN;
                default :
                    throw new Exception ("HasseNodeFactory: unhandled NodeType");
            }
            
        }
    }
}
