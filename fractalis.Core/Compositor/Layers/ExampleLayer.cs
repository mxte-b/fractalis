using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LayerCompositorTest.Compositor.Layers
{
    /// <summary>
    /// This is an example composite layer intended for basic guidance on how to
    /// create a custom, performant layer to use in a <see cref="LayerCompositor"/>.
    /// </summary>
    /// <param name="mix">A custom variable for the layer.</param>
    internal class ExampleLayer(float mix) : CompositeLayer
    {
        // Custom parameter for the layer
        private readonly float _mix = mix;

        // Example modification function
        private static Vector4 ExampleModification(Vector4 v, float mix) => v * mix;

        // The Apply() method is a function that takes the color buffer,
        // and applies the modifications defined in the function.
        /// <inheritdoc/>
        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            // In order to have good performance, we parallelize it
            Parallel.For(0, src.Length, idx =>
            {
                // Acccessing, and modifying the pixel
                dst.Span[idx] = ExampleModification(src.Span[idx], _mix);
            });
        }
    }
}
