using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using SNB.Desktop.ViewModels;

namespace SNB.Desktop.Services;

public sealed class ToastService : IToastService
{
    private const int DismissDelayMs = 4000;

    private MainWindowViewModel? _shell;
    private CancellationTokenSource? _dismissCts;

    public void Initialize(MainWindowViewModel shell) => _shell = shell;

    public void Show(string message, ToastKind kind = ToastKind.Success)
    {
        if (_shell is null)
        {
            return;
        }

        _dismissCts?.Cancel();
        _dismissCts = new CancellationTokenSource();
        var token = _dismissCts.Token;

        Dispatcher.UIThread.Post(() =>
        {
            _shell.ToastMessage = message;
            _shell.ToastKind = kind;
            _shell.IsToastVisible = true;
        });

        _ = DismissAfterDelayAsync(token);
    }

    private async Task DismissAfterDelayAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(DismissDelayMs, cancellationToken);
            if (cancellationToken.IsCancellationRequested || _shell is null)
            {
                return;
            }

            Dispatcher.UIThread.Post(() => _shell.IsToastVisible = false);
        }
        catch (OperationCanceledException)
        {
            // A newer toast replaced this one.
        }
    }
}
