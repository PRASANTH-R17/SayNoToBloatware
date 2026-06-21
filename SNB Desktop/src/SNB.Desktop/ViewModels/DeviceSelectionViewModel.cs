using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SNB.Desktop.Models;
using SNB.Desktop.Services;

namespace SNB.Desktop.ViewModels;

/// <summary>
/// Page 1 - device selection. This view model drives the initial device landing page and keeps
/// navigation to the existing shell routes lightweight so the first screen can mirror the design
/// reference while still using the app's established navigation flow.
/// </summary>
public partial class DeviceSelectionViewModel : ObservableObject
{
    private readonly IMockDataService _data;
    private readonly MainWindowViewModel _shell;

    public DeviceSelectionViewModel(IMockDataService data, MainWindowViewModel shell)
    {
        _data = data;
        _shell = shell;
        LoadDevices();
    }

    public ObservableCollection<DeviceCardModel> Devices { get; } = new();

    [RelayCommand]
    private void RefreshDevices() => LoadDevices();

    [RelayCommand]
    private void SelectDevice(DeviceCardModel? device)
    {
        if (device is not null)
        {
            _shell.ConnectDevice(device);
        }
    }

    [RelayCommand]
    private void NavigateSettings() => _shell.NavigateSettingsCommand.Execute(null);

    [RelayCommand]
    private void NavigateAbout() => _shell.NavigateAboutCommand.Execute(null);

    private void LoadDevices()
    {
        Devices.Clear();
        foreach (var device in _data.GetDevices())
        {
            Devices.Add(device);
        }
    }
}
