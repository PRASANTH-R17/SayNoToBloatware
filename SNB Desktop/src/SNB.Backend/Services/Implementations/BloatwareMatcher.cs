using SNB.Backend.Models;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Services.Implementations;

public sealed class BloatwareMatcher : IBloatwareMatcher
{
    private readonly IBloatwareCacheService _cacheService;

    public BloatwareMatcher(IBloatwareCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public DeviceApps Match(IReadOnlyList<string> installedPackages)
    {
        var allApps = new List<InstalledApp>();
        var recommended = new List<InstalledApp>();
        var alternatives = new List<InstalledApp>();

        foreach (var packageName in installedPackages.OrderBy(p => p, StringComparer.Ordinal))
        {
            BloatwareInfo? info = null;
            var isBloatware = _cacheService.TryGet(packageName, out info);

            var app = new InstalledApp
            {
                PackageName = packageName,
                AppName = packageName,
                IsBloatware = isBloatware,
                BloatwareRemovalType = info?.RemovalType,
                BloatwareDescription = info?.Description
            };

            allApps.Add(app);

            if (info?.RemovalType == RemovalType.Delete)
            {
                recommended.Add(app);
            }
            else if (info?.RemovalType == RemovalType.Replace)
            {
                alternatives.Add(app);
            }
        }

        return new DeviceApps
        {
            AllApps = allApps,
            RecommendedRemovalApps = recommended,
            AppsWithAlternatives = alternatives
        };
    }
}
