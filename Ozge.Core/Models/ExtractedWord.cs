using Ozge.Core.Domain.Enums;

namespace Ozge.Core.Models;

public sealed record ExtractedWord(
    string Text,
    PartOfSpeech PartOfSpeech,
    DifficultyLevel Difficulty,
    string? Definition,
    string? Synonym);
