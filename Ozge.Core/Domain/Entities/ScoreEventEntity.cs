namespace Ozge.Core.Domain.Entities;

public class ScoreEventEntity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid GroupId { get; set; }
    public int Delta { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public GroupEntity? Group { get; set; }
    public ClassEntity? Class { get; set; }
}
