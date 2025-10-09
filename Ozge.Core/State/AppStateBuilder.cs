using System.Collections.Immutable;
using Ozge.Core.Domain.Enums;

namespace Ozge.Core.State;

/// <summary>
/// Mutable helper to produce a new <see cref="AppState"/> safely.
/// </summary>
public sealed class AppStateBuilder
{
    private ImmutableList<ClassState> _classes;
    public Guid ActiveClassId { get; set; }
    public LessonMode ActiveMode { get; set; }
    public Guid? ActiveUnitId { get; set; }
    public Guid? ActiveGroupId { get; set; }
    public bool IsAnswerRevealEnabled { get; set; }
    public bool IsProjectorFrozen { get; set; }
    public string? PreferredProjectorDisplayId { get; set; }
    public QuizState Quiz { get; set; }

    public AppStateBuilder(AppState state)
    {
        _classes = state.Classes;
        ActiveClassId = state.ActiveClassId;
        ActiveMode = state.ActiveMode;
        ActiveUnitId = state.ActiveUnitId;
        ActiveGroupId = state.ActiveGroupId;
        IsAnswerRevealEnabled = state.IsAnswerRevealEnabled;
        IsProjectorFrozen = state.IsProjectorFrozen;
        PreferredProjectorDisplayId = state.PreferredProjectorDisplayId;
        Quiz = state.Quiz;
    }

    public AppStateBuilder SetClasses(IEnumerable<ClassState> classes)
    {
        _classes = classes.ToImmutableList();
        return this;
    }

    public AppStateBuilder UpdateClass(ClassState updated)
    {
        var list = _classes.ToBuilder();
        var index = list.FindIndex(x => x.Id == updated.Id);
        if (index >= 0)
        {
            list[index] = updated;
        }
        else
        {
            list.Add(updated);
        }

        _classes = list.ToImmutable();
        return this;
    }

    public AppStateBuilder WithQuizState(QuizState quizState)
    {
        Quiz = quizState;
        return this;
    }

    public AppState Build() => new(
        _classes,
        ActiveClassId,
        ActiveMode,
        ActiveUnitId,
        ActiveGroupId,
        IsAnswerRevealEnabled,
        IsProjectorFrozen,
        PreferredProjectorDisplayId,
        Quiz,
        DateTimeOffset.UtcNow);
}
