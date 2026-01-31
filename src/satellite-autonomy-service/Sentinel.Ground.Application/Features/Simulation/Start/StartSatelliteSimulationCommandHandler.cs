using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Enums;
using Sentinel.Core.Messaging;
using Sentinel.Ground.Application.Primitives.Results;
using Sentinel.Ground.Application.Services.Clients.SatelliteService;

namespace Sentinel.Ground.Application.Features.Simulation.Start;

public sealed class StartSatelliteSimulationCommandHandler(ISatelliteServiceClient simClient, IGroundDbContext context)
    : ICommandHandler<StartSatelliteSimulationCommand, Result>
{
    public async Task<Result> Handle(StartSatelliteSimulationCommand request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await simClient.SimStartAsync(request.Request, cancellationToken);
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

        if (sat is null) return Result.Ok();

        sat.LinkStatus = LinkStatus.Online;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
