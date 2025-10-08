using Ozge.Core.Domain.Enums;

namespace Ozge.Core.State;

public sealed record LessonLogSnapshot(
    Guid SessionId,
    LessonMode Mode,
    Guid? UnitId,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    string Summary);
