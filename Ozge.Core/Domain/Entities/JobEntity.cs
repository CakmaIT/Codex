using Ozge.Core.Domain.Enums;

namespace Ozge.Core.Domain.Entities;

public class JobEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? Error { get; set; }
}
