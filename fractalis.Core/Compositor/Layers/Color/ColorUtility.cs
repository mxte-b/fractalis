using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace fractalis.Core.Compositor.Layers.Color
{
    /// <summary>
    /// Provides high-performance color space conversion utilities.
    /// </summary>
    internal static class ColorUtility
    {
        /// <summary>
        /// Converts an RGB color to HSV in place.
        /// </summary>
        /// <param name="v">
        /// The color vector containing RGB components. After conversion,
        /// the X, Y, and Z components contain hue, saturation, and value.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RGBToHSV_Inplace(ref Vector4 v)
        {
            float K = 0;

            if (v.Y < v.Z)
            {
                (v.Y, v.Z) = (v.Z, v.Y);
                K = -1f;
            }

            if (v.X < v.Y)
            {
                (v.X, v.Y) = (v.Y, v.X);
                K = -2f / 6f - K;
            }

            float chroma = v.X - MathF.Min(v.Y, v.Z);

            float r = v.X;
            v.X = MathF.Abs(K + (v.Y - v.Z) / (6f * chroma + 1e-20f));
            v.Y = chroma / (r + 1e-20f);
            v.Z = r;
        }

        /// <summary>
        /// Converts an HSV color to RGB in place.
        /// </summary>
        /// <param name="v">
        /// The color vector containing HSV components. After conversion,
        /// the X, Y, and Z components contain red, green, and blue values.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void HSVToRGB_Inplace(ref Vector4 v)
        {
            float h = v.X * 6;
            int i = (int)h;
            float frac = h - i;

            float p = v.Z * (1 - v.Y);
            float q = v.Z * (1 - frac * v.Y);
            float t = v.Z * (1 - (1 - frac) * v.Y);

            switch (i % 6)
            {
                case 0: v.X = v.Z; v.Y = t; v.Z = p; break;
                case 1: v.X = q; v.Y = v.Z; v.Z = p; break;
                case 2: v.X = p; v.Y = v.Z; v.Z = t; break;
                case 3: v.X = p; v.Y = q; break;
                case 4: v.X = t; v.Y = p; break;
                case 5: v.X = v.Z; v.Y = p; v.Z = q; break;
            }
        }

        /// <summary>
        /// Converts a single byte value from sRGB to linear space.
        /// </summary>
        /// <param name="c">The sRGB byte to conver.</param>
        /// <returns>The value in linear space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ConvertToLinear(byte c)
            => c / 255.0f is var t && t < 0.04045 ? t / 12.92f : MathF.Pow((t + 0.055f) / 1.055f, 2.4f);

        /// <summary>
        /// Converts a single linear value to sRGB space.
        /// </summary>
        /// <param name="t">The linear value to convert</param>
        /// <returns>The value in sRGB space.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte ConvertTosRGB(float t)
        {
            float c = t < 0.0031308f ? t * 12.92f : 1.055f * MathF.Pow(t, 1f / 2.4f) - 0.055f;
            return (byte)Math.Clamp(c * 255f, 0, 255);
        }

        /// <summary>
        /// Converts a span of sRGB values to linear space and puts it in a preallocated vector span.
        /// </summary>
        /// <param name="buffer">The sRGB values.</param>
        /// <param name="linear">The preallocated span for the linear values.</param>
        public static void ToLinearSpace(Rgba32[] buffer, Memory<Vector4> linear)
        {
            Parallel.For(0, buffer.Length, idx =>
            {
                var pixel = buffer[idx];
                ref var outPixel = ref linear.Span[idx];

                outPixel.X = ConvertToLinear(pixel.R);
                outPixel.Y = ConvertToLinear(pixel.G);
                outPixel.Z = ConvertToLinear(pixel.B);
                outPixel.W = ConvertToLinear(pixel.A);
            });
        }

        /// <summary>
        /// Converts a span of linear colors to sRGB and stores them in the preallocated RGB span.
        /// </summary>
        /// <param name="linear">The linear values.</param>
        /// <param name="buffer">The preallocated span for the sRGB values.</param>
        public static void TosRGBSpace(Memory<Vector4> linear, Rgba32[] buffer)
        {
            Parallel.For(0, linear.Length, idx =>
            {
                var pixel = linear.Span[idx];

                buffer[idx].R = ConvertTosRGB(pixel.X);
                buffer[idx].G = ConvertTosRGB(pixel.Y);
                buffer[idx].B = ConvertTosRGB(pixel.Z);
                buffer[idx].A = ConvertTosRGB(pixel.W);
            });
        }
    }
}
