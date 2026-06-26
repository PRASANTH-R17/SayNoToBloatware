using System.Text.Json.Serialization;

namespace SNB.Backend.Models.Bridge;

public sealed class AppsQueryRequest
{
    [JsonPropertyName("packageNames")]
    public List<string> PackageNames { get; set; } = [];
}
