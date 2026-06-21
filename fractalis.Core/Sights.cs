using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using System.Text.Json.Serialization;

namespace fractalis.Core
{
    public sealed record Sight
    {
        public required string Name { get; init; }
        public required BigComplex Location { get; init; }
        public required FractalType FractalType { get; init; }
        public required FractalParameters FractalParameters { get; set; }
        public required int FractionalDigits { get; init; }

        [JsonIgnore]
        public BigFloat MaxRange => BigFloat.Pow(BigFloat.Ten, new BigFloat(Math.Min(FractionalDigits, BigFloat.Precision)));
    }

    /// <summary>
    /// Contains predefined <see cref="BigComplex"/> constants for various sights (coordinates).
    /// </summary>
    public static class Sights
    {
        public static readonly IReadOnlyList<Sight> All = ResourceManager.Instance.Sights.Values.ToList();
    }
}
