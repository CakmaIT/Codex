namespace Ozge.Core.Domain.Entities;

public class LessonLogEntity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid? SessionId { get; set; }
    public string DataJson { get; set; } = "{}";
    public DateTimeOffset Timestamp { get; set; }
    public ClassEntity? Class { get; set; }
    public SessionEntity? Session { get; set; }
}
