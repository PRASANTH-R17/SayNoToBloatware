using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SNB.Desktop.Models;
using SNB.Desktop.Services;

namespace SNB.Desktop.ViewModels;

/// <summary>
/// Shell view model. Owns the sidebar state and mirrors the navigation service's current
/// page / dialog / panel into bindable observable properties consumed by <c>MainWindow.axaml</c>.
/// The initial navigation is performed in <see cref="Initialize"/> (called by <c>App</c> after the
/// singleton is constructed) to avoid a DI construction cycle with page view models.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    /// <summary>Footer version label shown in the sidebar.</summary>
    public const string AppVersion = "SNB Desktop v1.0.0";

    private readonly INavigationService _navigation;
    private readonly IDeviceCatalogService _catalog;

    public MainWindowViewModel(INavigationService navigation, IDeviceCatalogService catalog)
    {
        _navigation = navigation;
        _catalog = catalog;

        _navigation.CurrentPageChanged += () => CurrentPage = _navigation.CurrentPage;
        _navigation.CurrentDialogChanged += () =>
        {
            CurrentDialog = _navigation.CurrentDialog;
            OnPropertyChanged(nameof(IsDialogOpen));
        };
        _navigation.CurrentPanelChanged += () =>
        {
            CurrentPanel = _navigation.CurrentPanel;
            OnPropertyChanged(nameof(IsPanelOpen));
        };
    }

    /// <summary>View model rendered in the main content host.</summary>
    [ObservableProperty]
    private object? _currentPage;

    /// <summary>Centered modal dialog content (null = none).</summary>
    [ObservableProperty]
    private object? _currentDialog;

    /// <summary>Right-docked slide-in panel content (null = none).</summary>
    [ObservableProperty]
    private object? _currentPanel;

    /// <summary>Drives the sidebar variant: false = Select Device view, true = Applications view.</summary>
    [ObservableProperty]
    private bool _isDeviceConnected;

    /// <summary>The device chosen on the selection page; shown in the sidebar summary once connected.</summary>
    [ObservableProperty]
    private DeviceCardModel? _selectedDevice;

    /// <summary>Label of the currently highlighted sidebar nav item.</summary>
    [ObservableProperty]
    private string _activeNavItem = "Select Device";

    public bool IsDialogOpen => CurrentDialog is not null;

    public bool IsPanelOpen => CurrentPanel is not null;

    /// <summary>True when any overlay (dialog or panel) is showing, so the dim layer can hit-test.</summary>
    public bool IsOverlayVisible => IsDialogOpen || IsPanelOpen;

    partial void OnCurrentDialogChanged(object? value) => OnPropertyChanged(nameof(IsOverlayVisible));

    partial void OnCurrentPanelChanged(object? value) => OnPropertyChanged(nameof(IsOverlayVisible));

    /// <summary>
    /// Performs the initial navigation to the device selection page. Called by <c>App</c> once this
    /// singleton is fully constructed so page view models that depend on the shell resolve cleanly.
    /// </summary>
    public void Initialize()
    {
        ActiveNavItem = "Select Device";
        _navigation.NavigateTo<DeviceSelectionViewModel>();
        CurrentPage = _navigation.CurrentPage;
    }

    /// <summary>
    /// Called when a device is selected on the device selection page. Flips the sidebar to the
    /// post-device state and navigates to the Applications page.
    /// </summary>
    public void ConnectDevice(DeviceCardModel device)
    {
        SelectedDevice = device;
        IsDeviceConnected = true;
        ActiveNavItem = "Applications";
        _navigation.NavigateTo<ApplicationsViewModel>();
    }

    /// <summary>Returns to the device selection page and resets the sidebar to its pre-device state.</summary>
    [RelayCommand]
    private void ChangeDevice()
    {
        // Best effort: stop the on-device bridge for the previous session.
        _ = _catalog.StopAsync();

        SelectedDevice = null;
        IsDeviceConnected = false;
        ActiveNavItem = "Select Device";
        _navigation.CloseDialog();
        _navigation.ClosePanel();
        _navigation.NavigateTo<DeviceSelectionViewModel>();
    }

    /// <summary>Sidebar primary item: Applications when a device is connected, otherwise Select Device.</summary>
    [RelayCommand]
    private void NavigateHome()
    {
        if (IsDeviceConnected)
        {
            ActiveNavItem = "Applications";
            _navigation.NavigateTo<ApplicationsViewModel>();
        }
        else
        {
            ActiveNavItem = "Select Device";
            _navigation.NavigateTo<DeviceSelectionViewModel>();
        }
    }

    [RelayCommand]
    private void NavigateSettings()
    {
        ActiveNavItem = "Settings";
        _navigation.NavigateTo<SettingsViewModel>();
    }

    [RelayCommand]
    private void NavigateAbout()
    {
        ActiveNavItem = "About";
        _navigation.NavigateTo<AboutViewModel>();
    }
}
