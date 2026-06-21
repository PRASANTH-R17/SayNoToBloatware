using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SNB.Backend.DependencyInjection;
using SNB.Backend.Services.Abstractions;
using SNB.Cli.Console;

namespace SNB.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddSnbBackend();
        services.AddSingleton<DeviceWorkflow>();

        await using var provider = services.BuildServiceProvider();

        ConsolePrompt.WriteLine("Say No to Bloatware CLI");
        ConsolePrompt.WriteLine();

        var databaseInitializer = provider.GetRequiredService<IDatabaseInitializer>();
        var bloatwareSyncService = provider.GetRequiredService<IBloatwareSyncService>();
        var bloatwareCacheService = provider.GetRequiredService<IBloatwareCacheService>();
        var iconCacheService = provider.GetRequiredService<IIconCacheService>();
        var deviceMetadataService = provider.GetRequiredService<IDeviceMetadataService>();
        var deviceWorkflow = provider.GetRequiredService<DeviceWorkflow>();

        using var cts = new CancellationTokenSource();
        System.Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            ConsolePrompt.WriteLine("Initializing database...");
            await databaseInitializer.InitializeAsync(cts.Token);

            ConsolePrompt.WriteLine("Syncing bloatware database... (oem.json, misc.json)");
            try
            {
                await bloatwareSyncService.SyncAsync(cts.Token);
            }
            catch (HttpRequestException ex)
            {
                ConsolePrompt.WriteLine($"Sync skipped. Using local database. Reason: {ex.Message}");
            }
            catch (TaskCanceledException) when (!cts.IsCancellationRequested)
            {
                ConsolePrompt.WriteLine("Sync skipped due to network timeout. Using local database.");
            }

            ConsolePrompt.WriteLine("Loading bloatware cache...");
            await bloatwareCacheService.LoadAsync(cts.Token);
            ConsolePrompt.WriteLine($"Loading bloatware cache... {bloatwareCacheService.Count:N0} packages");

            ConsolePrompt.WriteLine("Loading icon cache...");
            await iconCacheService.LoadAsync(cts.Token);
            ConsolePrompt.WriteLine($"Loading icon cache... {iconCacheService.Count} icons");

            ConsolePrompt.WriteLine("Loading device metadata...");
            try
            {
                await deviceMetadataService.LoadAsync(cts.Token);
                ConsolePrompt.WriteLine($"Loading device metadata... {deviceMetadataService.Entries.Count} devices ({deviceMetadataService.Source})");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                ConsolePrompt.WriteLine($"Device metadata unavailable. Reason: {ex.Message}");
            }

            ConsolePrompt.WriteLine("Detecting devices...");
            await deviceWorkflow.RunAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            ConsolePrompt.WriteLine();
            ConsolePrompt.WriteLine("Cancelled.");
            return 1;
        }
        catch (Exception ex)
        {
            ConsolePrompt.WriteLine();
            ConsolePrompt.WriteLine($"Error: {ex.Message}");
            return 1;
        }

        return 0;
    }
}
