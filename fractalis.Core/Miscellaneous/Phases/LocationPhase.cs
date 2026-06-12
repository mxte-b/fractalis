using fractalis.Core.Fractals;
using fractalis.Core.Numbers;

namespace fractalis.Core.Miscellaneous.Phases
{
    public record LocationPhaseResult(BigComplex Center, BigFloat Zoom);

    public class LocationPhase(FractalType fractalType, bool isVideo) : IPromptPhase<LocationPhaseResult>
    {
        public LocationPhaseResult Run()
        {
            Prompts.Section("Location");

            var sightOptions = Sights.All.Where(x => x.FractalType == fractalType).Select(x => x.Name).Append($"[{ThemeColor.Accent}]Custom[/]");

            var sight = Prompts.Selection(
                $"What [{ThemeColor.Accent}]location[/] should be in the center?",
                sightOptions,
                searchable: true).Convert(o => o == "Custom" ? null : Sights.All.FirstOrDefault(x => x.Name == o));

            BigComplex center;

            // Custom location
            if (sight is null)
            {
                var real = Prompts.Text<BigFloat>(
                    $"[{ThemeColor.Accent}]Real part[/] of the screen center?",
                    new(0)
                );

                var imaginary = Prompts.Text<BigFloat>(
                    $"[{ThemeColor.Accent}]Imaginary part[/] of the screen center?",
                    new(0)
                );

                center = new(real, imaginary);
            }
            else
            {
                center = sight.Location;
            }

            // For video rendering, asking the zoom value is unnecessary
            var zoom = isVideo 
                ? BigFloat.One 
                : Prompts.Text<BigFloat>($"What should the [{ThemeColor.Accent}]zoom[/] level be?");

            if (sight is not null && zoom > sight.MaxRange)
                Prompts.Warn("Zoom exceeds the recommended value for this sight - the image may render as a solid color.");

            Prompts.Done();
            return new(center, zoom);
        }
    }
}
