using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SNB.Desktop.Models;
using SNB.Desktop.Services;

namespace SNB.Desktop.ViewModels;

/// <summary>
/// Removal complete dialog. Summarises removed/failed counts, space freed, and an expandable app list.
/// </summary>
public partial class RemovalCompleteViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    public RemovalCompleteViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    public ObservableCollection<RemovalResultModel> Results { get; } = new();

    [ObservableProperty]
    private int _removedCount;

    [ObservableProperty]
    private int _failedCount;

    [ObservableProperty]
    private string _spaceFreed = "0 MB";

    [ObservableProperty]
    private bool _isDetailsExpanded = true;

    public string DetailsToggleLabel => IsDetailsExpanded
        ? "Hide Details"
        : "View Details";

    /// <summary>Supplies the final per-app results to summarise.</summary>
    public void Initialize(IEnumerable<RemovalResultModel> results)
    {
        Results.Clear();
        var removed = 0;
        var failed = 0;
        var freedMb = 0.0;

        foreach (var result in results)
        {
            Results.Add(result);
            if (result.Status == RemovalStatus.Removed)
            {
                removed++;
                freedMb += GetSizeMb(result.App);
            }
            else if (result.Status == RemovalStatus.Failed)
            {
                failed++;
            }
        }

        RemovedCount = removed;
        FailedCount = failed;
        SpaceFreed = FormatStorage(freedMb);
        IsDetailsExpanded = true;
    }

    partial void OnIsDetailsExpandedChanged(bool value) =>
        OnPropertyChanged(nameof(DetailsToggleLabel));

    [RelayCommand]
    private void ToggleDetails() => IsDetailsExpanded = !IsDetailsExpanded;

    [RelayCommand]
    private void Close() => _navigation.CloseDialog();

    private static double GetSizeMb(ApplicationItemModel app) =>
        app.SizeBytes > 0
            ? app.SizeBytes / (1024.0 * 1024.0)
            : TryParseSizeMb(app.Size);

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

    private static string FormatStorage(double megabytes)
    {
        if (megabytes >= 1024)
        {
            return $"{megabytes / 1024:0.##} GB";
        }

        return megabytes >= 100
            ? $"{megabytes:0} MB"
            : $"{megabytes:0.#} MB";
    }
}
