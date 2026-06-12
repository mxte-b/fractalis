namespace fractalis.Core.Fractals.Configurators
{
    internal class MandelbrotConfigurator : IFractalConfigurator
    {
        public FractalType TargetType => FractalType.Mandelbrot;

        public FractalParameters Configure() => new NoParameters();
    }
}
