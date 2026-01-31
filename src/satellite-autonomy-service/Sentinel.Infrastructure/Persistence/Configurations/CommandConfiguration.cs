using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Core.Entities;
using Sentinel.Core.Enums;

namespace Sentinel.Infrastructure.Persistence.Configurations;

public sealed class CommandConfiguration : IEntityTypeConfiguration<Command>
{
    public void Configure(EntityTypeBuilder<Command> b)
    {
        b.ToTable("commands");
        b.HasKey(x => x.Id);
        b.Property(x => x.SatelliteId).IsRequired();
        b.Property(x => x.Type).IsRequired().HasColumnName("command_type");
        b.Property(x => x.Status).IsRequired().HasConversion<string>();
        b.Property(x => x.CreatedAt).IsRequired();
        b.HasIndex(x => x.SatelliteId);
        b.HasIndex(x => new { x.SatelliteId, x.Status });
    }
}
