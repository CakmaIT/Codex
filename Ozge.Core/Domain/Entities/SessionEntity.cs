using Ozge.Core.Domain.Enums;

namespace Ozge.Core.Domain.Entities;

public class SessionEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClassId { get; set; }
    public Guid? UnitId { get; set; }
    public LessonMode Mode { get; set; } = LessonMode.Home;
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EndedAt { get; set; }

    public ClassEntity? Class { get; set; }
    public UnitEntity? Unit { get; set; }
    public ICollection<LessonLogEntity> LessonLogs { get; set; } = new List<LessonLogEntity>();
}
