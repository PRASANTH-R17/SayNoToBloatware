using SNB.Backend.Models;

namespace SNB.Backend.Services.Abstractions;

public interface IDeviceSessionOrchestrator
{
    Task<IReadOnlyList<DeviceInfo>> DetectDevicesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceListItem>> DetectDevicesWithImagesAsync(CancellationToken cancellationToken = default);
    Task<DeviceApps> PrepareDeviceAsync(string serial, IProgress<string>? progress = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RemovalResult>> RemoveAppsAsync(string serial, IReadOnlyList<string> packageNames, CancellationToken cancellationToken = default);
    Task StopBridgeAsync(string serial, CancellationToken cancellationToken = default);
    DeviceApps? CurrentDeviceApps { get; }
    string? CurrentSerial { get; }
}
