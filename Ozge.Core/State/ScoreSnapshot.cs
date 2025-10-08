namespace Ozge.Core.State;

public sealed record ScoreSnapshot(
    Guid GroupId,
    int Total,
    string Reason,
    DateTimeOffset Timestamp);
