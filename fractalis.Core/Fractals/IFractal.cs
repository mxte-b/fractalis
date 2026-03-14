using fractalis.Core.Numbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
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
        IterationResult IterationPerturbed(double deltaR, double deltaI, int maxIterations, in ReferenceOrbit referenceOrbit);
        (IterationResult r0, IterationResult r1, IterationResult r2, IterationResult r3) IterationPerturbedSIMD(Vector256<double> ndcX, double ndcY, double pixelSpacing, int maxIterations, in ReferenceOrbit referenceOrbit);
        IterationResult IterationFloatExp(ScaledComplex delta, int maxIterations, in ReferenceOrbit referenceOrbit);
    }
}
