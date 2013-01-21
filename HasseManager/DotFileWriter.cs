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
        private void WriteEdge(HasseEdge E)
        {
            HasseNode Node1 = E.LowerNode;
            HasseNode Node2 = E.UpperNode ;
            if ((Node1.KeyString.Contains ("\"") ) || (Node2.KeyString.Contains ("\"") )){
            throw new Exception ("disallowed character: double quote");
            }

            DotFile.Write("\""); // opening quote
            if (Node1.NodeType == HasseNode.HasseNodeTypes.ROOT)
            {
                DotFile.Write("{Ø}");
            }
            else
            {
                DotFile.Write(Node1.KeyString); // node 1 label
            }
            DotFile.Write("\""); // closing quote
            DotFile.Write(" -> "); // arrow
            DotFile.Write("\"");   // opening quote
            DotFile.Write(Node2.KeyString);  // node 2 label
            DotFile.Write("\" ");  // closing quote and space
            
            
            DotFile.Write("[label=\"");
            DotFile.Write("X");
            DotFile.Write ("\"]");
            
            DotFile.WriteLine(";");
        }
        public void CloseFile()
        {
            DotFile.WriteLine("}");
            DotFile.Close();
        }

        public void WriteDotFile()
        {
            List<HasseNode> Nodes;
            Nodes = NodeCollection.Values.ToList();
            Nodes.Sort();

            OpenFile("C:\\temp\\testdotfile1.dot");
            foreach (HasseNode Node in Nodes )
            {
                foreach (HasseEdge E in Node.EdgesToCovering )
                {
                    WriteEdge(E);
                }
            }
            CloseFile();
        }
    }
}
