using Ozge.Core.Domain.Enums;

namespace Ozge.Core.State;

public sealed record BehaviorSnapshot(
    Guid GroupId,
    BehaviorKind Kind,
    string Note,
    DateTimeOffset Timestamp);
