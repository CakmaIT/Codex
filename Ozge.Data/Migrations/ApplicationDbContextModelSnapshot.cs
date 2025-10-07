using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ozge.Data.Context;

#nullable disable

namespace Ozge.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.4");

            modelBuilder.Entity("Ozge.Core.Models.Attendance", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<DateOnly>("Date").HasColumnType("TEXT");
                b.Property<bool>("Present").HasColumnType("INTEGER");
                b.Property<Guid>("StudentId").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Attendance");
            });

            modelBuilder.Entity("Ozge.Core.Models.BehaviorEvent", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<Guid>("GroupId").HasColumnType("TEXT");
                b.Property<string>("Kind").HasColumnType("TEXT");
                b.Property<string>("Note").HasColumnType("TEXT");
                b.Property<DateTime>("Timestamp").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("BehaviorEvents");
            });

            modelBuilder.Entity("Ozge.Core.Models.ClassProfile", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<string>("Name").HasColumnType("TEXT");
                b.Property<string>("SettingsJson").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Classes");
            });

            modelBuilder.Entity("Ozge.Core.Models.Group", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<string>("Avatar").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<string>("Name").HasColumnType("TEXT");
                b.Property<int>("Score").HasColumnType("INTEGER");
                b.HasKey("Id");
                b.ToTable("Groups");
            });

            modelBuilder.Entity("Ozge.Core.Models.Job", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<string>("Error").HasColumnType("TEXT");
                b.Property<string>("PayloadJson").HasColumnType("TEXT");
                b.Property<string>("Status").HasColumnType("TEXT");
                b.Property<string>("Type").HasColumnType("TEXT");
                b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
                b.Property<DateTime?>("UpdatedAt").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Jobs");
            });

            modelBuilder.Entity("Ozge.Core.Models.LessonLog", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<string>("DataJson").HasColumnType("TEXT");
                b.Property<Guid?>("SessionId").HasColumnType("TEXT");
                b.Property<DateTime>("Timestamp").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("LessonLogs");
            });

            modelBuilder.Entity("Ozge.Core.Models.Question", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<string>("Correct").HasColumnType("TEXT");
                b.Property<string>("Difficulty").HasColumnType("TEXT");
                b.Property<string>("OptionsJson").HasColumnType("TEXT");
                b.Property<string>("Prompt").HasColumnType("TEXT");
                b.Property<string>("Type").HasColumnType("TEXT");
                b.Property<Guid>("UnitId").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Questions");
            });

            modelBuilder.Entity("Ozge.Core.Models.ScoreEvent", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<int>("Delta").HasColumnType("INTEGER");
                b.Property<Guid>("GroupId").HasColumnType("TEXT");
                b.Property<string>("Reason").HasColumnType("TEXT");
                b.Property<DateTime>("Timestamp").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("ScoreEvents");
            });

            modelBuilder.Entity("Ozge.Core.Models.Session", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<string>("Mode").HasColumnType("TEXT");
                b.Property<DateTime?>("EndedAt").HasColumnType("TEXT");
                b.Property<DateTime>("StartedAt").HasColumnType("TEXT");
                b.Property<Guid?>("UnitId").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Sessions");
            });

            modelBuilder.Entity("Ozge.Core.Models.Setting", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<string>("Key").HasColumnType("TEXT");
                b.Property<string>("Value").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Settings");
            });

            modelBuilder.Entity("Ozge.Core.Models.Snapshot", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<string>("Path").HasColumnType("TEXT");
                b.Property<DateTime>("Timestamp").HasColumnType("TEXT");
                b.Property<Guid?>("UnitId").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Snapshots");
            });

            modelBuilder.Entity("Ozge.Core.Models.Student", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<bool>("IsActive").HasColumnType("INTEGER");
                b.Property<string>("Name").HasColumnType("TEXT");
                b.Property<string>("Seat").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Students");
            });

            modelBuilder.Entity("Ozge.Core.Models.Unit", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<Guid>("ClassId").HasColumnType("TEXT");
                b.Property<string>("MetaJson").HasColumnType("TEXT");
                b.Property<string>("Name").HasColumnType("TEXT");
                b.Property<string>("Topic").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Units");
            });

            modelBuilder.Entity("Ozge.Core.Models.Word", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<string>("Difficulty").HasColumnType("TEXT");
                b.Property<string>("MetaJson").HasColumnType("TEXT");
                b.Property<string>("PartOfSpeech").HasColumnType("TEXT");
                b.Property<string>("Text").HasColumnType("TEXT");
                b.Property<Guid>("UnitId").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Words");
            });
#pragma warning restore 612, 618
        }
    }
}
