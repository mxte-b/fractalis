using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace LayerCompositorTest.Compositor.Layers.Stylistic
{
    internal static class Raster
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static (float, float) IndexToUV(int index, int width, int height)
        {
            return (
                (index % width) / (float)width,
                (index / width) / (float)height
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int UVToIndex(float u, float v, int width, int height)
        {
            int x = Math.Clamp((int)(u * width), 0, width - 1);
            int y = Math.Clamp((int)(v * height), 0, height - 1);
            return y * width + x;
        }
    }
}
