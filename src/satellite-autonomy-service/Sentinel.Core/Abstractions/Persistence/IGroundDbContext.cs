using Microsoft.EntityFrameworkCore.Infrastructure;
using Sentinel.Core.Entities;

namespace Sentinel.Core.Abstractions.Persistence;

public interface IGroundDbContext
{
    IQueryable<Mission> Missions { get; }
    IQueryable<Satellite> Satellites { get; }
    IQueryable<MlHealthResult> MlHealthResults { get; }
    IQueryable<Decision> Decisions { get; }
    DatabaseFacade Database { get; }
    void Add<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
