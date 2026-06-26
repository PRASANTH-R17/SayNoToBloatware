using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SNB.Desktop.Models;

namespace SNB.Desktop.Services;

/// <summary>
/// Desktop-facing facade over the backend <c>IDeviceSessionOrchestrator</c> and the
/// one-time startup bootstrap. Exposes async, UI-model-shaped operations so the view
/// models never touch backend types directly.
/// </summary>
public interface IDeviceCatalogService
{
    /// <summary>Runs the one-time startup sequence (DB init, bloatware sync, caches, metadata). Idempotent.</summary>
    Task EnsureInitializedAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>Detects connected devices (with resolved images) as UI card models.</summary>
    Task<IReadOnlyList<DeviceCardModel>> GetDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>Prepares a device: lists packages, matches bloatware, deploys the bridge, fetches metadata/icons.</summary>
    Task PrepareDeviceAsync(string serial, IProgress<string>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>The applications loaded by the most recent <see cref="PrepareDeviceAsync"/>, as UI models.</summary>
    IReadOnlyList<ApplicationItemModel> GetApplications();

    /// <summary>Headline statistics for the most recently prepared device.</summary>
    AppStatistics GetStatistics();

    /// <summary>
    /// Removes a single package from the current device. Returns the resulting <see cref="RemovalStatus"/>
    /// (Removed, Disabled, or Failed) and an error message when it failed.
    /// </summary>
    Task<(RemovalStatus Status, string? Error)> RemoveAppAsync(string packageName, CancellationToken cancellationToken = default);

    /// <summary>Stops the on-device bridge for the current session (best effort).</summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>The serial of the device currently prepared, if any.</summary>
    string? CurrentSerial { get; }
}
