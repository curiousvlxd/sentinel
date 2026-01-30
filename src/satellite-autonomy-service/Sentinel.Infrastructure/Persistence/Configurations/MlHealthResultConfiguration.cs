using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Core.Entities;

namespace Sentinel.Infrastructure.Persistence.Configurations;

public sealed class MlHealthResultConfiguration : IEntityTypeConfiguration<MlHealthResult>
{
    public void Configure(EntityTypeBuilder<MlHealthResult> b)
    {
        b.ToTable("ml_health_results");
        b.HasKey(x => x.Id);
        b.Property(x => x.SatelliteId).IsRequired();
        b.Property(x => x.BucketStart).IsRequired();
        b.Property(x => x.ModelName).IsRequired();
        b.Property(x => x.ModelVersion).IsRequired();
        b.Property(x => x.AnomalyScore).IsRequired();
        b.Property(x => x.Confidence).IsRequired();
        b.Property(x => x.PerSignalScore).IsRequired().HasColumnType("jsonb");
        b.Property(x => x.TopContributors).IsRequired().HasColumnType("jsonb");
        b.Property(x => x.CreatedAt).IsRequired();
        b.HasIndex(x => new { x.SatelliteId, x.BucketStart });
    }
}
