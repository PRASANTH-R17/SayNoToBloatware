using SNB.Backend.Infrastructure.Database;
using SNB.Backend.Models;
using SNB.Backend.Services.Abstractions;
using SNB.Cli.Console;

namespace SNB.Cli;

public sealed class DeviceWorkflow
{
    private readonly IDeviceSessionOrchestrator _orchestrator;
    private readonly IAppRemovalService _appRemovalService;

    public DeviceWorkflow(
        IDeviceSessionOrchestrator orchestrator,
        IAppRemovalService appRemovalService)
    {
        _orchestrator = orchestrator;
        _appRemovalService = appRemovalService;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var items = await _orchestrator.DetectDevicesWithImagesAsync(cancellationToken);
        if (items.Count == 0)
        {
            ConsolePrompt.WriteLine("No devices found. Connect a device with USB debugging enabled and try again.");
            return;
        }

        ConsolePrompt.WriteLine();
        ConsolePrompt.WriteLine("Connected Devices");
        ConsolePrompt.WriteLine();

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            ConsolePrompt.WriteLine(
                $"{i + 1}. {item.Device.DisplayName} — Android {item.Device.AndroidVersion} — {item.Device.Serial}");
            WriteMatchSummary(item, "     ");
            ConsolePrompt.WriteLine();
        }

        ConsolePrompt.WriteLine();
        var selection = ConsolePrompt.ReadIntInRange($"Select device (1-{items.Count}): ", 1, items.Count);
        var selectedDevice = items[selection - 1].Device;

        ConsolePrompt.WriteLine();
        ConsolePrompt.WriteLine($"Device: {selectedDevice.DisplayName}");
        WriteMatchSummary(items[selection - 1]);
        ConsolePrompt.WriteLine();

        var progress = new Progress<string>(message => ConsolePrompt.WriteLine(message));
        var deviceApps = await _orchestrator.PrepareDeviceAsync(selectedDevice.Serial, progress, cancellationToken);

