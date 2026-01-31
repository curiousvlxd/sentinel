using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Core.Entities;

namespace Sentinel.Infrastructure.Persistence.Configurations;

public sealed class DecisionConfiguration : IEntityTypeConfiguration<Decision>
{
    public void Configure(EntityTypeBuilder<Decision> b)
    {
        b.ToTable("decisions");
        b.HasKey(x => x.Id);
        b.Property(x => x.SatelliteId).IsRequired();
        b.Property(x => x.BucketStart).IsRequired();
        b.Property(x => x.DecisionType).IsRequired().HasConversion<string>();
        b.Property(x => x.Reason).IsRequired();
        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.Metadata).HasColumnType("jsonb");
        b.HasOne(x => x.Satellite).WithMany().HasForeignKey(x => x.SatelliteId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.SatelliteId, x.BucketStart });
    }
}
