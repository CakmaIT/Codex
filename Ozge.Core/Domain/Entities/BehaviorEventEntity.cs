namespace Ozge.Core.Domain.Entities;

public class BehaviorEventEntity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid GroupId { get; set; }
    public string Kind { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string Note { get; set; } = string.Empty;
    public GroupEntity? Group { get; set; }
    public ClassEntity? Class { get; set; }
}
