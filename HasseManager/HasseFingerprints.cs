using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.ggasoftware.indigo;
using System.Collections;

namespace HasseManager
{
    public  class HasseFingerprint
    {
        public int bitCount;
        public System.Collections.BitArray fp;


        public HasseFingerprint CloneHasseFingerprint()
        {
            BitArray bits = new BitArray(fp); //copies values
            HasseFingerprint chp = new HasseFingerprint();
            chp.fp = bits;
            chp.bitCount = this.bitCount;
            return chp;
        }


        protected int CountBits()
        {
            int cnt = 0;
            for (int i = 0; i < fp.Count; i++)
            //foreach (bool bit in fp)
            {
                //if (bit==true)
                if (fp[i] == true)
                    cnt++;
            }
            return cnt;
        }

        public string ToHex()
        {
            byte[] bytes = new byte[fp.Length/8];
            fp.CopyTo ( bytes,0);
            //return System.Text.Encoding.ASCII.GetString(bytes);
            StringBuilder sb = new StringBuilder(bytes.Length);
            foreach (byte b in bytes)
            {
                int c = Convert.ToInt32(b);
                int b1 = c / 16;
                int b2 = c % 16;
                int test = b1 * 16 + b2;
                if (test != c) System.Diagnostics.Debugger.Break();   
                //System.Diagnostics.Debug.WriteLine((int) 'A');
                //System.Diagnostics.Debug.WriteLine((int)'0');
               sb.Append((char) (65 + b1));
               sb.Append((char) (65 + b2));
            }
            return sb.ToString();
        }

        public void FromHex(string str)
        {

            //int[] integers = new int[byte[] bytes];
            byte[] bytes= new  byte[str.Length/2];
            fp = new BitArray (str.Length/2);
            for (int i = 0; i < str.Length;i+=2)
            {
                int int1= (int)str[i]-65;
                int int2 = (int)str[i+1]-65;
                bytes[i / 2] = (byte)((int1 * 16 + int2));
            }
            //byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);
            //byte[] bytes = BitConverter.GetBytes (  
            fp = new BitArray (bytes);
            this.bitCount = this.CountBits();
        }

        public  void AddBitsOf(HasseFingerprint F)
        {

            BitArray fp2 = ((HasseFingerprint)F).fp;


            if (fp == null)
            {
                fp = new System.Collections.BitArray(fp2);
            }
            else
            {
                fp.Or(fp2);
            }
            this.bitCount = this.CountBits();
        }

        public bool IsLargerThan(HasseFingerprint F)
        {

     //       if (HasAllBits == true) System.Diagnostics.Debugger.Break();
            if (this.bitCount <= F.bitCount) return false; // this must be larger
            bool HasAllBits = ContainsAllBitsOf(F);
            if (HasAllBits ) return true;
            return false;
        }


        public void AndBits(HasseFingerprint FP)
        {
            fp.And(FP.fp );
            this.bitCount = this.CountBits();
        }

        public void Minus(HasseFingerprint FP)
        {
            // do this AND (NOT that)
            BitArray fpx= new BitArray (FP.fp);
            fpx.Not ();
            fp.And(fpx);
            this.bitCount = this.CountBits();
        }

        public bool ContainsAllBitsOf(HasseFingerprint F)
        {
            BitArray fp2 = ((HasseFingerprint)F).fp;
            //todo: think is this good choice:
            // for now think F is null nad has no bits then all fp "contains null"
            // if (fp2 == null) return true;

            
            for (int i = 0; i < fp.Count; i++)
            {
                if (fp2[i] == true)
                {
                    if (fp[i] != true) return false;
                }
            }
            

            /*
            BitArray this_clone = new BitArray(fp);
            this_clone.Not(); //zeroes become ones
            this_clone.And(fp2); // a one at any position results in false 
            foreach (bool bit in this_clone )
                {
                    if (bit==true) return false;
                }
            */

            return true;
        }
    }


    public class ChemHasseFingerprint : HasseFingerprint
    {
        // uses a BitArray
        Indigo indigo;

        public ChemHasseFingerprint(Indigo _indigo)
        {
            indigo = _indigo;
        }
        public ChemHasseFingerprint(Indigo _indigo, BitArray _fp)
        {
            indigo = _indigo;
            fp = _fp;
            base.bitCount = base.CountBits();
        }
    }

    public class StringHasseFingerprint : HasseFingerprint
    {
    }


}
