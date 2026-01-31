using System.Diagnostics;
using Microsoft.Extensions.Options;
using Sentinel.Core.Contracts.Events;
using Sentinel.Core.Contracts.Telemetry;
using Sentinel.Core.Enums;
using Sentinel.Satellite.Service.Services.Clients.GroundApi;
using Sentinel.Satellite.Service.Services.TelemetrySimulator.Options;

namespace Sentinel.Satellite.Service.Services.TelemetrySimulator;

public sealed class TelemetrySimulatorService(
    IGroundApiClient groundApi,
    TelemetryIngestService ingest,
    IOptions<SimulatorOptions> simulatorOptions,
    ILogger<TelemetrySimulatorService> logger) : BackgroundService
{
    private readonly SemaphoreSlim _startStop = new(1, 1);

    public bool Running { get; private set; } = true;

    public SimScenario CurrentScenario { get; private set; } = SimScenario.Normal;

    public void Start(SimScenario scenario = SimScenario.Normal)
    {
        _startStop.Wait();
        try
        {
            CurrentScenario = scenario;
            Running = true;
            logger.LogInformation("Telemetry simulator started with scenario {Scenario}", scenario);
        }
        finally
        {
            _startStop.Release();
        }
    }

    public void Stop()
    {
        _startStop.Wait();
        try
        {
            Running = false;
            logger.LogInformation("Telemetry simulator stopped");
        }
        finally
        {
            _startStop.Release();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sw = Stopwatch.StartNew();
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

            if (!Running) continue;

            try
            {
                var simOpts = simulatorOptions.Value;
                var missionId = simOpts.MissionId;
                var satelliteId = simOpts.SatelliteId;

                var (lat, lon, alt) = OrbitalPosition(sw.Elapsed.TotalSeconds);
                var rng = Random.Shared;
                var useAnomaly = CurrentScenario == SimScenario.Anomaly || (CurrentScenario == SimScenario.Mixed && rng.NextDouble() < 0.2);
                var signals = useAnomaly ? AnomalySignals(rng) : NormalSignals(rng);

                var ts = DateTimeOffset.UtcNow;
                var location = LocationContract.Create(lat, lon, alt);
                var req = TelemetryIngestRequest.Create(missionId, satelliteId, ts, location, signals);
                await ingest.IngestAsync(req, stoppingToken);
                var evt = GroundEventContract.CreateTelemetry(missionId, satelliteId, ts, location, signals);
                await groundApi.PublishAsync(evt, stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Simulator tick failed");
            }
        }
    }

    private static TelemetrySignalsContract NormalSignals(Random rng)
    {
        return new TelemetrySignalsContract
        {
            CpuTemperature = 40 + (rng.NextDouble() * 15),
            BatteryVoltage = 12.2 + (rng.NextDouble() * 0.5),
            Pressure = 101300 + (rng.NextDouble() * 200),
            GyroSpeed = (rng.NextDouble() - 0.5) * 0.2,
            SignalStrength = -75 + (rng.NextDouble() * 15),
            PowerConsumption = 100 + (rng.NextDouble() * 80)
        };
    }

    private static TelemetrySignalsContract AnomalySignals(Random rng)
    {
        return new TelemetrySignalsContract
        {
            CpuTemperature = 75 + (rng.NextDouble() * 25),
            BatteryVoltage = 10.0 + (rng.NextDouble() * 1.5),
            Pressure = 101300 + (rng.NextDouble() * 5000),
            GyroSpeed = (rng.NextDouble() - 0.5) * 2.0,
            SignalStrength = -95 + (rng.NextDouble() * 15),
            PowerConsumption = 180 + (rng.NextDouble() * 100)
        };
    }

    private static (double lat, double lon, double altKm) OrbitalPosition(double elapsedSec)
    {
        const double periodSec = 90 * 60.0;
        const double incl = 51.6 * Math.PI / 180;
        const double altKm = 400;
        var lon = (elapsedSec / periodSec * 360.0 % 360) - 180;
        var lat = Math.Asin(Math.Sin(incl) * Math.Sin(2 * Math.PI * elapsedSec / 600)) * 180 / Math.PI;
        return (lat, lon, altKm);
    }
}
