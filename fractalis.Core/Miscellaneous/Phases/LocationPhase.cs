using fractalis.Core.Fractals;
using fractalis.Core.Numbers;
using Spectre.Console;

namespace fractalis.Core.Miscellaneous.Phases
{
    public record LocationPhaseResult(BigComplex Center, BigFloat Zoom);

    public class LocationPhase(FractalType fractalType, bool isVideo) : IPromptPhase<LocationPhaseResult>
    {
        private const string CUSTOM_LABEL = "Custom";
        private const string GENERATE_LABEL = "Generate new";

        private abstract record LocationChoice
        {
            public sealed record Custom : LocationChoice;
            public sealed record Generate : LocationChoice;
            public sealed record Predefined(Sight Sight) : LocationChoice;
        }

        public LocationPhaseResult Run()
        {
            Prompts.Section("Location");

            var choices = GetChoices();
            var generateChoices = choices.Where(x => x is string s && s != GENERATE_LABEL);

            var choice = Prompts
                .Selection(
                    $"What [{ThemeColor.Accent}]location[/] should be in the center?",
                    choices,
                    converter: c => c switch
                    {
                        Sight s => s.Name,
                        string s => $"[{ThemeColor.Accent}]{s}[/]",
                        _ => throw new Exception("Invalid location choice type encountered.")
                    },
                    searchable: true)
                .Convert<object, LocationChoice>(
                    c => c switch
                    {
                        CUSTOM_LABEL => new LocationChoice.Custom(),
                        GENERATE_LABEL => new LocationChoice.Generate(),
                        Sight s => new LocationChoice.Predefined(s),
                        _ => throw new Exception("Unknown choice type encountered for LocationChoice conversion.")
                    });

            Sight? sight = null;
            BigComplex center;
            BigFloat zoom = BigFloat.One;

            switch (choice)
            {
                case LocationChoice.Custom:
                    center = Prompts.Location(
                        $"[{ThemeColor.Accent}]Real part[/] of the location?",
                        $"[{ThemeColor.Accent}]Imaginary part[/] of the location?"
                    );
                    break;

                case LocationChoice.Generate:
                    var near = Prompts.Location(
                        $"[{ThemeColor.Accent}]Real part[/] of the initial guess for Newton iteration?",
                        $"[{ThemeColor.Accent}]Imaginary part[/] of the initial guess for Newton iteration?"
                    );

                    var period = Prompts.TextValidated(
                        $"[{ThemeColor.Accent}]Target period value[/] of the Minibrot?",
                        p => p > 0
                            ? ValidationResult.Success()
                            : ValidationResult.Error($"[red]Must be positive.[/]"), 
                        3
                    );

                    (center, zoom) = MandelbrotSightGenerator.FindMinibrot(near, period);
                    break;

                case LocationChoice.Predefined(var s):
                    sight = s;
                    center = s.Location;
                    break;

                default: throw new Exception("Unknown LocationChoice encountered.");
            }

            // For video rendering, asking the zoom value is unnecessary
            if (choice is not LocationChoice.Generate && !isVideo)
            {
                zoom = Prompts.Text<BigFloat>($"What should the [{ThemeColor.Accent}]zoom[/] level be?");
            }

            if (sight is not null && zoom > sight.MaxRange)
                Prompts.Warn("Zoom exceeds the recommended value for this sight - the image may render as a solid color.");

            Prompts.Done();
            return new(center, zoom);
        }

        private List<object> GetChoices()
        {
            var sights = Sights.All.Where(x => x.FractalType == fractalType);
            var options = new List<object>() { CUSTOM_LABEL };
            if (fractalType == FractalType.Mandelbrot) options.Add(GENERATE_LABEL);

            options.AddRange(sights);
            return options;
        }
    }
}
