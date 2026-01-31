namespace Sentinel.Satellite.Service.Services.Clients.GroundApi.Options;

public sealed class GroundApiOptions
{
    public const string SectionName = "Ground";

    public string BaseUrl { get; set; } = "http://localhost:5276";
}
