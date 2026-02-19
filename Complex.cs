using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtendedNumerics;

namespace fractalis
{
    internal class Complex(double r, double i)
    {
        public double Real = r;
        public double Imaginary = i;

        public double MagnitudeSquared
        {
            get
            {
                return Real * Real + Imaginary * Imaginary;
            }
        }
    }

    internal class BigComplex
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

        public override string ToString()
        {
            return Real.ToString() + "+" + Imaginary.ToString() + "i";
        }
    }
}
