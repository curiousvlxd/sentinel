using Microsoft.EntityFrameworkCore.Infrastructure;
using Sentinel.Core.Entities;

namespace Sentinel.Core.Abstractions.Persistence;

public interface IGroundDbContext
{
    IQueryable<Mission> Missions { get; }
    IQueryable<Satellite> Satellites { get; }
    IQueryable<MlHealthResult> MlHealthResults { get; }
    IQueryable<Decision> Decisions { get; }
    IQueryable<Command> Commands { get; }
    DatabaseFacade Database { get; }
    void Add<TEntity>(TEntity entity) where TEntity : class;
    void RemoveMission(Mission mission);
    void RemoveSatellite(Satellite satellite);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
