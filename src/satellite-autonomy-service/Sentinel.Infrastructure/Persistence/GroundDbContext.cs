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

    public DbSet<CommandTemplate> CommandTemplates => Set<CommandTemplate>();

    public DbSet<SatelliteOperation> SatelliteOperations => Set<SatelliteOperation>();

    IQueryable<Mission> IGroundDbContext.Missions => Missions;

    IQueryable<Satellite> IGroundDbContext.Satellites => Satellites;

    IQueryable<MlHealthResult> IGroundDbContext.MlHealthResults => MlHealthResults;

    IQueryable<Decision> IGroundDbContext.Decisions => Decisions;

    IQueryable<CommandTemplate> IGroundDbContext.CommandTemplates => CommandTemplates;

    IQueryable<SatelliteOperation> IGroundDbContext.SatelliteOperations => SatelliteOperations;

    void IGroundDbContext.Add<TEntity>(TEntity entity) => Set<TEntity>().Add(entity);

    void IGroundDbContext.RemoveMission(Mission mission) => Missions.Remove(mission);

    void IGroundDbContext.RemoveSatellite(Satellite satellite) => Satellites.Remove(satellite);

    void IGroundDbContext.RemoveCommandTemplate(CommandTemplate template) => CommandTemplates.Remove(template);

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
}
