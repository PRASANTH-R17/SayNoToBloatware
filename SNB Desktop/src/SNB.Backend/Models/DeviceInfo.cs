namespace SNB.Backend.Models;

public sealed class DeviceInfo
{
    public required string Serial { get; init; }
    public required string Manufacturer { get; init; }
    public required string Model { get; init; }
    public required string AndroidVersion { get; init; }
    public string Brand { get; init; } = string.Empty;
    public string MarketName { get; init; } = string.Empty;
    public string SdkVersion { get; init; } = string.Empty;
    public int BatteryPercent { get; init; }

    public string DisplayName => $"{Model} ({Manufacturer})";
}
