namespace SNB.Desktop.Models;

/// <summary>
/// Classification used to color-code applications and drive the Applications tabs.
/// </summary>
public enum AppCategory
{
    /// <summary>A normal installed app with no removal recommendation (green).</summary>
    RegularApp,

    /// <summary>Bloatware that SNB recommends removing (red).</summary>
    RecommendedRemoval,

    /// <summary>App that has known alternatives the user could switch to (amber).</summary>
    AppsWithAlternatives,
}
