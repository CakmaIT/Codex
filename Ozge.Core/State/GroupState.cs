namespace Ozge.Core.State;

public sealed record GroupState(
    Guid Id,
    string Name,
    string Avatar,
    int Score,
    int Streak,
    bool IsPenalized,
    DateTimeOffset LastUpdatedUtc);
