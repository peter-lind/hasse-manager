/*
HasseManager - a program for construction and mining of Hasse diagrams.
Copyright (C) 2012  Peter Lind

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA. 
 
*/


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HasseManager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

       
       

        private void ButtonWordFragmentation_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.DefaultExt= ".txt";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Tester.test4(openFileDialog1.FileName); 
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string infile = @"c:\temp\testdotfile.dot";
            string outfile = @"c:\temp\graph";
            string extension ="svg";
            string errmsg=GraphVizManager.RunDot ( infile,outfile,extension );
            string msg = "";
            if (errmsg== "")
            {
                msg = "Success. Input file: " + infile + ", output file: " + outfile + "." + extension;
            }
            else
            {
                msg = errmsg;
            }
            System.Windows.Forms.MessageBox.Show(msg, "", MessageBoxButtons.OK);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Tester.chemtest(); 
        }
    }
}
