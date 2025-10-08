namespace Ozge.Core.Models;

public sealed record SpeakingAttemptResult(
    Guid AttemptId,
    int Score,
    TimeSpan Duration,
    string Transcript,
    string Feedback);
