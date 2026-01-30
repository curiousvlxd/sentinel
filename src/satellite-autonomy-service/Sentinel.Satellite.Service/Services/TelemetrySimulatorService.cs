using System.Diagnostics;

namespace Sentinel.Satellite.Service.Services;

public sealed class TelemetrySimulatorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelemetrySimulatorService> _logger;
    private readonly SemaphoreSlim _startStop = new(1, 1);
    private bool _running;

    public TelemetrySimulatorService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<TelemetrySimulatorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        var satelliteId = configuration["Satellite:Id"] ?? configuration["Simulator:SatelliteId"];
        var missionId = configuration["Simulator:MissionId"];
        if (!string.IsNullOrEmpty(satelliteId) && !string.IsNullOrEmpty(missionId))
            _running = true;
    }

    public bool IsRunning => _running;

    public void Start()
    {
        _startStop.Wait();
        try
        {
            _running = true;
            _logger.LogInformation("Telemetry simulator started");
        }
        finally { _startStop.Release(); }
    }

    public void Stop()
    {
        _startStop.Wait();
        try
        {
            _running = false;
            _logger.LogInformation("Telemetry simulator stopped");
        }
        finally { _startStop.Release(); }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sw = Stopwatch.StartNew();
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            if (!_running) continue;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var ingest = scope.ServiceProvider.GetRequiredService<TelemetryIngestService>();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var satelliteIdStr = config["Satellite:Id"] ?? config["Simulator:SatelliteId"];
                var missionIdStr = config["Simulator:MissionId"];
                if (string.IsNullOrEmpty(satelliteIdStr) || !Guid.TryParse(satelliteIdStr, out var satelliteId) ||
                    string.IsNullOrEmpty(missionIdStr) || !Guid.TryParse(missionIdStr, out var missionId))
                    continue;

                var (lat, lon, alt) = OrbitalPosition(sw.Elapsed.TotalSeconds);
                var rng = Random.Shared;
                var req = new Sentinel.Core.Contracts.TelemetryIngestRequest
                {
                    SchemaVersion = "v1",
                    MissionId = missionId,
                    SatelliteId = satelliteId,
                    Timestamp = DateTime.UtcNow.ToString("O"),
                    Source = "sim",
                    Location = new Sentinel.Core.Contracts.LocationContract { Lat = lat, Lon = lon, AltKm = alt },
                    Signals = new Sentinel.Core.Contracts.TelemetrySignalsContract
                    {
                        CpuTemperature = 40 + rng.NextDouble() * 15,
                        BatteryVoltage = 12.2 + rng.NextDouble() * 0.5,
                        Pressure = 101300 + rng.NextDouble() * 200,
                        GyroSpeed = (rng.NextDouble() - 0.5) * 0.2,
                        SignalStrength = -75 + rng.NextDouble() * 15,
                        PowerConsumption = 100 + rng.NextDouble() * 80
                    }
                };
                await ingest.IngestAsync(req, stoppingToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Simulator tick failed");
            }
        }
    }

    private static (double lat, double lon, double altKm) OrbitalPosition(double elapsedSec)
    {
        const double periodSec = 90 * 60.0;
        const double incl = 51.6 * Math.PI / 180;
        const double altKm = 400;
        var lon = (elapsedSec / periodSec) * 360.0 % 360 - 180;
        var lat = Math.Asin(Math.Sin(incl) * Math.Sin(2 * Math.PI * elapsedSec / 600)) * 180 / Math.PI;
        return (lat, lon, altKm);
    }
}
