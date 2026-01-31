using Riok.Mapperly.Abstractions;
using Sentinel.Core.Entities;
using Sentinel.Ground.Application.Features.Operations.Common.Contracts;

namespace Sentinel.Ground.Application.Features.Operations.Common.Mappers;

[Mapper]
public static partial class CreateOperationMapper
{
    public static partial IQueryable<SatelliteOperationResponse> ToResponse(this IQueryable<SatelliteOperation> query);

    [MapProperty(nameof(SatelliteOperation.CreatedAt), nameof(SatelliteOperationResponse.CreatedAt), StringFormat = "O")]
    [MapProperty(nameof(SatelliteOperation.ClaimedAt), nameof(SatelliteOperationResponse.ClaimedAt), StringFormat = "O")]
    [MapProperty(nameof(SatelliteOperation.ExecutedAt), nameof(SatelliteOperationResponse.ExecutedAt), StringFormat = "O")]
    public static partial SatelliteOperationResponse ToResponse(this SatelliteOperation operation);
}
