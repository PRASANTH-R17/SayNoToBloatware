using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Services.Implementations;

public sealed class IconCacheService : IIconCacheService
{
    private readonly IPathProvider _pathProvider;
    private readonly IAppIconRepository _appIconRepository;
    private readonly HashSet<string> _cachedPackages = new(StringComparer.Ordinal);

    public IconCacheService(IPathProvider pathProvider, IAppIconRepository appIconRepository)
    {
        _pathProvider = pathProvider;
        _appIconRepository = appIconRepository;
    }

    public string IconsDirectory => Path.Combine(_pathProvider.DataDirectory, "Cache", "Icons");

    public int Count => _cachedPackages.Count;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        _cachedPackages.Clear();
        Directory.CreateDirectory(IconsDirectory);

        var packageNames = await _appIconRepository.GetAllPackageNamesAsync(cancellationToken);
        foreach (var packageName in packageNames)
        {
            _cachedPackages.Add(packageName);
        }
    }

    public IReadOnlyList<string> GetMissingPackages(IReadOnlyList<string> installedPackages)
    {
        return installedPackages
            .Where(p => !_cachedPackages.Contains(p))
            .ToList();
    }

    public async Task<string?> SaveIconAsync(string packageName, byte[] pngBytes, CancellationToken cancellationToken = default)
    {
        if (pngBytes.Length == 0)
        {
            return null;
        }

        Directory.CreateDirectory(IconsDirectory);
        var iconPath = Path.Combine(IconsDirectory, $"{packageName}.png");
        await File.WriteAllBytesAsync(iconPath, pngBytes, cancellationToken);
        await _appIconRepository.UpsertAsync(packageName, iconPath, DateTime.UtcNow, cancellationToken);
        _cachedPackages.Add(packageName);
        return iconPath;
    }

    public async Task<int> ClearAsync(CancellationToken cancellationToken = default)
    {
        var removedCount = 0;

        if (Directory.Exists(IconsDirectory))
        {
            foreach (var iconPath in Directory.EnumerateFiles(IconsDirectory, "*.png"))
            {
                cancellationToken.ThrowIfCancellationRequested();
                File.Delete(iconPath);
                removedCount++;
            }
        }

        await _appIconRepository.DeleteAllAsync(cancellationToken);
        _cachedPackages.Clear();
        return removedCount;
    }
}
