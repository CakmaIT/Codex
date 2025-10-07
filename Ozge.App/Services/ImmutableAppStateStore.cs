using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ozge.Core.Domain.Entities;
using Ozge.Core.Services;
using Ozge.Data.Context;

namespace Ozge.App.Services;

public class ImmutableAppStateStore : IAppStateStore
{
    private readonly OzgeDbContext _dbContext;
    private readonly ILogger<ImmutableAppStateStore> _logger;
    private readonly BehaviorSubject<AppStateSnapshot> _subject = new(AppStateSnapshot.Empty);

    public ImmutableAppStateStore(OzgeDbContext dbContext, ILogger<ImmutableAppStateStore> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public AppStateSnapshot Current => _subject.Value;

    public IObservable<AppStateSnapshot> Changes => _subject.AsObservable();

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var classes = await _dbContext.Classes.AsNoTracking().ToListAsync(cancellationToken);
        var groups = await _dbContext.Groups.AsNoTracking().ToListAsync(cancellationToken);
        var units = await _dbContext.Units.AsNoTracking().ToListAsync(cancellationToken);
        var words = await _dbContext.Words.AsNoTracking().ToListAsync(cancellationToken);
        var questions = await _dbContext.Questions.AsNoTracking().ToListAsync(cancellationToken);
        var sessions = await _dbContext.Sessions.AsNoTracking().ToListAsync(cancellationToken);
        var scoreEvents = await _dbContext.ScoreEvents.AsNoTracking().ToListAsync(cancellationToken);
        var behaviorEvents = await _dbContext.BehaviorEvents.AsNoTracking().ToListAsync(cancellationToken);
        var snapshots = await _dbContext.Snapshots.AsNoTracking().ToListAsync(cancellationToken);
        var lessonLogs = await _dbContext.LessonLogs.AsNoTracking().ToListAsync(cancellationToken);
        var attendance = await _dbContext.Attendance.AsNoTracking().ToListAsync(cancellationToken);
        var settings = await _dbContext.Settings.AsNoTracking().ToListAsync(cancellationToken);

        var snapshot = new AppStateSnapshot(
            classes.FirstOrDefault() ?? new ClassEntity { Id = Guid.Empty, Name = "" },
            classes,
            groups.GroupBy(g => g.ClassId).ToDictionary(g => g.Key, g => (IReadOnlyList<GroupEntity>)g.ToList()),
            units.GroupBy(u => u.ClassId).ToDictionary(u => u.Key, u => (IReadOnlyList<UnitEntity>)u.ToList()),
            questions.GroupBy(q => q.UnitId).ToDictionary(q => q.Key, q => (IReadOnlyList<QuestionEntity>)q.ToList()),
            words.GroupBy(w => w.UnitId).ToDictionary(w => w.Key, w => (IReadOnlyList<WordEntity>)w.ToList()),
            sessions.GroupBy(s => s.ClassId).ToDictionary(s => s.Key, s => (IReadOnlyList<SessionEntity>)s.ToList()),
            scoreEvents.GroupBy(s => s.ClassId).ToDictionary(s => s.Key, s => (IReadOnlyList<ScoreEventEntity>)s.ToList()),
            behaviorEvents.GroupBy(b => b.ClassId).ToDictionary(b => b.Key, b => (IReadOnlyList<BehaviorEventEntity>)b.ToList()),
            snapshots.GroupBy(s => s.ClassId).ToDictionary(s => s.Key, s => (IReadOnlyList<SnapshotEntity>)s.ToList()),
            lessonLogs.GroupBy(l => l.ClassId).ToDictionary(l => l.Key, l => (IReadOnlyList<LessonLogEntity>)l.ToList()),
            attendance.GroupBy(a => a.ClassId).ToDictionary(a => a.Key, a => (IReadOnlyList<AttendanceEntity>)a.ToList()),
            settings.GroupBy(s => s.ClassId).ToDictionary(s => s.Key, s => (IReadOnlyDictionary<string, string>)s.ToDictionary(x => x.Key, x => x.Value)),
            DateTimeOffset.UtcNow);

        _subject.OnNext(snapshot);
    }

    public void Reduce(Func<AppStateSnapshot, AppStateSnapshot> reducer)
    {
        var updated = reducer(_subject.Value);
        _subject.OnNext(updated with { LastUpdated = DateTimeOffset.UtcNow });
    }
}
