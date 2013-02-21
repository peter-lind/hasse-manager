using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HasseManager
{
    class DotFileWriter
    {
        private HasseNodeCollection NodeCollection;
        System.IO.StreamWriter DotFile;

        public DotFileWriter(HasseNodeCollection col) {
            NodeCollection = col;
        }
        public void OpenFile(string path) {
                  System.Text.Encoding Enc = new System.Text.UTF8Encoding(true, true);
                  DotFile = new System.IO.StreamWriter(path, false, Enc);
                  DotFile.WriteLine("digraph G ");
                  DotFile.WriteLine("{");
                  //DotFile.WriteLine("size =\"4,5\";");
        }


        private void WriteNode(HasseNode N)
        {
            DotFile.Write(DoubleQuoted(N.KeyString));
            if (!N.HasNodeType(HasseNode.HasseNodeTypes.REAL) ){  
            DotFile.WriteLine(" [style=dashed] ;");
            }
        }
        private void WriteEdge(HasseEdge E)
        {
            HasseNode Node1 = E.LowerNode;
            HasseNode Node2 = E.UpperNode ;
            if ((Node1.KeyString.Contains ("\"") ) || (Node2.KeyString.Contains ("\"") )){
            throw new Exception ("disallowed character: double quote");
            }

            DotFile.Write(DoubleQuoted(Node2.KeyString));  // node 2 label

            DotFile.Write(" -> "); // arrow

            DotFile.Write(DoubleQuoted(Node1.KeyString));  // node 1 label
                         
            DotFile.Write("[dir=back label=\"");
            DotFile.Write(E.LabelText  );
            DotFile.Write ("\"]");
            
            DotFile.WriteLine(";");
        }
        public void CloseFile()
        {
            DotFile.WriteLine("}");
            DotFile.Close();
        }

        public int WriteDotFile()
        {


            List<HasseNode> Nodes;
            Nodes = NodeCollection.Values.ToList();

            /*
            Nodes = new List<HasseNode> ();
            foreach (HasseNode N in NodeCollection.Values )
            {
                if (N.KeyString.Equals ("discrimination" ) ){
                    N.GetThisAndAllAbove(Nodes,0);
                }
            }
            List<HasseNode> Nodes2 = new List<HasseNode>(Nodes);
            foreach (HasseNode N in Nodes2)
                N.GetThisAndAllBelow(Nodes, 0);
            */


            Nodes.Sort();

            OpenFile("C:\\temp\\testdotfile1.dot");
            int EdgeCount = 0;
            int VisitCode = new System.Random().Next();

            foreach (HasseNode Node in Nodes)
            {
                WriteNode(Node);
            }            

            foreach (HasseNode Node in Nodes )
            {
                foreach (HasseEdge E in Node.EdgesToCovered )               
                {
                    if (!E.IsVisited(VisitCode)) // avoid duplication
                    {
                        WriteEdge(E);
                        E.Visit(VisitCode);
                        EdgeCount++;
                    }
                }
            }
            CloseFile();
            return EdgeCount;
        }
        private string DoubleQuoted(string s){
            return "\"" + s + "\"";
        }
    }
}
