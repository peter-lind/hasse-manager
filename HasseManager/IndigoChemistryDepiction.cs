using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.ggasoftware.indigo;

namespace HasseManager
{
    class IndigoChemistryDepiction
    {
        IndigoRenderer renderer;
        Indigo indigo;
        public IndigoChemistryDepiction(Indigo _indigo)
        {
            indigo = _indigo;
            renderer = new IndigoRenderer(indigo);
        }


        public void WriteImageFile(string fname, IndigoObject mol, string format, string comment,float weight)
        {
            switch (format)
            {
                case "png":{indigo.setOption("render-output-format", "png");break;}
                case "svg":{indigo.setOption("render-output-format", "svg");break;}
                case "pdf":{indigo.setOption("render-output-format", "pdf");break;}
                case "emf":{indigo.setOption("render-output-format", "emf");break;}

                default: throw new Exception("format not handled by Indigo: " + format);
            }

            // http://ggasoftware.com/opensource/indigo/api/options#rendering
            //png, svg, pdf are allowed. On Windows, emf is also allowed.
            indigo.setOption ("render-bond-length","18");
            
            indigo.setOption ("render-relative-thickness",  Math.Round (weight,2).ToString ());
            
            indigo.setOption("render-coloring", true);
            indigo.setOption("render-margins", 1, 1);
            //indigo.setOption("render-atom-ids-visible", true);
            indigo.setOption("render-comment", comment);
            indigo.setOption("render-implicit-hydrogens-visible", false);
            indigo.setOption("render-label-mode", "hetero");
            indigo.setOption("render-comment-font-size", "15");
            indigo.setOption("render-comment-offset", "6");
            mol.layout();
            renderer.renderToFile(mol, fname);

            //indigo.setOption("render-output-format", "svg");
        }
    }
}
