using SNB.Backend.Models;

namespace SNB.Backend.Services.Abstractions;

public interface IAdbExecutor
{
    Task<IReadOnlyList<(string Serial, string State)>> GetDevicesAsync(CancellationToken cancellationToken = default);
    Task<DeviceInfo?> GetDeviceInfoAsync(string serial, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListPackagesAsync(string serial, CancellationToken cancellationToken = default);
    Task<bool> IsBridgeInstalledAsync(string serial, CancellationToken cancellationToken = default);
    Task InstallBridgeAsync(string serial, string apkPath, bool reinstall, CancellationToken cancellationToken = default);
    Task LaunchBridgeAsync(string serial, CancellationToken cancellationToken = default);
    Task StopBridgeAsync(string serial, CancellationToken cancellationToken = default);
    Task ForwardPortAsync(string serial, int localPort, int remotePort, CancellationToken cancellationToken = default);
    Task RemovePortForwardAsync(string serial, int localPort, CancellationToken cancellationToken = default);
    Task<(bool Success, string Output)> UninstallPackageAsync(string serial, string packageName, CancellationToken cancellationToken = default);
}
