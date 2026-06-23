using SNB.Backend.Models;

namespace SNB.Backend.Infrastructure.Database;

public static class RemovalTypeMapper
{
    public static RemovalType FromString(string value) => value.Trim().ToLowerInvariant() switch
    {
        "delete" => RemovalType.Delete,
        "replace" => RemovalType.Replace,
        "caution" => RemovalType.Caution,
        "unsafe" => RemovalType.Unsafe,
        _ => RemovalType.Unknown
    };

    public static string ToString(RemovalType type) => type switch
    {
        RemovalType.Delete => "delete",
        RemovalType.Replace => "replace",
        RemovalType.Caution => "caution",
        RemovalType.Unsafe => "unsafe",
        _ => "unknown"
    };
}
