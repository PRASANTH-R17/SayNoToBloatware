using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SNB.Desktop.Models;
using SNB.Desktop.Services;

namespace SNB.Desktop.ViewModels;

/// <summary>
/// Right-docked App Details panel. Configured with <see cref="Initialize"/> before
/// being shown via <see cref="INavigationService.ShowPanel"/>.
/// </summary>
public partial class AppDetailsViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    public AppDetailsViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    [ObservableProperty]
    private ApplicationItemModel? _app;

    /// <summary>Supplies the application whose details should be shown.</summary>
    public void Initialize(ApplicationItemModel app) => App = app;

    [RelayCommand]
    private void Close() => _navigation.ClosePanel();
}
