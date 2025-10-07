using System.Collections.Concurrent;
using Ozge.Core.Domain.Entities;

namespace Ozge.Core.Services;

public interface IAppStateStore
{
    AppStateSnapshot Current { get; }
    IObservable<AppStateSnapshot> Changes { get; }
    void Reduce(Func<AppStateSnapshot, AppStateSnapshot> reducer);
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

public record AppStateSnapshot(
    ClassEntity ActiveClass,
    IReadOnlyList<ClassEntity> Classes,
    IReadOnlyDictionary<Guid, IReadOnlyList<GroupEntity>> Groups,
    IReadOnlyDictionary<Guid, IReadOnlyList<UnitEntity>> Units,
    IReadOnlyDictionary<Guid, IReadOnlyList<QuestionEntity>> Questions,
    IReadOnlyDictionary<Guid, IReadOnlyList<WordEntity>> Words,
    IReadOnlyDictionary<Guid, IReadOnlyList<SessionEntity>> Sessions,
    IReadOnlyDictionary<Guid, IReadOnlyList<ScoreEventEntity>> ScoreEvents,
    IReadOnlyDictionary<Guid, IReadOnlyList<BehaviorEventEntity>> BehaviorEvents,
    IReadOnlyDictionary<Guid, IReadOnlyList<SnapshotEntity>> Snapshots,
    IReadOnlyDictionary<Guid, IReadOnlyList<LessonLogEntity>> LessonLogs,
    IReadOnlyDictionary<Guid, IReadOnlyList<AttendanceEntity>> Attendance,
    IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, string>> Settings,
    DateTimeOffset LastUpdated)
{
    public static AppStateSnapshot Empty { get; } = new(
        new ClassEntity { Id = Guid.Empty, Name = "" },
        Array.Empty<ClassEntity>(),
        new Dictionary<Guid, IReadOnlyList<GroupEntity>>(),
        new Dictionary<Guid, IReadOnlyList<UnitEntity>>(),
        new Dictionary<Guid, IReadOnlyList<QuestionEntity>>(),
        new Dictionary<Guid, IReadOnlyList<WordEntity>>(),
        new Dictionary<Guid, IReadOnlyList<SessionEntity>>(),
        new Dictionary<Guid, IReadOnlyList<ScoreEventEntity>>(),
        new Dictionary<Guid, IReadOnlyList<BehaviorEventEntity>>(),
        new Dictionary<Guid, IReadOnlyList<SnapshotEntity>>(),
        new Dictionary<Guid, IReadOnlyList<LessonLogEntity>>(),
        new Dictionary<Guid, IReadOnlyList<AttendanceEntity>>(),
        new Dictionary<Guid, IReadOnlyDictionary<string, string>>(),
        DateTimeOffset.MinValue);
}
