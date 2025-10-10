using System.Text.Json.Serialization;

namespace Ozge.App.Infrastructure;

public sealed record SoundSettings(
    [property: JsonPropertyName("correctSoundPath")] string? CorrectSoundPath,
    [property: JsonPropertyName("incorrectSoundPath")] string? IncorrectSoundPath)
{
    public static SoundSettings Default { get; } = new(null, null);
}

