using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.ggasoftware.indigo;
using System.Collections;
namespace HasseManager
{
    public class ChemHasseNode : HasseNode
    {
        public bool TrustFingerprints = true; // we have reasonably verified that this can be trusted
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        static string latestaddition;
        IndigoChemistry ChemObject = null;
        public HasseFingerprint hFingerprint = null;

        public ChemHasseNode(string StrMolecule, HasseNodeTypes ElementType,  string debugInfo)
            : base(ElementType,  debugInfo)
        {
            ChemObject = new IndigoChemistry();
            if (ElementType == HasseNodeTypes.ROOT)
            {
                this.keystring  = "{}";
            }
            else
            {
                ChemObject.InitMolecule(StrMolecule);
                this.keystring = ChemObject.keystr;
                _isvalid = ChemObject.ChemistryIsValid;
                base.size = ChemObject.heavyAtomCount;
                hFingerprint = ChemObject.GetHasseFingerPrint();
            }
        }


        public ChemHasseNode(IndigoChemistry  mol, HasseNodeTypes ElementType, string debugInfo)
            : base(ElementType, debugInfo)
        {

            if (ElementType == HasseNodeTypes.ROOT)
            {
             //   molecule = null;
                this.keystring = "{}";
                return;
            }
            ChemObject = mol;
            this.keystring = ChemObject.keystr;
            _isvalid = ChemObject.ChemistryIsValid;
            base.size = ChemObject.heavyAtomCount;
            hFingerprint = ChemObject.GetHasseFingerPrint();
        }


        public bool ContainsFingerprintOf(HasseFingerprint  fp )
        {
            return hFingerprint.ContainsAllBitsOf(fp);
        }

        
        public HasseFingerprint  fingerprint()
        {
            return hFingerprint; 
        }

        public void CreateImage()
        {
            float defaultWeight = 2F;
            CreateImage(defaultWeight);
        }

        public void CreateImage(float weight)
        {
            // System.Diagnostics.Debug.WriteLine("render " + molecule.smiles());
            const string IMAGEDIR = @"C:\TEMP\IMAGES\";
            this.ImageFileName = IMAGEDIR + base.ID.ToString() + ".svg";
            ChemObject.CreateImage(this.ImageFileName ,  weight); 
        }

 
        public IndigoObject Molecule()
        {
            return ChemObject.molecule; 
        }


        public override bool IsLargerThan(HasseNode smallobj)
        {
            if (smallobj.HasNodeType(HasseNode.HasseNodeTypes.MAX_COMMON_FRAGMENT))
                System.Diagnostics.Debugger.Break();

            if (smallobj.NodeType == HasseNode.HasseNodeTypes.ROOT)
            {
                return true;
            }
            if (this.NodeType == HasseNode.HasseNodeTypes.ROOT)
            {
                return false;
            }
            bool IsPossiblyLargerThanByFingerprint = ContainsFingerprintOf(((ChemHasseNode)smallobj).fingerprint());
            if (TrustFingerprints)
            {
                if (IsPossiblyLargerThanByFingerprint)
                { 
                    ;
                } // is likely to be larger, but we are not sure, needs to be checked
                else
                {
                    return false; // if fingerprints work as they should we are guaranteed that IsLargerThan must be false
                }
            }
            // todo take a away those checks that fingerprints work
            if (IsPossiblyLargerThanByFingerprint)
            { // can be a substructure match but need not be 
                //if (matcher.countMatches(smallMol) == 0)
                //{
                //    System.Diagnostics.Debug.WriteLine("a fingerprint contained in b fingerprint - but a was not substruc of b");
                //    System.Diagnostics.Debug.WriteLine("a\t" + smallMol.smiles());
                //    System.Diagnostics.Debug.WriteLine("b\t" + molecule.smiles());
                //}
            }
            else
            {
                //if (matcher.countMatches(smallMol) != 0)
                  //  System.Diagnostics.Debugger.Break(); //we have a match where fp says we certainly have not!
            }

            return ChemObject.IsLargerThan(smallobj); 
        }


        public override string GetName()
        {
            return ChemObject.GetName(); 
        }
        public override void SetName(string n)
        {
            ChemObject.SetName(n);
        }

        public override bool GetMaxCommonFragments(
            HasseNode Node1, HasseNode Node2,
            bool dbg, HasseFragmentInsertionQueue NewFragmentList,
            int MinimumOverlap)
        {
           return  ChemObject.GetMaxCommonFragments(this, Node1, Node2, dbg, NewFragmentList, MinimumOverlap );
        }

        public override string[] GetDifferenceString(HasseNode LargerNode)
        {
            if (this.NodeType == HasseNodeTypes.ROOT)
            {
                return new string[] { }; //TODO - is this right?
            }

            return ChemObject.GetDifferenceString(LargerNode); 
        }




        /*
        protected override Dictionary<string, HasseNode> makeElementarySubobjects(HasseNodeCollection GlobalHasseNodeCollection)
        {
            if (this.NodeType == HasseNodeTypes.ROOT) return (new Dictionary<string, HasseNode>()); // empty dictionary
            return ChemObject.makeElementarySubobjects(GlobalHasseNodeCollection, this);
        }
        */

        /*
        public override HasseFingerprint CreateFingerprint()
        {
            ChemHasseFingerprint fp = ChemObject.CreateFingerprint();
            return fp;
        }
        */
    }


}

