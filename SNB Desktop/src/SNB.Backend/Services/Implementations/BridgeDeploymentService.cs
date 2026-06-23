using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNB.Backend.DependencyInjection;
using SNB.Backend.Infrastructure.Adb;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Services.Implementations;

public sealed class BridgeDeploymentService : IBridgeDeploymentService
{
    private readonly IAdbExecutor _adbExecutor;
    private readonly IBridgeHttpClient _bridgeHttpClient;
    private readonly IPathProvider _pathProvider;
    private readonly ILogger<BridgeDeploymentService> _logger;
    private readonly SnbBackendOptions _options;

    public BridgeDeploymentService(
        IAdbExecutor adbExecutor,
        IBridgeHttpClient bridgeHttpClient,
        IPathProvider pathProvider,
        ILogger<BridgeDeploymentService> logger,
        IOptions<SnbBackendOptions> options)
    {
        _adbExecutor = adbExecutor;
        _bridgeHttpClient = bridgeHttpClient;
        _pathProvider = pathProvider;
        _logger = logger;
        _options = options.Value;
    }

    public async Task DeployAsync(string serial, CancellationToken cancellationToken = default)
    {
        // Port forwarding is per adb-server session (not persisted on the device), so re-establish it
        // first. The call is cheap and idempotent, and it lets the health probe below reach the bridge.
        await _adbExecutor.ForwardPortAsync(serial, _options.BridgePort, _options.BridgePort, cancellationToken);

        // Fast path: when the bridge is already running and responsive (e.g. a re-scan in the same
        // session), skip the slow APK push and app launch entirely.
        if (await _bridgeHttpClient.CheckHealthAsync(cancellationToken))
        {
            _logger.LogInformation("Bridge already running and healthy; skipping deployment.");
            return;
        }

        var apkPath = Path.Combine(_pathProvider.BaseDirectory, "Bridge", "snb_bridge.apk");
        if (!File.Exists(apkPath))
        {
            throw new FileNotFoundException($"Bridge APK not found at {apkPath}");
        }

        // The USB APK transfer is the slowest step, so only push when the bridge is missing or the
        // installed version is older than the one bundled with the desktop app.
        var installedVersion = await _adbExecutor.GetInstalledVersionCodeAsync(serial, cancellationToken);
        if (installedVersion is null || installedVersion < _options.BridgeVersionCode)
        {
            await _adbExecutor.InstallBridgeAsync(serial, apkPath, reinstall: installedVersion is not null, cancellationToken);
        }
        else
        {
            _logger.LogInformation(
                "Bridge v{InstalledVersion} already installed (bundled v{BundledVersion}); skipping APK install.",
                installedVersion,
                _options.BridgeVersionCode);
        }

        await _adbExecutor.LaunchBridgeAsync(serial, cancellationToken);

        var deadline = DateTime.UtcNow + _options.BridgeHealthTimeout;
        var delay = TimeSpan.FromMilliseconds(500);

        while (DateTime.UtcNow < deadline)
        {
            if (await _bridgeHttpClient.CheckHealthAsync(cancellationToken))
            {
                _logger.LogInformation("Bridge health check succeeded.");
                return;
            }

            await Task.Delay(delay, cancellationToken);
            delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 1.5, 3000));
        }

        throw new TimeoutException("Bridge health check did not succeed within the configured timeout.");
    }

    public async Task StopAsync(string serial, CancellationToken cancellationToken = default)
    {
        await _adbExecutor.StopBridgeAsync(serial, cancellationToken).ConfigureAwait(false);
        await _adbExecutor.RemovePortForwardAsync(serial, _options.BridgePort, cancellationToken).ConfigureAwait(false);
    }
}
