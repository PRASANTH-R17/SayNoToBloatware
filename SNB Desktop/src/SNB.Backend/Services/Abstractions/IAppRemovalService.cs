using SNB.Backend.Models;

namespace SNB.Backend.Services.Abstractions;

public interface IAppRemovalService
{
    Task<IReadOnlyList<RemovalResult>> RemoveAppsAsync(string serial, IReadOnlyList<string> packageNames, CancellationToken cancellationToken = default);
}
