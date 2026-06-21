using SNB.Backend.Models.Bridge;

namespace SNB.Backend.Services.Abstractions;

public interface IBridgeHttpClient
{
    Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BridgeAppDto>> QueryAppsAsync(IReadOnlyList<string> packageNames, CancellationToken cancellationToken = default);
}
