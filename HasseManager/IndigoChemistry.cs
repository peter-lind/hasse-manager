using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.ggasoftware.indigo;
using System.Collections;
namespace HasseManager
{
    public class IndigoChemistry
    {
        const bool ALLOW_PSEUDOATOMS = false;
        const bool SKIP_SCAFFOLD_WITH_BOND_FROM_BROKEN_RING = true;
        const int MAX_SCAFFOLDS = 2;
        const string MCS_MODE = "exact 100000";
        public const string SUBSTRUCTURE_MATCH_PARAM = "";
        const bool TEST_THAT_MCS_IS_SUBSTRUC = true;
        static Indigo indigo; // indigo application
        public IndigoObject molecule = null;
        public int heavyAtomCount = 0;
        public bool ChemistryIsValid = false;
        public IndigoObject iFingerPrint = null;
        private BitArray BitArrayFp = null;
        public string keystr = "";
        public static int countMCS;
        static IndigoChemistryDepiction ICD;
        static List<RecapReaction> RecapReactions;

        public IndigoChemistry()
        {
            if (indigo == null) indigo = new Indigo();
            indigo.setOption("aromaticity-model", "generic");
        }


        public void InitMolecule(string strMolecule)
        {
            molecule = indigo.loadMolecule(strMolecule);
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

            if (!ALLOW_PSEUDOATOMS)
            {
                foreach (IndigoObject atom in molecule.iterateAtoms())
                {
                    if (atom.isPseudoatom()) throw new Exception("molecule had pseudoatom while ALLOW_PSEUDOATOMS = false");
                }
            }
            molecule.aromatize();
            try
            {
                // clean away explicit hydrogens on carbons - they cause problems in matchings
                List<int> HList = new List<int>();
                foreach (IndigoObject atom in molecule.iterateAtoms())
                    {
                        if (atom.isPseudoatom() && atom.atomicNumber() == 6)
                        {
                            foreach (IndigoObject nei in atom.iterateNeighbors())
                                if (nei.isPseudoatom() && nei.atomicNumber() == 1) HList.Add(nei.index());
                        }
                    }
                molecule.removeAtoms(HList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(molecule.smiles ());   
                System.Diagnostics.Debugger.Break();   
            }
            heavyAtomCount = molecule.countHeavyAtoms();
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



            iFingerPrint = molecule.fingerprint("sub");
            heavyAtomCount = molecule.countHeavyAtoms();

            // System.Diagnostics.Debug.WriteLine (  "Is Chiral:" +   molecule.isChiral().ToString ());


            if (heavyAtomCount == 1)
            {
                //this.keyString = StrMolecule;
                //this.keyString = molecule.getAtom(0).symbol ();
                foreach (IndigoObject atom in molecule.iterateAtoms())
                    if (atom.atomicNumber() > 1)
                        keystr = "[" + atom.symbol() + "]";
                //sad
            }
            else
            {
                keystr = molecule.canonicalSmiles();
            }

            if (keystr == "") throw new Exception("Could not generate key");
            ChemistryIsValid = true;
        }

        public HasseFingerprint   GetHasseFingerPrint()
        {
            BitArray BitArrayFp = new BitArray(iFingerPrint.toBuffer());
            ChemHasseFingerprint hFingerprint  = new ChemHasseFingerprint(indigo, BitArrayFp);
            return hFingerprint;
    }
        public HasseFingerprint GetEmptyHasseFingerPrint()
        {
            ChemHasseFingerprint hFingerprint = new ChemHasseFingerprint(indigo);
            return hFingerprint;
        }

        /*
        public Dictionary<string, HasseNode> makeElementarySubobjects(HasseNodeCollection GlobalHasseNodeCollection, HasseNode Node)
        {

            Dictionary<string, HasseNode> elmobjects = new Dictionary<string, HasseNode>();
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
                    element = new ChemHasseNode(buf, HasseNode.HasseNodeTypes.ELEMENT,  "");
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
                elmobjects.Add(keystr, Node);
                Node.AddNodeType(HasseNode.HasseNodeTypes.ELEMENT);
            }

            return elmobjects;

        }

        */

        public void CreateImage(string ImageFileName, float weight)
        {
            if (ICD == null) ICD = new IndigoChemistryDepiction(indigo);
            ICD.WriteImageFile(ImageFileName, molecule, "svg", molecule.name(), weight);
        }


        /*
        public bool ContainsFingerprintOf(IndigoObject fp)
        {
            int commonBits = indigo.commonBits(this.fingerPrint, fp);
            if (commonBits == fp.countBits()) return true;
            return false;
        }
         */

        public bool IsLargerThan(HasseNode smallobj)
        {

            /*
            if (((ChemHasseNode)smallobj).elementCount() >= this.heavyAtomCount)
            {
                return false; // was same or larger
            }
             */

            // fingerprints compare benchmark: 3 microseconds or 300 per ms

            //bool IsPossiblyLargerThanByFingerprint = (this.BitArrayFingerPrint.ContainsAllBitsOf(smallobj.BitArrayFingerPrint));

            // todo - cache the query form of all mols along with their normal form

            //IndigoObject o = ((ChemHasseNode)smallobj).Molecule().clone(); 

            // molecule.resetSymmetricCisTrans(); // TODOOO
            // molecule.markEitherCisTrans(); //TODOOOO 
            //IndigoObject smallMol = indigo.loadQueryMolecule(o.canonicalSmiles ());

            IndigoObject smallMol = indigo.loadQueryMolecule(smallobj.KeyString);

            SetQueryConstraints(smallMol);
            IndigoObject targetMol = this.molecule;

            // prepare matcher for the target 
            // todo - cache the matcher


            IndigoObject matcher = indigo.substructureMatcher(targetMol, SUBSTRUCTURE_MATCH_PARAM);
            //indigo.setOption("embedding-uniqueness", "atoms");
            // do matching
            //smallMol.clearCisTrans();


            matcher.match(smallMol);

            //System.Diagnostics.Debug.WriteLine(matcher.countMatches(smallMol));   



            return (matcher.countMatches(smallMol) == 0) ? false : true;
        }


        private void SetQueryConstraints(IndigoObject mol)
        {

            // all this typically < 0.1 msec
            List<int> HList = new List<int>();
            //System.Diagnostics.Debug.WriteLine("xxxx" + mol.smiles());
            foreach (IndigoObject atom in mol.iterateAtoms())
            {
                //System.Diagnostics.Debug.WriteLine("atm: " + atom.symbol () );
                atom.removeConstraints("hydrogens"); // critical !!
                //atom.removeConstraints("charge"); // propably desired in general
                //atom.setCharge(0);
                //if (atom.atomicNumber() == 7)
                // {
                //atom.removeConstraints("charge");
                //  atom.addConstraint("smarts", "[NX2,NX3,NX4,NX4H3+,N+]");
                // }

                if (atom.atomicNumber() == 1) HList.Add(atom.index());
                if (atom.atomicNumber() == 6)
                {
                    int sp = 3;
                    foreach (IndigoObject nei in atom.iterateNeighbors())
                    {
                        IndigoObject b = nei.bond();
                        int bo = b.bondOrder();
                        if (bo == 4) { sp = 4; break; } // aromatic
                        if (bo == 3) { sp = 1; break; }
                        if (bo == 2 && sp == 2) { sp = 0; break; } // allene
                        if (bo == 2) { sp = 2; } // sp2, possibly allene..next bond bo can be 1 or 2
                    }
                    // System.Diagnostics.Debug.WriteLine("sp  " + sp);
                    if (sp == 4) // aromatic
                        atom.addConstraint("smarts", "[c]");
                    if (sp == 3) // sp3
                        atom.addConstraint("smarts", "[CD1H3,CD2H2,CD3H1,CD4H0]");
                    if (sp == 2)  // sp2
                        atom.addConstraint("smarts", "[CD1H2,CD2H1,CD3H0]");
                    if (sp == 1) // sp1
                        atom.addConstraint("smarts", "[CD1H1,CD2H0]");
                    if (sp == 0) // allene
                        atom.addConstraint("smarts", "[$([CX2](=X)=X)]");

                }
                //atom.addConstraint("smarts", "blabla")
                //if (atom.atomicNumber() == 7)
                //{
                //    bool isAromatic = false;
                //    int singleBondsCount = 0;
                //    foreach (IndigoObject nei in atom.iterateNeighbors())
                //    {
                //        IndigoObject b = nei.bond();
                //        if (nei.atomicNumber() !=1) substCount ++;
                //        int bo = b.bondOrder();
                //        if (bo == 4) isAromatic = true;
                //    }
                //    if (isAromatic && substCount == 2) atom.addConstraint("smarts", "[nD2H0]");
                //    if (isAromatic && substCount == 2) atom.addConstraint("smarts", "[nD2H0]");

                //}

            }
            mol.removeAtoms(HList); // e.g. to make -n[H]- substructure of -n[C]-  
        }


        public string GetName()
        {
            if (this.molecule != null)
                return this.molecule.name();
            else
                return ("");
        }
        public void SetName(string n)
        {
            this.molecule.setName(n);
        }


        public bool GetMaxCommonFragments(
            HasseNode Lowernode,
    HasseNode Node1, HasseNode Node2,
    bool dbg, HasseFragmentInsertionQueue NewFragmentList,
    int MinimumOverlap)
        {

            if (Node1 == Node2) throw new Exception("Nodes 1 and 2 same in GetMaxCommonFragments");
            countMCS++;
            IndigoObject N1 = ((ChemHasseNode)Node1).Molecule();
            IndigoObject N2 = ((ChemHasseNode)Node2).Molecule();

            System.Diagnostics.Debug.WriteLineIf(dbg, "N1: " + (N1.smiles()));
            System.Diagnostics.Debug.WriteLineIf(dbg, "N2: " + (N2.smiles()));
            //            IndigoObject scaf = getMCS(((ChemHasseNode)Node1).Molecule(), ((ChemHasseNode)Node2).Molecule());
            IndigoObject[] scaffolds = new IndigoObject[2] { N1, N2 };

            IndigoObject scaf = null;
            try
            {
                // note: approx 50 000 gives same as approx 100 000 or exact 100 000
                // exact 100000 is fastest
                scaf = indigo.extractCommonScaffold(scaffolds, MCS_MODE);// 20000000");//"approx 100");
            }
            catch (Exception ex)
            {
                if (ex.ToString().StartsWith("com.ggasoftware.indigo.IndigoException: Molecule Scaffold detection: There are no scaffolds found"))
                {
                    System.Diagnostics.Debug.WriteLine("N1: " + (N1.smiles()));
                    System.Diagnostics.Debug.WriteLine("N2: " + (N2.smiles()));
                    System.Diagnostics.Debug.WriteLine("No scaffold found - exception handled.");
                }
                else if (ex.ToString().StartsWith("com.ggasoftware.indigo.IndigoException: Scaffold detection: scaffold detection exact searching max iteration limit reached"))
                {
                    System.Diagnostics.Debug.WriteLine("iteration limit reached - exception handled.");
                }

                else
                {
                    throw new Exception(ex.ToString());
                }
            }

            List<IndigoObject> ListMCS = new List<IndigoObject>();
            bool hadMCS = false;
            if (scaf != null)
            {
                //try{
                IndigoObject matcher1 = indigo.substructureMatcher(N1, SUBSTRUCTURE_MATCH_PARAM);
                IndigoObject matcher2 = indigo.substructureMatcher(N2, SUBSTRUCTURE_MATCH_PARAM);
                int scafcount = 0;
                // loop all scaffolds
                foreach (IndigoObject scaffold in scaf.allScaffolds().iterateArray())
                {
                    scafcount++;
                    System.Diagnostics.Debug.WriteLineIf(dbg, "scaffold " + scafcount.ToString() + ": " + scaffold.smiles());
                    SetQueryConstraints(scaffold); //TODO check
                    bool entireScaffoldIsOK = false;
                    //IndigoObject m = matcher1.match(scaffold); // align scaffold on one of nodes
                    foreach (IndigoObject m1 in matcher1.iterateMatches(scaffold))
                    {
                        if (m1 != null)
                        {
                            foreach (IndigoObject m2 in matcher2.iterateMatches(scaffold))
                            {
                                if (m2 != null)
                                {
                                    // we must clone matched part because a scaffold is not a mol 
                                    // at the same time, remove partial rings in MSC 
                                    // to do: must we test against both nodes for partial rings
                                    entireScaffoldIsOK = CheckMCS(ListMCS, scaffold, m1, m2, N1, N2);
                                    //if (entireScaffoldIsOK) 
                                    //  break; // was no trimming, we need not check more matches
                                }
                            } //end for each m2
                            if (entireScaffoldIsOK) break;
                        }
                    }// end for each m1
                    if (scafcount == MAX_SCAFFOLDS) break;
                } // foreach scaf


                //------ phase 2 ---------------------------------------

                foreach (IndigoObject o in ListMCS)
                {

                    if (TEST_THAT_MCS_IS_SUBSTRUC)
                    {
                        //             System.Diagnostics.Debug.WriteLine(o.smiles());  
                        IndigoObject q = indigo.loadQueryMolecule(o.smiles());
                        if (matcher1.countMatches(q) == 0)
                        {
                            //System.Diagnostics.Debugger.Break();  
                            continue;
                        }
                        if (matcher2.countMatches(q) == 0)
                        {
                            //System.Diagnostics.Debugger.Break();
                            continue;
                        }

                    }

                    System.Diagnostics.Debug.WriteLineIf(dbg, "MCS in list : " + o.smiles());
                    o.dearomatize(); // seems important!
                    string msg1 = o.checkBadValence();
                    string msg2 = o.checkAmbiguousH();
                    hadMCS = true;

                    // todo check again if smiles works for all molecules
                    o.resetSymmetricCisTrans();
                    string StrScaffold = o.molfile();
                    string debugInfo = "MCS " + Node1.GetID().ToString() + " " + Node2.GetID().ToString();

                    NewFragmentList.Add(
                        new HasseNode[1] { Lowernode }, // this is lower than new frag
                        new HasseNode[2] { Node1, Node2 }, // those are higher than new frag
                        StrScaffold, // string to use for new node creation later
                        debugInfo, HasseNode.HasseNodeTypes.FRAGMENT | HasseNode.HasseNodeTypes.MAX_COMMON_FRAGMENT, // type of frag
                        null // this new frag is not associated with a single edge
                        );
                }

            }
            return hadMCS;
        }

        /*
        public ChemHasseFingerprint CreateFingerprint()
        {
            ChemHasseFingerprint fp = new ChemHasseFingerprint(indigo);
            return fp;
        }
        */

        public string[] GetDifferenceString(HasseNode LargerNode)
        {

            List<string> DiffList = new List<string>(); // for return strings
            IndigoObject LargerMol = ((ChemHasseNode)LargerNode).Molecule();

            //LargerMol.clearCisTrans();

            // Find substructure. target is larger, query is smaller 
            IndigoObject matcher = indigo.substructureMatcher(LargerMol, SUBSTRUCTURE_MATCH_PARAM);
            indigo.setOption("embedding-uniqueness", "atoms");

            //for (int i = 1; i < 10000; i++)
            //{
            //    IndigoObject test = indigo.loadQueryMolecule(this.molecule.smiles());
            //   SetQueryConstraints(test);
            //}
            IndigoObject QueryMol = indigo.loadQueryMolecule(this.molecule.smiles());
            SetQueryConstraints(QueryMol);


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


        private bool hasBrokenRing(IndigoObject scaffold, IndigoObject thisMatch)
        {
            foreach (IndigoObject bond in scaffold.iterateBonds()) //loop bonds of scaffold
            {
                if (thisMatch.mapBond(bond) != null)
                {
                    int top1 = bond.topology();  // bond topology of scaffold
                    int top2 = thisMatch.mapBond(bond).topology(); // bond topology of superstructure 
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


        private bool CheckMCS(List<IndigoObject> Clones, IndigoObject scaffold,
    IndigoObject matchA,
    IndigoObject matchB,
    IndigoObject superstruc1,
    IndigoObject superstruc2)
        {

            //System.Diagnostics.Debug.WriteLine("scaffold " + scaffold.smiles());


            if (matchA == null) return false;
            if (matchB == null) return false;
            IndigoObject clone1 = superstruc1.clone();

            List<int> AtomsToKeep = new List<int>();
            List<int> BondsToKeep = new List<int>();

            int ringCount = scaffold.countSSSR();

            // rule: do not allow aromatic macrocycles
            if (ringCount == 1)
            {
                foreach (IndigoObject ring in scaffold.iterateRings(9, 100)) //
                {
                    foreach (IndigoObject bond in ring.iterateBonds())
                    {
                        if (bond.bondOrder() == 4)
                            return false; // The MCS has a single ring which is nine or more members and aromatics
                        else
                            break; // no need to check more than one bond in the ring for aromaticity
                    }
                }
            }

            int countRemovedBonds = 0;
            int countRemovedAtoms = 0;
            foreach (IndigoObject atom in scaffold.iterateAtoms())
            {
                //rule: if nitrogen has one or more aromatic bonds in the scaffold, then disallow match if different H count
                if (atom.atomicNumber() == 7)
                {
                    bool isAromatic = false;
                    foreach (IndigoObject nei in atom.iterateNeighbors())
                    {
                        if (nei.bond().bondOrder() == 4) isAromatic = true;
                    }
                    if (isAromatic && (matchA.mapAtom(atom).countHydrogens() != matchB.mapAtom(atom).countHydrogens()))
                        return false;
                }

                // Add target atom indexes to the list 
                if (atom.atomicNumber() != 1)
                    AtomsToKeep.Add(matchA.mapAtom(atom).index());
            }


            foreach (IndigoObject bondScaff in scaffold.iterateBonds()) //loop bonds of scaffold
            {
                if (matchA.mapBond(bondScaff) == null || matchB.mapBond(bondScaff) == null)
                {
                    System.Diagnostics.Debugger.Break();
                    countRemovedBonds++;
                    continue;
                }

                IndigoObject bondA = matchA.mapBond(bondScaff);
                IndigoObject bondB = matchB.mapBond(bondScaff);

                if (bondA.bondOrder() != bondB.bondOrder())
                {
                    return false; // the match was bad - not really a match!
                }


                // bond order MUST match from scaffold to superstrucs
                int bo = bondScaff.bondOrder();
                if ((bondA.bondOrder() != bo) || (bondB.bondOrder() != bo))
                {
                    countRemovedBonds++;
                    continue;
                }

                int bondTopologyScaff = bondScaff.topology();  // bond topology of scaffold, always one of RING or UNKNOWN, never CHAIN
                int bondTopologyA = bondA.topology(); // bond topology of superstructure 
                int bondTopologyB = bondB.topology(); // bond topology of superstructure 

                // bond is aromatic but in broken ring 
                if (bo == 4 && bondTopologyScaff != Indigo.RING)
                {
                    return false;
                    countRemovedBonds++;
                    continue;
                }


                // bonds of unbroken rings, always keep
                if (bondTopologyScaff == Indigo.RING && bondTopologyA == Indigo.RING && bondTopologyB == Indigo.RING)
                {
                    BondsToKeep.Add(matchA.mapBond(bondScaff).index());
                    continue;
                }

                // chain bond, OK
                if (bondTopologyScaff == 0 && (bondTopologyA == Indigo.CHAIN && bondTopologyB == Indigo.CHAIN))
                {
                    BondsToKeep.Add(matchA.mapBond(bondScaff).index());
                    continue;
                }


                if ((bondTopologyScaff == 0 || bondTopologyScaff == Indigo.CHAIN)
                    && (bondTopologyA == Indigo.RING || bondTopologyB == Indigo.RING))
                {
                    // bond unknown/chain on scaffold, ring on one or both of superstruc, means bond is from broken ring
                    // usually OK

                    if (SKIP_SCAFFOLD_WITH_BOND_FROM_BROKEN_RING)
                    { return false; }
                    else
                    {
                        BondsToKeep.Add(matchA.mapBond(bondScaff).index());
                        continue;
                    }
                }
                // we get to here if unhandled case
                countRemovedBonds++;
                System.Diagnostics.Debug.WriteLine("unhandled bond matching: scaff " + bondTopologyScaff + ", A " + bondTopologyA + ", B " + bondTopologyB);
                System.Diagnostics.Debugger.Break();
            } // end bond loop


            // -------------  stereo ---------------------------------
           // System.Diagnostics.Debug.WriteLine(scaffold.countAtoms().ToString() + " atoms in scaff " + scaffold.smiles());

            foreach (IndigoObject atom in scaffold.iterateAtoms()) // loop scaffold atoms
            {
                int IdxStruc1 = matchA.mapAtom(atom).index();
                int stereoTypeA = matchA.mapAtom(atom).stereocenterType();
                int stereoTypeB = matchB.mapAtom(atom).stereocenterType();

                int a = Indigo.ABS;
                int b = Indigo.ALLENE;
                int c = Indigo.AND;
                int d = Indigo.CHAIN;
                int e = Indigo.CIS;
                int f = Indigo.DOUBLET;
                int g = Indigo.DOWN;
                int h = Indigo.EITHER;
                //int i = Indigo.Equals ;
                int j = Indigo.OR;
                int k = Indigo.TRANS;


                if (stereoTypeA != 0 || stereoTypeB != 0)
                {
                    bool StereoMatch = IsBothChiralAndSameChiral(atom, scaffold, matchA, matchB);
                    //System.Diagnostics.Debug.WriteLine("atom " + atom.index().ToString() + " stereo same? " + StereoMatch.ToString());

                    if (StereoMatch == false)
                    {
                        List<IndigoObject> terminalAtomsOnChiralcenter = new List<IndigoObject>();

                        RemoveChirality(clone1.getAtom(IdxStruc1));
                        foreach (IndigoObject nei in clone1.getAtom(IdxStruc1).iterateNeighbors())
                        {
                            if (nei.degree() == 1)
                            {
                                terminalAtomsOnChiralcenter.Add(nei);
                            }

                            return false;
                            if (false)
                            // if (terminalAtomsOnChiralcenter.Count() == 1)
                            {
                                IndigoObject atm = terminalAtomsOnChiralcenter[0];
                                System.Diagnostics.Debug.WriteLine("removing atm " + atm.symbol() + " from " + clone1.getAtom(IdxStruc1).symbol());
                                AtomsToKeep.Remove(atm.index());
                                BondsToKeep.Remove(atm.bond().index());
                            }
                        }


                    }
                    else
                    {
                        // Is a stereo match, get on with this MCS..
                    }
                }
            } //------------------ end stereo -----------------------------


            // ===== clone  ===========================
            //clone1.removeAtoms(AtomsToRemove.ToArray()); 

            IndigoObject sub = null;
            try
            {
                sub = clone1.createEdgeSubmolecule(AtomsToKeep.ToArray(), BondsToKeep.ToArray());
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
                return false;
            }

            IndigoObject sub2 = clone1.createSubmolecule(AtomsToKeep.ToArray());
            //System.Diagnostics.Debug.WriteLine(AtomsToKeep.Count().ToString() + " atoms kept from  MCS " + clone1.smiles());

            IndigoObject matchedpart = sub2.clone();

            matchedpart.dearomatize();
            foreach (IndigoObject comp in sub.iterateComponents())
            {
                if (comp.countAtoms() > 1)
                {
                    {
                        if (comp.clone().smiles().Equals("C(N[C@@H](C(C)C)C(O)=O)(C)=O") || comp.clone().smiles().Equals("C(N[C@H](C(C)C)C(O)=O)(C)=O") || comp.clone().smiles().Equals("C(NC(C(C)C)C(O)=O)(C)=O"))
                        {
                            //System.Diagnostics.Debugger.Break();  
                        }
                        Clones.Add(comp.clone());
                    }
                }
            }
            if (countRemovedBonds > 0)
                return false;
            return true;
        }

        private double AngleFromXY(double x, double y)
        {
            //double r = Math.Sqrt(x * x + y * y);
            //r = (x2 + y2)1/2 
            double Atan = Math.Atan(y / x);
            double atan = 0;
            if (y > 0) //upper
            {
                if (x == 0) atan = Math.PI / 2; else if (x > 0) atan = Atan; else atan = Atan + Math.PI;
            }
            else if (y < 0) //lower
            {
                if (x == 0) atan = 3 * Math.PI / 2; else if (x > 0) atan = Atan + 2 * Math.PI; else atan = Atan + Math.PI;
            }
            else
            {
                if (x == 0) atan = double.NaN; else if (x > 0) atan = 0; else atan = Math.PI;
            }
            return (double)360 * atan / (2 * Math.PI);

        }

        private class stereobond
        {
            public double angle;
            public int stereo;
            public int index;
            public string symbol; // for debug only
        };

        class StereoBondComparer : IComparer<stereobond>
        {
            int IComparer<stereobond>.Compare(stereobond x, stereobond y)
            {
                if (x.angle < y.angle) return -1; else return 1;
            }
        }

        private bool IsBothChiralAndSameChiral(IndigoObject scaffoldStereoCenter, IndigoObject scaffold, IndigoObject MatchMapA, IndigoObject MatchMapB)
        {
            float[] aCenterXYZ = new float[3]; // for coordinates of stereocenter a
            float[] bCenterXYZ = new float[3]; // for coordinates of stereocenter b

            float[] aXYZ = new float[3];
            float[] bXYZ = new float[3];


            List<stereobond> aBonds = new List<stereobond>();
            List<stereobond> bBonds = new List<stereobond>();

            aCenterXYZ = MatchMapA.mapAtom(scaffoldStereoCenter).xyz();
            bCenterXYZ = MatchMapB.mapAtom(scaffoldStereoCenter).xyz();


            int idx = 0;
            foreach (IndigoObject nei in scaffoldStereoCenter.iterateNeighbors())
            {

                aXYZ = MatchMapA.mapAtom(nei).xyz(); // coordinates of this neighbor to center a
                bXYZ = MatchMapB.mapAtom(nei).xyz(); // coordinates of this neighbor to center b

                // From cartesian to polar coordinates. We calculate angle relative to x-axis
                stereobond sb1 = new stereobond(); // to keep stereo, angle, and index
                stereobond sb2 = new stereobond(); // to keep stereo, angle, and index

                sb1.angle = AngleFromXY(aXYZ[0] - aCenterXYZ[0], aXYZ[1] - aCenterXYZ[1]);
                sb2.angle = AngleFromXY(bXYZ[0] - bCenterXYZ[0], bXYZ[1] - bCenterXYZ[1]);

                IndigoObject bond1 = MatchMapA.mapBond(nei.bond());
                IndigoObject bond2 = MatchMapB.mapBond(nei.bond());
                sb1.symbol = MatchMapA.mapAtom(nei).symbol();
                sb2.symbol = MatchMapB.mapAtom(nei).symbol();
                sb1.stereo = bond1.bondStereo();
                sb2.stereo = bond2.bondStereo();
                sb1.index = idx;
                sb2.index = idx;

                aBonds.Add(sb1);
                bBonds.Add(sb2);
                //   if (bond1.bondStereo() != bond2.bondStereo()) return false;
                idx++;
            }

            // note: the matching stereocenters may have more bonds than those in the matchmap, 
            // but if so these bonds are not matches!
            if (aBonds.Count < 3) return false;
            if (bBonds.Count < 3) return false;

            // validate has stereo bonds that makes sense
            bool aSteroValid = ValidateTetrahedralStero(aBonds);
            bool bSteroValid = ValidateTetrahedralStero(bBonds);
            if (!aSteroValid) return false;
            if (!bSteroValid) return false;

            // Sort on angles of bonds in plane relative to x-axis 
            StereoBondComparer bc = new StereoBondComparer();
            aBonds.Sort(bc);
            bBonds.Sort(bc);
            AddImplicitHydrogenChirally(aBonds);
            AddImplicitHydrogenChirally(bBonds);
            if (aBonds.Count < 4) return false;
            if (bBonds.Count < 4) return false;
            aBonds.Sort(bc); // sort again
            bBonds.Sort(bc); // sort again

            // pick one index as reference - is bond up or down?


            int aRefBondStereo = getStereoOfBondWithIndex(aBonds, 0);
            int bRefBondStereo = getStereoOfBondWithIndex(bBonds, 0);
            int aRotation = getRotation(aBonds, aBonds[0]);
            int bRotation = getRotation(bBonds, bBonds[0]);

            // rule: if tetrahedral stereocenter:
            // three explicit bonds (one implicit) - allow 1, 2, 3 if same.
            // if two none-stereodefining bonds, put explicit H in widest angle, opposite up/down as existing stereo defining bond

            if (aRefBondStereo == bRefBondStereo)
            {
                if (aRotation == bRotation) return true; else return false;
            }
            else
            {
                if (aRotation != bRotation) return true; else return false;
            }

            return true;
        }

        private int getRotation(List<stereobond> bonds, stereobond bond)
        // assumes sorted on angles
        {
            bool dbg = false;
            if (bonds.Count < 4) System.Diagnostics.Debugger.Break();
            List<stereobond> bonds2 = new List<stereobond>();
            if (dbg) System.Diagnostics.Debug.WriteLine("debug rotation: ");
            foreach (stereobond b in bonds)
            {
                if (!Object.ReferenceEquals(b, bond))
                {
                    if (dbg) System.Diagnostics.Debug.WriteLine("index: " + b.index + " symbol: " + b.symbol);
                    bonds2.Add(b);
                }
            }
            int sum = 0;
            sum += (bonds2[0].index > bonds2[1].index) ? 1 : -1;
            sum += (bonds2[1].index > bonds2[2].index) ? 1 : -1;
            sum += (bonds2[2].index > bonds2[0].index) ? 1 : -1;
            if (dbg) System.Diagnostics.Debug.WriteLine("rotation " + sum.ToString());
            return sum;
        }

        private int getStereoOfBondWithIndex(List<stereobond> bonds, int index)
        {
            // we ask stero of bond ar index
            // assumes bonds are sorted on angle
            // arbitrarily start on first, note if index bond and first up or down bond are same or different odd/even
            int isOdd = -1;
            int indexBondIsOdd = 0;
            int stereoBondIsOdd = 0;
            int s = 0;
            bool dbg = false;
            if (dbg) System.Diagnostics.Debug.WriteLine("debug getStereoOfBondWithIndex:");
            foreach (stereobond b in bonds)
            {
                isOdd *= -1; // 1 for first , then -1,1,-1
                if (dbg) System.Diagnostics.Debug.WriteLine(Math.Round(b.angle, 0).ToString() + " bond to: " + b.symbol + " , index: " + b.index.ToString() + " stereo: " + b.stereo + " ref: " + (b.index == index).ToString());
                if (b.stereo != 0)
                {
                    s = ((b.stereo == 5) ? 1 : -1); // one or minus one
                    stereoBondIsOdd = isOdd;
                }
                if (b.index == index)
                { // this is the bond
                    indexBondIsOdd = isOdd;
                }
            }
            return stereoBondIsOdd * indexBondIsOdd * s;
        }


        private void AddImplicitHydrogenChirally(List<stereobond> bonds)
        {
            int existingstereo = 0;
            stereobond BondWithStereo = new stereobond();
            if (bonds.Count == 2) System.Diagnostics.Debugger.Break();
            double a;
            int countInPlaneBonds = 0;
            if (bonds.Count == 4) return;
            bool dbg = false;
            if (dbg) System.Diagnostics.Debug.WriteLine("debug AddImplicitHydrogenChirally:");

            foreach (stereobond b in bonds)
            {
                if (dbg) System.Diagnostics.Debug.WriteLine(Math.Round(b.angle, 0).ToString() + "\tbond to: " + b.symbol + " , index: " + b.index.ToString() + " stereo: " + b.stereo);

                if (b.stereo == 0)
                    countInPlaneBonds++;
                else
                    existingstereo = b.stereo;
                BondWithStereo = b;
            }

            if (dbg) System.Diagnostics.Debug.WriteLine("In-plane bonds: " + countInPlaneBonds.ToString());

            if (countInPlaneBonds == 0)
            {
                a = bonds[0].angle; // just pick one - the first:
                stereobond sb = new stereobond();
                sb.angle = a;
                sb.stereo = 0;// (existingstereo == 5) ? 6 : 5; 
                sb.index = 3; // check this
                sb.symbol = "H";
                bonds.Add(sb);
            }
            else if (countInPlaneBonds == 1)
            {
                foreach (stereobond b in bonds)
                {
                    if (b.stereo == 0)
                    {
                        a = bonds[0].angle; // just pick one - the first:
                        stereobond sb = new stereobond();
                        sb.angle = a;
                        sb.stereo = 0;// (bonds[0].stereo == 5) ? 6 : 5;
                        sb.index = 3; // check this
                        sb.symbol = "H";
                        bonds.Add(sb);
                        break;
                    }
                }
                a = bonds[0].angle;
                stereobond sb2 = new stereobond();
                sb2.angle = a;
                sb2.stereo = 0;// (existingstereo == 5) ? 6 : 5; 
                sb2.index = 3; // check this
                sb2.symbol = "H";
                bonds.Add(sb2);
            }
            else if (countInPlaneBonds == 2)
            {
                // put new bond in widest angle of existing in-plane bonds
                double AngleBetweenInPlaneBonds = 0;
                if (Object.ReferenceEquals(BondWithStereo, bonds[2])) AngleBetweenInPlaneBonds = bonds[1].angle - bonds[0].angle;
                if (Object.ReferenceEquals(BondWithStereo, bonds[0])) AngleBetweenInPlaneBonds = bonds[2].angle - bonds[1].angle;
                if (Object.ReferenceEquals(BondWithStereo, bonds[1])) AngleBetweenInPlaneBonds = bonds[0].angle - bonds[2].angle;

                if (dbg) System.Diagnostics.Debug.WriteLine("angle between inplane bonds: " + Math.Round(AngleBetweenInPlaneBonds, 1).ToString());

                double newBondAngle = 0;
                if (AngleBetweenInPlaneBonds < 180)
                {
                    // typical case, use close to same angle as the one with up/down stereo
                    newBondAngle = BondWithStereo.angle + 1;
                }
                else
                {
                    newBondAngle = BondWithStereo.angle + 181;
                }
                if (newBondAngle > 360) newBondAngle -= 360;

                stereobond sb2 = new stereobond();
                sb2.angle = newBondAngle;
                sb2.stereo = 0;//(existingstereo == 5) ? 6 : 5;
                sb2.index = 3; // check this
                sb2.symbol = "H";
                bonds.Add(sb2);
            }

        }



        private bool ValidateTetrahedralStero(List<stereobond> bonds)
        {
            if (bonds.Count == 3)
            {
                int previousBondStereo = 0;
                foreach (stereobond b1 in bonds)
                {
                    if (b1.stereo != 0)
                    {
                        if (previousBondStereo != 0 && previousBondStereo != b1.stereo) { return false; }
                        previousBondStereo = b1.stereo;
                    }
                }
            }
            if (bonds.Count == 4)
            {
                int previousBondStereo = 0;
                foreach (stereobond b1 in bonds)
                {
                    if (b1.stereo != 0)
                    {
                        if (previousBondStereo != 0 && previousBondStereo == b1.stereo) { return false; }
                        previousBondStereo = b1.stereo;
                    }
                }
            }
            return true;
        }


        private void RemoveChirality(IndigoObject atm)
        {
            foreach (IndigoObject nei in atm.iterateNeighbors())
            {
                nei.bond().resetStereo();
            }
            atm.resetStereo();
        }

        private IndigoObject cloneMatchedPart(IndigoObject scaffold,
            IndigoObject thisMatch,
            IndigoObject superstructure)
        {
            if (thisMatch == null) return null;

            // Prepare atoms list 
            List<int> L = new List<int>();

            foreach (IndigoObject atom in scaffold.iterateAtoms())
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
                */

                if (atom.atomicNumber() != 1)
                    L.Add(thisMatch.mapAtom(atom).index());
            }
            //int[] atr = L.ToArray();
            IndigoObject sub = superstructure.createSubmolecule(L.ToArray());
            return sub.clone();
        }





    }
}
