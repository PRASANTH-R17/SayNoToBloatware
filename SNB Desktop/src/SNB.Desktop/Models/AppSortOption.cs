namespace SNB.Desktop.Models;

/// <summary>
/// Sort order for the Applications list.
/// </summary>
public enum AppSortOption
{
    Name,
    PackageName,
    Size,
    Type,
}

/// <summary>
/// Sort direction applied to the currently selected <see cref="AppSortOption"/>.
/// </summary>
public enum SortDirection
{
    Ascending,
    Descending,
}

public static class AppSortOptionExtensions
{
    public static string GetDisplayName(this AppSortOption option) => option switch
    {
        AppSortOption.PackageName => "Package name",
        AppSortOption.Size => "Size",
        AppSortOption.Type => "App type",
        _ => "Name",
    };
}
