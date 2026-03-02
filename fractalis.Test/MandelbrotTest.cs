using fractalis.Core;
using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Test
{
    public class MandelbrotTest
    {
        [Fact]
        public void FloatExp_GivesSameResult()
        {
            Mandelbrot m = new Mandelbrot();

            ReferenceOrbit orbit = new ReferenceOrbit();
            m.CalculateReferenceOrbit(Sights.HardestTrip, 100, out orbit);

            Complex delta = new Complex(5e-300, 0);
            int maxIterations = 150000;

            var result = m.IterationPerturbed(delta, maxIterations, in orbit);

            // Result of perturbation with FloatExp
            var resultTest = m.IterationPerturbed(delta, maxIterations, in orbit);

            Assert.Equal(result, resultTest);
        }
    }
}
