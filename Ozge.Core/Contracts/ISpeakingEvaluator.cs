using Ozge.Core.Models;

namespace Ozge.Core.Contracts;

public interface ISpeakingEvaluator
{
    Task<SpeakingAttemptResult> EvaluateAsync(Guid classId, Guid groupId, string prompt, Stream audioStream, CancellationToken cancellationToken);
}
