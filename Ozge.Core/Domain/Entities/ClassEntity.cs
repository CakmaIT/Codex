using System.Collections.Generic;

namespace Ozge.Core.Domain.Entities;

public class ClassEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SettingsJson { get; set; } = "{}";
    public ICollection<StudentEntity> Students { get; set; } = new List<StudentEntity>();
    public ICollection<GroupEntity> Groups { get; set; } = new List<GroupEntity>();
    public ICollection<UnitEntity> Units { get; set; } = new List<UnitEntity>();
    public ICollection<SessionEntity> Sessions { get; set; } = new List<SessionEntity>();
    public ICollection<ScoreEventEntity> ScoreEvents { get; set; } = new List<ScoreEventEntity>();
    public ICollection<BehaviorEventEntity> BehaviorEvents { get; set; } = new List<BehaviorEventEntity>();
    public ICollection<SnapshotEntity> Snapshots { get; set; } = new List<SnapshotEntity>();
    public ICollection<LessonLogEntity> LessonLogs { get; set; } = new List<LessonLogEntity>();
    public ICollection<SettingEntity> Settings { get; set; } = new List<SettingEntity>();
}
