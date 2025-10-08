using System.Collections.Immutable;
using Ozge.Core.Domain.Enums;

namespace Ozge.Core.State;

/// <summary>
/// Immutable snapshot of the application state shared between the teacher dashboard and projector views.
/// </summary>
public sealed record AppState(
    ImmutableList<ClassState> Classes,
    Guid ActiveClassId,
    LessonMode ActiveMode,
    Guid? ActiveUnitId,
    Guid? ActiveGroupId,
    bool IsAnswerRevealEnabled,
    bool IsProjectorFrozen,
    QuizState Quiz,
    DateTimeOffset LastUpdatedUtc)
{
    public static AppState Empty { get; } = new(
        ImmutableList<ClassState>.Empty,
        Guid.Empty,
        LessonMode.Home,
        null,
        null,
        false,
        false,
        QuizState.Empty,
        DateTimeOffset.UtcNow);
}
