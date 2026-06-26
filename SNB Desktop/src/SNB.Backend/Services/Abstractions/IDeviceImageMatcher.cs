using SNB.Backend.Models;

namespace SNB.Backend.Services.Abstractions;

public interface IDeviceImageMatcher
{
    MatchOutcome Match(
        DeviceInfo device,
        IReadOnlyList<DeviceMetadataEntry> entries);
}
