using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.ggasoftware.indigo;

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


        public HasseNode NewNode(string s, HasseNode.HasseNodeTypes e, string debugInfo)
        {
            //if (s.Contains("xyz")) System.Diagnostics.Debugger.Break();   

            switch (nType)
            {
                case NodeType.STRING  :
                    StringHasseNode SN = new StringHasseNode(s, e, elementsCollection,debugInfo);
                    return SN;
                case NodeType.CHEM  :
                    ChemHasseNode CN = new ChemHasseNode(s, e, elementsCollection, debugInfo);
                    return CN;
                default :
                    throw new Exception ("HasseNodeFactory: unhandled NodeType");
            }
            
        }

        public HasseNode NewNode(object  o, HasseNode.HasseNodeTypes e, string debugInfo)
        {
            //if (s.Contains("xyz")) System.Diagnostics.Debugger.Break();   

            switch (nType)
            {
                case NodeType.STRING:
                    StringHasseNode SN = new StringHasseNode((string) o, e, elementsCollection, debugInfo);
                    return SN;
                case NodeType.CHEM:
                    ChemHasseNode CN = new ChemHasseNode((IndigoObject )o, e, elementsCollection, debugInfo);
                    return CN;
                default:
                    throw new Exception("HasseNodeFactory: unhandled NodeType");
            }

        }


    }
}
