using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Ozge.Core.Contracts;
using Ozge.Core.Domain.Entities;
using Ozge.Core.Domain.Enums;

namespace Ozge.App.Infrastructure;

public sealed class NullJobQueue : IJobQueue
{
    public Task<JobEntity> EnqueueAsync(string type, object payload, CancellationToken cancellationToken)
    {
        var job = new JobEntity
        {
            Type = type,
            PayloadJson = JsonSerializer.Serialize(payload),
            Status = JobStatus.Completed,
            CreatedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        };

        return Task.FromResult(job);
    }

    public IAsyncEnumerable<JobEntity> DequeueAsync(CancellationToken cancellationToken) => EmptyAsync();

    public Task MarkCompletedAsync(Guid jobId, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task MarkFailedAsync(Guid jobId, string error, CancellationToken cancellationToken) => Task.CompletedTask;

    private static async IAsyncEnumerable<JobEntity> EmptyAsync()
    {
        await Task.CompletedTask;
        yield break;
    }
}
