using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SNB.Desktop.Models;
using SNB.Desktop.Services;

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
    private readonly IMockDataService _data;
    private readonly INavigationService _navigation;
    private readonly IServiceProvider _services;
    private readonly MainWindowViewModel _shell;
    private readonly ObservableCollection<ApplicationItemModel> _allApplications = new();

    public ApplicationsViewModel(
        IMockDataService data,
        INavigationService navigation,
        IServiceProvider services,
        MainWindowViewModel shell)
    {
        _data = data;
        _navigation = navigation;
        _services = services;
        _shell = shell;

        foreach (var app in _data.GetApplications())
        {
            _allApplications.Add(app);
            app.PropertyChanged += OnApplicationPropertyChanged;
        }

        Statistics = _data.GetStatistics();
        ApplyFilters();
    }

    [ObservableProperty]
    private TabFilter _selectedTab = TabFilter.AllApps;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private AppSortOption _selectedSort = AppSortOption.Name;

    public IReadOnlyList<AppSortOption> SortOptions { get; } =
        Enum.GetValues<AppSortOption>().Cast<AppSortOption>().ToArray();

    public ObservableCollection<ApplicationItemModel> Applications { get; } = new();

    public AppStatistics Statistics { get; }

    /// <summary>Currently selected device, surfaced from the shell for the page header.</summary>
    public DeviceCardModel? SelectedDevice => _shell.SelectedDevice;

    public int SelectedCount => _allApplications.Count(a => a.IsSelected);

    public bool HasSelectedItems => SelectedCount > 0;

    public string RemoveButtonText => HasSelectedItems
        ? $"Remove Selected ({SelectedCount})"
        : "Remove Selected (0)";

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

    private void OnApplicationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ApplicationItemModel.IsSelected))
        {
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(HasSelectedItems));
            OnPropertyChanged(nameof(RemoveButtonText));
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

        filtered = SelectedSort switch
        {
            AppSortOption.PackageName => filtered.OrderBy(a => a.PackageName, StringComparer.OrdinalIgnoreCase),
            AppSortOption.Size => filtered.OrderByDescending(a => a.SizeBytes).ThenBy(a => a.AppName, StringComparer.OrdinalIgnoreCase),
            AppSortOption.Category => filtered.OrderBy(a => a.Category).ThenBy(a => a.AppName, StringComparer.OrdinalIgnoreCase),
            _ => filtered.OrderBy(a => a.AppName, StringComparer.OrdinalIgnoreCase),
        };

        foreach (var app in filtered)
        {
            Applications.Add(app);
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        ApplyFilters();
    }

    [RelayCommand]
    private void Export()
    {
        // TODO: wire to export service when backend integration lands.
    }

    [RelayCommand]
    private void ScanAgain()
    {
        foreach (var app in _allApplications)
        {
            app.PropertyChanged -= OnApplicationPropertyChanged;
        }

        _allApplications.Clear();

        foreach (var app in _data.GetApplications())
        {
            app.PropertyChanged += OnApplicationPropertyChanged;
            _allApplications.Add(app);
        }

        ApplyFilters();
    }

    [RelayCommand]
    private void ChangeTab(TabFilter tab)
    {
        SelectedTab = tab;
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var app in _allApplications)
        {
            app.IsSelected = false;
        }
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
