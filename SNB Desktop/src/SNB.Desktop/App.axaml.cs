using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SNB.Desktop.Services;
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

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel,
            };

            // Perform the initial navigation only after the shell singleton is fully constructed,
            // so page view models that depend on MainWindowViewModel resolve without a DI cycle.
            mainViewModel.Initialize();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Services (singletons).
        services.AddSingleton<IMockDataService, MockDataService>();
        services.AddSingleton<INavigationService, NavigationService>();

        // Shell view model (singleton - one shell for the app lifetime).
        services.AddSingleton<MainWindowViewModel>();

        // Page view models (transient - fresh instance per navigation).
        services.AddTransient<DeviceSelectionViewModel>();
        services.AddTransient<ApplicationsViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<AboutViewModel>();

        // Dialog / panel view models (transient - fresh instance per show).
        services.AddTransient<AppDetailsViewModel>();
        services.AddTransient<RemovalConfirmationViewModel>();
        services.AddTransient<RemovalProgressViewModel>();
        services.AddTransient<RemovalCompleteViewModel>();

        return services.BuildServiceProvider();
    }
}
