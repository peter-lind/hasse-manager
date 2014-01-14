using System;

namespace HasseManager
{

    public class FingerprintChemHasseNode : HasseNode
    {
        IndigoChemistry ChemObject = null;
        public HasseFingerprint hFingerprint = null;

        public FingerprintChemHasseNode(string StrMolecule, HasseNodeTypes ElementType, string debugInfo)
            : base(ElementType, debugInfo)
        {
            ChemObject = new IndigoChemistry();
            if (ElementType == HasseNodeTypes.ROOT)
            {
                this.keystring = "0";
            }
            else if (Convert.ToBoolean(ElementType &  HasseNodeTypes.FRAGMENT)) // is fragment
            {
                //   molecule = null;
                hFingerprint = ChemObject.GetEmptyHasseFingerPrint();
                hFingerprint.FromHex(StrMolecule);
                base.size = hFingerprint.bitCount;
                this.keystring = StrMolecule;
                return;
            }

            else
            {
                ChemObject.InitMolecule(StrMolecule);
                this.keystring = ChemObject.keystr;
                _isvalid = ChemObject.ChemistryIsValid; 
                hFingerprint = ChemObject.GetHasseFingerPrint();
                base.size = hFingerprint.bitCount;
            }
        }


        public FingerprintChemHasseNode(IndigoChemistry mol, HasseNodeTypes ElementType, string debugInfo)
            : base(ElementType, debugInfo)
        {

            if (ElementType == HasseNodeTypes.ROOT)
            {
                //   molecule = null;
                this.keystring = "";
                return;
            }
            ChemObject = mol;
            this.keystring = ChemObject.keystr;
            _isvalid = ChemObject.ChemistryIsValid;
            base.size = ChemObject.heavyAtomCount;
            hFingerprint = ChemObject.GetHasseFingerPrint();
        }

        private void Initialize()
        {
        }


        public void CreateImage()
        {
            if (ChemObject == null) return;
            if (ChemObject.molecule  == null) return;
            float defaultWeight = 2F;
            CreateImage(defaultWeight);
        }

        public void CreateImage(float weight)
        {
            // System.Diagnostics.Debug.WriteLine("render " + molecule.smiles());
            const string IMAGEDIR = @"C:\TEMP\IMAGES\";
            this.ImageFileName = IMAGEDIR + base.ID.ToString() + ".svg";
            ChemObject.CreateImage(this.ImageFileName, weight);
        }


        public override string[] GetDifferenceString(HasseNode LargerNode)
        {

            if (this.NodeType == HasseNodeTypes.ROOT)
            {
                return new string[] { };
            }
            else 
            {
                string buf;
                HasseFingerprint fp = this.hFingerprint;
                HasseFingerprint fpclone = fp.CloneHasseFingerprint();
                fpclone.Minus(((FingerprintChemHasseNode )LargerNode).hFingerprint );
                buf = fpclone.ToHex(); 
                return new string[] {buf };
            }

            return ChemObject.GetDifferenceString(LargerNode);
        }

        public override bool GetMaxCommonFragments(
        HasseNode Node1, HasseNode Node2,
        bool dbg, HasseFragmentInsertionQueue NewFragmentList,
        int MinimumOverlap)
        {            
            string debugInfo = "";
            HasseFingerprint fp1 = ((FingerprintChemHasseNode)Node1).hFingerprint;
            HasseFingerprint fp1_clone = fp1.CloneHasseFingerprint();
            HasseFingerprint fp2 = ((FingerprintChemHasseNode)Node2).hFingerprint;
            fp1_clone.AndBits(fp2);
            if(fp1_clone.bitCount >= MinimumOverlap ){
                string strMCS = fp1_clone.ToHex();
                
                HasseFingerprint test = new HasseFingerprint();
                test.FromHex(strMCS);

            NewFragmentList.Add(
                           new HasseNode[1] { this }, // this is lower than new frag
                           new HasseNode[2] { Node1, Node2 }, // those are higher than new frag
                           strMCS, // string to use for new node creation later
                           debugInfo, HasseNode.HasseNodeTypes.FRAGMENT | HasseNode.HasseNodeTypes.MAX_COMMON_FRAGMENT, // type of frag
                           null // this new frag is not associated with a single edge
                           );
                return true;
            }
            return false;
        }

        public override bool IsLargerThan(HasseNode smallobj)
        {
            if (this.HasNodeType(HasseNodeTypes.ROOT)) { return false; }
            if (smallobj.HasNodeType(HasseNodeTypes.ROOT)) { return true; }
            bool retval= this.hFingerprint.IsLargerThan(((FingerprintChemHasseNode )smallobj).hFingerprint );
           // if (retval == true) System.Diagnostics.Debugger.Break();
            return retval;
        }

    }
}