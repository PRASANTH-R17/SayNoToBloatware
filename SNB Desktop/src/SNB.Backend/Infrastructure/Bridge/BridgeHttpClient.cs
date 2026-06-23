using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNB.Backend.DependencyInjection;
using SNB.Backend.Models.Bridge;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Infrastructure.Bridge;

public sealed class BridgeHttpClient : IBridgeHttpClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<BridgeHttpClient> _logger;
    private readonly SnbBackendOptions _options;

    public BridgeHttpClient(
        HttpClient httpClient,
        ILogger<BridgeHttpClient> logger,
        IOptions<SnbBackendOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
        _httpClient.BaseAddress = new Uri($"http://127.0.0.1:{_options.BridgePort}/");
    }

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<BridgeHealthResponse>("/health", JsonOptions, cancellationToken);
            return string.Equals(response?.Status, "ok", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Bridge health check failed.");
            return false;
        }
    }

    public async Task<IReadOnlyList<BridgeAppDto>> QueryAppsAsync(IReadOnlyList<string> packageNames, CancellationToken cancellationToken = default)
    {
        if (packageNames.Count == 0)
        {
            return [];
        }

        var request = new AppsQueryRequest { PackageNames = packageNames.ToList() };
        using var response = await _httpClient.PostAsJsonAsync("/apps/query", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var apps = await JsonSerializer.DeserializeAsync<List<BridgeAppDto>>(stream, JsonOptions, cancellationToken);
        return apps ?? [];
    }
}
