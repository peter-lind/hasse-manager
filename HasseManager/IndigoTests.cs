﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.ggasoftware.indigo;
using System.Collections;

namespace HasseManager
{
    class IndigoTests
    {
        Indigo indigo = new Indigo();
        public void test1()
        {
            //string strQmol= "NC(C1:C:C(C=CCNC2:C:C(C(F)(F)F):C(OC3CCN(C(C)=N)CC3):C:C:2):C:C:C:1)=N";
            //string strQmol = "N(C(=N([H])[H])c1cccc(/C=C/CN(c2ccc(OC3CCN(/C(=N/[H])/C)CC3)c(C(F)(F)F)c2)[H])c1)([H])[H]"; //problem!
           //string strQmol = "NC(C1:C:C(C=CCNC2:C:C(C(F)(F)F):C(OC3CCN(C(C)=N)CC3):C:C:2):C:C:C:1)=N"; // OK

            string strQmol = "[H]/N=C(C)/N1CCCCC1"; //shows problem!
            //string strQmol = "[H]/N=C(C)/N"; // still shows problem
            //string strQmol = "[H]/N=C(C)/C"; // no problem
            //string strQmol = "[H]/N=C(C)/C(C)(C)"; // no problem again
           // string strLmol = "[H]/N=C(C)/N1CCCCC1CCC";
            string strLmol = "N(C(=N([H])[H])c1cccc(C=CCN(C(=O)CCC(=O)OCC)c2ccc(OC3CCN(C(=N[H])C)CC3)c(C(F)(F)F)c2)c1)([H])[H]";
            IndigoObject qmol = indigo.loadQueryMolecule(strQmol );


            IndigoObject mol = indigo.loadMolecule(strLmol);

           // RemoveHydrogenConstraints(qmol);

            System.Diagnostics.Debug.WriteLine("QueryMol 1: " + qmol.smiles());

            //qmol.clearCisTrans();

            //

           string problem= mol.checkAmbiguousH();
           problem =  mol.checkBadValence();

            System.Diagnostics.Debug.WriteLine("QueryMol 2: " + qmol.smiles());
            System.Diagnostics.Debug.WriteLine("Largermol is : " + mol.smiles());


            IndigoObject matcher = indigo.substructureMatcher(mol, ChemHasseNode.SUBSTRUCTURE_MATCH_PARAM);
            indigo.setOption("embedding-uniqueness", "atoms");
            foreach (IndigoObject m in matcher.iterateMatches(qmol))
            {
                System.Diagnostics.Debug.WriteLine(m.highlightedTarget().smiles()); 
            }
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


    }
}