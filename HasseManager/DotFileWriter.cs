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
        private void WriteEdge(HasseNode Node1,HasseNode Node2)
        {
            if ((Node1.UniqueString.Contains ("\"") ) || (Node2.UniqueString.Contains ("\"") )){
            throw new Exception ("disallowed character: double quote");
            }

            DotFile.Write("\"");
            DotFile.Write(Node1.UniqueString);
            DotFile.Write("\"");
            DotFile.Write(" -> ");
            DotFile.Write("\"");
            DotFile.Write(Node2.UniqueString);
            DotFile.Write("\"");
            DotFile.WriteLine(";");
        }
        public void CloseFile()
        {
            DotFile.WriteLine("}");
            DotFile.Close();
        }

        public void WriteDotFile()
        {
            OpenFile("C:\\temp\\testdotfile1.dot");
            foreach (HasseNode Node in NodeCollection.Values )
            {
                foreach (HasseNode Node2 in Node.NodesCovering().Values)
                {
                    //if (Node.NodeType != HasseNode.HasseNodeTypes.ELEMENT)  
                    WriteEdge(Node, Node2);
                }
            }
            CloseFile();
        }
    }
}
