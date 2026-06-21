namespace SNB.Desktop.Models;

/// <summary>
/// View model friendly representation of a connected/selectable Android device.
/// Used by the device selection page, sidebar summary, and applications header.
/// </summary>
public sealed class DeviceCardModel
{
    /// <summary>Bindable image path (avares:// or file path); empty -> placeholder silhouette.</summary>
    public string ImagePath { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string AndroidVersion { get; init; } = string.Empty;

    public string Serial { get; init; } = string.Empty;

    public string Manufacturer { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public string SdkVersion { get; init; } = string.Empty;

    public int BatteryPercent { get; init; }

    public string Status { get; init; } = "Connected";

    public string FirstConnected { get; init; } = string.Empty;

    /// <summary>Convenience label, e.g. "Android 14".</summary>
    public string AndroidVersionLabel => $"Android {AndroidVersion}";
}
