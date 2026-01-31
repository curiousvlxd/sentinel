using System.Text.Json.Serialization;

namespace Sentinel.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SimScenario
{
    Normal,
    Mixed,
    Anomaly
}
