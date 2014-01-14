using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics; 

namespace HasseManager
{

    class GraphVizManager
    {
        public static string RunDot(string infile, string outfile,string extension)
        {
            extension = extension.ToLower (); 
            switch (extension)
            {
                case "pdf": break;
                case "gif": break;
                case "svg": break;
                case "emf": break;
                default: { throw new Exception("this file format not handled: " + extension); }
            }

            string tempPath = System.IO.Path.GetTempPath(); 

         
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.CreateNoWindow = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;
            string fileNameWithExtension = outfile + "." + extension;
            processStartInfo.Arguments = @"-T" + extension + " " + infile + " -o " + fileNameWithExtension ; //"<insert command line arguments here>";
            processStartInfo.FileName = @"C:\Program Files (x86)\Graphviz2.30\bin\dot";//<insert tool path here>";

            Process process = new Process();
            process.StartInfo = processStartInfo;
          
                process.Start();

            process.WaitForExit();
            string error = process.StandardError.ReadToEnd();
            return error;
        }
    }
}
