namespace fractalis.Core.Miscellaneous
{
    internal static class StringExtensions
    {
        public static TConverted Convert<TConverted>(this string value, Func<string, TConverted> converter)
        {
            return converter(value);
        }
    }
}