        await RunMainMenuAsync(selectedDevice.Serial, deviceApps, cancellationToken);
    }

    private static void WriteMatchSummary(DeviceListItem item, string indent = "")
    {
        var d = item.Device;
        var img = item.Image;
        ConsolePrompt.WriteLine($"{indent}Match inputs: Brand='{d.Brand}', Model='{d.Model}', Market Name='{d.MarketName}'");
        ConsolePrompt.WriteLine($"{indent}Matched using: {img.MatchStrategy} (found: {img.MatchFound})");
        if (img.MatchFound)
        {
            ConsolePrompt.WriteLine($"{indent}Match parameter: {img.MatchedValue}");
            ConsolePrompt.WriteLine($"{indent}Matched entry: {img.DeviceName} ({img.MatchedEntrySlug})");
            if (img.MatchStrategy == MatchStrategy.Fuzzy && img.FuzzyScore is { } score)
            {
                ConsolePrompt.WriteLine($"{indent}Fuzzy score: {score:F3}");
            }
        }
        ConsolePrompt.WriteLine($"{indent}Image Path: {img.ImagePath}");
    }

    private async Task RunMainMenuAsync(string serial, DeviceApps deviceApps, CancellationToken cancellationToken)
    {
        while (true)
        {
            ConsolePrompt.WriteLine();
            ConsolePrompt.WriteLine("Main Menu");
            ConsolePrompt.WriteLine();
            ConsolePrompt.WriteLine($"1. All Apps");
            ConsolePrompt.WriteLine($"2. Recommended Removal ({deviceApps.RecommendedRemovalApps.Count} apps)");
            ConsolePrompt.WriteLine($"3. Apps with Alternatives ({deviceApps.AppsWithAlternatives.Count} apps)");
            ConsolePrompt.WriteLine("4. Remove YouTube (com.google.android.youtube)");
            ConsolePrompt.WriteLine("5. Exit");
            ConsolePrompt.WriteLine();

            var option = ConsolePrompt.ReadIntInRange("Select option (1-5): ", 1, 5);

            switch (option)
            {
                case 1:
                    await HandleAllAppsAsync(serial, deviceApps, cancellationToken);
                    break;
                case 2:
                    await HandleAutoSelectAsync(
                        serial,
                        deviceApps.RecommendedRemovalApps,
                        "Recommended Removal",
                        cancellationToken);
                    break;
                case 3:
                    await HandleAutoSelectAsync(
                        serial,
                        deviceApps.AppsWithAlternatives,
                        "Apps with Alternatives",
                        cancellationToken);
                    break;
                case 4:
                    await HandleRemoveYouTubeAsync(serial, cancellationToken);
                    break;
                case 5:
                    ConsolePrompt.WriteLine();
                    ConsolePrompt.WriteLine("Stopping SNB Bridge...");
                    await _orchestrator.StopBridgeAsync(serial, cancellationToken);
                    ConsolePrompt.WriteLine("Goodbye.");
                    return;
            }
        }
    }

    private async Task HandleRemoveYouTubeAsync(string serial, CancellationToken cancellationToken)
    {
        ConsolePrompt.WriteLine();
        if (!ConsolePrompt.ReadYesNo("Remove YouTube (com.google.android.youtube) for user 0? (Y/N): "))
        {
            return;
        }

        var youtube = new InstalledApp
        {
            AppName = "YouTube",
            PackageName = "com.google.android.youtube"
        };

        await ExecuteRemovalAsync(serial, [youtube], cancellationToken);
    }

    private async Task HandleAllAppsAsync(string serial, DeviceApps deviceApps, CancellationToken cancellationToken)
    {
        ConsolePrompt.WriteLine();
        ConsolePrompt.WriteLine($"All Installed Apps ({deviceApps.AllApps.Count})");
        ConsolePrompt.WriteLine();

        for (var i = 0; i < deviceApps.AllApps.Count; i++)
        {
            WriteAppLine(i + 1, deviceApps.AllApps[i], includeRemovalTag: true);
        }

        ConsolePrompt.WriteLine();
        var indices = ConsolePrompt.ReadCommaSeparatedIndices(
            "Enter app numbers to remove (comma-separated, e.g. 1,4,7): ",
            deviceApps.AllApps.Count);

        var selectedApps = indices.Select(i => deviceApps.AllApps[i - 1]).ToList();
        await ReviewAndRemoveAsync(serial, selectedApps, cancellationToken);
    }

    private async Task HandleAutoSelectAsync(
        string serial,
        IReadOnlyList<InstalledApp> apps,
        string title,
        CancellationToken cancellationToken)
    {
        ConsolePrompt.WriteLine();
        ConsolePrompt.WriteLine($"{title} — {apps.Count} apps");
        ConsolePrompt.WriteLine();

        for (var i = 0; i < apps.Count; i++)
        {
            WriteAppLine(i + 1, apps[i], includeRemovalTag: false);
        }

        ConsolePrompt.WriteLine();
        if (!ConsolePrompt.ReadYesNo($"Remove all {apps.Count} apps? (Y/N): "))
        {
            return;
        }

        await ExecuteRemovalAsync(serial, apps, cancellationToken);
    }

    private async Task ReviewAndRemoveAsync(
        string serial,
        IReadOnlyList<InstalledApp> selectedApps,
        CancellationToken cancellationToken)
    {
        ConsolePrompt.WriteLine();
        ConsolePrompt.WriteLine($"Review — {selectedApps.Count} apps selected for removal");
        ConsolePrompt.WriteLine();

        for (var i = 0; i < selectedApps.Count; i++)
        {
            WriteAppLine(i + 1, selectedApps[i], includeRemovalTag: true, reviewMode: true);
        }

        ConsolePrompt.WriteLine();
        if (!ConsolePrompt.ReadYesNo("Remove these apps? (Y/N): "))
        {
            return;
        }

        await ExecuteRemovalAsync(serial, selectedApps, cancellationToken);
    }

    private async Task ExecuteRemovalAsync(
        string serial,
        IReadOnlyList<InstalledApp> apps,
        CancellationToken cancellationToken)
    {
        ConsolePrompt.WriteLine();
        ConsolePrompt.WriteLine("Removing apps...");
        ConsolePrompt.WriteLine();

        var packageNames = apps.Select(a => a.PackageName).ToList();
        var results = await _appRemovalService.RemoveAppsAsync(serial, packageNames, cancellationToken);

        var removed = 0;
        var failed = 0;

        foreach (var result in results)
        {
            if (result.Success)
            {
                removed++;
                ConsolePrompt.WriteLine($"  [OK]   {result.PackageName}");
            }
            else
            {
                failed++;
                ConsolePrompt.WriteLine($"  [FAIL] {result.PackageName} — {result.ErrorMessage}");
            }
        }

        ConsolePrompt.WriteLine();
        ConsolePrompt.WriteLine($"Done. {removed} removed, {failed} failed.");
        ConsolePrompt.WaitForEnter();
    }

    private static void WriteAppLine(int index, InstalledApp app, bool includeRemovalTag, bool reviewMode = false)
    {
        var displayName = string.IsNullOrWhiteSpace(app.AppName) ? app.PackageName : app.AppName;
        var tag = string.Empty;

        if (includeRemovalTag && app.IsBloatware && app.BloatwareRemovalType is not null)
        {
            var removal = RemovalTypeMapper.ToString(app.BloatwareRemovalType.Value);
            tag = reviewMode ? $"[{removal}]" : $"[bloatware: {removal}]";
        }

        ConsolePrompt.WriteLine($"  {index,3}. {displayName,-32} {app.PackageName,-40} {tag}".TrimEnd());
    }
}
