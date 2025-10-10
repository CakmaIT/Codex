using System.Collections.Immutable;

namespace Ozge.Core.State;

public sealed record QuizState(
    Guid? UnitId,
    int CurrentQuestionIndex,
    int TotalQuestions,
    QuizQuestionState? CurrentQuestion,
    ImmutableList<QuizQuestionState> Questions)
{
    public static QuizState Empty { get; } = new(
        null,
        0,
        0,
        null,
        ImmutableList<QuizQuestionState>.Empty);
}
