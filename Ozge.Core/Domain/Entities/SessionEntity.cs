using System.Collections.Generic;

namespace Ozge.Core.Domain.Entities;

public class SessionEntity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid? UnitId { get; set; }
    public string Mode { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public ClassEntity? Class { get; set; }
    public UnitEntity? Unit { get; set; }
    public ICollection<LessonLogEntity> LessonLogs { get; set; } = new List<LessonLogEntity>();
}
