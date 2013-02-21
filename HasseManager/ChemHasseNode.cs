using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.ggasoftware.indigo;

namespace HasseManager
{
    public class ChemHasseNode : HasseNode
    {
        static Indigo indigo; // indigo application
        IndigoObject molecule = null;
        int atomCount = 0;
        string keyString = "";

        public ChemHasseNode(string StrMolecule, HasseNodeTypes ElementType, HasseNodeCollection globalElementCollection)
            : base(ElementType, globalElementCollection)
        {
            if (indigo == null) indigo = new Indigo();
            if (ElementType == HasseNodeTypes.ROOT) { 
                molecule=null;
                this.keyString ="{Ø}";
                return;
            }
  
            molecule = indigo.loadMolecule(StrMolecule);
            molecule.aromatize();
            atomCount = molecule.countHeavyAtoms();
            if (atomCount == 1)
            {
                this.keyString = StrMolecule;
            }
            else
            {
                this.keyString = molecule.canonicalSmiles();
            }
        }

        public IndigoObject Molecule()
        {
            return molecule;
        }
        public override int elementCount()
        {

            if (atomCount == 0) { countElements(); }
            return atomCount;
        }
        private void countElements()
        {
            atomCount = molecule.countHeavyAtoms();
        }



        /*
        public override bool  IsIdenticalTo(HasseNode elm)
        {
            throw new NotImplementedException();
        }
         * 
         */

        public override bool IsLargerThan(HasseNode smallobj)
        {
            if (smallobj.NodeType == HasseNodeTypes.ROOT)
            {
                return true;
            }
            if (this.NodeType == HasseNodeTypes.ROOT)
            {
                return false;
            }


            if (((ChemHasseNode)smallobj).atomCount  >= this.atomCount)
            {
                return false; // was same or larger
            }
            // todo - cache the query form of all mols along with their normal form
            IndigoObject smallMol = indigo.loadQueryMolecule(smallobj.KeyString);
            IndigoObject targetMol = this.molecule;

            // prepare matcher for the target 
            // todo - cache the matcher
            IndigoObject matcher = indigo.substructureMatcher(targetMol);
            indigo.setOption("embedding-uniqueness", "atoms");
            // do matching
            matcher.match(smallMol);
            return (matcher.countMatches(smallMol) == 0) ? false : true;
        }

        public override string KeyString
        {
            get
            {
                if (keyString.Length == 0)
                {
                    keyString = this.molecule.canonicalSmiles();
                }
                return keyString;
            }
        }


        private IndigoObject getMCSNotUsed(IndigoObject MolA, IndigoObject MolB)
        {
            //IndigoObject arr = indigo.createArray();
            //arr.arrayAdd(MolA);
            //arr.arrayAdd(MolB);
            IndigoObject scaf = null;
            //scaf = indigo.extractCommonScaffold(arr, "exact");
            scaf = indigo.extractCommonScaffold(new IndigoObject[2] { MolA, MolB }, "exact");
            if (scaf != null)
            {
                System.Console.WriteLine("max scaffold: " + scaf.smiles());
                foreach (IndigoObject scaffold in scaf.allScaffolds().iterateArray())
                    System.Console.WriteLine("current scaffold: " + scaffold.smiles());
            }
            return scaf;
        }


