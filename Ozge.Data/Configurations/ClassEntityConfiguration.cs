using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ozge.Core.Domain.Entities;

namespace Ozge.Data.Configurations;

public class ClassEntityConfiguration : IEntityTypeConfiguration<ClassEntity>
{
    public void Configure(EntityTypeBuilder<ClassEntity> builder)
    {
        builder.ToTable("Classes");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(64).IsRequired();
        builder.Property(c => c.SettingsJson).HasColumnType("TEXT");
    }
}
