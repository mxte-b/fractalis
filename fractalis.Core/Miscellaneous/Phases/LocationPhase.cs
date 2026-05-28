using fractalis.Core.Fractals;
using fractalis.Core.Numbers;

namespace fractalis.Core.Miscellaneous.Phases
{
    public record LocationPhaseResult(Sight Sight, BigFloat Zoom);

    public class LocationPhase(FractalType fractalType, bool isVideo) : IPromptPhase<LocationPhaseResult>
    {
        public LocationPhaseResult Run()
        {
            Prompts.Section("Location");

            var sight = Prompts.Selection(
                $"What [{ThemeColor.Accent}]location[/] should be in the center?",
                Sights.All.Where(x => x.FractalType == fractalType),
                converter: x => x.Name,
                searchable: true);

            // For video rendering, asking the zoom value is unnecessary
            var zoom = isVideo ? BigFloat.One : Prompts.Text<BigFloat>($"What should the [{ThemeColor.Accent}]zoom[/] level be?");

            if (zoom > sight.MaxRange)
                Prompts.Warn("Zoom exceeds the precision limit — the image may render as a solid color.");

            Prompts.Done();
            return new(sight, zoom);
        }
    }
}
