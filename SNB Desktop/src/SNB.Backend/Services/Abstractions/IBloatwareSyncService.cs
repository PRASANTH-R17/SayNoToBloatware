namespace SNB.Backend.Services.Abstractions;

public interface IBloatwareSyncService
{
    Task SyncAsync(CancellationToken cancellationToken = default);
}
