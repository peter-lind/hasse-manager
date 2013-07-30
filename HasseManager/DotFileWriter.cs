using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HasseManager
{

    public enum labelMode
    {
        NO_LABELS,
        USE_NODE_KEY,
        USE_NODE_LABEL,
        USE_NODE_ID,
        USE_MOLNAME
    }

    class DotFileWriter
    {
        private string FilenameAndPath;
            public bool WriteEdgeLabels=false;
            public bool UseImage = false;
            public labelMode LabelMode = labelMode.NO_LABELS  ;
            public int FilterMaxLevelFromRoot = int.MaxValue;

        private HasseNodeCollection NodeCollection;
        System.IO.StreamWriter DotFile;

        public DotFileWriter(HasseNodeCollection col, string path)
        {
            NodeCollection = col;
            FilenameAndPath = path;
        }


        public void SetLabelsToNumericSequence()
        {
            int Count=0;
            foreach (HasseNode N in NodeCollection.Values) { 
                Count ++;
                N.LabelText = Count.ToString(); 
            }
        }
        public void SetDrawingColors()
        {
            foreach (HasseNode N in NodeCollection.Values)
            {
                if (!N.HasNodeType(HasseNode.HasseNodeTypes.REAL))
                {
                    N.DrawingColor = "green";
                }
                else
                {
                    N.DrawingColor = "blue";
                }
            }

        }

            public void OpenFile(string path) {
                  
                // at least on 64bit windows - critical to use no byte order mark;
                System.Text.Encoding Enc = new System.Text.UTF8Encoding(false, true);
                  //System.Text.Encoding Enc = new System.Text.ASCIIEncoding();
                  
                DotFile = new System.IO.StreamWriter(path, false, Enc);
                  DotFile.WriteLine("digraph G {");
                  //DotFile.Write(@"imagepath=C:\temp\");

              
            if (false){
            //for twopi:
                  DotFile.WriteLine("size = \"20\" ;");
                  DotFile.WriteLine("overlap = \"false\" ;");
                  DotFile.WriteLine("fontname = \"Arial\" ;");
                  DotFile.WriteLine("ranksep = \"0.5\" ;");
                  DotFile.WriteLine("ratio = \"auto\" ;");
                  DotFile.WriteLine("root = \"{}\" ;");
            }

        }


        private void WriteNode(HasseNode N)
        {


            System.Diagnostics.Debug.WriteLine(((ChemHasseNode)N).molname);   
            DotFile.Write(DoubleQuoted(N.KeyString));
            DotFile.Write(" ["); // start node attributes

            switch (LabelMode)
            {
                case labelMode.NO_LABELS:
                    DotFile.Write(" label=\"\" "); //empty label
                    break;
                case labelMode.USE_NODE_ID:
                    DotFile.Write(" label=" + DoubleQuoted(N.GetID().ToString () ) ); 
                    break;

                case labelMode.USE_MOLNAME:
                    if (((ChemHasseNode)N).molname != "")
                    {
                        DotFile.Write(" label=" + DoubleQuoted(((ChemHasseNode)N).molname));
                    }
                    else {
                        DotFile.Write(" label=" + DoubleQuoted(N.GetID().ToString())); 
                    }
                    break;
 
                case labelMode.USE_NODE_KEY:
                    // need not do anything, the node key is shown by default
                    break;

                case labelMode.USE_NODE_LABEL:
                    DotFile.Write(" label=" + DoubleQuoted(N.LabelText));
                    break;

                default:
                    throw new Exception("unhandled labelmode");
            }

            

            DotFile.Write(" color=" + N.DrawingColor + " "); 

            if (UseImage && N.ImageFileName.Length >0  ) {
            DotFile.Write(" image=\"" + N.ImageFileName + "\"");
            }

            DotFile.Write("] "); // end node attributes
            DotFile.WriteLine(";");

        }
        private void WriteEdge(HasseEdge E)
        {
            HasseNode Node1 = E.LowerNode;
            HasseNode Node2 = E.UpperNode;
            if ((Node1.KeyString.Contains("\"")) || (Node2.KeyString.Contains("\"")))
            {
                throw new Exception("disallowed character: double quote");
            }

            DotFile.Write(DoubleQuoted(Node2.KeyString));  // node 2 key

            DotFile.Write(" -> "); // arrow

            DotFile.Write(DoubleQuoted(Node1.KeyString));  // node 1 key

            DotFile.Write("[dir=back ");
            DotFile.Write(" color=" + Node1.DrawingColor + " " );
            if ( this.WriteEdgeLabels)
            {
                DotFile.Write("label=");
                DotFile.Write( DoubleQuoted (E.LabelText));
            }
            DotFile.Write("]");

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
             */ 
            List<HasseNode> Nodes2 = new List<HasseNode>(Nodes);
            Nodes.Clear();
            foreach (HasseNode N in Nodes2)
                if (N.LevelFromRoot <= FilterMaxLevelFromRoot)
                    Nodes.Add(N);
           

            //Nodes.Sort(); // compare is on key strings

            OpenFile(FilenameAndPath);
            System.IO.StreamWriter LogFile = new System.IO.StreamWriter(@"C:\temp\log.txt", false);
            int EdgeCount = 0;
            int VisitCode = new System.Random().Next();

            foreach (HasseNode Node in Nodes)
            {
                LogFile.WriteLine(Node.LabelText + "\t" + Node.KeyString);  
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
            LogFile.Close(); 
            return EdgeCount;
        }
        private string DoubleQuoted(string s){
            return "\"" + s + "\"";
        }
    }
}
