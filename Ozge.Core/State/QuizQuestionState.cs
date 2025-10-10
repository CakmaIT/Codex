using System.Collections.Immutable;

namespace Ozge.Core.State;

public sealed record QuizQuestionState(
    Guid QuestionId,
    string Prompt,
    ImmutableList<QuizOptionState> Options);
