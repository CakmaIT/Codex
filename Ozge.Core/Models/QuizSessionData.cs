namespace Ozge.Core.Models;

public sealed record QuizSessionData(
    Guid ClassId,
    Guid UnitId,
    IReadOnlyList<QuizQuestionData> Questions);
