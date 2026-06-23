using CommunityToolkit.Mvvm.ComponentModel;

namespace SNB.Desktop.Models;

/// <summary>
/// Tracks the removal outcome of a single <see cref="ApplicationItemModel"/> during
/// the progress / complete dialogs. Observable so the progress dialog can update
/// per-row status live as the simulated removal advances.
/// </summary>
public partial class RemovalResultModel : ObservableObject
{
    public RemovalResultModel(ApplicationItemModel app)
    {
        App = app;
    }

    public ApplicationItemModel App { get; }

    [ObservableProperty]
    private RemovalStatus _status = RemovalStatus.Pending;

    public string StatusLabel => Status switch
    {
        RemovalStatus.Removed => "Removed",
        RemovalStatus.Removing => "Removing...",
        RemovalStatus.Failed => "Failed",
        _ => "Pending",
    };

    /// <summary>Optional error / detail message shown when <see cref="Status"/> is Failed.</summary>
    [ObservableProperty]
    private string _message = string.Empty;

    partial void OnStatusChanged(RemovalStatus value) => OnPropertyChanged(nameof(StatusLabel));
}
