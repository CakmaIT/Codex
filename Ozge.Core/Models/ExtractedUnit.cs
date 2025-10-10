using System.Collections.Immutable;
using Ozge.Core.Domain.Enums;

namespace Ozge.Core.Models;

public sealed record ExtractedUnit(
    string Name,
    string Topic,
    DifficultyLevel Difficulty,
    ImmutableList<ExtractedWord> Words,
    ImmutableList<GeneratedQuestion> Questions);
