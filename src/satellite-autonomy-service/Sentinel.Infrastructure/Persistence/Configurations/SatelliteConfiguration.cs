using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Core.Entities;

namespace Sentinel.Infrastructure.Persistence.Configurations;

public sealed class SatelliteConfiguration : IEntityTypeConfiguration<Satellite>
{
    public void Configure(EntityTypeBuilder<Satellite> b)
    {
        b.ToTable("satellites");
        b.HasKey(x => x.Id);
        b.Property(x => x.MissionId).IsRequired();
        b.Property(x => x.Name).IsRequired();
        b.Property(x => x.Status).IsRequired().HasConversion<string>();
        b.Property(x => x.CreatedAt).IsRequired();
        b.HasOne<Mission>().WithMany().HasForeignKey(x => x.MissionId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.MissionId);
        b.HasIndex(x => x.Status);
    }
}
