using SNB.Backend.Models;

namespace SNB.Backend.Services.Abstractions;

public interface IDeviceImageService
{
    Task<DeviceImageResult> ResolveAsync(DeviceInfo device, CancellationToken cancellationToken = default);
}