        public override bool GetMaxCommonFragments(
            HasseNode Node1, HasseNode Node2,
            bool dbg, HasseFragmentInsertionQueue NewFragmentList,
            int MinimumOverlap)
        {
            System.Diagnostics.Debug.WriteLine("N2: " + ((ChemHasseNode)Node2).Molecule().smiles());
            //            IndigoObject scaf = getMCS(((ChemHasseNode)Node1).Molecule(), ((ChemHasseNode)Node2).Molecule());
            IndigoObject[] scaffolds = new IndigoObject[2] {
                ((ChemHasseNode)Node1).Molecule(),
                ((ChemHasseNode)Node2).Molecule() };
            IndigoObject scaf = indigo.extractCommonScaffold(scaffolds, "exact");

            bool hadMCS = false;
            if (scaf != null)
            {
                try{
                IndigoObject matcher = indigo.substructureMatcher(((ChemHasseNode)Node1).Molecule());
                System.Diagnostics.Debug.WriteLine("max scaffold: " + scaf.smiles());
                foreach (IndigoObject scaffold in scaf.allScaffolds().iterateArray())
                {
                    System.Diagnostics.Debug.WriteLine("i:th scaffold-: " + scaffold.smiles());
            IndigoObject m = matcher.match(scaffold);
            IndigoObject o = makeMatchedPart(m, scaffold, ((ChemHasseNode)Node1).Molecule());
            //System.Console.WriteLine("i:th scaffold: " + o.smiles());
            o.dearomatize();
            o.foldHydrogens();
           // o.aromatize();
                    hadMCS = true;
                    
                    System.Diagnostics.Debug.WriteLine("i:th scaffold+: " + o.smiles());
                    //System.Diagnostics.Debug.WriteLine("i:th scaffold: " + scaffold.smiles());
                    // todo - problem here, not canonical (canonical not defined for scaffold)
                    string StrScaffold = o.canonicalSmiles();

                    NewFragmentList.Add(
                        new HasseNode[1] { this }, // this is lower than new frag
                        new HasseNode[2] { Node1, Node2 }, // those are higher than new frag
                        StrScaffold, // string to use for new node creation later
                        "MCS", HasseNodeTypes.FRAGMENT | HasseNodeTypes.MAX_COMMON_FRAGMENT, // type of frag
                        null // this new frag is not associated with a single edge
                        );
                    break;
                }
                }
                catch{;}
                
            }

            return hadMCS;
        }

        public override string[] GetDifferenceString(HasseNode LargerNode)
        {
            if (this.NodeType == HasseNodeTypes.ROOT)
            {
                return new string[] { }; //TODO - is this right?
            }

            List<string> DiffList = new List<string>(); // for return strings
            IndigoObject LargerMol = ((ChemHasseNode)LargerNode).Molecule();

            // Find substructure. target is larger, query is smaller 
            IndigoObject matcher = indigo.substructureMatcher(LargerMol);

            //            IndigoObject scaf = indigo.extractCommonScaffold(new IndigoObject[2] { this.Molecule(), LargerMol }, "exact");
            //            if (scaf != null)
            //            {
            //foreach (IndigoObject scaffold in scaf.allScaffolds().iterateArray())
            //{
            // }
            //            }

            //System.Diagnostics.Debug.WriteLine("this: " + this.molecule.smiles());     
            IndigoObject QueryMol = indigo.loadQueryMolecule(this.molecule.smiles());
            QueryMol.aromatize(); // this makes difference?
            foreach (IndigoObject m in matcher.iterateMatches(QueryMol))
            {
                System.Diagnostics.Debug.WriteLine("QueryMol: " + QueryMol.smiles());
                System.Diagnostics.Debug.WriteLine("Largermol is : " + LargerMol.smiles());
                IndigoObject o = makeDifference(m, QueryMol, LargerMol);
                if (o.countAtoms() > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Diff is : " + o.smiles());
                    DiffList.Add(o.canonicalSmiles());
                }
                //System.Diagnostics.Debug.WriteLine("Diffence becomes: " + o.smiles());
            }
            string[] s = DiffList.ToArray();
            return s;
        }



