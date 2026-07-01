using System.Text.Json.Serialization;

namespace fractalis.Core.Video
{
    /// <summary>
    /// Represents a range of frames.
    /// </summary>
    public sealed record FrameRange
    {
        public required int Start { get; init; }
        public required int Count { get; init; }

        [JsonIgnore]
        public int End => Start + Count - 1;

        /// <summary>
        /// Creates a list of frame ranges based on a list of integer indicies.
        /// </summary>
        /// <param name="indicies">The list of indicies.</param>
        /// <returns>The constructed list of frame ranges.</returns>
        public static List<FrameRange> FromIndicies(IEnumerable<int> indicies)
        {
            List<FrameRange> result = [];

            var sorted = indicies.Order().ToList();

            int start = 0;
            int end = 0;

            while (end < sorted.Count)
            {
                // Move end index until the difference is not 1
                while (end + 1 < sorted.Count && sorted[end + 1] - sorted[end] == 1) end++;

                int count = end - start + 1;

                result.Add(new FrameRange()
                {
                    Start = sorted[start],
                    Count = count
                });

                start += count;
                end = start;
            } 

            return result;
        }

        /// <summary>
        /// Inverts a list of frame ranges.
        /// </summary>
        /// <param name="ranges">The frame ranges to invert.</param>
        /// <param name="minRangeValue">The minimum range value.</param>
        /// <param name="maxRangeValue">The maximum range value.</param>
        /// <returns>The inverted ranges.</returns>
        public static List<FrameRange> Invert(List<FrameRange> ranges, int minRangeValue, int maxRangeValue)
        {
            List<FrameRange> inverted = [];

            // Empty range list
            if (ranges.Count == 0)
            {
                inverted.Add(new FrameRange
                {
                    Start = minRangeValue,
                    Count = maxRangeValue - minRangeValue + 1
                });

                return inverted;
            }

            // Check if the first range starts at minRangeValue
            var first = ranges[0].Start;
            if (first > minRangeValue)
            {
                inverted.Add(new FrameRange
                {
                    Start = minRangeValue,
                    Count = first - minRangeValue
                });
            }

            // Invert all other ranges
            for (int i = 0; i < ranges.Count; i++)
            {
                var range = ranges[i];
                var next = i + 1 < ranges.Count ? ranges[i + 1] : null;
                
                if (next is not null && next.Start - range.End != 1)
                {
                    inverted.Add(new FrameRange 
                    {
                        Start = range.End + 1,
                        Count = next.Start - range.End - 1
                    });
                }
                else if (range.End < maxRangeValue)
                {
                    inverted.Add(new FrameRange
                    {
                        Start = range.End + 1,
                        Count = maxRangeValue - range.End
                    });
                }
            }

            return inverted;
        }

        public override string ToString()
        {
            return $"FrameRange[Start={Start}, Count={Count}, End={End}]";
        }
    }
}
