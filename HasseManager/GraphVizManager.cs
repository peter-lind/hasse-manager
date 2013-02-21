using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics; 

namespace HasseManager
{
    class GraphVizManager
    {
        public static void RunDot(string infile, string outfile)
        {

            string tempPath = System.IO.Path.GetTempPath(); 

         
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.CreateNoWindow = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.Arguments = @"-Tgif " + infile + " -o " + outfile ; //"<insert command line arguments here>";
            processStartInfo.FileName = @"C:\Program Files\Graphviz 2.28\bin\dot";//<insert tool path here>";

            Process process = new Process();
            process.StartInfo = processStartInfo;
          
                process.Start();

            process.WaitForExit();
            string error = process.StandardError.ReadToEnd();
        }
    }
}
