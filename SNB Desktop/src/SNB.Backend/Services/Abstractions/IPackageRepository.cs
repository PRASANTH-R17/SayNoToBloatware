using SNB.Backend.Models;

namespace SNB.Backend.Services.Abstractions;

public interface IPackageRepository
{
    Task UpsertAsync(BloatwareInfo package, CancellationToken cancellationToken = default);
    Task UpsertIfNotFromSourceAsync(BloatwareInfo package, string protectedSource, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BloatwareInfo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task DeleteBySourceAsync(string source, CancellationToken cancellationToken = default);
}
