using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SNB.Desktop.Models;
using SNB.Desktop.ViewModels;

namespace SNB.Desktop.Services;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _services;
    private MainWindowViewModel? _shell;

    private object? _currentPage;
    private object? _currentDialog;
    private object? _currentPanel;

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public object? CurrentPage => _currentPage;
    public object? CurrentDialog => _currentDialog;
    public object? CurrentPanel => _currentPanel;

    public event Action? CurrentPageChanged;
    public event Action? CurrentDialogChanged;
    public event Action? CurrentPanelChanged;

    public void Initialize(MainWindowViewModel shell) => _shell = shell;

    public void NavigateTo<TViewModel>() where TViewModel : class
        => SetCurrentPage(_services.GetRequiredService<TViewModel>());

    public void NavigateTo(Type viewModelType)
        => SetCurrentPage(_services.GetRequiredService(viewModelType));

    public void NavigateToDeviceSelection()
    {
        EnsureShell();
        _shell!.IsDeviceConnected = false;
        _shell.SelectedDevice = null;
        _shell.ActiveNavItem = "Select Device";
        NavigateTo<DeviceSelectionViewModel>();
    }

    public void NavigateToApplications()
    {
        EnsureShell();
        _shell!.IsDeviceConnected = true;
        _shell.ActiveNavItem = "Applications";
        NavigateTo<ApplicationsViewModel>();
    }

    public void SelectDevice(DeviceCardModel device)
    {
        EnsureShell();
        _shell!.SelectedDevice = device;
        NavigateToApplications();
    }

    public void ChangeDevice()
    {
        ClosePanel();
        CloseDialog();
        NavigateToDeviceSelection();
    }

    public void ShowDialog(object viewModel) => SetCurrentDialog(viewModel);

    public void CloseDialog() => SetCurrentDialog(null);

    public void ShowPanel(object viewModel) => SetCurrentPanel(viewModel);

    public void ClosePanel() => SetCurrentPanel(null);

    public void StartRemovalFlow(IReadOnlyList<ApplicationItemModel> applications)
    {
        var confirmation = ActivatorUtilities.CreateInstance<RemovalConfirmationViewModel>(_services, applications);
        ShowDialog(confirmation);
    }

    private void SetCurrentPage(object? page)
    {
        if (ReferenceEquals(_currentPage, page))
        {
            return;
        }

        _currentPage = page;
        if (_shell is not null)
        {
            _shell.CurrentPage = page;
        }

        CurrentPageChanged?.Invoke();
    }

    private void SetCurrentDialog(object? dialog)
    {
        if (ReferenceEquals(_currentDialog, dialog))
        {
            return;
        }

        _currentDialog = dialog;
        if (_shell is not null)
        {
            _shell.CurrentDialog = dialog;
        }

        CurrentDialogChanged?.Invoke();
    }

    private void SetCurrentPanel(object? panel)
    {
        if (ReferenceEquals(_currentPanel, panel))
        {
            return;
        }

        _currentPanel = panel;
        if (_shell is not null)
        {
            _shell.CurrentPanel = panel;
        }

        CurrentPanelChanged?.Invoke();
    }

    private void EnsureShell()
    {
        if (_shell is null)
        {
            throw new InvalidOperationException("Navigation shell has not been initialized.");
        }
    }
}
