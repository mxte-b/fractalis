namespace fractalis.Core.Numbers
{
    public struct BigComplex
    {
        public BigFloat Real { get; set; }
        public BigFloat Imaginary { get; set; }
        public BigFloat MagnitudeSquared => Real * Real + Imaginary * Imaginary;
        public BigFloat Magnitude => BigFloat.Sqrt(MagnitudeSquared);

        public BigComplex(BigFloat r, BigFloat i)
        {
            Real = r;
            Imaginary = i;
        }

        public BigComplex(double r, double i)
        {
            Real = new BigFloat(r);
            Imaginary = new BigFloat(i);
        }

        public BigComplex(string r, string i)
        {
            Real = new BigFloat(r);
            Imaginary = new BigFloat(i);
        }

        public static BigComplex operator +(BigComplex a, BigComplex b)
        {
            return new BigComplex(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

        public Complex ToComplex()
        {
            return new Complex(Real.ToDouble(), Imaginary.ToDouble());
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