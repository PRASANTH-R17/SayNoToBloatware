namespace SNB.Desktop.Models;

/// <summary>
/// Filter options for the Applications tab navigation.
/// </summary>
public enum TabFilter
{
    /// <summary>Show all applications regardless of category.</summary>
    AllApps,

    /// <summary>Show only apps recommended for removal.</summary>
    RecommendedRemoval,

    /// <summary>Show only apps that have known alternatives.</summary>
    AppsWithAlternatives,
}
