using Sentinel.Core.Entities;
using Sentinel.Ground.Application.Features.Satellites.Common.Contracts;

namespace Sentinel.Ground.Application.Features.Satellites.Common.Mappers;

public static class CreateSatelliteMapper
{
    public static SatelliteResponse ToResponse(Satellite satellite, string? missionName = null)
    {
        return new SatelliteResponse
        {
            Id = satellite.Id,
            MissionId = satellite.MissionId,
            MissionName = missionName ?? (satellite.Mission != null ? satellite.Mission.Name : null),
            Name = satellite.Name,
            Status = satellite.Status,
            Mode = satellite.Mode,
            State = satellite.State,
            LinkStatus = satellite.LinkStatus,
            LastBucketStart = satellite.LastBucketStart?.ToString("O"),
            CreatedAt = satellite.CreatedAt.ToString("O")
        };
    }

    public static IQueryable<SatelliteResponse> ToResponse(this IQueryable<Satellite> query)
    {
        return query.Select(s => new SatelliteResponse
        {
            Id = s.Id,
            MissionId = s.MissionId,
            MissionName = s.Mission != null ? s.Mission.Name : null,
            Name = s.Name,
            Status = s.Status,
            Mode = s.Mode,
            State = s.State,
            LinkStatus = s.LinkStatus,
            LastBucketStart = s.LastBucketStart != null ? s.LastBucketStart.Value.ToString("O") : null,
            CreatedAt = s.CreatedAt.ToString("O")
        });
    }
}
