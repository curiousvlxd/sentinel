using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Entities;
using Sentinel.Core.Enums;
using Sentinel.Ground.Api.Services.Seed.Options;

namespace Sentinel.Ground.Api.Services;

public sealed class SeedService(
    IGroundDbContext context,
    IOptions<SeedOptions> seedOptions,
    ILogger<SeedService> logger)
{
    public async Task SeedIfEmptyAsync(CancellationToken cancellationToken = default)
    {
        if (await context.Missions.AnyAsync(cancellationToken))
            return;

        var missionIdStr = seedOptions.Value.MissionId;
        var satelliteIdsStr = seedOptions.Value.SatelliteIds;

        if (string.IsNullOrWhiteSpace(missionIdStr) || string.IsNullOrWhiteSpace(satelliteIdsStr) ||
            !Guid.TryParse(missionIdStr.Trim(), out var missionId))
            return;

        var ids = satelliteIdsStr
            .Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Guid.TryParse(s, out var g) ? (Guid?)g : null)
            .Where(g => g.HasValue)
            .SelectMany(g => g is { } v ? new[] { v } : Array.Empty<Guid>())
            .ToList();

        if (ids.Count == 0)
            return;

        var mission = new Mission
        {
            Id = missionId,
            Name = "Airbus Sentinel Mission",
            Description = "Hackathon demo",
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
        context.Add(mission);

        for (var i = 0; i < ids.Count; i++)
        {
            context.Add(new Satellite
            {
                Id = ids[i],
                MissionId = mission.Id,
                Name = $"airbus-sentinel-{i + 1}",
                Status = SatelliteStatus.Active,
                Mode = SatelliteMode.Assisted,
                State = SatelliteState.Ok,
                LinkStatus = LinkStatus.Offline,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded mission {MissionId} and {Count} satellites.", mission.Id, ids.Count);
    }
}
