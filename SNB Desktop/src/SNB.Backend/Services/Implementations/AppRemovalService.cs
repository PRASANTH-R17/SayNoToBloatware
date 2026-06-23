using Microsoft.Extensions.Logging;
using SNB.Backend.Infrastructure.Adb;
using SNB.Backend.Models;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Services.Implementations;

public sealed class AppRemovalService : IAppRemovalService
{
    private readonly IAdbExecutor _adbExecutor;
    private readonly ILogger<AppRemovalService> _logger;

    public AppRemovalService(IAdbExecutor adbExecutor, ILogger<AppRemovalService> logger)
    {
        _adbExecutor = adbExecutor;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RemovalResult>> RemoveAppsAsync(
        string serial,
        IReadOnlyList<string> packageNames,
        CancellationToken cancellationToken = default)
    {
        var results = new List<RemovalResult>();

        foreach (var packageName in packageNames)
        {
            var (success, output) = await _adbExecutor.UninstallPackageAsync(serial, packageName, cancellationToken);
            var error = success ? null : AdbOutputParser.ExtractUninstallFailure(output);

            if (!success)
            {
                _logger.LogWarning("Failed to remove {Package}: {Error}", packageName, error);
            }

            results.Add(new RemovalResult
            {
                PackageName = packageName,
                Success = success,
                ErrorMessage = error
            });
        }

        return results;
    }
}
