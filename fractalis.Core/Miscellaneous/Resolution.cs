namespace fractalis.Core.Miscellaneous;

public sealed record ResolutionPreset(string Name, Resolution Resolution);

public readonly record struct Resolution(int Width, int Height)
{
    public override string ToString() => $"{Width}x{Height}";
    
    public static readonly IReadOnlyList<ResolutionPreset> CommonResolutions =
    [
        new("HD (720p)",  new Resolution(1280, 720)),
        new("FHD (1080p)", new Resolution(1920, 1080)),
        new("QHD (1440p)", new Resolution(2560, 1440)),
        new("UHD (4K)",    new Resolution(3840, 2160)),
        new("UHD (8K)",    new Resolution(7680, 4320)),
    ];
}