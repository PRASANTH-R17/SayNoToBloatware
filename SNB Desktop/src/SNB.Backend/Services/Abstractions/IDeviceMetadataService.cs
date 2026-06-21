using SNB.Backend.Models;

namespace SNB.Backend.Services.Abstractions;

public interface IDeviceMetadataService
{
    IReadOnlyList<DeviceMetadataEntry> Entries { get; }

    MetadataSource Source { get; }

    Task LoadAsync(CancellationToken cancellationToken = default);
}
