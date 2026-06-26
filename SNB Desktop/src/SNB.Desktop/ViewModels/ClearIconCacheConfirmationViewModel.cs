using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SNB.Backend.Services.Abstractions;
using SNB.Desktop.Services;
using SNB.Desktop.Services.Localization;

namespace SNB.Desktop.ViewModels;

/// <summary>
/// Confirmation dialog shown before clearing the on-disk icon cache.
/// </summary>
public partial class ClearIconCacheConfirmationViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly IIconCacheService _iconCacheService;
    private readonly IToastService _toast;

    public ClearIconCacheConfirmationViewModel(
        INavigationService navigation,
        IIconCacheService iconCacheService,
        IToastService toast)
    {
        _navigation = navigation;
        _iconCacheService = iconCacheService;
        _toast = toast;
    }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [RelayCommand]
    private void Cancel() => _navigation.CloseDialog();

    [RelayCommand]
    private async Task Confirm()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            await _iconCacheService.ClearAsync();
            _navigation.CloseDialog();
            _toast.Show(
                LocalizationService.Instance["Settings.ClearIconCacheSuccess"],
                ToastKind.Success);
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Format(
                LocalizationService.Instance["Settings.ClearIconCacheFailed"],
                ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
