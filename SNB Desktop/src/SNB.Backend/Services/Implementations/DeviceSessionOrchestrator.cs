using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNB.Backend.DependencyInjection;
using SNB.Backend.Models;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Services.Implementations;

public sealed class DeviceSessionOrchestrator : IDeviceSessionOrchestrator
{
    private readonly IAdbExecutor _adbExecutor;
    private readonly IBloatwareMatcher _bloatwareMatcher;
    private readonly IBridgeDeploymentService _bridgeDeploymentService;
    private readonly IBridgeHttpClient _bridgeHttpClient;
    private readonly IIconCacheService _iconCacheService;
    private readonly IAppRemovalService _appRemovalService;
    private readonly IDeviceImageService _deviceImageService;
    private readonly ILogger<DeviceSessionOrchestrator> _logger;
    private readonly SnbBackendOptions _options;

    public DeviceSessionOrchestrator(
        IAdbExecutor adbExecutor,
        IBloatwareMatcher bloatwareMatcher,
        IBridgeDeploymentService bridgeDeploymentService,
        IBridgeHttpClient bridgeHttpClient,
        IIconCacheService iconCacheService,
        IAppRemovalService appRemovalService,
        IDeviceImageService deviceImageService,
        ILogger<DeviceSessionOrchestrator> logger,
        IOptions<SnbBackendOptions> options)
    {
        _adbExecutor = adbExecutor;
        _bloatwareMatcher = bloatwareMatcher;
        _bridgeDeploymentService = bridgeDeploymentService;
        _bridgeHttpClient = bridgeHttpClient;
        _iconCacheService = iconCacheService;
        _appRemovalService = appRemovalService;
        _deviceImageService = deviceImageService;
        _logger = logger;
        _options = options.Value;
    }

    public DeviceApps? CurrentDeviceApps { get; private set; }
    public string? CurrentSerial { get; private set; }

    public async Task<IReadOnlyList<DeviceInfo>> DetectDevicesAsync(CancellationToken cancellationToken = default)
    {
        var rawDevices = await _adbExecutor.GetDevicesAsync(cancellationToken);
        var usable = rawDevices
            .Where(d => string.Equals(d.State, "device", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (usable.Count == 0)
        {
            usable = rawDevices
                .Where(d => !string.Equals(d.State, "offline", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var devices = new List<DeviceInfo>();
        foreach (var (serial, _) in usable)
        {
            var info = await _adbExecutor.GetDeviceInfoAsync(serial, cancellationToken);
            if (info is not null)
            {
                devices.Add(info);
            }
        }

        return devices;
    }

    public async Task<IReadOnlyList<DeviceListItem>> DetectDevicesWithImagesAsync(
        CancellationToken cancellationToken = default)
    {
        var devices = await DetectDevicesAsync(cancellationToken);
        var items = new List<DeviceListItem>(devices.Count);
        foreach (var device in devices)
        {
            var image = await _deviceImageService.ResolveAsync(device, cancellationToken);
            items.Add(new DeviceListItem { Device = device, Image = image });
        }

        return items;
    }

    public async Task<DeviceApps> PrepareDeviceAsync(
        string serial,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        CurrentSerial = serial;

        progress?.Report("Reading device info...");
        var deviceInfo = await _adbExecutor.GetDeviceInfoAsync(serial, cancellationToken);

        progress?.Report("Listing installed packages...");
        var packages = await _adbExecutor.ListPackagesAsync(serial, cancellationToken);
        progress?.Report($"Listing installed packages... {packages.Count} packages found");

        progress?.Report("Matching bloatware...");
        var deviceApps = _bloatwareMatcher.Match(packages);
        var matchedCount = deviceApps.AllApps.Count(a => a.IsBloatware);
        progress?.Report($"Matching bloatware... {matchedCount} matched");

        progress?.Report("Deploying SNB Bridge...");
        await _bridgeDeploymentService.DeployAsync(serial, cancellationToken);

        progress?.Report("Configuring port forwarding...");
        progress?.Report("Waiting for Bridge health check...");

        var missing = _iconCacheService.GetMissingPackages(packages);
        progress?.Report($"Retrieving missing icons... {missing.Count} icons");

        await FetchMissingIconsAsync(deviceApps, missing, cancellationToken);

        CurrentDeviceApps = deviceApps;
        progress?.Report("Ready.");
        _logger.LogInformation(
            "Prepared device {Device} ({Serial}) with {PackageCount} packages.",
            deviceInfo?.DisplayName,
            serial,
            packages.Count);

        return deviceApps;
    }

    public Task<IReadOnlyList<RemovalResult>> RemoveAppsAsync(
        string serial,
        IReadOnlyList<string> packageNames,
        CancellationToken cancellationToken = default)
    {
        return _appRemovalService.RemoveAppsAsync(serial, packageNames, cancellationToken);
    }

    public Task StopBridgeAsync(string serial, CancellationToken cancellationToken = default)
    {
        return _bridgeDeploymentService.StopAsync(serial, cancellationToken);
    }

    private async Task FetchMissingIconsAsync(
        DeviceApps deviceApps,
        IReadOnlyList<string> missingPackages,
        CancellationToken cancellationToken)
    {
        if (missingPackages.Count == 0)
        {
            ApplyCachedIconPaths(deviceApps);
            return;
        }

        var appLookup = deviceApps.AllApps.ToDictionary(a => a.PackageName, StringComparer.Ordinal);
        var batchSize = _options.IconQueryBatchSize;

        for (var i = 0; i < missingPackages.Count; i += batchSize)
        {
            var batch = missingPackages.Skip(i).Take(batchSize).ToList();
            var bridgeApps = await _bridgeHttpClient.QueryAppsAsync(batch, cancellationToken);

            foreach (var bridgeApp in bridgeApps)
            {
                if (!appLookup.TryGetValue(bridgeApp.PackageName, out var app))
                {
                    continue;
                }

                app.AppName = string.IsNullOrWhiteSpace(bridgeApp.Label)
                    ? bridgeApp.PackageName
                    : bridgeApp.Label;

                if (!string.IsNullOrWhiteSpace(bridgeApp.IconBase64))
                {
                    var bytes = Convert.FromBase64String(bridgeApp.IconBase64);
                    app.IconPath = await _iconCacheService.SaveIconAsync(bridgeApp.PackageName, bytes, cancellationToken);
                }
            }
        }

        ApplyCachedIconPaths(deviceApps);
    }

    private void ApplyCachedIconPaths(DeviceApps deviceApps)
    {
        foreach (var app in deviceApps.AllApps)
        {
            if (app.IconPath is not null)
            {
                continue;
            }

            var cachedPath = Path.Combine(_iconCacheService.IconsDirectory, $"{app.PackageName}.png");
            if (File.Exists(cachedPath))
            {
                app.IconPath = cachedPath;
            }
        }
    }
}
