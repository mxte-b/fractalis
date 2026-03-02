using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Numbers
{
    public struct BigComplex
    {
        public BigFixed Real { get; set; }
        public BigFixed Imaginary { get; set; }

        public BigFixed MagnitudeSquared
        {
            get
            {
                return Real * Real + Imaginary * Imaginary;
            }
        }

        public BigComplex(BigFixed r, BigFixed i)
        {
            Real = r;
            Imaginary = i;
        }

        public BigComplex(string r, string i)
        {
            Real = new BigFixed(r);
            Imaginary = new BigFixed(i);
        }

        public static BigComplex operator +(BigComplex a, BigComplex b)
        {
            return new BigComplex(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

        public Complex ToComplex()
        {
            return new Complex((double)Real, (double)Imaginary);
        }

        public ScaledComplex ToScaledComplex()
        {
            return new ScaledComplex((FloatExp)Real, (FloatExp)Imaginary);
        }

        public override string ToString()
        {
            return Real.ToString() + "+" + Imaginary.ToString() + "i";
        }
    }
}
