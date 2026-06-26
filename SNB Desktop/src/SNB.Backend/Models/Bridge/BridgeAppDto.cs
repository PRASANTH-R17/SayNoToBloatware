using System.Text.Json.Serialization;

namespace SNB.Backend.Models.Bridge;

public sealed class BridgeAppDto
{
    [JsonPropertyName("packageName")]
    public string PackageName { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("isSystem")]
    public bool IsSystem { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("versionName")]
    public string VersionName { get; set; } = string.Empty;

    [JsonPropertyName("sizeBytes")]
    public long SizeBytes { get; set; }

    [JsonPropertyName("permissions")]
    public IReadOnlyList<string> Permissions { get; set; } = new List<string>();

    [JsonPropertyName("iconBase64")]
    public string? IconBase64 { get; set; }
}
