using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SNB.Desktop.Models;
using SNB.Desktop.Services;

namespace SNB.Desktop.ViewModels;

/// <summary>
/// Removal confirmation dialog. Shows a warning, storage estimate, and expandable app preview
/// before chaining to <see cref="RemovalProgressViewModel"/>.
/// </summary>
public partial class RemovalConfirmationViewModel : ObservableObject
{
    private const int MaxPreviewApps = 5;

    private readonly INavigationService _navigation;
    private readonly IServiceProvider _services;

    public RemovalConfirmationViewModel(INavigationService navigation, IServiceProvider services)
    {
        _navigation = navigation;
        _services = services;
    }

    public ObservableCollection<ApplicationItemModel> AppsToRemove { get; } = new();

    public ObservableCollection<ApplicationItemModel> PreviewApps { get; } = new();

    [ObservableProperty]
    private bool _isAppListExpanded;

    public int Count => AppsToRemove.Count;

    public string ConfirmationTitle => Count == 1
        ? "Remove 1 Application?"
        : $"Remove {Count} Applications?";

    public string ToggleLabel => IsAppListExpanded
        ? "Hide apps to be removed"
        : "View apps to be removed";

    public int RemainingAppsCount => Math.Max(0, Count - MaxPreviewApps);

    public bool HasMoreApps => RemainingAppsCount > 0;

    public string MoreAppsText => $"+ {RemainingAppsCount} more applications";

    public string StorageFreedText => FormatStorageMb(CalculateStorageMb(AppsToRemove));

    /// <summary>Supplies the set of applications to be removed.</summary>
    public void Initialize(IEnumerable<ApplicationItemModel> apps)
    {
        AppsToRemove.Clear();
        foreach (var app in apps)
        {
            AppsToRemove.Add(app);
        }

        RefreshPreviewApps();
        IsAppListExpanded = false;

        NotifySummaryChanged();
    }

    partial void OnIsAppListExpandedChanged(bool value)
    {
        OnPropertyChanged(nameof(ToggleLabel));
    }

    [RelayCommand]
    private void Cancel() => _navigation.CloseDialog();

    [RelayCommand]
    private void ToggleAppList() => IsAppListExpanded = !IsAppListExpanded;

    [RelayCommand]
    private void Confirm()
    {
        var vm = _services.GetRequiredService<RemovalProgressViewModel>();
        vm.Initialize(AppsToRemove);
        _navigation.ShowDialog(vm);
    }

    private void RefreshPreviewApps()
    {
        PreviewApps.Clear();
        foreach (var app in AppsToRemove.Take(MaxPreviewApps))
        {
            PreviewApps.Add(app);
        }
    }

    private void NotifySummaryChanged()
    {
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(nameof(ConfirmationTitle));
        OnPropertyChanged(nameof(RemainingAppsCount));
        OnPropertyChanged(nameof(HasMoreApps));
        OnPropertyChanged(nameof(MoreAppsText));
        OnPropertyChanged(nameof(StorageFreedText));
    }

    private static double CalculateStorageMb(IEnumerable<ApplicationItemModel> apps)
    {
        var total = 0.0;
        foreach (var app in apps)
        {
            total += app.SizeBytes > 0
                ? app.SizeBytes / (1024.0 * 1024.0)
                : TryParseSizeMb(app.Size);
        }

        return total;
    }

    private static double TryParseSizeMb(string size)
    {
        if (string.IsNullOrWhiteSpace(size))
        {
            return 0;
        }

        var parts = size.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return 0;
        }

        return parts[1].ToUpperInvariant() switch
        {
            "GB" => value * 1024,
            "KB" => value / 1024,
            _ => value,
        };
    }

    private static string FormatStorageMb(double megabytes) =>
        megabytes >= 100
            ? $"{megabytes:0} MB"
            : $"{megabytes:0.#} MB";
}
