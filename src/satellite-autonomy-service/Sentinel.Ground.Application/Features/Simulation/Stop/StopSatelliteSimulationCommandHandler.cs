using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Enums;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;
using Sentinel.Ground.Application.Services.Clients.SatelliteService;

namespace Sentinel.Ground.Application.Features.Simulation.Stop;

public sealed class StopSatelliteSimulationCommandHandler(ISatelliteServiceClient simClient, IGroundDbContext context)
    : ICommandHandler<StopSatelliteSimulationCommand, Result>
{
    public async Task<Result> Handle(StopSatelliteSimulationCommand request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await simClient.SimStopAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return Error.ServiceUnavailable(ex.Message);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return Error.ExternalService((int)response.StatusCode, string.IsNullOrEmpty(body) ? null : body);
        }

        var sat = await context.Satellites.FirstOrDefaultAsync(s => s.Id == request.SatelliteId, cancellationToken);
        if (sat is not null)
        {
            sat.LinkStatus = LinkStatus.Offline;
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok();
    }
}
