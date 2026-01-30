using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Entities;
using Sentinel.Core.Enums;
using Sentinel.Ground.Api.Options;

namespace Sentinel.Ground.Api.Services;

public sealed class SeedService
{
    private readonly IGroundDbContext _context;
    private readonly IOptions<SeedOptions> _seedOptions;
    private readonly ILogger<SeedService> _logger;

    public SeedService(IGroundDbContext context, IOptions<SeedOptions> seedOptions, ILogger<SeedService> logger)
    {
        _context = context;
        _seedOptions = seedOptions;
        _logger = logger;
    }

    public async Task SeedIfEmptyAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Missions.AnyAsync(cancellationToken))
            return;

        var missionIdStr = _seedOptions.Value.MissionId;
        var satelliteIdsStr = _seedOptions.Value.SatelliteIds;
        if (string.IsNullOrWhiteSpace(missionIdStr) || string.IsNullOrWhiteSpace(satelliteIdsStr) ||
            !Guid.TryParse(missionIdStr.Trim(), out var missionId))
            return;

        var ids = satelliteIdsStr!
            .Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Guid.TryParse(s, out var g) ? (Guid?)g : null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToList();
        if (ids.Count == 0)
            return;

        var mission = new Mission
        {
            Id = missionId,
            Name = "Airbus Sentinel Mission",
            Description = "Hackathon demo",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Add(mission);

        for (var i = 0; i < ids.Count; i++)
        {
            _context.Add(new Satellite
            {
                Id = ids[i],
                MissionId = mission.Id,
                Name = $"airbus-sentinel-{i + 1}",
                Status = SatelliteStatus.Active,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded mission {MissionId} and {Count} satellites.", mission.Id, ids.Count);
    }
}
