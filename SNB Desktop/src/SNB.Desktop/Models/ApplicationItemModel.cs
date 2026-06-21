using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SNB.Desktop.Models;

/// <summary>
/// A single application row shown in the Applications grid and details panel.
/// Observable because <see cref="IsSelected"/> is toggled by row checkboxes and
/// drives the "Remove Selected (N)" counter.
/// </summary>
public partial class ApplicationItemModel : ObservableObject
{
    /// <summary>Bindable icon path (avares:// or file path); empty -> placeholder fallback.</summary>
    public string IconPath { get; init; } = string.Empty;

    public string AppName { get; init; } = string.Empty;

    public string PackageName { get; init; } = string.Empty;

    public AppCategory Category { get; init; } = AppCategory.RegularApp;

    [ObservableProperty]
    private bool _isSelected;

    public string Description { get; init; } = string.Empty;

    /// <summary>Where the bloatware classification came from, e.g. "oem.json".</summary>
    public string Source { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;

    public string Size { get; init; } = string.Empty;

    /// <summary>True for system apps (cannot fully uninstall, only disable/remove for user).</summary>
    public bool IsSystemApp { get; init; }

    public string FirstFound { get; init; } = string.Empty;

    public IReadOnlyList<string> Permissions { get; init; } = new List<string>();

    public IReadOnlyList<AlternativeApp> Alternatives { get; init; } = new List<AlternativeApp>();

    /// <summary>Amber safety note about the impact of removing this app.</summary>
    public string Warning { get; init; } = string.Empty;

    public string TypeLabel => IsSystemApp ? "System App" : "User App";

    public string CategoryDisplayName => Category switch
    {
        AppCategory.RecommendedRemoval => "Recommended Removal",
        AppCategory.AppsWithAlternatives => "Has Alternatives",
        _ => "Installed",
    };

    public string Subtitle => TypeLabel;

    public int PermissionCount => Permissions.Count;

    /// <summary>Parsed size in bytes for removal space estimates.</summary>
    public long SizeBytes { get; init; }
}
