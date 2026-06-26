using System.Collections.Concurrent;
using SNB.Backend.Models;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Services.Implementations;

public sealed class BloatwareCacheService : IBloatwareCacheService
{
    private readonly IPackageRepository _packageRepository;
    private readonly ConcurrentDictionary<string, BloatwareInfo> _cache = new(StringComparer.Ordinal);

    public BloatwareCacheService(IPackageRepository packageRepository)
    {
        _packageRepository = packageRepository;
    }

    public int Count => _cache.Count;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        _cache.Clear();
        var packages = await _packageRepository.GetAllAsync(cancellationToken);
        foreach (var package in packages)
        {
            _cache[package.PackageName] = package;
        }
    }

    public bool TryGet(string packageName, out BloatwareInfo? info)
    {
        return _cache.TryGetValue(packageName, out info);
    }

    public IReadOnlyDictionary<string, BloatwareInfo> GetAll()
    {
        return _cache;
    }
}
