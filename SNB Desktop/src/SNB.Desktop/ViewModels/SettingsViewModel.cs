using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SNB.Backend.DependencyInjection;
using SNB.Backend.Services.Abstractions;
using SNB.Desktop.Services;
using SNB.Desktop.Services.Localization;
using SNB.Desktop.Services.Preferences;

namespace SNB.Desktop.ViewModels;

/// <summary>
/// Settings page - database, cache, and general preferences. Database version / last sync
/// come from the bloatware <see cref="ISourceRepository"/>; the icon cache path comes from
/// the backend <see cref="IIconCacheService"/>.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    /// <summary>Display label for the code-mixed Tamil ("Tanglish") option.</summary>
    public const string TamilLanguageLabel = "தமிழ் (Easy)";

    public const string EnglishLanguageLabel = "English";

    public const string LightThemeLabel = "Light";

    public const string DarkThemeLabel = "Dark";

    private readonly ISourceRepository _sourceRepository;
    private readonly IIconCacheService _iconCacheService;
    private readonly INavigationService _navigation;
    private readonly IServiceProvider _services;

    public SettingsViewModel(
        ISourceRepository sourceRepository,
        IIconCacheService iconCacheService,
        INavigationService navigation,
        IServiceProvider services)
    {
        _sourceRepository = sourceRepository;
        _iconCacheService = iconCacheService;
        _navigation = navigation;
        _services = services;
        _iconCacheLocation = _iconCacheService.IconsDirectory;
        _language = LocalizationService.Instance.CurrentLanguage == AppLanguage.Tamil
            ? TamilLanguageLabel
            : EnglishLanguageLabel;
        _theme = string.Equals(PreferencesService.Instance.Current.Theme, DarkThemeLabel, StringComparison.OrdinalIgnoreCase)
            ? DarkThemeLabel
            : LightThemeLabel;
        _autoUpdateDatabase = PreferencesService.Instance.Current.AutoUpdateDatabase;
        _ = LoadAsync();
    }

    public ObservableCollection<string> ThemeOptions { get; } = new()
    {
        LightThemeLabel,
        DarkThemeLabel,
    };

    public ObservableCollection<string> LanguageOptions { get; } = new()
    {
        EnglishLanguageLabel,
        TamilLanguageLabel,
    };

    [ObservableProperty]
    private string _databaseVersion = "Unknown";

    [ObservableProperty]
    private string _lastSync = "Never";

    [ObservableProperty]
    private bool _autoUpdateDatabase = true;

    [ObservableProperty]
    private string _iconCacheLocation;

    [ObservableProperty]
    private string _theme;

    [ObservableProperty]
    private string _language;

    partial void OnThemeChanged(string value)
    {
        var isDark = value == DarkThemeLabel;
        if (Application.Current is { } app)
        {
            app.RequestedThemeVariant = isDark ? ThemeVariant.Dark : ThemeVariant.Light;
        }

        PreferencesService.Instance.SetTheme(isDark ? DarkThemeLabel : LightThemeLabel);
    }

    partial void OnLanguageChanged(string value)
    {
        var language = value == TamilLanguageLabel ? AppLanguage.Tamil : AppLanguage.English;
        LocalizationService.Instance.SetLanguage(language);
        PreferencesService.Instance.SetLanguage(language);
    }

    partial void OnAutoUpdateDatabaseChanged(bool value)
    {
        PreferencesService.Instance.SetAutoUpdateDatabase(value);
    }

    [RelayCommand]
    private void ClearCache()
    {
        var vm = _services.GetRequiredService<ClearIconCacheConfirmationViewModel>();
        _navigation.ShowDialog(vm);
    }

    private async Task LoadAsync()
    {
        try
        {
            var oem = await _sourceRepository.GetAsync(SnbBackendOptions.OemSourceName);
            var misc = await _sourceRepository.GetAsync(SnbBackendOptions.MiscSourceName);

            DateTime? latest = null;
            foreach (var info in new[] { oem, misc })
            {
                if (info?.LastSyncUtc is { } synced && (latest is null || synced > latest))
                {
                    latest = synced;
                }
            }

            if (latest is { } latestUtc)
            {
                var local = latestUtc.ToLocalTime();
                DatabaseVersion = local.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture);
                LastSync = local.ToString("MMM d, yyyy h:mm tt", CultureInfo.CurrentCulture);
            }
            else
            {
                DatabaseVersion = "Bundled";
                LastSync = "Never";
            }
        }
        catch
        {
            DatabaseVersion = "Unknown";
            LastSync = "Unavailable";
        }
    }
}
