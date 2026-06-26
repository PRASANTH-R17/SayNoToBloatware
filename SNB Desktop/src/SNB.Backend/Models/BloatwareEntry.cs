using System.Text.Json.Serialization;

namespace SNB.Backend.Models;

public sealed class BloatwareEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("removal")]
    public string Removal { get; set; } = string.Empty;
}
