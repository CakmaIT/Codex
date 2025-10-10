using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ozge.Core.Domain.Entities;
using Ozge.Core.Domain.Enums;
using Ozge.Data.Storage;

namespace Ozge.Data;

public class OzgeDbContext : DbContext
{
    public OzgeDbContext(DbContextOptions<OzgeDbContext> options)
        : base(options)
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

        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
            v => v.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            v => DateOnly.FromDateTime(DateTime.SpecifyKind(v, DateTimeKind.Utc)));

        ConfigureClass(modelBuilder.Entity<ClassEntity>());
        ConfigureStudent(modelBuilder.Entity<StudentEntity>());
        ConfigureGroup(modelBuilder.Entity<GroupEntity>());
        ConfigureUnit(modelBuilder.Entity<UnitEntity>());
        ConfigureWord(modelBuilder.Entity<WordEntity>());
        ConfigureQuestion(modelBuilder.Entity<QuestionEntity>());
        ConfigureSession(modelBuilder.Entity<SessionEntity>());
        ConfigureScoreEvent(modelBuilder.Entity<ScoreEventEntity>());
        ConfigureBehaviorEvent(modelBuilder.Entity<BehaviorEventEntity>());
        ConfigureSnapshot(modelBuilder.Entity<SnapshotEntity>());
        ConfigureSetting(modelBuilder.Entity<SettingEntity>());
        ConfigureAttendance(modelBuilder.Entity<AttendanceEntity>(), dateOnlyConverter);
        ConfigureLessonLog(modelBuilder.Entity<LessonLogEntity>());
        ConfigureJob(modelBuilder.Entity<JobEntity>());
    }

    private static void ConfigureClass(EntityTypeBuilder<ClassEntity> builder)
    {
        builder.ToTable("Class");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(64);
        builder.Property(x => x.SettingsJson)
            .IsRequired()
            .HasColumnType("TEXT");
        builder.HasIndex(x => x.Name)
            .IsUnique();
    }

    private static void ConfigureStudent(EntityTypeBuilder<StudentEntity> builder)
    {
        builder.ToTable("Student");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(128);
        builder.Property(x => x.Seat)
            .HasMaxLength(8);
        builder.HasIndex(x => new { x.ClassId, x.Name });
        builder.HasOne(x => x.Class)
            .WithMany(x => x.Students)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureGroup(EntityTypeBuilder<GroupEntity> builder)
    {
        builder.ToTable("Group");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(32);
        builder.HasIndex(x => new { x.ClassId, x.Name })
            .IsUnique();
        builder.HasOne(x => x.Class)
            .WithMany(x => x.Groups)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureUnit(EntityTypeBuilder<UnitEntity> builder)
    {
        builder.ToTable("Unit");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(128);
        builder.Property(x => x.Topic)
            .HasMaxLength(128);
        builder.Property(x => x.MetaJson)
            .HasColumnType("TEXT");
        builder.Property(x => x.Difficulty)
            .HasConversion<string>()
            .HasMaxLength(16);
        builder.HasIndex(x => new { x.ClassId, x.Name })
            .IsUnique();
        builder.HasOne(x => x.Class)
            .WithMany(x => x.Units)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureWord(EntityTypeBuilder<WordEntity> builder)
    {
        builder.ToTable("Word");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Text)
            .IsRequired()
            .HasMaxLength(64);
        builder.Property(x => x.PartOfSpeech)
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(x => x.Difficulty)
            .HasConversion<string>()
            .HasMaxLength(16);
        builder.Property(x => x.MetaJson)
            .HasColumnType("TEXT");
        builder.HasOne(x => x.Unit)
            .WithMany(x => x.Words)
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.UnitId, x.Text })
            .IsUnique();
    }

    private static void ConfigureQuestion(EntityTypeBuilder<QuestionEntity> builder)
    {
        builder.ToTable("Question");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(x => x.Prompt)
            .IsRequired()
            .HasColumnType("TEXT");
        builder.Property(x => x.CorrectAnswer)
            .IsRequired()
            .HasColumnType("TEXT");
        builder.Property(x => x.OptionsJson)
            .HasColumnType("TEXT");
        builder.Property(x => x.Difficulty)
            .HasConversion<string>()
            .HasMaxLength(16);
        builder.HasOne(x => x.Unit)
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureSession(EntityTypeBuilder<SessionEntity> builder)
    {
        builder.ToTable("Session");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Mode)
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(x => x.StartedAt)
            .IsRequired();
        builder.Property(x => x.EndedAt);
        builder.HasOne(x => x.Class)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Unit)
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureScoreEvent(EntityTypeBuilder<ScoreEventEntity> builder)
    {
        builder.ToTable("ScoreEvent");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason)
            .HasColumnType("TEXT");
        builder.Property(x => x.Timestamp)
            .IsRequired();
        builder.HasOne(x => x.Class)
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Group)
            .WithMany(x => x.ScoreEvents)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.ClassId, x.Timestamp });
    }

    private static void ConfigureBehaviorEvent(EntityTypeBuilder<BehaviorEventEntity> builder)
    {
        builder.ToTable("BehaviorEvent");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Kind)
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(x => x.Timestamp)
            .IsRequired();
        builder.Property(x => x.Note)
            .HasColumnType("TEXT");
        builder.HasOne(x => x.Class)
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Group)
            .WithMany(x => x.BehaviorEvents)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.ClassId, x.Timestamp });
    }

    private static void ConfigureSnapshot(EntityTypeBuilder<SnapshotEntity> builder)
    {
        builder.ToTable("Snapshot");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Path)
            .IsRequired()
            .HasColumnType("TEXT");
        builder.Property(x => x.Timestamp)
            .IsRequired();
        builder.HasOne(x => x.Class)
            .WithMany(x => x.Snapshots)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Unit)
            .WithMany(x => x.Snapshots)
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => new { x.ClassId, x.Timestamp });
    }

    private static void ConfigureSetting(EntityTypeBuilder<SettingEntity> builder)
    {
        builder.ToTable("Setting");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(128);
        builder.Property(x => x.Value)
            .IsRequired()
            .HasColumnType("TEXT");
        builder.HasOne(x => x.Class)
            .WithMany(x => x.Settings)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.ClassId, x.Key })
            .IsUnique();
    }

    private static void ConfigureAttendance(EntityTypeBuilder<AttendanceEntity> builder, ValueConverter<DateOnly, DateTime> dateConverter)
    {
        builder.ToTable("Attendance");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Date)
            .HasConversion(dateConverter);
        builder.Property(x => x.Present)
            .IsRequired();
        builder.HasIndex(x => new { x.ClassId, x.StudentId, x.Date })
            .IsUnique();
        builder.HasOne(x => x.Class)
            .WithMany(x => x.AttendanceEntries)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Student)
            .WithMany(x => x.AttendanceEntries)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureLessonLog(EntityTypeBuilder<LessonLogEntity> builder)
    {
        builder.ToTable("LessonLog");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DataJson)
            .HasColumnType("TEXT");
        builder.Property(x => x.Timestamp)
            .IsRequired();
        builder.HasOne(x => x.Class)
            .WithMany(x => x.LessonLogs)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Session)
            .WithMany(x => x.LessonLogs)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureJob(EntityTypeBuilder<JobEntity> builder)
    {
        builder.ToTable("Job");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(128);
        builder.Property(x => x.PayloadJson)
            .HasColumnType("TEXT");
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(x => x.Error)
            .HasColumnType("TEXT");
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}
