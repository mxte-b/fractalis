namespace fractalis.Core.Fractals
{
    public struct IterationResult(int e, double m, bool es = true)
    {
        public bool Escaped             = es;
        public int Iteration            = e;
        public double MagnitudeSquared  = m;
    }
}
