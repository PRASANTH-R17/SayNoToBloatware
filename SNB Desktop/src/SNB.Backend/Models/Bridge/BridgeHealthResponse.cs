using System.Text.Json.Serialization;

namespace SNB.Backend.Models.Bridge;

public sealed class BridgeHealthResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
