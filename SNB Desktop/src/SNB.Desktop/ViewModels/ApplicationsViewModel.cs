using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SNB.Desktop.Models;
using SNB.Desktop.Services;
using SNB.Desktop.Services.Localization;

namespace SNB.Desktop.ViewModels;

/// <summary>
/// Page 2 - applications list with tabs, search filtering, and selection tracking.
///
/// Overlay contract (so later agents never touch DI): child overlay view models
/// (<see cref="AppDetailsViewModel"/>, <see cref="RemovalConfirmationViewModel"/>) are resolved
/// from the <see cref="IServiceProvider"/>, configured via their <c>Initialize</c> methods, and
/// shown through <see cref="INavigationService.ShowPanel"/> / <see cref="INavigationService.ShowDialog"/>.
/// </summary>
public partial class ApplicationsViewModel : ObservableObject
{
    private readonly IDeviceCatalogService _catalog;
    private readonly INavigationService _navigation;
    private readonly IServiceProvider _services;
    private readonly MainWindowViewModel _shell;
    private readonly ObservableCollection<ApplicationItemModel> _allApplications = new();

    public ApplicationsViewModel(
        IDeviceCatalogService catalog,
        INavigationService navigation,
        IServiceProvider services,
        MainWindowViewModel shell)
    {
        _catalog = catalog;
        _navigation = navigation;
        _services = services;
        _shell = shell;

        // Refresh the localized "Remove Selected (N)" label when the language changes.
        LocalizationService.Instance.PropertyChanged += (_, _) => OnPropertyChanged(nameof(RemoveButtonText));

        LoadApplications();
    }

    [ObservableProperty]
    private TabFilter _selectedTab = TabFilter.AllApps;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private AppSortOption _selectedSort = AppSortOption.Name;

    [ObservableProperty]
    private SortDirection _sortDirection = SortDirection.Ascending;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public IReadOnlyList<AppSortOption> SortOptions { get; } =
        Enum.GetValues<AppSortOption>().Cast<AppSortOption>().ToArray();

    public ObservableCollection<ApplicationItemModel> Applications { get; } = new();

    [ObservableProperty]
    private AppStatistics _statistics = new(0, 0, 0);

    /// <summary>Currently selected device, surfaced from the shell for the page header.</summary>
    public DeviceCardModel? SelectedDevice => _shell.SelectedDevice;

    public int SelectedCount => _allApplications.Count(a => a.IsSelected);

    public bool HasSelectedItems => SelectedCount > 0;

    public string RemoveButtonText =>
        string.Format(LocalizationService.Instance["Apps.RemoveSelected"], SelectedCount);

    /// <summary>
    /// Drives the footer "Select All" checkbox. Reflects/sets selection across the currently
    /// visible (filtered/searched) apps only, matching the "{N} apps found" count next to it.
    /// </summary>
    public bool IsAllSelected
    {
        get => Applications.Count > 0 && Applications.All(a => a.IsSelected);
        set
        {
            foreach (var app in Applications)
            {
                app.IsSelected = value;
            }

            OnPropertyChanged();
        }
    }

