namespace fractalis.Core.Renderers
{
    /// <summary>
    /// Available rendering modes depending on precision and perturbation usage.
    /// </summary>
    public enum RenderMode
    {
        Default,
        HighPrecision,              // Perturbation Theory
        HighPrecisionWithFloatExp   // FloatExp + Perturbation Theory
    }
}
