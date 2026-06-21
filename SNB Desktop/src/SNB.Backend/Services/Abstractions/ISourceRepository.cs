namespace SNB.Backend.Services.Abstractions;

public sealed class SourceSyncInfo
{
    public required string Name { get; init; }
    public string? ETag { get; init; }
    public DateTime? LastSyncUtc { get; init; }
}

public interface ISourceRepository
{
    Task<SourceSyncInfo?> GetAsync(string name, CancellationToken cancellationToken = default);
    Task UpsertAsync(string name, string? eTag, DateTime lastSyncUtc, CancellationToken cancellationToken = default);
}
