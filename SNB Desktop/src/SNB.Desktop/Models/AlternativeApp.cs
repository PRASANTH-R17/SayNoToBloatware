namespace SNB.Desktop.Models;

/// <summary>
/// A suggested replacement for an app classified as <see cref="AppCategory.AppsWithAlternatives"/>.
/// </summary>
public sealed class AlternativeApp
{
    public string Name { get; init; } = string.Empty;

    public string PackageName { get; init; } = string.Empty;

    /// <summary>Short reason / description shown in the alternatives list.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Bindable icon path (avares:// or file path); may be empty -> placeholder fallback.</summary>
    public string IconPath { get; init; } = string.Empty;
}
