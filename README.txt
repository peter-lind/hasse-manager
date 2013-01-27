Description

This is software for organisation of data items into a structure that can used for creation of Hasse Diagrams. 

Hasse diagrams are line drawings that visualise relations between data items. Items which are "larger" are drawn higher up in the drawing than smaller items. A line is drawn from Item1 up to Item2 if Item2 is the largest and no other item comes between the two in size.

The mathematical definition is about binary relations and partially ordered sets (posets).   "Larger" generalises to any sort of comparison that can be done between two elements in a set. Hasse diagrams and posets are well studied and there is a lot of related math.

The software here is on-line in the sense that data items are added one at a time, with the data structure is updated for every new data addition. It consists of classes for HasseDiagram, elements in the diagram, solvers etc. There is also a Windows form which is a testbench for the classes.

It is possible to output files in DOT format, which is used to create drawings with Graphviz (http:\\www.graphviz.org).

There is a HasseNode base class implementing the general properties of the elements in a Hasse diagram. Specific types of elementents will use classes that derive from this class. The StringHasseNode is used for testing. It is for Hasse diagrams showing relations between strings and substrings.

The motivation is from chemoinformatics where Hasse diagrams based on substructture/superstructure relations should be useful. Support for chemistry will be added as soon as main functions are implemented and stable. One idea is to add to the diagram common substructures of pairs of molecules. This should open up for new ways to analyse and characterise libraries of structures.