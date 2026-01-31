using Riok.Mapperly.Abstractions;
using Sentinel.Core.Entities;
using Sentinel.Ground.Application.Features.Decisions.Common.Contracts;

namespace Sentinel.Ground.Application.Features.Decisions.GetDecisionById;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public static partial class GetDecisionByIdMapper
{
    public static partial IQueryable<DecisionResponse> ToResponse(this IQueryable<Decision> query);

    [MapProperty(nameof(Decision.BucketStart), nameof(DecisionResponse.BucketStart), StringFormat = "O")]
    [MapProperty(nameof(Decision.CreatedAt), nameof(DecisionResponse.CreatedAt), StringFormat = "O")]
    [MapProperty(nameof(Decision.DecisionType), nameof(DecisionResponse.Type))]
    private static partial DecisionResponse ToResponse(Decision decision);
}
