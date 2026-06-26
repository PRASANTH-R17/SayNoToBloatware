using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SNB.Desktop.Models;
using SNB.Desktop.Services;

namespace SNB.Desktop.ViewModels;

/// <summary>
/// Export dialog. Lets the user choose a format (CSV/JSON) and saves the supplied applications
/// list to disk through a native Save As picker.
/// </summary>
public partial class ExportViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly IFileSaveService _fileSave;
    private readonly List<ApplicationItemModel> _apps = new();

    public ExportViewModel(INavigationService navigation, IFileSaveService fileSave)
    {
        _navigation = navigation;
        _fileSave = fileSave;
    }

    [ObservableProperty]
    private ExportFormat _selectedFormat = ExportFormat.Csv;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public bool IsCsv
    {
        get => SelectedFormat == ExportFormat.Csv;
        set
        {
            if (value)
            {
                SelectedFormat = ExportFormat.Csv;
            }
        }
    }

    public bool IsJson
    {
        get => SelectedFormat == ExportFormat.Json;
        set
        {
            if (value)
            {
                SelectedFormat = ExportFormat.Json;
            }
        }
    }

    public int AppCount => _apps.Count;

    /// <summary>Supplies the applications to export (the currently shown list).</summary>
    public void Initialize(IEnumerable<ApplicationItemModel> apps)
    {
        _apps.Clear();
        _apps.AddRange(apps);
        OnPropertyChanged(nameof(AppCount));
    }

    partial void OnSelectedFormatChanged(ExportFormat value)
    {
        OnPropertyChanged(nameof(IsCsv));
        OnPropertyChanged(nameof(IsJson));
    }

    [RelayCommand]
    private void Cancel() => _navigation.CloseDialog();

    [RelayCommand]
    private async Task Export()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        StatusMessage = string.Empty;

        try
        {
            var content = SelectedFormat == ExportFormat.Json
                ? AppListExporter.ToJson(_apps)
                : AppListExporter.ToCsv(_apps);

            var extension = SelectedFormat.GetFileExtension();
            var savedPath = await _fileSave.SaveTextAsync(
                content,
                $"snb-app-list.{extension}",
                $"{SelectedFormat.GetDisplayName()} file",
                extension);

            if (savedPath is not null)
            {
                _navigation.CloseDialog();
            }
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
