using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core
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

        public override string ToString()
        {
            return Real.ToString() + "+" + Imaginary.ToString() + "i";
        }
    }
    internal struct ScaledComplex
    {
        public FloatExp Real;
        public FloatExp Imaginary;

        public ScaledComplex(FloatExp real, FloatExp imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        public ScaledComplex(double real, double imaginary)
        {
            Real = new FloatExp(real, 0);
            Imaginary = new FloatExp(imaginary, 0);
        }

        public static ScaledComplex operator +(ScaledComplex a, ScaledComplex b)
        {
            return new ScaledComplex(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

        public static ScaledComplex operator -(ScaledComplex a, ScaledComplex b)
        {
            return new ScaledComplex(a.Real - b.Real, a.Imaginary - b.Imaginary);
        }

        public static ScaledComplex operator *(ScaledComplex a, ScaledComplex b)
        {
            return new ScaledComplex(a.Real * b.Real - a.Imaginary * b.Imaginary, a.Real * b.Imaginary + a.Imaginary * b.Real);
        }

        public ScaledComplex Squared()
        {
            FloatExp x2 = Real * Real;
            FloatExp y2 = Imaginary * Imaginary;
            FloatExp xy2 = Real * Imaginary * new FloatExp(2.0, 0);

            return new ScaledComplex(x2 - y2, xy2);
        }

        public FloatExp MagnitudeSquared()
        {
            return Real * Real + Imaginary * Imaginary;
        }

        public override string ToString()
        {
            return $"({Real} + {Imaginary}i)";
        }
    }
}
