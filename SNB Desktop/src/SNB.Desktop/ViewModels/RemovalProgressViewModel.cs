using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SNB.Desktop.Models;
using SNB.Desktop.Services;

namespace SNB.Desktop.ViewModels;

/// <summary>
/// Removal progress dialog. Performs real per-app removal through the backend
/// (one package at a time via the orchestrator) with live row status updates,
/// then chains to <see cref="RemovalCompleteViewModel"/> when finished.
/// </summary>
public partial class RemovalProgressViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly IServiceProvider _services;
    private readonly IDeviceCatalogService _catalog;
    private CancellationTokenSource? _cts;

    public RemovalProgressViewModel(
        INavigationService navigation,
        IServiceProvider services,
        IDeviceCatalogService catalog)
    {
        _navigation = navigation;
        _services = services;
        _catalog = catalog;
    }

    public ObservableCollection<RemovalResultModel> Results { get; } = new();

    [ObservableProperty]
    private int _completedCount;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _isRunning;

    public double ProgressPercent => TotalCount > 0
        ? CompletedCount * 100.0 / TotalCount
        : 0;

    public string ProgressSummary => $"{CompletedCount} / {TotalCount} completed";

    public string ProgressPercentLabel => $"{ProgressPercent:0}%";

    /// <summary>Seeds the per-app result rows (all Pending) and starts the real removal.</summary>
    public void Initialize(IEnumerable<ApplicationItemModel> apps)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        Results.Clear();
        foreach (var app in apps)
        {
            Results.Add(new RemovalResultModel(app));
        }

        TotalCount = Results.Count;
        CompletedCount = 0;
        IsRunning = TotalCount > 0;

        NotifyProgressChanged();

        if (TotalCount > 0)
        {
            _ = RunRemovalAsync(_cts.Token);
        }
    }

    partial void OnCompletedCountChanged(int value) => NotifyProgressChanged();

    partial void OnTotalCountChanged(int value) => NotifyProgressChanged();

    [RelayCommand]
    private void Cancel()
    {
        _cts?.Cancel();
        IsRunning = false;
        _navigation.CloseDialog();

        // If any apps were already removed/disabled before cancelling, refresh the list so it reflects them.
        if (Results.Any(r => r.Status is RemovalStatus.Removed or RemovalStatus.Disabled))
        {
            _navigation.NavigateTo<ApplicationsViewModel>();
        }
    }

    private async Task RunRemovalAsync(CancellationToken token)
    {
        try
        {
            foreach (var result in Results)
            {
                token.ThrowIfCancellationRequested();

                await Dispatcher.UIThread.InvokeAsync(() => result.Status = RemovalStatus.Removing);

                var (status, error) = await _catalog
                    .RemoveAppAsync(result.App.PackageName, token)
                    .ConfigureAwait(false);

                token.ThrowIfCancellationRequested();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    result.Status = status;
                    if (status == RemovalStatus.Failed)
                    {
                        result.Message = error ?? "Removal failed.";
                    }

                    CompletedCount++;
                });
            }

            if (!token.IsCancellationRequested)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    IsRunning = false;
                    NavigateToComplete();
                });
            }
        }
        catch (OperationCanceledException)
        {
            await Dispatcher.UIThread.InvokeAsync(() => IsRunning = false);
        }
    }

    private void NavigateToComplete()
    {
        var vm = _services.GetRequiredService<RemovalCompleteViewModel>();
        vm.Initialize(Results);
        _navigation.ShowDialog(vm);
    }

    private void NotifyProgressChanged()
    {
        OnPropertyChanged(nameof(ProgressPercent));
        OnPropertyChanged(nameof(ProgressSummary));
        OnPropertyChanged(nameof(ProgressPercentLabel));
    }
}
