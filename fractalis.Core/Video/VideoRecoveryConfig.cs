using fractalis.Core.Miscellaneous;
using fractalis.Core.Renderers;

namespace fractalis.Core.Video
{
    /// <summary>
    /// Configuration for resuming/recovering a video render.
    /// </summary>
    public sealed record VideoRecoveryConfig
    {
        public required string RenderId { get; init; }
        public required string OutputPath { get; init; }
        public required VideoMode VideoMode { get; init; }
        public required VideoConfig VideoConfig { get; init; }
        public required FractalRendererConfig FractalRendererConfig { get; init; }
        public DistributedRendererConfig? DistributedRendererConfig { get; init; } = null;
    }
}
