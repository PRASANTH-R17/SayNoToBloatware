using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SNB.Desktop.ViewModels;

/// <summary>
/// Settings page — database, cache, and general preferences.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    public ObservableCollection<string> ThemeOptions { get; } = new()
    {
        "System",
        "Light",
        "Dark",
    };

    public ObservableCollection<string> LanguageOptions { get; } = new()
    {
        "English",
    };

    [ObservableProperty]
    private string _databaseVersion = "2025.05.25";

    [ObservableProperty]
    private string _lastSync = "May 25, 2025 10:24 AM";

    [ObservableProperty]
    private bool _autoUpdateDatabase = true;

    [ObservableProperty]
    private string _iconCacheLocation = @"C:\SNB\Cache\Icons";

    [ObservableProperty]
    private string _theme = "System";

    [ObservableProperty]
    private string _language = "English";

    [RelayCommand]
    private void ClearCache()
    {
        // TODO: wire to icon cache service when backend integration lands.
    }
}
