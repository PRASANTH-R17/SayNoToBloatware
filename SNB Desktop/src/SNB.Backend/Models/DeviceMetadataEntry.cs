using System.Text.Json.Serialization;

namespace SNB.Backend.Models;

public sealed class DeviceMetadataEntry
{
    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("local_image")]
    public string LocalImage { get; set; } = string.Empty;

    [JsonPropertyName("brand")]
    public string Brand { get; set; } = string.Empty;
}
