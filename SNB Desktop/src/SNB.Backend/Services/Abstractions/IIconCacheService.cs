namespace SNB.Backend.Services.Abstractions;

public interface IIconCacheService
{
    Task LoadAsync(CancellationToken cancellationToken = default);
    int Count { get; }
    IReadOnlyList<string> GetMissingPackages(IReadOnlyList<string> installedPackages);
    Task<string?> SaveIconAsync(string packageName, byte[] pngBytes, CancellationToken cancellationToken = default);
    string IconsDirectory { get; }
}
