using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LayerCompositorTest.Compositor.Layers.Tone
{
    /// <summary>
    /// Represents an effect layer that applies gamma correction.
    /// </summary>
    /// <param name="gamma">
    /// The gamma correction value. Values below 1 brighten midtones, while values above 1 darken them.
    /// </param>
    internal class GammaLayer(float gamma) : CompositeLayer
    {
        private readonly float _gamma = gamma;

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            float exp = 1 / _gamma;

            Parallel.For(0, src.Length, idx =>
            {
                var pIn = src.Span[idx];
                ref var pOut = ref dst.Span[idx];

                pOut.X = MathF.Pow(pIn.X, exp);
                pOut.Y = MathF.Pow(pIn.Y, exp);
                pOut.Z = MathF.Pow(pIn.Z, exp);
            });
        }
    }
}
