using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNB.Backend.DependencyInjection;
using SNB.Backend.Infrastructure.Adb;
using SNB.Backend.Models;
using SNB.Backend.Services.Abstractions;

namespace SNB.Backend.Infrastructure.Adb;

public sealed class AdbProcessRunner : IAdbExecutor
{
    private readonly AdbLocator _adbLocator;
    private readonly ILogger<AdbProcessRunner> _logger;
    private readonly SnbBackendOptions _options;

    public AdbProcessRunner(
        AdbLocator adbLocator,
        ILogger<AdbProcessRunner> logger,
        IOptions<SnbBackendOptions> options)
    {
        _adbLocator = adbLocator;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<(string Serial, string State)>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        var output = await RunAdbAsync(["devices"], cancellationToken);
        return AdbOutputParser.ParseDevices(output);
    }

    public async Task<DeviceInfo?> GetDeviceInfoAsync(string serial, CancellationToken cancellationToken = default)
    {
        var manufacturer = (await RunAdbAsync(["-s", serial, "shell", "getprop", "ro.product.manufacturer"], cancellationToken)).Trim();
        var model = (await RunAdbAsync(["-s", serial, "shell", "getprop", "ro.product.model"], cancellationToken)).Trim();
        var androidVersion = (await RunAdbAsync(["-s", serial, "shell", "getprop", "ro.build.version.release"], cancellationToken)).Trim();
        var brand = (await RunAdbAsync(["-s", serial, "shell", "getprop", "ro.product.brand"], cancellationToken)).Trim();
        var marketName = (await RunAdbAsync(["-s", serial, "shell", "getprop", "ro.product.marketname"], cancellationToken)).Trim();
        var marketingName = (await RunAdbAsync(["-s", serial, "shell", "getprop", "ro.config.marketing_name"], cancellationToken)).Trim();
        var vendorMarketName = (await RunAdbAsync(["-s", serial, "shell", "getprop", "ro.product.vendor.marketname"], cancellationToken)).Trim();
        // vivo/iQOO devices leave the standard marketname props empty and expose the marketing name here.
        var vivoMarketName = (await RunAdbAsync(["-s", serial, "shell", "getprop", "ro.vivo.market.name"], cancellationToken)).Trim();
        var vivoReleaseName = (await RunAdbAsync(["-s", serial, "shell", "getprop", "ro.vivo.product.release.name"], cancellationToken)).Trim();
        var sdkVersion = (await RunAdbAsync(["-s", serial, "shell", "getprop", "ro.build.version.sdk"], cancellationToken)).Trim();
        var batteryOutput = await RunAdbAsync(["-s", serial, "shell", "dumpsys", "battery"], cancellationToken);

        if (string.IsNullOrWhiteSpace(serial))
        {
            return null;
        }

        var resolvedManufacturer = string.IsNullOrWhiteSpace(manufacturer) ? "Unknown" : manufacturer;
        var resolvedModel = string.IsNullOrWhiteSpace(model) ? "Unknown Device" : model;
        var resolvedBrand = string.IsNullOrWhiteSpace(brand) ? resolvedManufacturer : brand;
        var resolvedMarketName = FirstNonEmpty(marketName, marketingName, vendorMarketName, vivoMarketName, vivoReleaseName) ?? resolvedModel;

        return new DeviceInfo
        {
            Serial = serial,
            Manufacturer = resolvedManufacturer,
            Model = resolvedModel,
            AndroidVersion = string.IsNullOrWhiteSpace(androidVersion) ? "Unknown" : androidVersion,
            Brand = resolvedBrand,
            MarketName = resolvedMarketName,
            SdkVersion = sdkVersion,
            BatteryPercent = ParseBatteryLevel(batteryOutput)
        };
    }

