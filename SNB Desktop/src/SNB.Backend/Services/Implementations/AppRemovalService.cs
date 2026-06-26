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
            results.Add(await RemoveOrDisableAsync(serial, packageName, cancellationToken));
        }

        return results;
    }

    private async Task<RemovalResult> RemoveOrDisableAsync(
        string serial,
        string packageName,
        CancellationToken cancellationToken)
    {
        var (uninstalled, uninstallOutput) = await _adbExecutor.UninstallPackageAsync(serial, packageName, cancellationToken);
        if (uninstalled)
        {
            return new RemovalResult { PackageName = packageName, Outcome = RemovalOutcome.Removed };
        }

        // Some OEMs (e.g. Vivo) block uninstalling protected core apps. Fall back to disabling
        // the package for the current user so it stops running, but only for that specific case.
        if (AdbOutputParser.IsUninstallRestricted(uninstallOutput))
        {
            _logger.LogInformation(
                "Uninstall of {Package} is user-restricted; attempting to disable instead.",
                packageName);

            var (disabled, disableOutput) = await _adbExecutor.DisablePackageAsync(serial, packageName, cancellationToken);
            if (disabled)
            {
                return new RemovalResult { PackageName = packageName, Outcome = RemovalOutcome.Disabled };
            }

            // Both uninstall and disable were rejected. On some OEM ROMs (e.g. Vivo/OriginOS) the
            // shell user is explicitly denied any package-management action on protected core apps,
            // so there is nothing more a non-root client can do. Log the raw device output for
            // diagnostics but surface a clean, human-readable reason instead of a Java stack trace.
            _logger.LogWarning(
                "Disable of {Package} was rejected by the device: {Output}",
                packageName,
                string.IsNullOrWhiteSpace(disableOutput) ? "(no output)" : disableOutput);
            return new RemovalResult
            {
                PackageName = packageName,
                Outcome = RemovalOutcome.Failed,
                ErrorMessage = "Protected by the device manufacturer \u2014 can't be removed or disabled without root access.",
            };
        }

        var error = AdbOutputParser.ExtractUninstallFailure(uninstallOutput);
        _logger.LogWarning("Failed to remove {Package}: {Error}", packageName, error);
        return new RemovalResult
        {
            PackageName = packageName,
            Outcome = RemovalOutcome.Failed,
            ErrorMessage = error,
        };
    }
}
