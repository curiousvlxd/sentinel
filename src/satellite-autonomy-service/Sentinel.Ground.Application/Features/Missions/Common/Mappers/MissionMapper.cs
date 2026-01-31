using Riok.Mapperly.Abstractions;
using Sentinel.Core.Entities;
using Sentinel.Ground.Application.Features.Missions.Common.Contracts;

namespace Sentinel.Ground.Application.Features.Missions.Common.Mappers;

[Mapper]
public static partial class MissionMapper
{
    public static partial IQueryable<MissionResponse> ToResponse(this IQueryable<Mission> query);

    [MapProperty(nameof(Mission.CreatedAt), nameof(MissionResponse.CreatedAt), StringFormat = "O")]
    public static partial MissionResponse ToResponse(this Mission mission);
}
