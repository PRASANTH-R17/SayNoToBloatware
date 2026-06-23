namespace SNB.Desktop.Models;

/// <summary>
/// File format for exporting the applications list.
/// </summary>
public enum ExportFormat
{
    Csv,
    Json,
}

public static class ExportFormatExtensions
{
    public static string GetFileExtension(this ExportFormat format) => format switch
    {
        ExportFormat.Json => "json",
        _ => "csv",
    };

    public static string GetDisplayName(this ExportFormat format) => format switch
    {
        ExportFormat.Json => "JSON",
        _ => "CSV",
    };
}
