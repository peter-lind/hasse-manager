Description

This is software for organisation of data items into a structure that can used for creation of Hasse Diagrams. 

Hasse diagrams are line drawings that visualise relations between data items. Items which are "larger" are drawn higher up in the drawing than smaller items. A line is drawn from Item1 up to Item2 if Item2 is the largest and no other item comes between the two in size.

The technical definition is about binary relations and partially ordered sets (posets), and "larger" generalises to any sort of comparison that can be done between two elements in a set. Hasse diagrams and posets are well studied and there is a lot of related math.

The software here is on-line in the sense that data items are added one at a time, with the data structure is updated for every new data addition. It consists of classes for HasseDiagram, elements in the diagram, solvers etc. There is also a Windows form which is a testbench for the classes.

There is a HasseNode base class implementing the general properties of the elements in a Hasse diagram. Specific types of elementents will use classes that derive from this class. The StringHasseNode is currently used for testing. It is for Hasse diagrams showing relations between strings and substrings. A class for natural numbers with divisibility defining the partial order will be added and used for testing.