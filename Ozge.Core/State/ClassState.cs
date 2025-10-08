using System.Collections.Immutable;

namespace Ozge.Core.State;

public sealed record ClassState(
    Guid Id,
    string Name,
    ImmutableList<GroupState> Groups,
    ImmutableList<UnitSummary> Units,
    ImmutableList<StudentState> Students,
    ImmutableList<ScoreSnapshot> Scores,
    ImmutableList<BehaviorSnapshot> Behaviors,
    ImmutableList<LessonLogSnapshot> LessonLogs)
{
    public static ClassState Empty(Guid id, string name) => new(
        id,
        name,
        ImmutableList<GroupState>.Empty,
        ImmutableList<UnitSummary>.Empty,
        ImmutableList<StudentState>.Empty,
        ImmutableList<ScoreSnapshot>.Empty,
        ImmutableList<BehaviorSnapshot>.Empty,
        ImmutableList<LessonLogSnapshot>.Empty);
}
