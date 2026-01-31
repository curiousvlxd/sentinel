using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Core.Entities;

namespace Sentinel.Infrastructure.Persistence.Configurations;

public sealed class CommandTemplateFieldConfiguration : IEntityTypeConfiguration<CommandTemplateField>
{
    public void Configure(EntityTypeBuilder<CommandTemplateField> b)
    {
        b.ToTable("command_template_fields");
        b.HasKey(x => x.Id);
        b.Property(x => x.CommandTemplateId).IsRequired().HasColumnName("command_template_id");
        b.Property(x => x.Name).IsRequired().HasColumnName("name");
        b.Property(x => x.FieldType).IsRequired().HasColumnName("field_type");
        b.Property(x => x.Unit).IsRequired().HasConversion<short>().HasColumnType("smallint").HasColumnName("unit");
        b.Property(x => x.DefaultValue).HasColumnName("default_value");
        b.HasIndex(x => x.CommandTemplateId);
    }
}
