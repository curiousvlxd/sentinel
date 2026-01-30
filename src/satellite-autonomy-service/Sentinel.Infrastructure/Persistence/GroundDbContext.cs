using Microsoft.EntityFrameworkCore;
using Sentinel.Core.Abstractions.Persistence;
using Sentinel.Core.Entities;

namespace Sentinel.Infrastructure.Persistence;

public sealed class GroundDbContext(DbContextOptions<GroundDbContext> options) : DbContext(options), IGroundDbContext
{
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<Satellite> Satellites => Set<Satellite>();
    public DbSet<MlHealthResult> MlHealthResults => Set<MlHealthResult>();
    public DbSet<Decision> Decisions => Set<Decision>();

    IQueryable<Mission> IGroundDbContext.Missions => Missions;
    IQueryable<Satellite> IGroundDbContext.Satellites => Satellites;
    IQueryable<MlHealthResult> IGroundDbContext.MlHealthResults => MlHealthResults;
    IQueryable<Decision> IGroundDbContext.Decisions => Decisions;
    void IGroundDbContext.Add<TEntity>(TEntity entity) => Set<TEntity>().Add(entity);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }
}
