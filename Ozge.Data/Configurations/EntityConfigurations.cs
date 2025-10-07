using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ozge.Core.Domain.Entities;

namespace Ozge.Data.Configurations;

public class StudentEntityConfiguration : IEntityTypeConfiguration<StudentEntity>
{
    public void Configure(EntityTypeBuilder<StudentEntity> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(128).IsRequired();
        builder.Property(s => s.Seat).HasMaxLength(16);
        builder.HasIndex(s => new { s.ClassId, s.Name });
    }
}

public class GroupEntityConfiguration : IEntityTypeConfiguration<GroupEntity>
{
    public void Configure(EntityTypeBuilder<GroupEntity> builder)
    {
        builder.ToTable("Groups");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).HasMaxLength(32).IsRequired();
        builder.Property(g => g.Avatar).HasMaxLength(64);
        builder.HasIndex(g => new { g.ClassId, g.Name }).IsUnique();
    }
}

public class UnitEntityConfiguration : IEntityTypeConfiguration<UnitEntity>
{
    public void Configure(EntityTypeBuilder<UnitEntity> builder)
    {
        builder.ToTable("Units");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).HasMaxLength(128).IsRequired();
        builder.Property(u => u.Topic).HasMaxLength(128);
        builder.Property(u => u.MetaJson).HasColumnType("TEXT");
        builder.HasIndex(u => new { u.ClassId, u.Name }).IsUnique();
    }
}

public class WordEntityConfiguration : IEntityTypeConfiguration<WordEntity>
{
    public void Configure(EntityTypeBuilder<WordEntity> builder)
    {
        builder.ToTable("Words");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Text).HasMaxLength(128).IsRequired();
        builder.Property(w => w.PartOfSpeech).HasMaxLength(32);
        builder.Property(w => w.Difficulty).HasMaxLength(16);
        builder.Property(w => w.MetaJson).HasColumnType("TEXT");
        builder.HasIndex(w => new { w.UnitId, w.Text }).IsUnique();
    }
}

public class QuestionEntityConfiguration : IEntityTypeConfiguration<QuestionEntity>
{
    public void Configure(EntityTypeBuilder<QuestionEntity> builder)
    {
        builder.ToTable("Questions");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Type).HasMaxLength(32).IsRequired();
        builder.Property(q => q.Prompt).HasColumnType("TEXT").IsRequired();
        builder.Property(q => q.Correct).HasColumnType("TEXT").IsRequired();
        builder.Property(q => q.OptionsJson).HasColumnType("TEXT");
        builder.Property(q => q.Difficulty).HasMaxLength(16);
        builder.HasIndex(q => new { q.UnitId, q.Type });
    }
}

public class SessionEntityConfiguration : IEntityTypeConfiguration<SessionEntity>
{
    public void Configure(EntityTypeBuilder<SessionEntity> builder)
    {
        builder.ToTable("Sessions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Mode).HasMaxLength(32);
        builder.HasIndex(s => new { s.ClassId, s.StartedAt });
    }
}

public class ScoreEventEntityConfiguration : IEntityTypeConfiguration<ScoreEventEntity>
{
    public void Configure(EntityTypeBuilder<ScoreEventEntity> builder)
    {
        builder.ToTable("ScoreEvents");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Reason).HasMaxLength(128);
        builder.Property(s => s.Timestamp).HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.HasIndex(s => new { s.ClassId, s.Timestamp });
    }
}

public class BehaviorEventEntityConfiguration : IEntityTypeConfiguration<BehaviorEventEntity>
{
    public void Configure(EntityTypeBuilder<BehaviorEventEntity> builder)
    {
        builder.ToTable("BehaviorEvents");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Kind).HasMaxLength(32);
        builder.Property(b => b.Note).HasColumnType("TEXT");
        builder.Property(b => b.Timestamp).HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.HasIndex(b => new { b.ClassId, b.Timestamp });
    }
}

public class SnapshotEntityConfiguration : IEntityTypeConfiguration<SnapshotEntity>
{
    public void Configure(EntityTypeBuilder<SnapshotEntity> builder)
    {
        builder.ToTable("Snapshots");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Path).HasMaxLength(512);
        builder.Property(s => s.Timestamp).HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.HasIndex(s => new { s.ClassId, s.Timestamp });
    }
}

public class SettingEntityConfiguration : IEntityTypeConfiguration<SettingEntity>
{
    public void Configure(EntityTypeBuilder<SettingEntity> builder)
    {
        builder.ToTable("Settings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Key).HasMaxLength(64).IsRequired();
        builder.Property(s => s.Value).HasColumnType("TEXT");
        builder.HasIndex(s => new { s.ClassId, s.Key }).IsUnique();
    }
}

public class AttendanceEntityConfiguration : IEntityTypeConfiguration<AttendanceEntity>
{
    public void Configure(EntityTypeBuilder<AttendanceEntity> builder)
    {
        builder.ToTable("Attendance");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Date).HasConversion(
            v => v.ToDateTime(TimeOnly.MinValue),
            v => DateOnly.FromDateTime(DateTime.SpecifyKind(v, DateTimeKind.Utc)));
        builder.HasIndex(a => new { a.ClassId, a.StudentId, a.Date }).IsUnique();
    }
}

public class LessonLogEntityConfiguration : IEntityTypeConfiguration<LessonLogEntity>
{
    public void Configure(EntityTypeBuilder<LessonLogEntity> builder)
    {
        builder.ToTable("LessonLogs");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.DataJson).HasColumnType("TEXT");
        builder.Property(l => l.Timestamp).HasConversion(v => v.UtcDateTime, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.HasIndex(l => new { l.ClassId, l.Timestamp });
    }
}

public class JobEntityConfiguration : IEntityTypeConfiguration<JobEntity>
{
    public void Configure(EntityTypeBuilder<JobEntity> builder)
    {
        builder.ToTable("Jobs");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Type).HasMaxLength(64).IsRequired();
        builder.Property(j => j.Status).HasMaxLength(32);
        builder.Property(j => j.PayloadJson).HasColumnType("TEXT");
        builder.Property(j => j.Error).HasColumnType("TEXT");
        builder.HasIndex(j => j.Status);
    }
}
