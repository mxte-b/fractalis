using LayerCompositorTest.Compositor.Layers;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LayerCompositorTest.Compositor.Layers.Tone
{
    /// <summary>
    /// Represents an effect layer that applies exposure adjustment.
    /// </summary>
    /// <param name="exposure">
    /// The exposure value applied to the image. Positive values increase brightness,
    /// while negative values darken the image.
    /// </param>
    internal class ExposureLayer(float exposure) : CompositeLayer
    {
        private readonly float _exposure = exposure;

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            Parallel.For(0, src.Length, idx =>
            {
                dst.Span[idx] = src.Span[idx] * _exposure;
            });
        }
    }
}
