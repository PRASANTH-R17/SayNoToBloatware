using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SNB.Backend.DependencyInjection;
using SNB.Desktop.Services;
using SNB.Desktop.Services.Localization;
using SNB.Desktop.Services.Preferences;
using SNB.Desktop.ViewModels;
using SNB.Desktop.Views;

namespace SNB.Desktop;

public partial class App : Application
{
    /// <summary>
    /// The composition root. Every service and view model is registered here up front so later
    /// agents can flesh out views/view models WITHOUT editing this file. Add new registrations
    /// here only when introducing genuinely new types.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Services = ConfigureServices();

        // Restore the persisted theme + language before the first window is shown.
        var prefs = PreferencesService.Instance.Load();
        RequestedThemeVariant = string.Equals(prefs.Theme, "Dark", StringComparison.OrdinalIgnoreCase)
            ? ThemeVariant.Dark
            : ThemeVariant.Light;
        LocalizationService.Instance.SetLanguage(prefs.Language);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();
            var navigation = Services.GetRequiredService<INavigationService>();
            var toast = Services.GetRequiredService<IToastService>();

            navigation.Initialize(mainViewModel);
            if (toast is ToastService toastService)
            {
                toastService.Initialize(mainViewModel);
            }

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel,
            };

            // Stop the on-device bridge (best effort) when the app shuts down.
            desktop.ShutdownRequested += OnShutdownRequested;

            // Perform the initial navigation only after the shell singleton is fully constructed,
            // so page view models that depend on MainWindowViewModel resolve without a DI cycle.
            mainViewModel.Initialize();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        try
        {
            var catalog = Services.GetService<IDeviceCatalogService>();
            if (catalog is not null)
            {
                // Run on the thread pool (no UI SynchronizationContext) so the async
                // adb cleanup cannot deadlock against a blocked UI thread, and cap it
                // so a slow/hung device never blocks process exit.
                Task.Run(() => catalog.StopAsync()).Wait(TimeSpan.FromSeconds(3));
            }
        }
        catch
        {
            // Best effort during shutdown; never block app exit.
        }
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Real backend: ADB + on-device bridge client + SQLite repositories + orchestrator.
        services.AddSnbBackend();

        // Desktop services (singletons).
        services.AddSingleton<IDeviceCatalogService, DeviceCatalogService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IFileSaveService, FileSaveService>();
        services.AddSingleton<IToastService, ToastService>();

        // Shell view model (singleton - one shell for the app lifetime).
        services.AddSingleton<MainWindowViewModel>();

        // Page view models (transient - fresh instance per navigation).
        services.AddTransient<DeviceSelectionViewModel>();
        services.AddTransient<ApplicationsViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<AboutViewModel>();

        // Dialog / panel view models (transient - fresh instance per show).
        services.AddTransient<AppDetailsViewModel>();
        services.AddTransient<ExportViewModel>();
        services.AddTransient<ClearIconCacheConfirmationViewModel>();
        services.AddTransient<RemovalConfirmationViewModel>();
        services.AddTransient<RemovalProgressViewModel>();
        services.AddTransient<RemovalCompleteViewModel>();

        return services.BuildServiceProvider();
    }
}
