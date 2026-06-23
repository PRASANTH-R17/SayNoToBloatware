namespace SNB.Backend.Models;

public sealed class DeviceListItem
{
    public required DeviceInfo Device { get; init; }
    public required DeviceImageResult Image { get; init; }
}
