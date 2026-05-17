using fractalis.Core.Video;

namespace fractalis.Core.Miscellaneous;

public enum AppMode
{
    Image,
    Video,
    Benchmark
}

public record AppSettings
{
    public required AppMode                 Mode                    { get; init; }
    public required FractalRendererConfig   FractalRendererConfig   { get; init; }
    public VideoConfig?                     VideoConfig             { get; init; }
}