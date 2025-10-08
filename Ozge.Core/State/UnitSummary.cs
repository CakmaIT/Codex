using Ozge.Core.Domain.Enums;

namespace Ozge.Core.State;

public sealed record UnitSummary(
    Guid Id,
    string Name,
    string Topic,
    DifficultyLevel Difficulty,
    int WordCount,
    int QuestionCount,
    bool IsSelected);
