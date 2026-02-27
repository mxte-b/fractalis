using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Fractals
{
    public interface IFractal
    {
        IterationResult Iteration(Complex c, int maxIterations);
        double GetContinousValue(IterationResult result);
    }

    public interface IPerturbableFractal : IFractal
    {
        void CalculateReferenceOrbit(BigComplex center, int maxIterations, out ReferenceOrbit referenceOrbit);
        IterationResult IterationPerturbed(Complex delta, int maxIterations, in ReferenceOrbit referenceOrbit);
    }
}
