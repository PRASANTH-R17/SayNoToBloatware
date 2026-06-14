using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SNB.Backend.DependencyInjection;
using SNB.Backend.Infrastructure.Database;
using SNB.Backend.Models;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Services.Implementations;

public sealed class BloatwareSyncService : IBloatwareSyncService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly IPackageRepository _packageRepository;
    private readonly ISourceRepository _sourceRepository;
    private readonly ILogger<BloatwareSyncService> _logger;

    public BloatwareSyncService(
        HttpClient httpClient,
        IPackageRepository packageRepository,
        ISourceRepository sourceRepository,
        ILogger<BloatwareSyncService> logger)
    {
        _httpClient = httpClient;
        _packageRepository = packageRepository;
        _sourceRepository = sourceRepository;
        _logger = logger;
    }

    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        // oem.json takes precedence over misc.json on package conflicts.
        await SyncSourceAsync(
            SnbBackendOptions.OemSourceName,
            SnbBackendOptions.OemSourceUrl,
            preferOverwrite: true,
            cancellationToken);

        await SyncSourceAsync(
            SnbBackendOptions.MiscSourceName,
            SnbBackendOptions.MiscSourceUrl,
            preferOverwrite: false,
            cancellationToken);
    }

    private async Task SyncSourceAsync(
        string sourceName,
        string sourceUrl,
        bool preferOverwrite,
        CancellationToken cancellationToken)
    {
        var existing = await _sourceRepository.GetAsync(sourceName, cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, sourceUrl);
        if (!string.IsNullOrWhiteSpace(existing?.ETag))
        {
            request.Headers.IfNoneMatch.Add(EntityTagHeaderValue.Parse(FormatEntityTag(existing.ETag)));
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
        {
            _logger.LogInformation("{Source} unchanged (304).", sourceName);
            await _sourceRepository.UpsertAsync(sourceName, existing?.ETag, DateTime.UtcNow, cancellationToken);
            return;
        }

        response.EnsureSuccessStatusCode();

        var eTag = response.Headers.ETag?.Tag?.Trim('"');
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var entries = await JsonSerializer.DeserializeAsync<List<BloatwareEntry>>(stream, JsonOptions, cancellationToken)
            ?? [];

        await _packageRepository.DeleteBySourceAsync(sourceName, cancellationToken);

        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Id))
            {
                continue;
            }

            var info = new BloatwareInfo
            {
                PackageName = entry.Id.Trim(),
                Description = entry.Description?.Trim() ?? string.Empty,
                RemovalType = RemovalTypeMapper.FromString(entry.Removal),
                Source = sourceName
            };

            if (preferOverwrite)
            {
                await _packageRepository.UpsertAsync(info, cancellationToken);
            }
            else
            {
                await _packageRepository.UpsertIfNotFromSourceAsync(info, SnbBackendOptions.OemSourceName, cancellationToken);
            }
        }

        await _sourceRepository.UpsertAsync(sourceName, eTag, DateTime.UtcNow, cancellationToken);
        _logger.LogInformation("Synced {Count} entries from {Source}.", entries.Count, sourceName);
    }

    private static string FormatEntityTag(string eTag)
    {
        var trimmed = eTag.Trim();
        if (trimmed.StartsWith('"'))
        {
            return trimmed;
        }

        return $"\"{trimmed}\"";
    }
}
