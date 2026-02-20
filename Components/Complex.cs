using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Components
{
    internal struct Complex(double r, double i)
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

        public static Complex operator *(Complex a, Complex b)
        {
            return new Complex(a.Real * b.Real - a.Imaginary * b.Imaginary, a.Real * b.Imaginary + a.Imaginary * b.Real);
        }

        public static Complex operator +(Complex a, Complex b)
        {
            return new Complex(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

        public static Complex operator -(Complex a, Complex b)
        {
            return new Complex(a.Real - b.Real, a.Imaginary - b.Imaginary);
        }

        public static Complex operator *(double a, Complex b)
        {
            return new Complex(b.Real * a, b.Imaginary * a);
        }

        public override string ToString()
        {
            return Real.ToString() + "+" + Imaginary.ToString() + "i";
        }
    }

    internal struct BigComplex
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

        public override string ToString()
        {
            return Real.ToString() + "+" + Imaginary.ToString() + "i";
        }
    }
}
