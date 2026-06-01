using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace LayerCompositorTest.Compositor.Layers.Stylistic
{
    /// <summary>
    /// Represents an effect layer that applies a bloom (glow) effect.
    /// </summary>
    internal class BloomLayer : CompositeLayer
    {
        private readonly float _intensity;
        private readonly float _bloomStart;
        private readonly float _bloomEnd;
        private readonly int _radius;
        private readonly float[] _weights;
        private readonly float _weightSum;

        private static readonly Vector4 _luma = new(0.2126f, 0.7152f, 0.0722f, 0.0f);

        /// <summary>
        /// Initializes a new instance of the bloom layer.
        /// </summary>
        /// <param name="intensity">
        /// The overall strength of the bloom effect.
        /// </param>
        /// <param name="radius">
        /// The radius of the blur kernel used for the bloom spread.
        /// </param>
        /// <param name="bloomStart">
        /// The luminance threshold where bloom begins to appear.
        /// </param>
        /// <param name="bloomEnd">
        /// The luminance level at which bloom reaches full intensity.
        /// </param>
        /// <param name="sigma">
        /// The standard deviation used for Gaussian weighting of the blur kernel.
        /// </param>
        public BloomLayer(float intensity = 1f, int radius = 6, float bloomStart = 0.6f, float bloomEnd = 0.8f, float sigma = 8f)
        {
            _intensity = intensity;
            _radius = radius;
            _bloomStart = bloomStart;
            _bloomEnd = bloomEnd;

            // Precalculate weight values
            _weights = new float[radius * 2 + 1];

            float sum = 0;
            for (int i = -radius; i <= radius; i++)
            {
                float w = MathF.Exp(-(i * i) / (2f * sigma * sigma));

                sum += w;
                _weights[i + radius] = w;
            }

            _weightSum = sum;
        }

        private static float SmoothStep(float edge0, float edge1, float x)
        {
            x = Math.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
            return x * x * (3.0f - 2.0f * x);
        }

        private void LumaThreshold(Memory<Vector4> src, Memory<Vector4> dst)
        {
            Parallel.For(0, src.Length, idx =>
            {
                var pixel = src.Span[idx];

                float luma = Vector4.Dot(pixel, _luma);
                float bloom = SmoothStep(_bloomStart, _bloomEnd, luma);

                dst.Span[idx] = pixel * new Vector4(bloom, bloom, bloom, 1.0f);
            });
        }

        private void GaussianH(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            var weights = _weights;
            float weightSum = _weightSum;
            int radius = _radius;

            Parallel.For(0, height, y =>
            {
                int rowOffset = y * width;
                var srcSpan = src.Span;
                var dstSpan = dst.Span;

                for (int x = 0; x < width; x++)
                {
                    Vector4 sum = Vector4.Zero;

                    for (int i = -radius; i <= radius; i++)
                    {
                        int sx = Math.Clamp(x + i, 0, width - 1);
                        sum += srcSpan[rowOffset + sx] * weights[i + radius];
                    }

                    dstSpan[rowOffset + x] = sum / weightSum;
                }
            });
        }

        private void GaussianV(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            var weights = _weights;
            float weightSum = _weightSum;
            int radius = _radius;

            Parallel.For(0, height, y =>
            {
                int rowOffset = y * width;
                var srcSpan = src.Span;
                var dstSpan = dst.Span;

                for (int x = 0; x < width; x++)
                {
                    Vector4 sum = Vector4.Zero;

                    for (int i = -radius; i <= radius; i++)
                    {
                        int sy = Math.Clamp(y + i, 0, height - 1);
                        sum += srcSpan[sy * width + x] * weights[i + radius];
                    }

                    dstSpan[rowOffset + x] = sum / weightSum;
                }
            });
        }

        public override void Apply(Memory<Vector4> src, Memory<Vector4> dst, int width, int height)
        {
            var thresholdRent = ArrayPool<Vector4>.Shared.Rent(src.Length);
            var blurRent = ArrayPool<Vector4>.Shared.Rent(src.Length);

            var threshold = thresholdRent.AsMemory(0, src.Length);
            var blur = blurRent.AsMemory(0, src.Length);

            try
            {
                LumaThreshold(src, threshold);
                GaussianH(threshold, blur, width, height);
                GaussianV(blur, threshold, width, height);

                float intensity = _intensity;
                Parallel.For(0, src.Length, idx =>
                {
                    dst.Span[idx] = src.Span[idx] + threshold.Span[idx] * new Vector4(intensity, intensity, intensity, 0f);
                });
            }
            finally
            {
                ArrayPool<Vector4>.Shared.Return(thresholdRent);
                ArrayPool<Vector4>.Shared.Return(blurRent);
            }
        }
    }
}
