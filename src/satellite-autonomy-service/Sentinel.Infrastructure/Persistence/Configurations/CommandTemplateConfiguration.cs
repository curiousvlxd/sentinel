using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Core.Entities;

namespace Sentinel.Infrastructure.Persistence.Configurations;

public sealed class CommandTemplateConfiguration : IEntityTypeConfiguration<CommandTemplate>
{
    public void Configure(EntityTypeBuilder<CommandTemplate> b)
    {
        b.ToTable("command_templates");
        b.HasKey(x => x.Id);
        b.Property(x => x.Type).IsRequired().HasColumnName("type");
        b.Property(x => x.Description).IsRequired().HasColumnName("description");
        b.Property(x => x.CreatedAt).IsRequired().HasColumnName("created_at");
        b.HasIndex(x => x.Type).IsUnique();
        b.HasMany(x => x.Fields).WithOne(x => x.CommandTemplate).HasForeignKey(x => x.CommandTemplateId).OnDelete(DeleteBehavior.Cascade);
    }
}
