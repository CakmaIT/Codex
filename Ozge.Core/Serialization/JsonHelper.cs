using System.Text.Json;

namespace Ozge.Core.Serialization;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T? Deserialize<T>(string json) => string.IsNullOrWhiteSpace(json)
        ? default
        : JsonSerializer.Deserialize<T>(json, Options);
}