        private IndigoObject makeDifference(IndigoObject thisMatch,
            IndigoObject smaller, IndigoObject larger)
        {
            IndigoObject largerClone = larger.clone();
            largerClone.foldHydrogens(); //that is, remove H
            //System.Diagnostics.Debug.WriteLine("makeDifference, small:\t" + smaller.smiles());
            //System.Diagnostics.Debug.WriteLine("makeDifference, large:\t" + largerClone.smiles());

            // Prepare atoms to remove list 
            List<int> L = new List<int>();

            if (thisMatch != null)
            {
                foreach (IndigoObject atom in smaller.iterateAtoms())
                {
                    // Add target atom indexes to the list 
                    // System.Diagnostics.Debug.WriteLine(atom.symbol() + " " + atom.isRSite());
                    if (atom.atomicNumber() != 1)
                        L.Add(thisMatch.mapAtom(atom).index());
                }
            }
            int[] atr = L.ToArray();
            // Remove atoms 
            largerClone.removeAtoms(atr);
            return largerClone;
        }

        private IndigoObject makeMatchedPart(IndigoObject thisMatch,
            IndigoObject smaller, IndigoObject larger)
        {
            //IndigoObject largerClone = larger.clone();
            //largerClone.foldHydrogens(); //that is, remove H

            // Prepare atoms to remove list 
            List<int> L = new List<int>();
            List<int> L2 = new List<int>();
            System.Diagnostics.Debug.WriteLine("makeMatchedPart");
            if (thisMatch != null)
            {
                  
                foreach (IndigoObject atom in smaller.iterateAtoms())
                {
                    // Add target atom indexes to the list 
                     //System.Diagnostics.Debug.WriteLine(atom.symbol() + " " + atom.degree().ToString()  );
                    if (atom.atomicNumber() != 1)
                        L.Add(thisMatch.mapAtom(atom).index());
                }
                foreach (IndigoObject bond in smaller.iterateBonds())

                {
                     //System.Diagnostics.Debug.WriteLine("order" + bond.bondOrder ());
                     //System.Diagnostics.Debug.WriteLine("dest" + bond.destination() );
                     //System.Diagnostics.Debug.WriteLine("src" + bond.source ());
                     if (bond.destination().atomicNumber() != 1 && bond.source().atomicNumber() != 1)
                    L2.Add(thisMatch.mapBond(bond).index()); 
                }
            }
            int[] atr = L.ToArray();
            IndigoObject sub = larger.createEdgeSubmolecule(L.ToArray(), L2.ToArray());
            // Remove atoms 
            //largerClone.removeAtoms(atr);
            return sub.clone ();
        }


        protected override Dictionary<string, HasseNode> makeElementarySubobjects(HasseNodeCollection GlobalHasseNodeCollection)
        {
            Dictionary<string, HasseNode> elmobjects = new Dictionary<string, HasseNode>();
            if (this.NodeType == HasseNodeTypes.ROOT) return (elmobjects);


            List<string> RawList = new List<string>();

            foreach (IndigoObject atom in molecule.iterateAtoms())
            {
                //System.Console.Write("{0} ", atom.symbol());
                if (atom.atomicNumber()>1 )
                RawList.Add("[" +atom.symbol() +"]");
            }

            for (int i = 0; i <= RawList.Count() - 1; i++)
            {
                string buf = RawList[i];

                //get existing reference to this elm or create new if not exist 
                ChemHasseNode element = null;
                if (GlobalHasseNodeCollection.ContainsKey(buf))
                {
                    element = (ChemHasseNode)GlobalHasseNodeCollection[buf];
                }
                else
                {
                    element = new ChemHasseNode(buf, HasseNodeTypes.ELEMENT, GlobalHasseNodeCollection);
                    GlobalHasseNodeCollection.Add(buf, element);
                }

                //todo change to better names for elements collections
                if (!elmobjects.ContainsKey(buf))
                    elmobjects.Add(buf, element);
            }
            // if only one elementary object, then use same object for this (as item) and that (as element) 
            if (elmobjects.Count == 1)
            {
                elmobjects.Clear();
                elmobjects.Add(this.KeyString, this);
                this.AddNodeType(HasseNodeTypes.ELEMENT);
            }

            return elmobjects;




        }
    }
}
