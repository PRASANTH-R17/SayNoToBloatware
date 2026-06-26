using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SNB.Desktop.Models;
using SNB.Desktop.Services;
using SNB.Desktop.Services.Localization;

namespace SNB.Desktop.ViewModels;

/// <summary>
/// Page 1 - device selection. Runs the one-time backend bootstrap, detects connected devices
/// via the orchestrator, and prepares the chosen device (deploy bridge, scan apps) before
/// navigating to the Applications page.
/// </summary>
public partial class DeviceSelectionViewModel : ObservableObject
{
    private readonly IDeviceCatalogService _catalog;
    private readonly MainWindowViewModel _shell;

    public DeviceSelectionViewModel(IDeviceCatalogService catalog, MainWindowViewModel shell)
    {
        _catalog = catalog;
        _shell = shell;
        _ = LoadDevicesAsync();
    }

    public ObservableCollection<DeviceCardModel> Devices { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool HasNoDevices => !IsBusy && Devices.Count == 0 && !HasError;

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(HasNoDevices));
    }

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(HasNoDevices));

    [RelayCommand]
    private Task RefreshDevices() => LoadDevicesAsync();

    [RelayCommand]
    private async Task SelectDevice(DeviceCardModel? device)
    {
        if (device is null || IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        IsBusy = true;
        var progress = new Progress<string>(message => StatusMessage = LocalizationService.Instance.Dynamic(message));

        try
        {
            StatusMessage = LocalizationService.Instance["Status.PreparingDevice"];
            await _catalog.PrepareDeviceAsync(device.Serial, progress);
            _shell.ConnectDevice(device);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to prepare {device.Name}: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NavigateSettings() => _shell.NavigateSettingsCommand.Execute(null);

    [RelayCommand]
    private void NavigateAbout() => _shell.NavigateAboutCommand.Execute(null);

    private async Task LoadDevicesAsync()
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        IsBusy = true;
        var progress = new Progress<string>(message => StatusMessage = LocalizationService.Instance.Dynamic(message));

        try
        {
            await _catalog.EnsureInitializedAsync(progress);

            StatusMessage = LocalizationService.Instance["Status.DetectingDevices"];
            var devices = await _catalog.GetDevicesAsync();

            Devices.Clear();
            foreach (var device in devices)
            {
                Devices.Add(device);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not detect devices: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            StatusMessage = string.Empty;
            OnPropertyChanged(nameof(HasNoDevices));
        }
    }
}
