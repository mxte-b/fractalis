using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Numbers
{
    public struct Complex(double r, double i)
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
        public double Magnitude
        {
            get
            {
                return Math.Sqrt(Real * Real + Imaginary * Imaginary);
            }
        }

        public static Complex operator *(Complex a, double b)
        {
            return new Complex(a.Real * b, a.Imaginary * b);
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
}
