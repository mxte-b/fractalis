using fractalis.Core.Miscellaneous;
using fractalis.Core.Numbers;

namespace fractalis.Core.Fractals.Configurators
{
    internal class JuliaConfigurator : IFractalConfigurator
    {
        public FractalType TargetType => FractalType.Julia;

        public FractalParameters Configure()
        {
            var real = Prompts.Text<BigFloat>(
                $"[{ThemeColor.Accent}]Real part[/] of the Julia constant?",
                new(0)
            );

            var imaginary = Prompts.Text<BigFloat>(
                $"[{ThemeColor.Accent}]Imaginary part[/] of the Julia constant?",
                new(0)
            );  

            return new JuliaParameters()
            {
                Constant = new BigComplex(real, imaginary)
            };
        }
    }
}
