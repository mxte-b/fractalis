using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Miscellaneous
{
    public sealed record IterationPreset(string Name, int Value);

    public static class Iteration
    {
        public static readonly IReadOnlyList<IterationPreset> IterationPresets = 
        [
            new("Draft   —     1,000 ",         1_000),
            new("Low     —     4,000",          4_000),
            new("Medium  —    16,000",          16_000),
            new("High    —    80,000",          80_000),
            new("Ultra   —   200,000",          200_000),
            new("Custom  —   enter a value",    int.MinValue),
        ];
    }
}
