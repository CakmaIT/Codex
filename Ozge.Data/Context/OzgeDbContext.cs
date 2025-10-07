using Microsoft.EntityFrameworkCore;
using Ozge.Core.Domain.Entities;

namespace Ozge.Data.Context;

public class OzgeDbContext : DbContext
{
    public OzgeDbContext(DbContextOptions<OzgeDbContext> options) : base(options)
    {
    }

    public DbSet<ClassEntity> Classes => Set<ClassEntity>();
    public DbSet<StudentEntity> Students => Set<StudentEntity>();
    public DbSet<GroupEntity> Groups => Set<GroupEntity>();
    public DbSet<UnitEntity> Units => Set<UnitEntity>();
    public DbSet<WordEntity> Words => Set<WordEntity>();
    public DbSet<QuestionEntity> Questions => Set<QuestionEntity>();
    public DbSet<SessionEntity> Sessions => Set<SessionEntity>();
    public DbSet<ScoreEventEntity> ScoreEvents => Set<ScoreEventEntity>();
    public DbSet<BehaviorEventEntity> BehaviorEvents => Set<BehaviorEventEntity>();
    public DbSet<SnapshotEntity> Snapshots => Set<SnapshotEntity>();
    public DbSet<SettingEntity> Settings => Set<SettingEntity>();
    public DbSet<AttendanceEntity> Attendance => Set<AttendanceEntity>();
    public DbSet<LessonLogEntity> LessonLogs => Set<LessonLogEntity>();
    public DbSet<JobEntity> Jobs => Set<JobEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OzgeDbContext).Assembly);
    }
}
