using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SNB.Backend.Services.Abstractions;
using SNB.Desktop.Models;
using SNB.Desktop.Services.Preferences;

namespace SNB.Desktop.Services;

/// <inheritdoc cref="IDeviceCatalogService"/>
public sealed class DeviceCatalogService : IDeviceCatalogService
{
    private readonly IDeviceSessionOrchestrator _orchestrator;
    private readonly IDatabaseInitializer _databaseInitializer;
    private readonly IBloatwareSyncService _bloatwareSyncService;
    private readonly IBloatwareCacheService _bloatwareCacheService;
    private readonly IIconCacheService _iconCacheService;
    private readonly IDeviceMetadataService _deviceMetadataService;
    private readonly ILogger<DeviceCatalogService> _logger;

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public DeviceCatalogService(
        IDeviceSessionOrchestrator orchestrator,
        IDatabaseInitializer databaseInitializer,
        IBloatwareSyncService bloatwareSyncService,
        IBloatwareCacheService bloatwareCacheService,
        IIconCacheService iconCacheService,
        IDeviceMetadataService deviceMetadataService,
        ILogger<DeviceCatalogService> logger)
    {
        _orchestrator = orchestrator;
        _databaseInitializer = databaseInitializer;
        _bloatwareSyncService = bloatwareSyncService;
        _bloatwareCacheService = bloatwareCacheService;
        _iconCacheService = iconCacheService;
        _deviceMetadataService = deviceMetadataService;
        _logger = logger;
    }

    public string? CurrentSerial => _orchestrator.CurrentSerial;

    public async Task EnsureInitializedAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            progress?.Report("Initializing database...");
            await _databaseInitializer.InitializeAsync(cancellationToken);

            if (PreferencesService.Instance.Current.AutoUpdateDatabase)
            {
                progress?.Report("Syncing bloatware database...");
                try
                {
                    await _bloatwareSyncService.SyncAsync(cancellationToken);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "Bloatware sync skipped; using local database.");
                }
                catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Bloatware sync skipped due to network timeout; using local database.");
                }
            }
            else
            {
                _logger.LogInformation("Bloatware sync skipped; auto-update disabled in settings.");
            }

            progress?.Report("Loading bloatware cache...");
            await _bloatwareCacheService.LoadAsync(cancellationToken);

            progress?.Report("Loading icon cache...");
            await _iconCacheService.LoadAsync(cancellationToken);

            progress?.Report("Loading device metadata...");
            try
            {
                await _deviceMetadataService.LoadAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Device metadata unavailable.");
            }

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<IReadOnlyList<DeviceCardModel>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _orchestrator.DetectDevicesWithImagesAsync(cancellationToken);
        var cards = new List<DeviceCardModel>(items.Count);
        foreach (var item in items)
        {
            cards.Add(BackendMapper.ToCardModel(item));
        }

        return cards;
    }

    public Task PrepareDeviceAsync(string serial, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
        => _orchestrator.PrepareDeviceAsync(serial, progress, cancellationToken);

    public IReadOnlyList<ApplicationItemModel> GetApplications()
    {
        var apps = _orchestrator.CurrentDeviceApps;
        return apps is null
            ? Array.Empty<ApplicationItemModel>()
            : BackendMapper.ToItemModels(apps.AllApps);
    }

    public AppStatistics GetStatistics()
    {
        var apps = _orchestrator.CurrentDeviceApps;
        return apps is null
            ? new AppStatistics(0, 0, 0)
            : BackendMapper.ToStatistics(apps);
    }

    public async Task<(RemovalStatus Status, string? Error)> RemoveAppAsync(string packageName, CancellationToken cancellationToken = default)
    {
        var serial = _orchestrator.CurrentSerial;
        if (string.IsNullOrWhiteSpace(serial))
        {
            return (RemovalStatus.Failed, "No device is currently connected.");
        }

        var results = await _orchestrator.RemoveAppsAsync(serial, new[] { packageName }, cancellationToken);
        if (results.Count == 0)
        {
            return (RemovalStatus.Failed, "No result returned for removal.");
        }

        var result = results[0];
        var status = result.Outcome switch
        {
            SNB.Backend.Models.RemovalOutcome.Removed => RemovalStatus.Removed,
            SNB.Backend.Models.RemovalOutcome.Disabled => RemovalStatus.Disabled,
            _ => RemovalStatus.Failed,
        };
        return (status, result.ErrorMessage);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        var serial = _orchestrator.CurrentSerial;
        if (string.IsNullOrWhiteSpace(serial))
        {
            return;
        }

        try
        {
            await _orchestrator.StopBridgeAsync(serial, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to stop bridge on device {Serial}.", serial);
        }
    }
}
