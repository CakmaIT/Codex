using Ozge.Core.Domain.Enums;

namespace Ozge.Core.Domain.Entities;

public class BehaviorEventEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClassId { get; set; }
    public Guid GroupId { get; set; }
    public BehaviorKind Kind { get; set; } = BehaviorKind.Good;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string Note { get; set; } = string.Empty;

    public ClassEntity? Class { get; set; }
    public GroupEntity? Group { get; set; }
}
