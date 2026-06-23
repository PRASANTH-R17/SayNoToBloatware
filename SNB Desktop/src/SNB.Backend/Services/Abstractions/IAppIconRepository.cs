namespace SNB.Backend.Services.Abstractions;

public interface IAppIconRepository
{
    Task UpsertAsync(string packageName, string iconPath, DateTime createdUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetAllPackageNamesAsync(CancellationToken cancellationToken = default);
}
