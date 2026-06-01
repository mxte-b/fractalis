using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace LayerCompositorTest.Compositor.Layers.Stylistic
{
    /// <summary>
    /// Represents an effect layer that applies chromatic aberration.
    /// </summary>
    /// <param name="rgbDisplacement">
    /// The per-channel RGB displacement amount. Each component controls how far
    /// red, green, and blue channels are shifted from the original pixel position.
    /// </param>
    internal class ChromaticAberrationLayer(Vector3 rgbDisplacement) : CompositeLayer
    {
        private readonly Vector3 _rgbDisplacement = rgbDisplacement;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Mirror(float x)
        {
            if (x < 0) return -x;
            if (x > 1f) return 2f - x;
            return x;
        }

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            float cx = 0.5f;
            float cy = 0.5f;

            Parallel.For(0, src.Length, idx =>
            {
                (float u, float v) = Raster.IndexToUV(idx, width, height);

                // Calculate direction from center
                float dx = u - cx;
                float dy = v - cy;

                var pixel = src.Span[idx];

                float rdu = MathF.FusedMultiplyAdd(dx, _rgbDisplacement.X, u);
                float rdv = MathF.FusedMultiplyAdd(dy, _rgbDisplacement.X, v);
                float gdu = MathF.FusedMultiplyAdd(dx, _rgbDisplacement.Y, u);
                float gdv = MathF.FusedMultiplyAdd(dy, _rgbDisplacement.Y, v);
                float bdu = MathF.FusedMultiplyAdd(dx, _rgbDisplacement.Z, u);
                float bdv = MathF.FusedMultiplyAdd(dy, _rgbDisplacement.Z, v);

                int ri = Raster.UVToIndex(Mirror(rdu), Mirror(rdv), width, height);
                int gi = Raster.UVToIndex(Mirror(gdu), Mirror(gdv), width, height);
                int bi = Raster.UVToIndex(Mirror(bdu), Mirror(bdv), width, height);

                dst.Span[idx].X = src.Span[ri].X;
                dst.Span[idx].Y = src.Span[gi].Y;
                dst.Span[idx].Z = src.Span[bi].Z;
            });
        }
    }
}
