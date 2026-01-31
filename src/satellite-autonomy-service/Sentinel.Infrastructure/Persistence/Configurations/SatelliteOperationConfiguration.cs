using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Core.Entities;

namespace Sentinel.Infrastructure.Persistence.Configurations;

public sealed class SatelliteOperationConfiguration : IEntityTypeConfiguration<SatelliteOperation>
{
    public void Configure(EntityTypeBuilder<SatelliteOperation> b)
    {
        b.ToTable("commands");
        b.HasKey(x => x.Id);
        b.Property(x => x.SatelliteId).IsRequired().HasColumnName("satellite_id");
        b.Property(x => x.CommandTemplateId).HasColumnName("command_template_id");
        b.Property(x => x.Type).IsRequired().HasColumnName("command_type");
        b.Property(x => x.Status).IsRequired().HasConversion<string>().HasColumnName("status");
        b.Property(x => x.CreatedAt).IsRequired().HasColumnName("created_at");
        b.HasIndex(x => x.SatelliteId);
        b.HasIndex(x => new { x.SatelliteId, x.Status });
        b.HasOne(x => x.Satellite).WithMany().HasForeignKey(x => x.SatelliteId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Mission).WithMany().HasForeignKey(x => x.MissionId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.CommandTemplate).WithMany().HasForeignKey(x => x.CommandTemplateId).OnDelete(DeleteBehavior.SetNull);
    }
}
