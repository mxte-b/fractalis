using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LayerCompositorTest.Compositor.Layers.Color
{
    /// <summary>
    /// Represent a composite layer for vibrancy effect.
    /// </summary>
    /// <param name="vibrance">The strength of the effect in the range [0,1].</param>
    internal class VibranceLayer(float vibrance) : CompositeLayer
    {
        private readonly float _vibrance = Math.Clamp(vibrance, 0, 1);

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            Parallel.For(0, src.Length, idx =>
            {
                var pixel = src.Span[idx];

                ColorUtility.RGBToHSV_Inplace(ref pixel);

                float vibranceBoost = _vibrance * (1f - pixel.Y);
                pixel.Y = Math.Clamp(pixel.Y + vibranceBoost, 0, 1);

                ColorUtility.HSVToRGB_Inplace(ref pixel);

                dst.Span[idx] = pixel;
            });
        }
    }
}
