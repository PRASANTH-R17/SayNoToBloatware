namespace SNB.Desktop.Models;

/// <summary>
/// Sort order for the Applications list.
/// </summary>
public enum AppSortOption
{
    Name,
    PackageName,
    Size,
    Category,
}

public static class AppSortOptionExtensions
{
    public static string GetDisplayName(this AppSortOption option) => option switch
    {
        AppSortOption.PackageName => "Package name",
        AppSortOption.Size => "Size",
        AppSortOption.Category => "Category",
        _ => "Name",
    };
}
