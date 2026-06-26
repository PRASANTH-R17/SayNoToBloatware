using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace SNB.Desktop;

/// <summary>
/// Resolves a view (a <see cref="Control"/>) for a given view model instance by naming convention.
/// Registered in <c>App.axaml</c> under <c>Application.DataTemplates</c> so any
/// <see cref="ContentControl"/> bound to a view model object renders the matching view.
///
/// NAMING CONTRACT (later agents must follow this so views resolve automatically):
///   * Strip the trailing "ViewModel" from the runtime type name to get {Name}.
///   * Page views live in the <c>SNB.Desktop.Views</c> namespace and are named <c>{Name}View</c>.
///         e.g. ApplicationsViewModel       -> SNB.Desktop.Views.ApplicationsView
///              DeviceSelectionViewModel    -> SNB.Desktop.Views.DeviceSelectionView
///   * Dialog / panel views live in the <c>SNB.Desktop.Dialogs</c> namespace. Because a single
///     suffix is not consistent across dialogs, the locator probes several conventions in order:
///         SNB.Desktop.Dialogs.{Name}            (e.g. AppDetails        -> AppDetails — unused)
///         SNB.Desktop.Dialogs.{Name}Dialog      (e.g. RemovalConfirmationViewModel -> RemovalConfirmationDialog)
///         SNB.Desktop.Dialogs.{Name}Panel       (e.g. AppDetailsViewModel          -> AppDetailsPanel)
///         SNB.Desktop.Dialogs.{Name}View
///
/// Concrete examples honored by the shell:
///   RemovalConfirmationViewModel -> SNB.Desktop.Dialogs.RemovalConfirmationDialog
///   RemovalProgressViewModel     -> SNB.Desktop.Dialogs.RemovalProgressDialog
///   RemovalCompleteViewModel     -> SNB.Desktop.Dialogs.RemovalCompleteDialog
///   AppDetailsViewModel          -> SNB.Desktop.Dialogs.AppDetailsPanel
/// </summary>
public sealed class ViewLocator : IDataTemplate
{
    private const string ViewModelSuffix = "ViewModel";
    private const string ViewsNamespace = "SNB.Desktop.Views";
    private const string DialogsNamespace = "SNB.Desktop.Dialogs";

    public Control Build(object? data)
    {
        if (data is null)
        {
            return new TextBlock { Text = "No view model" };
        }

        var vmType = data.GetType();
        var vmName = vmType.Name;
        var baseName = vmName.EndsWith(ViewModelSuffix, StringComparison.Ordinal)
            ? vmName[..^ViewModelSuffix.Length]
            : vmName;

        var assembly = vmType.Assembly;

        // Probe each candidate fully-qualified type name in priority order.
        foreach (var candidate in CandidateTypeNames(baseName))
        {
            var type = assembly.GetType(candidate);
            if (type is not null && typeof(Control).IsAssignableFrom(type))
            {
                return (Control)Activator.CreateInstance(type)!;
            }
        }

        return new TextBlock { Text = $"View not found for: {vmName}" };
    }

    public bool Match(object? data) => data is CommunityToolkit.Mvvm.ComponentModel.ObservableObject;

    private static System.Collections.Generic.IEnumerable<string> CandidateTypeNames(string baseName)
    {
        yield return $"{ViewsNamespace}.{baseName}View";
        yield return $"{DialogsNamespace}.{baseName}Panel";
        yield return $"{DialogsNamespace}.{baseName}Dialog";
        yield return $"{DialogsNamespace}.{baseName}View";
        yield return $"{DialogsNamespace}.{baseName}";
    }
}
