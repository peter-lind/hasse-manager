﻿using System;
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


        public void WriteImageFile(string fname, IndigoObject mol, string format)
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
            //indigo.setOption ("render-image-size",100);
            
            indigo.setOption("render-coloring", true);
            indigo.setOption("render-margins", 10, 10);
            indigo.setOption("render-atom-ids-visible", true);
            mol.layout();
            //indigo.setOption("render-comment", "N-Hydroxyaniline")
            renderer.renderToFile(mol, fname);

            //indigo.setOption("render-output-format", "svg");
            //byte[] svg = renderer.renderToBuffer(rxn);
        }
    }
}
