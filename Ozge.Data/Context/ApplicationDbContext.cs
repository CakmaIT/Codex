using Microsoft.EntityFrameworkCore;
using Ozge.Core.Models;

namespace Ozge.Data.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ClassProfile> Classes => Set<ClassProfile>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Word> Words => Set<Word>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<ScoreEvent> ScoreEvents => Set<ScoreEvent>();
    public DbSet<BehaviorEvent> BehaviorEvents => Set<BehaviorEvent>();
    public DbSet<Snapshot> Snapshots => Set<Snapshot>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<Attendance> Attendance => Set<Attendance>();
    public DbSet<LessonLog> LessonLogs => Set<LessonLog>();
    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClassProfile>().ToTable("Classes").HasKey(x => x.Id);
        modelBuilder.Entity<Student>().ToTable("Students").HasKey(x => x.Id);
        modelBuilder.Entity<Group>().ToTable("Groups").HasKey(x => x.Id);
        modelBuilder.Entity<Unit>().ToTable("Units").HasKey(x => x.Id);
        modelBuilder.Entity<Word>().ToTable("Words").HasKey(x => x.Id);
        modelBuilder.Entity<Question>().ToTable("Questions").HasKey(x => x.Id);
        modelBuilder.Entity<Session>().ToTable("Sessions").HasKey(x => x.Id);
        modelBuilder.Entity<ScoreEvent>().ToTable("ScoreEvents").HasKey(x => x.Id);
        modelBuilder.Entity<BehaviorEvent>().ToTable("BehaviorEvents").HasKey(x => x.Id);
        modelBuilder.Entity<Snapshot>().ToTable("Snapshots").HasKey(x => x.Id);
        modelBuilder.Entity<Setting>().ToTable("Settings").HasKey(x => x.Id);
        modelBuilder.Entity<Attendance>().ToTable("Attendance").HasKey(x => x.Id);
        modelBuilder.Entity<LessonLog>().ToTable("LessonLogs").HasKey(x => x.Id);
        modelBuilder.Entity<Job>().ToTable("Jobs").HasKey(x => x.Id);

        modelBuilder.Entity<ClassProfile>().HasIndex(x => x.Name).IsUnique(false);
        modelBuilder.Entity<Unit>().HasIndex(x => new { x.ClassId, x.Name }).IsUnique(false);
        modelBuilder.Entity<Word>().HasIndex(x => new { x.UnitId, x.Text }).IsUnique(false);
        modelBuilder.Entity<Question>().HasIndex(x => new { x.UnitId, x.Type });
        modelBuilder.Entity<ScoreEvent>().HasIndex(x => x.Timestamp);
        modelBuilder.Entity<BehaviorEvent>().HasIndex(x => x.Timestamp);
        modelBuilder.Entity<LessonLog>().HasIndex(x => x.Timestamp);
        modelBuilder.Entity<Job>().HasIndex(x => x.Status);

        base.OnModelCreating(modelBuilder);
    }
}
