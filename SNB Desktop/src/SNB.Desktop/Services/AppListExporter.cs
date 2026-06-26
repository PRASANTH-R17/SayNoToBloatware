using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using SNB.Desktop.Models;

namespace SNB.Desktop.Services;

/// <summary>
/// Serializes the applications list into CSV or JSON text for export.
/// </summary>
public static class AppListExporter
{
    private static readonly string[] CsvHeaders =
    [
        "App Name",
        "Package Name",
        "Type",
        "Category",
        "Size",
        "Version",
        "Source",
    ];

    public static string ToCsv(IEnumerable<ApplicationItemModel> apps)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(',', CsvHeaders.Select(Escape)));

        foreach (var app in apps)
        {
            builder.AppendLine(string.Join(',', new[]
            {
                Escape(app.AppName),
                Escape(app.PackageName),
                Escape(app.TypeLabel),
                Escape(app.CategoryDisplayName),
                Escape(app.Size),
                Escape(app.Version),
                Escape(app.Source),
            }));
        }

        return builder.ToString();
    }

    public static string ToJson(IEnumerable<ApplicationItemModel> apps)
    {
        var payload = apps.Select(app => new
        {
            app.AppName,
            app.PackageName,
            Type = app.TypeLabel,
            Category = app.CategoryDisplayName,
            app.Size,
            app.SizeBytes,
            app.Version,
            app.Source,
            app.IsSystemApp,
        });

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true,
        });
    }

    /// <summary>Quotes a CSV field when it contains a comma, quote, or line break (RFC 4180).</summary>
    private static string Escape(string? value)
    {
        var field = value ?? string.Empty;
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}
