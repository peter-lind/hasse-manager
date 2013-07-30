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
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        static Indigo indigo; // indigo application
        static IndigoChemistryDepiction ICD;
        static List<RecapReaction> RecapReactions;
        public bool TrustFingerprints = true;

        IndigoObject molecule = null;
        public IndigoObject fingerPrint = null;
        public BitArray BitArrayFp = null;
        public string molname;
        int heavyAtomCount = 0;
        string keyString = "";
        public static int countMCS;


        public ChemHasseNode(string StrMolecule, HasseNodeTypes ElementType, HasseNodeCollection globalElementCollection, string debugInfo)
            : base(ElementType, globalElementCollection, debugInfo)
        {
            if (indigo == null) indigo = new Indigo();


            if (ElementType == HasseNodeTypes.ROOT)
            {
                molecule = null;
                this.keyString = "{}";
                return;
            }

            molecule = indigo.loadMolecule(StrMolecule);
            molname = molecule.name().Trim ();  

        // validate molecule
            string problem1 = molecule.checkAmbiguousH();
            string problem2 = molecule.checkBadValence();
            if (problem1.Length > 0)
            {
                throw new Exception(problem1);
            }
            if (problem2.Length > 0)
            {
                throw new Exception(problem2);
            }


            molecule.aromatize();
        // clean away explicit hydrogens on carbons - they cause problems in matchings
            List<int> HList = new List<int>();
            foreach (IndigoObject atom in molecule.iterateAtoms())
                if (atom.atomicNumber() == 6)
                {
                    {
                        foreach (IndigoObject nei in atom.iterateNeighbors())
                            if (nei.atomicNumber() == 1) HList.Add(nei.index());
                    }
                }
            molecule.removeAtoms(HList);
            molecule = molecule.clone();  // critical for atom numbering, typically max 0.1 msec

            // init Recap reactions if not already done
            //if (RecapReactions == null)
            //  RecapReactions = InitRecapReactionList();
            //sw.Reset();
            //sw.Start();
            //React(RecapReactions, molecule );
            //sw.Stop();
            //long ticks = sw.ElapsedTicks ;
            //long freq = System.Diagnostics.Stopwatch.Frequency;
            //double ms = (double)(ticks*1000) / (double)freq;

            //      molecule.aromatize();



            fingerPrint = molecule.fingerprint("sub");
            BitArrayFp = new BitArray(fingerPrint.toBuffer());
            this.BitArrayFingerPrint = new ChemHasseFingerprint(indigo, BitArrayFp);
            heavyAtomCount = molecule.countHeavyAtoms();
            if (heavyAtomCount == 1)
            {
                //this.keyString = StrMolecule;
                //this.keyString = molecule.getAtom(0).symbol ();
                foreach (IndigoObject atom in molecule.iterateAtoms())
                    if (atom.atomicNumber() >1) 
                    this.keyString = "[" + atom.symbol() + "]"; 
                //sad
            }
            else
            {
                this.keyString = molecule.canonicalSmiles();
            }
            if (this.keyString == "") throw new Exception(StrMolecule +  ": empty key generated"); 
        }
        public override void CreateImage()
        {
            // System.Diagnostics.Debug.WriteLine("render " + molecule.smiles());
            const string IMAGEDIR = @"C:\TEMP\IMAGES\";
            this.ImageFileName = IMAGEDIR + base.ID.ToString() + ".svg";
            if (ICD == null) ICD = new IndigoChemistryDepiction(indigo);
            ICD.WriteImageFile(this.ImageFileName, molecule, "svg");
        }
        public IndigoObject Molecule()
        {
            return molecule;
        }
        public bool ContainsFingerprintOf(IndigoObject fp)
        {
            int commonBits = indigo.commonBits(this.fingerPrint, fp);
            if (commonBits == fp.countBits()) return true;
            return false;
        }
        public override int elementCount()
        {

            if (heavyAtomCount == 0) { countElements(); }
            return heavyAtomCount;
        }
        private void countElements()
        {
            heavyAtomCount = molecule.countHeavyAtoms();
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

            if (smallobj.HasNodeType ( HasseNodeTypes.MAX_COMMON_FRAGMENT ))
                System.Diagnostics.Debugger.Break ();   


            if (smallobj.NodeType == HasseNodeTypes.ROOT)
            {
                return true;
            }
            if (this.NodeType == HasseNodeTypes.ROOT)
            {
                return false;
            }


            if (((ChemHasseNode)smallobj).heavyAtomCount >= this.heavyAtomCount)
            {
                return false; // was same or larger
            }

            // fingerprints compare benchmark: 3 microseconds or 300 per ms
            bool IsPossiblyLargerThanByFingerprint = ContainsFingerprintOf(((ChemHasseNode)smallobj).fingerPrint);

            if (TrustFingerprints)
            {
                if (!IsPossiblyLargerThanByFingerprint)
                { return false; }
                else
                {
                    return true;
                }
            }

            // todo - cache the query form of all mols along with their normal form

            //IndigoObject o = ((ChemHasseNode)smallobj).Molecule().clone(); 

            //o.resetSymmetricCisTrans(); // TODOOO
            //o.markEitherCisTrans(); //TODOOOO 
            //IndigoObject smallMol = indigo.loadQueryMolecule(o.canonicalSmiles ());

            IndigoObject smallMol = indigo.loadQueryMolecule(smallobj.KeyString);

            RemoveHydrogenConstraints(smallMol);
            IndigoObject targetMol = this.molecule;


            // prepare matcher for the target 
            // todo - cache the matcher


            IndigoObject matcher = indigo.substructureMatcher(targetMol);
            indigo.setOption("embedding-uniqueness", "atoms");
            // do matching
            matcher.match(smallMol);
            //System.Diagnostics.Debug.WriteLine(matcher.countMatches(smallMol));   


            // todo take a away those checks that fingerprints work
            if (IsPossiblyLargerThanByFingerprint)
            { // can be a substructure match but need not be 
                if (matcher.countMatches(smallMol) == 0)
                {
                    System.Diagnostics.Debug.WriteLine("smaller original\t" + smallobj.KeyString);
                    System.Diagnostics.Debug.WriteLine("expected smaller\t" + smallMol.smiles());
                    System.Diagnostics.Debug.WriteLine("expected larger \t" + molecule.smiles());
                    System.Diagnostics.Debugger.Break();
                }
            }
            else
            {
                if (matcher.countMatches(smallMol) != 0)
                    System.Diagnostics.Debugger.Break(); //we have a match where fp says we certainly have not!
            }
            return (matcher.countMatches(smallMol) == 0) ? false : true;
        }


        private void RemoveHydrogenConstraints(IndigoObject mol)
        {

            // all this typically < 0.1 msec
            List<int> HList = new List<int>();
            foreach (IndigoObject atom in mol.iterateAtoms())
            {
                atom.removeConstraints("hydrogens"); // critical !!
                if (atom.atomicNumber() == 1) HList.Add(atom.index());
            }
            mol.removeAtoms(HList); // e.g. to make -n[H]- substructure of -n[C]-  
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
            sw.Start();
            countMCS++;
            System.Diagnostics.Debug.WriteLineIf(dbg, "N1: " + ((ChemHasseNode)Node1).Molecule().smiles());
            System.Diagnostics.Debug.WriteLineIf(dbg, "N2: " + ((ChemHasseNode)Node2).Molecule().smiles());
            //            IndigoObject scaf = getMCS(((ChemHasseNode)Node1).Molecule(), ((ChemHasseNode)Node2).Molecule());
            IndigoObject[] scaffolds = new IndigoObject[2] {
                ((ChemHasseNode)Node1).Molecule(),
                ((ChemHasseNode)Node2).Molecule() };

            IndigoObject scaf = null;
            try
            {
                scaf = indigo.extractCommonScaffold(scaffolds, "exact");//"approx 100");
            }
            catch (Exception ex)
            {
                if (ex.ToString().StartsWith("com.ggasoftware.indigo.IndigoException: Molecule Scaffold detection: There are no scaffolds found"))
                {
                    System.Diagnostics.Debug.WriteLine("No scaffold found.");
                }
                else
                {
                    throw new Exception(ex.ToString());
                }
            }

            bool hadMCS = false;
            if (scaf != null)
            {
                //try{
                IndigoObject matcher = indigo.substructureMatcher(((ChemHasseNode)Node1).Molecule());
                System.Diagnostics.Debug.WriteLineIf(dbg, "max scaffold: " + scaf.smiles());
                // loop all scaffolds
                foreach (IndigoObject scaffold in scaf.allScaffolds().iterateArray())
                {

                    //
                    RemoveHydrogenConstraints(scaffold); //TODO check
                    //

                   // System.Diagnostics.Debug.Write(".");
                   // System.Diagnostics.Debug.WriteLineIf(dbg, "one of scaffolds: " + scaffold.smiles());
                    IndigoObject m = matcher.match(scaffold);
                    if (m != null)
                    {
                        // does this scaffold qualify as useful?
                        if (!hasBrokenRing(m, scaffold, ((ChemHasseNode)Node1).Molecule()))
                        {
                            // align scaffold on one of nodes, clone matched part.
                            // this is because a scaffold is not a mol 
                            IndigoObject o = makeMatchedPart(m, scaffold, ((ChemHasseNode)Node1).Molecule());
                            o.dearomatize(); // seems important!
                            System.Diagnostics.Debug.WriteLineIf(dbg, "matched part: " + o.smiles());
                            hadMCS = true;

                            // todo check again if smiles works for all molecules
                            o.resetSymmetricCisTrans();
                            string StrScaffold = o.molfile();
                            string debugInfo = "MCS " + Node1.GetID().ToString() + " " + Node2.GetID().ToString();

                            NewFragmentList.Add(
                                new HasseNode[1] { this }, // this is lower than new frag
                                new HasseNode[2] { Node1, Node2 }, // those are higher than new frag
                                StrScaffold, // string to use for new node creation later
                                debugInfo, HasseNodeTypes.FRAGMENT | HasseNodeTypes.MAX_COMMON_FRAGMENT, // type of frag
                                null // this new frag is not associated with a single edge
                                );
                        }
                    }
                    //break;
                }

            }
            sw.Stop();
            //if (sw.ElapsedMilliseconds > 10000) System.Diagnostics.Debugger.Break();
            sw.Reset();
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

            LargerMol.clearCisTrans();

            // Find substructure. target is larger, query is smaller 
            IndigoObject matcher = indigo.substructureMatcher(LargerMol);
            indigo.setOption("embedding-uniqueness", "atoms");

            //            IndigoObject scaf = indigo.extractCommonScaffold(new IndigoObject[2] { this.Molecule(), LargerMol }, "exact");
            //            if (scaf != null)
            //            {
            //foreach (IndigoObject scaffold in scaf.allScaffolds().iterateArray())
            //{
            // }
            //            }

            // System.Diagnostics.Debug.WriteLine("this: " + this.molecule.smiles());
            IndigoObject QueryMol = indigo.loadQueryMolecule(this.molecule.smiles());

            RemoveHydrogenConstraints(QueryMol);
            //QueryMol.clearCisTrans(); 


            //
            //   System.Diagnostics.Debug.WriteLine("QueryMol: " + QueryMol.smiles());
            //    System.Diagnostics.Debug.WriteLine("Largermol is : " + LargerMol.smiles());
            //

            //QueryMol.aromatize(); // this makes difference? 
            foreach (IndigoObject m in matcher.iterateMatches(QueryMol))
            {
                IndigoObject o = makeDifference(m, QueryMol, LargerMol);
                o.dearomatize();

                if (o.countAtoms() > 0)
                {
                    //System.Diagnostics.Debug.WriteLine("Diff is : " + o.smiles());
                    DiffList.Add(o.canonicalSmiles());
                }
                //System.Diagnostics.Debug.WriteLine("Diffence becomes: " + o.smiles());
                //break; // do not iterate all matchings - todo think about this, can possibly match different ways with different differences - or just several symmetrical matches
            }
            string[] s = DiffList.ToArray();
            return s;
        }



        private IndigoObject makeDifference(IndigoObject thisMatch,
            IndigoObject smaller, IndigoObject larger)
        {
            IndigoObject largerClone = larger.clone();
            largerClone.foldHydrogens(); //that is, remove H
            largerClone.dearomatize(); // seems essential 
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
                        //if (thisMatch.mapAtom(atom).index()< largerClone.countAtoms () ) // todo make more efficient
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
            // Prepare atoms-to-remove list 
            List<int> L = new List<int>();

            //System.Diagnostics.Debug.WriteLine("makeMatchedPart");
            if (thisMatch == null) return null;

            foreach (IndigoObject atom in smaller.iterateAtoms())
            {
                // Add target atom indexes to the list 
                //System.Diagnostics.Debug.Write(atom.symbol());

                /*
                foreach (IndigoObject b in smaller.iterateBonds())
                {
                    int a1 = b.source().index();
                    int a2 = b.destination().index();
                    int aidx = atom.index();
                    if (a1 == aidx | a2 == aidx)
                    {
                        System.Diagnostics.Debug.Write("-" + b.topology());
                    }
                }
                System.Diagnostics.Debug.WriteLine("");
                */

                if (atom.atomicNumber() != 1)
                    L.Add(thisMatch.mapAtom(atom).index());
            }

            int[] atr = L.ToArray();
            IndigoObject sub = larger.createSubmolecule(L.ToArray());
            return sub.clone();

        }


        protected override Dictionary<string, HasseNode> makeElementarySubobjects(HasseNodeCollection GlobalHasseNodeCollection)
        {
            Dictionary<string, HasseNode> elmobjects = new Dictionary<string, HasseNode>();
            if (this.NodeType == HasseNodeTypes.ROOT) return (elmobjects);
            List<string> RawList = new List<string>();

            foreach (IndigoObject atom in molecule.iterateAtoms())
            {
                if (atom.atomicNumber() > 1)
                    RawList.Add("[" + atom.symbol() + "]");
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
                    element = new ChemHasseNode(buf, HasseNodeTypes.ELEMENT, GlobalHasseNodeCollection, "from " + base.ID.ToString());
                    GlobalHasseNodeCollection.Add(buf, element);
                }

                //todo change to better names for elements collections
                if (!elmobjects.ContainsKey(buf))
                    elmobjects.Add(buf, element);
            }
            // if only one elementary object, then use same object for this (as item) and that (as element) 
            if (elmobjects.Count == 1 && RawList.Count == 1)
            {
                elmobjects.Clear();
                elmobjects.Add(this.KeyString, this);
                this.AddNodeType(HasseNodeTypes.ELEMENT);
            }

            return elmobjects;

        }



        private bool hasBrokenRing(IndigoObject thisMatch,
            IndigoObject smaller, IndigoObject larger)
        {
            foreach (IndigoObject bond in smaller.iterateBonds()) //loop bonds of scaffold
            {
                if (thisMatch.mapBond(bond) != null)
                {
                    int top1 = bond.topology();  // bond topology of scaffold
                    int top2 = thisMatch.mapBond(bond).topology(); // bond topology of origin molecule
                    if (top1 != top2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }



        private void TestRecap(string reactantSmiles)
        {
            List<RecapReaction> RecapReactions = InitRecapReactionList();
            string[] RecapTests =
            {
                "CC(=O)OC",
                "CNC(=O)NC",
                "CC(=O)NC",
                "CN","CNC","CN(C)C","C1CNCC1","C1CN(C)CC1","C-N-C=S",
                "CCOCC","C1COCC1",
                "CC=CC","c1cn(CCC)cc1",
                "C1CC(=O)N(CCC)CC1",
                "c1ccccc1c1ccccc1",
                "c1ccccn1c1ccccc1",
                "CCNS(=O)(=O)CCC"
            };
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            foreach (string s in RecapTests)
            {
                //IndigoObject reactant = indigo.loadMolecule(reactantSmiles);
                IndigoObject reactant = indigo.loadMolecule(s);
                React(RecapReactions, reactant);
            }
            sw.Stop();
            long ms = sw.ElapsedMilliseconds;
        }


        public IndigoObject React(List<RecapReaction> Reactions, IndigoObject reac)
        {
            IndigoObject reactant = reac.clone();
            bool DEBUG = true;
            int countHeavyAtoms = reactant.countHeavyAtoms();
            System.Diagnostics.Debug.WriteLineIf(DEBUG, "before:\t" + reactant.smiles());
            foreach (RecapReaction r in Reactions)
            {
                string smarts = r.smarts;
                IndigoObject rxn = indigo.loadReactionSmarts(smarts);
                int countComponents = reactant.countComponents();


                indigo.transform(rxn, reactant);
                rxn.automap("discard");
                int countComponents2 = reactant.countComponents();
                if (countComponents != countComponents2)
                {
                    System.Diagnostics.Debug.WriteLineIf(DEBUG, r.label);
                    int x = 0;
                    foreach (IndigoObject atom in reactant.iterateAtoms())
                    {
                        //if (x==0)   atom.setAttachmentPoint(2);
                        //System.Diagnostics.Debug.WriteLine(rxn.mapAtom(atom) );// + " " + rxn.atomMappingNumber(atom));
                        x++;
                    }

                }
                if (countHeavyAtoms != reactant.countHeavyAtoms())
                {
                    // changed atom count - bad smarts for RECAP
                    System.Diagnostics.Debug.WriteLine("bad smarts: " + smarts);
                    System.Diagnostics.Debugger.Break();
                }





            }// end smarts loop




            System.Diagnostics.Debug.WriteLineIf(DEBUG, "after:\t" + reactant.smiles() + " " + reactant.countAttachmentPoints());
            return reactant;
        }

        public List<RecapReaction> InitRecapReactionList()
        {
            List<RecapReaction> RecapReactions = new List<RecapReaction>();
            // RECAP smarts adopted from  http://www.rdkit.org/docs/api/rdkit.Chem.Recap-pysrc.html
            // and credits to original RECAP paper
            {// "[*:1]C(=O)OC[*:2].O>>[*:1]C(=O)O.OC[*:2]", // ester including ring

                //RecapReactions.Add(new RecapReaction("[C:1]-[O:2]>>[C:1].[O:2]", "ester, ether, alcohol etc"));
                RecapReactions.Add(new RecapReaction("[*:1][C:2](=!@[O:3])!@[O:4][*:5]>>[*:1][C:2](=[O:3]).[O:4][*:5]", "acyclic ester"));
                RecapReactions.Add(new RecapReaction("[*:1][#7;+0;D2,D3:2]!@[C:3](!@=[O:4])!@[#7;+0;D2,D3:5][*:6]>>[*:1][#7:2].[C:3](=[O:4]).[#7:5][*:6]", "all urea"));
                RecapReactions.Add(new RecapReaction("[*:1][C;!$(C([#7])[#7]):2](=!@[O:3])!@[#7;+0;!D1:4][*:5]>>[*:1][C:2]=[O:3].[#7:4][*:5]", "amide"));
                RecapReactions.Add(new RecapReaction("[N;!D1;+0;!$(N-C=[#7,#8,#15,#16]):1](-!@[*:2])-!@[*:3]>>[N:1].[*:2].[*:3]", "amines, cut 2 acyclic R"));
                //"[N;!D1:1](!@[*:2])!@[*:3]>>[N:1].[*:2].[*:3]", // 2:ary not cyclic amines    ????
                RecapReactions.Add(new RecapReaction("[#7;R;D3;+0:1]-!@[*:2]>>[#7:1].[*:2]", "3:ary amine, cut where no cycle"));
                RecapReactions.Add(new RecapReaction("[#6:1]-!@[O;+0]-!@[#6:2]>>[#6:1].[O].[#6:2]", "non-cyclic ether"));
                RecapReactions.Add(new RecapReaction("[C:1]=!@[C:2]>>[C:1].[C:2]", "olefin"));
                RecapReactions.Add(new RecapReaction("[n;+0:1]-!@[C:2]>>[n:1].[C:2]", "aromatic nitrogen - aliphatic carbon"));
                RecapReactions.Add(new RecapReaction("[O:1]=[C:2]-@[N;+0:3]-!@[C:4]>>[O:1]=[C:2]-[N:3].[C:4]", "lactam nitrogen - aliphatic carbon"));
                RecapReactions.Add(new RecapReaction("[c:1]-!@[c:2]>>[c:1].[c:2]", "aromatic carbon - aromatic carbon"));
                RecapReactions.Add(new RecapReaction("[n;+0:1]-!@[c:2]>>[n:1].[c:2]", "aromatic nitrogen - aromatic carbon"));
                RecapReactions.Add(new RecapReaction("[#7;+0;D2,D3:1]-!@[S:2](=[O:3])=[O:4]>>[#7:1].[S:2](=[O:3])=[O:4]", "sulphonamide, cut N-S"));
            };
            return RecapReactions;
        }

        public class RecapReaction
        {
            public string smarts;
            public string label;

            public RecapReaction(string s, string l)
            {
                smarts = s;
                label = l;
            }
        }

        public override HasseFingerprint CreateFingerprint()
        {
            ChemHasseFingerprint fp = new ChemHasseFingerprint(indigo);
            return fp;
        }

    }

    public class ChemHasseFingerprint : HasseFingerprint
    {
        Indigo indigo;
        System.Collections.BitArray fp;

        public ChemHasseFingerprint(Indigo _indigo)
        {
            indigo = _indigo;
        }
        public ChemHasseFingerprint(Indigo _indigo, BitArray _fp)
        {
            indigo = _indigo;
            fp = _fp;
        }

        public override void AddBitsOf(HasseFingerprint F)
        {

            BitArray fp2 = ((ChemHasseFingerprint)F).fp;


            if (fp == null)
            {
                fp = new System.Collections.BitArray(fp2);
            }
            else
            {
                fp.Or(fp2);
            }

        }
        public override bool ContainsAllBitsOf(HasseFingerprint F)
        {
            BitArray fp2 = ((ChemHasseFingerprint)F).fp;
            //todo: think is this good choice:
            // for now think F is null nad has no bits then all fp "contains null"
            if (fp2 == null) return true;

            for (int i = 0; i < fp.Count; i++)
            {
                if (fp[i] == true)
                {
                    if (fp2[i] != true) return false;
                }
            }
            return true;
        }
    }

}

