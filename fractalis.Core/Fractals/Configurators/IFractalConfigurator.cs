namespace fractalis.Core.Fractals.Configurators
{
    internal interface IFractalConfigurator
    {
        /// <summary>
        /// The type of the fractal that is configured.
        /// </summary>
        public FractalType TargetType { get; }

        /// <summary>
        /// Creates a parameter record using CLI prompts.
        /// </summary>
        /// <returns>An instance of the parameter record.</returns>
        public FractalParameters Configure();
    }
}
