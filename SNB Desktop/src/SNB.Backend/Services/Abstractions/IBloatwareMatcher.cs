using SNB.Backend.Models;

namespace SNB.Backend.Services.Abstractions;

public interface IBloatwareMatcher
{
    DeviceApps Match(IReadOnlyList<string> installedPackages);
}
