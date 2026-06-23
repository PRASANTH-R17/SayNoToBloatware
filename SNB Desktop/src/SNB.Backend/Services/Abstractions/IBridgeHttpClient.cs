using SNB.Backend.Models.Bridge;

namespace SNB.Backend.Services.Abstractions;

public interface IBridgeHttpClient
{
    Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BridgeAppDto>> QueryAppsAsync(IReadOnlyList<string> packageNames, CancellationToken cancellationToken = default);

    /// <summary>Fetches metadata (no icons) for every installed app via GET /apps.</summary>
    Task<IReadOnlyList<BridgeAppDto>> GetAllAppsAsync(CancellationToken cancellationToken = default);
}
