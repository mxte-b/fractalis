using fractalis.Core.Renderers;
using fractalis.Core.Video;

namespace fractalis.Core.Miscellaneous
{
    /// <summary>Defines the operating mode of the application.</summary>
    public enum AppMode
    {
        /// <summary>Renders a single image.</summary>
        Image,

        /// <summary>Renders a video.</summary>
        Video,

        /// <summary>Runs renderer performance benchmarks.</summary>
        Benchmark
    }

    /// <summary>Defines how video rendering is executed.</summary>
    public enum VideoMode
    {
        /// <summary>Renders frames locally.</summary>
        Local,

        /// <summary>Renders frames using distributed workers.</summary>
        Distributed
    }

    /// <summary>Application startup and rendering configuration.</summary>
    public record AppSettings
    {
        /// <summary>Primary operating mode of the application.</summary>
        public required AppMode                 Mode                        { get; init; }

        /// <summary>Fractal renderer configuration.</summary>
        public required FractalRendererConfig   FractalRendererConfig       { get; init; }

        /// <summary>Whether to automatically open the rendered image. This setting is only adhered to in <see cref="AppMode.Image"/>.</summary>
        public bool                             OpenRenderedImage           { get; init; } = true;

        /// <summary>The path where the app will save the result. Can only be null in Benchmark mode. </summary>
        public string?                          OutputPath                  { get; init; }

        /// <summary>Video rendering configuration.</summary>
        public VideoConfig?                     VideoConfig                 { get; init; }

        /// <summary>Video rendering execution mode.</summary>
        public VideoMode?                       VideoMode                   { get; init; }

        /// <summary>Distributed renderer settings.</summary>
        public DistributedRendererConfig?       DistributedRendererSettings { get; init; }

        /// <summary>Fractal benchmark settings.</summary>
        public FractalBenchmarkConfig?          FractalBenchmarkConfig      { get; init; }
    }
}