    private static int ParseBatteryLevel(string dumpsysOutput)
    {
        if (string.IsNullOrWhiteSpace(dumpsysOutput))
        {
            return 0;
        }

        foreach (var rawLine in dumpsysOutput.Split('\n'))
        {
            var line = rawLine.Trim();
            if (!line.StartsWith("level:", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var value = line["level:".Length..].Trim();
            if (int.TryParse(value, out var level))
            {
                return Math.Clamp(level, 0, 100);
            }
        }

        return 0;
    }

    public async Task<IReadOnlyList<string>> ListPackagesAsync(string serial, CancellationToken cancellationToken = default)
    {
        var output = await RunAdbAsync(["-s", serial, "shell", "pm", "list", "packages"], cancellationToken);
        return AdbOutputParser.ParsePackageList(output);
    }

    public async Task<bool> IsBridgeInstalledAsync(string serial, CancellationToken cancellationToken = default)
    {
        var output = await RunAdbAsync(
            ["-s", serial, "shell", "pm", "list", "packages", _options.BridgePackageName],
            cancellationToken);
        return AdbOutputParser.IsPackageInstalled(output, _options.BridgePackageName);
    }

    public async Task<int?> GetInstalledVersionCodeAsync(string serial, CancellationToken cancellationToken = default)
    {
        var output = await RunAdbAsync(
            ["-s", serial, "shell", "dumpsys", "package", _options.BridgePackageName],
            cancellationToken);
        return AdbOutputParser.ParseVersionCode(output);
    }

    public async Task InstallBridgeAsync(string serial, string apkPath, bool reinstall, CancellationToken cancellationToken = default)
    {
        var args = new List<string> { "-s", serial, "install" };
        if (reinstall)
        {
            args.Add("-r");
        }

        args.Add(apkPath);
        await RunAdbAsync(args, cancellationToken);
    }

    public async Task LaunchBridgeAsync(string serial, CancellationToken cancellationToken = default)
    {
        await RunAdbAsync(
            ["-s", serial, "shell", "monkey", "-p", _options.BridgePackageName, "1"],
            cancellationToken);
    }

    public async Task StopBridgeAsync(string serial, CancellationToken cancellationToken = default)
    {
        await RunAdbAsync(
            ["-s", serial, "shell", "am", "force-stop", _options.BridgePackageName],
            cancellationToken);
    }

    public async Task ForwardPortAsync(string serial, int localPort, int remotePort, CancellationToken cancellationToken = default)
    {
        await RunAdbAsync(
            ["-s", serial, "forward", $"tcp:{localPort}", $"tcp:{remotePort}"],
            cancellationToken);
    }

    public async Task RemovePortForwardAsync(string serial, int localPort, CancellationToken cancellationToken = default)
    {
        await RunAdbAsync(
            ["-s", serial, "forward", "--remove", $"tcp:{localPort}"],
            cancellationToken);
    }

    public async Task<(bool Success, string Output)> UninstallPackageAsync(string serial, string packageName, CancellationToken cancellationToken = default)
    {
        var output = await RunAdbAsync(
            ["-s", serial, "shell", "pm", "uninstall", "--user", "0", packageName],
            cancellationToken);
        var success = AdbOutputParser.IsUninstallSuccess(output);
        return (success, output.Trim());
    }

    public async Task<(bool Success, string Output)> DisablePackageAsync(string serial, string packageName, CancellationToken cancellationToken = default)
    {
        var output = await RunAdbAsync(
            ["-s", serial, "shell", "pm", "disable-user", "--user", "0", packageName],
            cancellationToken);
        var success = AdbOutputParser.IsDisableSuccess(output);
        return (success, output.Trim());
    }

    private static string? FirstNonEmpty(params string[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    internal async Task<string> RunAdbAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var adbPath = _adbLocator.GetAdbPath();
        var startInfo = new ProcessStartInfo
        {
            FileName = adbPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        foreach (var arg in arguments)
        {
            startInfo.ArgumentList.Add(arg);
        }

        _logger.LogDebug("Running adb {Arguments}", string.Join(' ', arguments));

        using var process = new Process { StartInfo = startInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                outputBuilder.AppendLine(e.Data);
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                errorBuilder.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        var stdout = outputBuilder.ToString();
        var stderr = errorBuilder.ToString();

        if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(stderr))
        {
            _logger.LogWarning("adb exited with code {ExitCode}: {Stderr}", process.ExitCode, stderr.Trim());
        }

        return string.IsNullOrWhiteSpace(stdout) ? stderr : stdout;
    }
}
