using System.Text.Json;
using System.Text.Json.Serialization;

namespace fractalis.Core
{
    public static class FractalisJsonOptions
    {
        public static readonly JsonSerializerOptions Default = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }
}
