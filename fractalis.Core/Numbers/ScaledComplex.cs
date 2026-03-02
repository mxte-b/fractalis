using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Numbers
{
    public struct ScaledComplex
    {
        public FloatExp Real;
        public FloatExp Imaginary;

        public FloatExp MagnitudeSquared
        {
            get
            {
                return Real * Real + Imaginary * Imaginary;
            }
        }
        public FloatExp Magnitude
        {
            get
            {
                return (Real * Real + Imaginary * Imaginary).Sqrt();
            }
        }

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

        public static ScaledComplex operator *(ScaledComplex a, double b)
        {
            return new ScaledComplex(a.Real * b, a.Imaginary * b);
        }

        public static ScaledComplex operator *(double a, ScaledComplex b)
        {
            return new ScaledComplex(b.Real * a, b.Imaginary * a);
        }
        public override string ToString()
        {
            return $"{(double)Real} {(Imaginary >= 0 ? "+" : "-")} {Math.Abs(Imaginary)}i";
        }
    }
}
