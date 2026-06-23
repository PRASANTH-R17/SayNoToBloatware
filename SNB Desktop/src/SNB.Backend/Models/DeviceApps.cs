namespace SNB.Backend.Models;

public sealed class DeviceApps
{
    public required IReadOnlyList<InstalledApp> AllApps { get; init; }
    public required IReadOnlyList<InstalledApp> RecommendedRemovalApps { get; init; }
    public required IReadOnlyList<InstalledApp> AppsWithAlternatives { get; init; }
}
