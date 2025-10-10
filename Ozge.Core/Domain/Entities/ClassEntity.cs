using System.Collections.ObjectModel;

namespace Ozge.Core.Domain.Entities;

/// <summary>
/// Represents a classroom scope within the system.
/// </summary>
public class ClassEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string SettingsJson { get; set; } = "{}";
    public ICollection<StudentEntity> Students { get; set; } = new Collection<StudentEntity>();
    public ICollection<GroupEntity> Groups { get; set; } = new Collection<GroupEntity>();
    public ICollection<UnitEntity> Units { get; set; } = new Collection<UnitEntity>();
    public ICollection<SettingEntity> Settings { get; set; } = new Collection<SettingEntity>();
    public ICollection<LessonLogEntity> LessonLogs { get; set; } = new Collection<LessonLogEntity>();
    public ICollection<SessionEntity> Sessions { get; set; } = new Collection<SessionEntity>();
    public ICollection<AttendanceEntity> AttendanceEntries { get; set; } = new Collection<AttendanceEntity>();
    public ICollection<SnapshotEntity> Snapshots { get; set; } = new Collection<SnapshotEntity>();
}
