using System;
using System.Collections.Generic;
using SNB.Desktop.Models;
using SNB.Desktop.ViewModels;

namespace SNB.Desktop.Services;

public interface INavigationService
{
    object? CurrentPage { get; }
    object? CurrentDialog { get; }
    object? CurrentPanel { get; }

    event Action? CurrentPageChanged;
    event Action? CurrentDialogChanged;
    event Action? CurrentPanelChanged;

    void Initialize(MainWindowViewModel shell);
    void NavigateTo<TViewModel>() where TViewModel : class;
    void NavigateTo(Type viewModelType);
    void NavigateToDeviceSelection();
    void NavigateToApplications();
    void SelectDevice(DeviceCardModel device);
    void ChangeDevice();
    void ShowDialog(object viewModel);
    void CloseDialog();
    void ShowPanel(object viewModel);
    void ClosePanel();
    void StartRemovalFlow(IReadOnlyList<ApplicationItemModel> applications);
}
