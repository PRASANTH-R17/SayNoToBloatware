namespace SNB.Backend.Infrastructure.Adb;

public static class AdbOutputParser
{
    public static IReadOnlyList<(string Serial, string State)> ParseDevices(string output)
    {
        var results = new List<(string Serial, string State)>();
        var lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if (line.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length >= 2)
            {
                results.Add((parts[0], parts[1]));
            }
        }

        return results;
    }

    public static IReadOnlyList<string> ParsePackageList(string output)
    {
        var packages = new List<string>();
        var lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            const string prefix = "package:";
            if (line.StartsWith(prefix, StringComparison.Ordinal))
            {
                packages.Add(line[prefix.Length..].Trim());
            }
        }

        return packages;
    }

    public static bool IsUninstallSuccess(string output)
    {
        return output.Contains("Success", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// True when an uninstall failed because the OS/OEM forbids removing the package for the user
    /// (e.g. Vivo's protected core apps), which is the signal to fall back to disabling instead.
    /// </summary>
    public static bool IsUninstallRestricted(string output)
    {
        return output.Contains("DELETE_FAILED_USER_RESTRICTED", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>True when <c>pm disable-user</c> reported the package is now disabled.</summary>
    public static bool IsDisableSuccess(string output)
    {
        return output.Contains("new state: disabled", StringComparison.OrdinalIgnoreCase);
    }

    public static string? ExtractUninstallFailure(string output)
    {
        if (IsUninstallSuccess(output))
        {
            return null;
        }

        var trimmed = output.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? "Unknown failure" : trimmed;
    }

    public static bool IsPackageInstalled(string output, string packageName)
    {
        return output.Contains($"package:{packageName}", StringComparison.Ordinal);
    }

    /// <summary>
    /// Extracts the first <c>versionCode=</c> value from <c>dumpsys package</c> output.
    /// Returns <see langword="null"/> when the package is not installed or no version code is present.
    /// </summary>
    public static int? ParseVersionCode(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return null;
        }

        const string token = "versionCode=";
        var index = output.IndexOf(token, StringComparison.Ordinal);
        if (index < 0)
        {
            return null;
        }

        var start = index + token.Length;
        var end = start;
        while (end < output.Length && char.IsDigit(output[end]))
        {
            end++;
        }

        if (end == start)
        {
            return null;
        }

        return int.TryParse(output.AsSpan(start, end - start), out var versionCode)
            ? versionCode
            : null;
    }
}