    partial void OnSelectedTabChanged(TabFilter value)
    {
        ApplyFilters();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedSortChanged(AppSortOption value)
    {
        ApplyFilters();
    }

    public bool IsSortDescending => SortDirection == SortDirection.Descending;

    partial void OnSortDirectionChanged(SortDirection value)
    {
        OnPropertyChanged(nameof(IsSortDescending));
        ApplyFilters();
    }

    [RelayCommand]
    private void ToggleSortDirection()
        => SortDirection = SortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;

    private void OnApplicationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ApplicationItemModel.IsSelected))
        {
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(HasSelectedItems));
            OnPropertyChanged(nameof(RemoveButtonText));
            OnPropertyChanged(nameof(IsAllSelected));
        }
    }

    private void ApplyFilters()
    {
        Applications.Clear();

        var filtered = _allApplications.AsEnumerable();

        // Filter by tab
        filtered = SelectedTab switch
        {
            TabFilter.RecommendedRemoval => filtered.Where(a => a.Category == AppCategory.RecommendedRemoval),
            TabFilter.AppsWithAlternatives => filtered.Where(a => a.Category == AppCategory.AppsWithAlternatives),
            _ => filtered
        };

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            filtered = filtered.Where(a =>
                a.AppName.ToLowerInvariant().Contains(searchLower) ||
                a.PackageName.ToLowerInvariant().Contains(searchLower));
        }

        var desc = SortDirection == SortDirection.Descending;
        filtered = SelectedSort switch
        {
            AppSortOption.PackageName => desc
                ? filtered.OrderByDescending(a => a.PackageName, StringComparer.OrdinalIgnoreCase)
                : filtered.OrderBy(a => a.PackageName, StringComparer.OrdinalIgnoreCase),
            AppSortOption.Size => desc
                ? filtered.OrderByDescending(a => a.SizeBytes)
                : filtered.OrderBy(a => a.SizeBytes),
            AppSortOption.Type => desc
                ? filtered.OrderByDescending(a => a.IsSystemApp).ThenBy(a => a.AppName, StringComparer.OrdinalIgnoreCase)
                : filtered.OrderBy(a => a.IsSystemApp).ThenBy(a => a.AppName, StringComparer.OrdinalIgnoreCase),
            _ => desc
                ? filtered.OrderByDescending(a => a.AppName, StringComparer.OrdinalIgnoreCase)
                : filtered.OrderBy(a => a.AppName, StringComparer.OrdinalIgnoreCase),
        };

        foreach (var app in filtered)
        {
            Applications.Add(app);
        }

        OnPropertyChanged(nameof(IsAllSelected));
    }

    [RelayCommand]
    private void Refresh()
    {
        ApplyFilters();
    }

    [RelayCommand]
    private void Export()
    {
        if (Applications.Count == 0)
        {
            return;
        }

        var vm = _services.GetRequiredService<ExportViewModel>();
        vm.Initialize(Applications.ToList());
        _navigation.ShowDialog(vm);
    }

    [RelayCommand]
    private async Task ScanAgain()
    {
        var serial = _catalog.CurrentSerial;
        if (IsBusy || string.IsNullOrWhiteSpace(serial))
        {
            return;
        }

        IsBusy = true;
        var progress = new Progress<string>(message => StatusMessage = LocalizationService.Instance.Dynamic(message));

        try
        {
            await _catalog.PrepareDeviceAsync(serial, progress);
            LoadApplications();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Scan failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadApplications()
    {
        foreach (var app in _allApplications)
        {
            app.PropertyChanged -= OnApplicationPropertyChanged;
        }

        _allApplications.Clear();

        foreach (var app in _catalog.GetApplications())
        {
            app.PropertyChanged += OnApplicationPropertyChanged;
            _allApplications.Add(app);
        }

        Statistics = _catalog.GetStatistics();
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelectedItems));
        OnPropertyChanged(nameof(RemoveButtonText));
        OnPropertyChanged(nameof(IsAllSelected));
        ApplyFilters();
    }

    [RelayCommand]
    private void ChangeTab(TabFilter tab)
    {
        SelectedTab = tab;
    }

    [RelayCommand]
    private void OpenDetails(ApplicationItemModel? app)
    {
        if (app is null)
        {
            return;
        }

        var vm = _services.GetRequiredService<AppDetailsViewModel>();
        vm.Initialize(app);
        _navigation.ShowPanel(vm);
    }

    [RelayCommand]
    private void RemoveSelected()
    {
        var selected = _allApplications.Where(a => a.IsSelected).ToList();
        if (selected.Count == 0)
        {
            return;
        }

        var vm = _services.GetRequiredService<RemovalConfirmationViewModel>();
        vm.Initialize(selected);
        _navigation.ShowDialog(vm);
    }
}
