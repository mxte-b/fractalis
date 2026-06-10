namespace fractalis.Core.Miscellaneous
{
    internal static class StringExtensions
    {
        public static TOut Convert<TIn, TOut>(this TIn value, Func<TIn, TOut> converter)
        {
            return converter(value);
        }
    }
}
