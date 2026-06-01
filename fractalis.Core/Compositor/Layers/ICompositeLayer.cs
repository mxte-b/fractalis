using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LayerCompositorTest.Compositor.Layers
{
    /// <summary>
    /// Represents a composite layer with the ability to apply an effect on an array of colors.
    /// </summary>
    public interface ICompositeLayer
    {
        /// <summary>
        /// Applies the layer to the color array.
        /// </summary>
        /// <param name="src">The source color buffer.</param>
        /// <param name="dst">The destination color buffer.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        public void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height);
    }
}
