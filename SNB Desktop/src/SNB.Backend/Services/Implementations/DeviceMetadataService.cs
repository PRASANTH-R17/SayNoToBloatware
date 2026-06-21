using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNB.Backend.DependencyInjection;
using SNB.Backend.Models;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Services.Implementations;

public sealed class DeviceMetadataService : IDeviceMetadataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<DeviceMetadataService> _logger;
    private readonly DeviceImageOptions _options;

    public DeviceMetadataService(
        HttpClient httpClient,
        ILogger<DeviceMetadataService> logger,
        IOptions<DeviceImageOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public IReadOnlyList<DeviceMetadataEntry> Entries { get; private set; } = [];

    public MetadataSource Source { get; private set; } = MetadataSource.None;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var cachePath = ResolveCachePath();

        try
        {
            _logger.LogInformation("Downloading metadata from GitHub Pages");
            var json = await _httpClient.GetStringAsync(_options.MetadataUrl, cancellationToken);
            var entries = ParseEntries(json);

            var directory = Path.GetDirectoryName(cachePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(cachePath, json, cancellationToken);

            Entries = entries;
            Source = MetadataSource.Internet;
            return;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Metadata download failed; attempting cache.");
        }

        try
        {
            if (File.Exists(cachePath))
            {
                var json = await File.ReadAllTextAsync(cachePath, cancellationToken);
                var entries = ParseEntries(json);

                Entries = entries;
                Source = MetadataSource.Cache;
                _logger.LogInformation("Loaded metadata from cache");
                return;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Reading metadata cache failed.");
        }

        Entries = [];
        Source = MetadataSource.None;
    }

    private static List<DeviceMetadataEntry> ParseEntries(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // The dataset is published as a wrapper object: { "meta": {...}, "devices": [...] }.
        // Older/raw formats may be a bare array, so support both.
        var devices = root.ValueKind switch
        {
            JsonValueKind.Array => root,
            JsonValueKind.Object when root.TryGetProperty("devices", out var d) => d,
            _ => default
        };

        if (devices.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return devices.Deserialize<List<DeviceMetadataEntry>>(JsonOptions) ?? [];
    }

    private string ResolveCachePath()
    {
        if (!string.IsNullOrWhiteSpace(_options.MetadataCachePath))
        {
            return _options.MetadataCachePath;
        }

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "SNB", "Cache", "devices.json");
    }
}
