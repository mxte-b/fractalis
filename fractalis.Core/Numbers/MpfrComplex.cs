using Sdcb.Arithmetic.Mpfr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Numbers
{
    public struct MpfrComplex
    {
        public MpfrFloat Real { get; set; }
        public MpfrFloat Imaginary { get; set; }

        public MpfrFloat MagnitudeSquared
        {
            get
            {
                return Real * Real + Imaginary * Imaginary;
            }
        }

        public MpfrComplex(MpfrFloat r, MpfrFloat i)
        {
            Real = r;
            Imaginary = i;
        }

        public MpfrComplex(string r, string i)
        {
            Real = MpfrFloat.Parse(r);
            Imaginary = MpfrFloat.Parse(i);
        }

        public static MpfrComplex operator +(MpfrComplex a, MpfrComplex b)
        {
            return new MpfrComplex(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

        public Complex ToComplex()
        {
            return new Complex((double)Real, (double)Imaginary);
        }

        //public ScaledComplex ToScaledComplex()
        //{
        //    return new ScaledComplex((FloatExp)Real, (FloatExp)Imaginary);
        //}

        public override string ToString()
        {
            return Real.ToString() + "+" + Imaginary.ToString() + "i";
        }
     }
 }