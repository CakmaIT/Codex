namespace Ozge.Core.Domain.Entities;

public class JobEntity
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public string Status { get; set; } = "Pending";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ScheduledAt { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
}
