namespace Ozge.Core.Domain.Entities;

public class SnapshotEntity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid? UnitId { get; set; }
    public string Path { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public ClassEntity? Class { get; set; }
    public UnitEntity? Unit { get; set; }
}
