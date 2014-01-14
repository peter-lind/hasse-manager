using System;

namespace HasseManager
{

    public class FingerPrintChemHasseNode : HasseNode
    {

        public FingerPrintChemHasseNode(string StrMolecule, HasseNodeTypes ElementType, HasseNodeCollection globalElementCollection, string debugInfo)
            : base(ElementType, globalElementCollection, debugInfo)
        {
            ChemObject = new IndigoChemistry();
            if (ElementType == HasseNodeTypes.ROOT)
            {
                this.keyString = "0";
            }
            else
            {
                ChemObject.InitMolecule(StrMolecule);
                this.keyString = ChemObject.keystr;
                _isvalid = ChemObject.ChemistryIsValid;
                base.size = ChemObject.heavyAtomCount; 
            }
        }

        public FingerPrintChemHasseNode(IndigoChemistry mol, HasseNodeTypes ElementType, HasseNodeCollection globalElementCollection, string debugInfo)
            : base(ElementType, globalElementCollection, debugInfo)
        {

            if (ElementType == HasseNodeTypes.ROOT)
            {
             //   molecule = null;
                this.keyString = "";
                return;
            }
            ChemObject = mol;
            this.keyString = ChemObject.keystr;
            _isvalid = ChemObject.ChemistryIsValid;
            base.size = ChemObject.heavyAtomCount;
        }

    }
}