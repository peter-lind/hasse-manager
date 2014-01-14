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
            CHEM,
            FINGERPRINTCHEM
        }
        //HasseNodeCollection elementsCollection;

        public HasseNodeFactory(NodeType t)
        {
            nType = t;
         //   elementsCollection = elements;
        }


        public HasseNode NewNode(string s, HasseNode.HasseNodeTypes e, string debugInfo)
            // make node based on string
        {

            switch (nType)
            {
                case NodeType.STRING  :
                    StringHasseNode SN = new StringHasseNode(s, e, debugInfo);
                    return SN;
                case NodeType.CHEM  :
                    ChemHasseNode CN = new ChemHasseNode(s, e,  debugInfo);
                    return CN;
                case  NodeType.FINGERPRINTCHEM :
                    FingerprintChemHasseNode FPC = new FingerprintChemHasseNode(s, e,  debugInfo);
                    return FPC;
                default :
                    throw new Exception ("HasseNodeFactory: unhandled NodeType");
            }
            
        }

        public HasseNode NewNode(object  o, HasseNode.HasseNodeTypes e, string debugInfo)
        {
            // make node based on object of a class that the node implementing class knows about
            switch (nType)
            {
                case NodeType.STRING:
                    StringHasseNode SN = new StringHasseNode((string) o, e, debugInfo);
                    return SN;
                case NodeType.CHEM:
                    ChemHasseNode CN = new ChemHasseNode((IndigoChemistry )o, e,  debugInfo);
                    return CN;
                case NodeType.FINGERPRINTCHEM:
                    FingerprintChemHasseNode FPC = new FingerprintChemHasseNode((IndigoChemistry)o, e,  debugInfo);
                    return FPC;
                default:
                    throw new Exception("HasseNodeFactory: unhandled NodeType");
            }

        }
    }
}
