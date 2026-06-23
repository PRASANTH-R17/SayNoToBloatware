using System.Text.Json.Serialization;

namespace SNB.Backend.Models.Bridge;

public sealed class BridgeAppDto
{
    [JsonPropertyName("packageName")]
    public string PackageName { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("iconBase64")]
    public string? IconBase64 { get; set; }
}
