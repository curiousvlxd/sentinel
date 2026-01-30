namespace Sentinel.Satellite.Service.Contracts.OnboardAi;

public sealed class SignalFeaturesDto
{
    public double Mean { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Std { get; set; }
    public double Slope { get; set; }
    public double P95 { get; set; }
    public double MissingRate { get; set; }
}
