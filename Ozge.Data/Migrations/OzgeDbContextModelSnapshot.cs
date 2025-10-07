using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ozge.Data.Context;

#nullable disable

namespace Ozge.Data.Migrations
{
    [DbContext(typeof(OzgeDbContext))]
    partial class OzgeDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.2");

            modelBuilder.Entity("Ozge.Core.Domain.Entities.AttendanceEntity", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<DateTime>("Date").HasColumnType("TEXT");
                b.Property<bool>("Present").HasColumnType("INTEGER");
                b.Property<Guid>("StudentId").HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("StudentId");
                b.HasIndex("ClassId", "StudentId", "Date").IsUnique();
                b.ToTable("Attendance");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.BehaviorEventEntity", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<Guid>("GroupId").HasColumnType("TEXT");
                b.Property<string>("Kind").HasMaxLength(32).HasColumnType("TEXT");
                b.Property<string>("Note").HasColumnType("TEXT");
                b.Property<DateTimeOffset>("Timestamp").HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("GroupId");
                b.HasIndex("ClassId", "Timestamp");
                b.ToTable("BehaviorEvents");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.ClassEntity", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<string>("Name").HasMaxLength(64).HasColumnType("TEXT");
                b.Property<string>("SettingsJson").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Classes");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.GroupEntity", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<string>("Avatar").HasMaxLength(64).HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<string>("Name").HasMaxLength(32).HasColumnType("TEXT");
                b.Property<int>("Score").HasColumnType("INTEGER");
                b.HasKey("Id");
                b.HasIndex("ClassId");
                b.HasIndex("ClassId", "Name").IsUnique();
                b.ToTable("Groups");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.JobEntity", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<string>("Error").HasColumnType("TEXT");
                b.Property<string>("PayloadJson").HasColumnType("TEXT");
                b.Property<string>("Status").HasMaxLength(32).HasColumnType("TEXT");
                b.Property<string>("Type").HasMaxLength(64).HasColumnType("TEXT");
                b.Property<DateTimeOffset>("CreatedAt").HasColumnType("TEXT");
                b.Property<int>("RetryCount").HasColumnType("INTEGER");
                b.Property<DateTimeOffset?>("ScheduledAt").HasColumnType("TEXT");
                b.Property<DateTimeOffset?>("UpdatedAt").HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("Status");
                b.ToTable("Jobs");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.LessonLogEntity", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<string>("DataJson").HasColumnType("TEXT");
                b.Property<Guid?>("SessionId").HasColumnType("TEXT");
                b.Property<DateTimeOffset>("Timestamp").HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("SessionId");
                b.HasIndex("ClassId", "Timestamp");
                b.ToTable("LessonLogs");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.QuestionEntity", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<string>("Correct").HasColumnType("TEXT");
                b.Property<string>("Difficulty").HasMaxLength(16).HasColumnType("TEXT");
                b.Property<string>("OptionsJson").HasColumnType("TEXT");
                b.Property<string>("Prompt").HasColumnType("TEXT");
                b.Property<string>("Type").HasMaxLength(32).HasColumnType("TEXT");
                b.Property<Guid>("UnitId").HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("UnitId");
                b.HasIndex("UnitId", "Type");
                b.ToTable("Questions");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.ScoreEventEntity", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<int>("Delta").HasColumnType("INTEGER");
                b.Property<Guid>("GroupId").HasColumnType("TEXT");
                b.Property<string>("Reason").HasMaxLength(128).HasColumnType("TEXT");
                b.Property<DateTimeOffset>("Timestamp").HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("GroupId");
                b.HasIndex("ClassId", "Timestamp");
                b.ToTable("ScoreEvents");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.SessionEntity", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<string>("Mode").HasMaxLength(32).HasColumnType("TEXT");
                b.Property<DateTimeOffset?>("EndedAt").HasColumnType("TEXT");
                b.Property<DateTimeOffset>("StartedAt").HasColumnType("TEXT");
                b.Property<Guid?>("UnitId").HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("UnitId");
                b.HasIndex("ClassId", "StartedAt");
                b.ToTable("Sessions");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.SettingEntity", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<string>("Key").HasMaxLength(64).HasColumnType("TEXT");
                b.Property<string>("Value").HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("ClassId");
                b.HasIndex("ClassId", "Key").IsUnique();
                b.ToTable("Settings");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.SnapshotEntity", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<string>("Path").HasMaxLength(512).HasColumnType("TEXT");
                b.Property<DateTimeOffset>("Timestamp").HasColumnType("TEXT");
                b.Property<Guid?>("UnitId").HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("UnitId");
                b.HasIndex("ClassId", "Timestamp");
                b.ToTable("Snapshots");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.StudentEntity", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<bool>("IsActive").HasColumnType("INTEGER");
                b.Property<string>("Name").HasMaxLength(128).HasColumnType("TEXT");
                b.Property<string>("Seat").HasMaxLength(16).HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("ClassId");
                b.HasIndex("ClassId", "Name");
                b.ToTable("Students");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.UnitEntity", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<string>("MetaJson").HasColumnType("TEXT");
                b.Property<string>("Name").HasMaxLength(128).HasColumnType("TEXT");
                b.Property<string>("Topic").HasMaxLength(128).HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("ClassId");
                b.HasIndex("ClassId", "Name").IsUnique();
                b.ToTable("Units");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.WordEntity", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<string>("Difficulty").HasMaxLength(16).HasColumnType("TEXT");
                b.Property<string>("MetaJson").HasColumnType("TEXT");
                b.Property<string>("PartOfSpeech").HasMaxLength(32).HasColumnType("TEXT");
                b.Property<string>("Text").HasMaxLength(128).HasColumnType("TEXT");
                b.Property<Guid>("UnitId").HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("UnitId");
                b.HasIndex("UnitId", "Text").IsUnique();
                b.ToTable("Words");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.AttendanceEntity", b =>
            {
                b.HasOne("Ozge.Core.Domain.Entities.ClassEntity", "Class")
                    .WithMany()
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Ozge.Core.Domain.Entities.StudentEntity", "Student")
                    .WithMany("AttendanceRecords")
                    .HasForeignKey("StudentId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Class");
                b.Navigation("Student");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.BehaviorEventEntity", b =>
            {
                b.HasOne("Ozge.Core.Domain.Entities.ClassEntity", "Class")
                    .WithMany("BehaviorEvents")
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Ozge.Core.Domain.Entities.GroupEntity", "Group")
                    .WithMany("BehaviorEvents")
                    .HasForeignKey("GroupId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Class");
                b.Navigation("Group");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.GroupEntity", b =>
            {
                b.HasOne("Ozge.Core.Domain.Entities.ClassEntity", "Class")
                    .WithMany("Groups")
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Class");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.LessonLogEntity", b =>
            {
                b.HasOne("Ozge.Core.Domain.Entities.ClassEntity", "Class")
                    .WithMany("LessonLogs")
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Ozge.Core.Domain.Entities.SessionEntity", "Session")
                    .WithMany()
                    .HasForeignKey("SessionId");

                b.Navigation("Class");
                b.Navigation("Session");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.QuestionEntity", b =>
            {
                b.HasOne("Ozge.Core.Domain.Entities.UnitEntity", "Unit")
                    .WithMany("Questions")
                    .HasForeignKey("UnitId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Unit");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.ScoreEventEntity", b =>
            {
                b.HasOne("Ozge.Core.Domain.Entities.ClassEntity", "Class")
                    .WithMany("ScoreEvents")
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Ozge.Core.Domain.Entities.GroupEntity", "Group")
                    .WithMany("ScoreEvents")
                    .HasForeignKey("GroupId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Class");
                b.Navigation("Group");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.SessionEntity", b =>
            {
                b.HasOne("Ozge.Core.Domain.Entities.ClassEntity", "Class")
                    .WithMany("Sessions")
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Ozge.Core.Domain.Entities.UnitEntity", "Unit")
                    .WithMany()
                    .HasForeignKey("UnitId");

                b.Navigation("Class");
                b.Navigation("Unit");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.SettingEntity", b =>
            {
                b.HasOne("Ozge.Core.Domain.Entities.ClassEntity", "Class")
                    .WithMany("Settings")
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Class");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.SnapshotEntity", b =>
            {
                b.HasOne("Ozge.Core.Domain.Entities.ClassEntity", "Class")
                    .WithMany("Snapshots")
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Ozge.Core.Domain.Entities.UnitEntity", "Unit")
                    .WithMany()
                    .HasForeignKey("UnitId");

                b.Navigation("Class");
                b.Navigation("Unit");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.StudentEntity", b =>
            {
                b.HasOne("Ozge.Core.Domain.Entities.ClassEntity", "Class")
                    .WithMany("Students")
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Class");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.UnitEntity", b =>
            {
                b.HasOne("Ozge.Core.Domain.Entities.ClassEntity", "Class")
                    .WithMany("Units")
                    .HasForeignKey("ClassId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Class");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.WordEntity", b =>
            {
                b.HasOne("Ozge.Core.Domain.Entities.UnitEntity", "Unit")
                    .WithMany("Words")
                    .HasForeignKey("UnitId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Unit");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.ClassEntity", b =>
            {
                b.Navigation("BehaviorEvents");
                b.Navigation("Groups");
                b.Navigation("LessonLogs");
                b.Navigation("ScoreEvents");
                b.Navigation("Sessions");
                b.Navigation("Settings");
                b.Navigation("Snapshots");
                b.Navigation("Students");
                b.Navigation("Units");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.GroupEntity", b =>
            {
                b.Navigation("BehaviorEvents");
                b.Navigation("ScoreEvents");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.SessionEntity", b =>
            {
                b.Navigation("LessonLogs");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.StudentEntity", b =>
            {
                b.Navigation("AttendanceRecords");
            });

            modelBuilder.Entity("Ozge.Core.Domain.Entities.UnitEntity", b =>
            {
                b.Navigation("Questions");
                b.Navigation("Words");
            });
#pragma warning restore 612, 618
        }
    }
}
