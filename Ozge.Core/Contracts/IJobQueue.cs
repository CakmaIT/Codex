using Ozge.Core.Domain.Entities;

namespace Ozge.Core.Contracts;

public interface IJobQueue
{
    Task<JobEntity> EnqueueAsync(string type, object payload, CancellationToken cancellationToken);
    IAsyncEnumerable<JobEntity> DequeueAsync(CancellationToken cancellationToken);
    Task MarkCompletedAsync(Guid jobId, CancellationToken cancellationToken);
    Task MarkFailedAsync(Guid jobId, string error, CancellationToken cancellationToken);
}
