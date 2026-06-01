using LayerCompositorTest.Compositor.Layers;
using LayerCompositorTest.Compositor.Layers.Color;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace LayerCompositorTest
{
    internal class LayerCompositor(int width, int height)
    {
        private readonly int _width = width;
        private readonly int _height = height;
        private readonly List<CompositeLayer> _layers = [];

        /// <summary>
        /// Appends an <see cref="ICompositeLayer"/> to the compositor.
        /// </summary>
        /// <param name="layer">The layer to append.</param>
        /// <returns>A reference to this instance for chaining.</returns>
        public LayerCompositor AddLayer(CompositeLayer layer)
        {
            Console.WriteLine("Adding layer to the compositor");
            _layers.Add(layer);

            return this;
        }

        public void Apply(Rgba32[] buffer)
        {
            if (_layers.Count == 0) return;

            // Converting to linear color space
            var rent = ArrayPool<Vector4>.Shared.Rent(buffer.Length);
            var rent2 = ArrayPool<Vector4>.Shared.Rent(buffer.Length);

            // Since Rent() can give a larger array than what we ask for,
            // we slice it to the required length.
            var src = rent.AsMemory(0, buffer.Length);
            var dst = rent2.AsMemory(0, buffer.Length);

            try
            {
                Console.WriteLine("to linear");

                // Converting to linear space
                ColorUtility.ToLinearSpace(buffer, src);

                Console.WriteLine("layers");

                // Applying all layers in sequence
                foreach (var layer in _layers)
                {
                    layer.Apply(src, dst, _width, _height);
                    (src, dst) = (dst, src);
                }

                Console.WriteLine("to srgb");

                // Converting back to sRGB
                ColorUtility.TosRGBSpace(src, buffer);
            }
            // Always return the rented array
            finally
            {
                ArrayPool<Vector4>.Shared.Return(rent);
                ArrayPool<Vector4>.Shared.Return(rent2);
            }
        }
    }
}
