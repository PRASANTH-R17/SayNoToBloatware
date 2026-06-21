using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
/// Removal progress dialog. Simulates per-app removal with live row status updates,
/// then chains to <see cref="RemovalCompleteViewModel"/> when finished.
/// </summary>
public partial class RemovalProgressViewModel : ObservableObject
{
    private static readonly TimeSpan StepDelay = TimeSpan.FromMilliseconds(1400);

    private readonly INavigationService _navigation;
    private readonly IServiceProvider _services;
    private CancellationTokenSource? _simulationCts;

    public RemovalProgressViewModel(INavigationService navigation, IServiceProvider services)
    {
        _navigation = navigation;
        _services = services;
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

    /// <summary>Seeds the per-app result rows (all Pending) and starts simulated removal.</summary>
    public void Initialize(IEnumerable<ApplicationItemModel> apps)
    {
        _simulationCts?.Cancel();
        _simulationCts?.Dispose();
        _simulationCts = new CancellationTokenSource();

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
            _ = RunSimulationAsync(_simulationCts.Token);
        }
    }

    partial void OnCompletedCountChanged(int value) => NotifyProgressChanged();

    partial void OnTotalCountChanged(int value) => NotifyProgressChanged();

    [RelayCommand]
    private void Cancel()
    {
        _simulationCts?.Cancel();
        IsRunning = false;
        _navigation.CloseDialog();
    }

    private async Task RunSimulationAsync(CancellationToken token)
    {
        try
        {
            foreach (var result in Results)
            {
                token.ThrowIfCancellationRequested();

                await Dispatcher.UIThread.InvokeAsync(() => result.Status = RemovalStatus.Removing);

                await Task.Delay(StepDelay, token).ConfigureAwait(false);

                token.ThrowIfCancellationRequested();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    result.Status = RemovalStatus.Removed;
                    CompletedCount++;
                });
            }

            await Task.Delay(500, token).ConfigureAwait(false);

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
