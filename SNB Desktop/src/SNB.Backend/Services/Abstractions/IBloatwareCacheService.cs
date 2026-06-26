using SNB.Backend.Models;

namespace SNB.Backend.Services.Abstractions;

public interface IBloatwareCacheService
{
    Task LoadAsync(CancellationToken cancellationToken = default);
    int Count { get; }
    bool TryGet(string packageName, out BloatwareInfo? info);
    IReadOnlyDictionary<string, BloatwareInfo> GetAll();
}
