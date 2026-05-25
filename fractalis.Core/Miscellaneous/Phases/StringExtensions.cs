using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fractalis.Core.Miscellaneous.Phases
{
    internal static class StringExtensions
    {
        public static TConverted Convert<TConverted>(this string value, Func<string, TConverted> converter)
        {
            return converter(value);
        }
    }
}
