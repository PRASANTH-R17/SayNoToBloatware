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
}
