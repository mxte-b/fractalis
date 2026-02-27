using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Fractals
{
    public struct IterationResult(int e, double m, bool es = true)
    {
        public bool Escaped = es;
        public int Iteration = e;
        public double Magnitude = m;
    }
}
