using System.Threading.Tasks;

namespace SNB.Desktop.Services;

/// <summary>
/// Saves text content to disk via a native "Save As" picker.
/// </summary>
public interface IFileSaveService
{
    /// <summary>
    /// Prompts the user for a destination and writes <paramref name="content"/> there.
    /// Returns the saved file path, or null if the user cancelled.
    /// </summary>
    /// <param name="content">Text to write.</param>
    /// <param name="suggestedFileName">Default file name (including extension).</param>
    /// <param name="fileTypeName">Human-readable file type label, e.g. "CSV file".</param>
    /// <param name="extension">Extension without the dot, e.g. "csv".</param>
    Task<string?> SaveTextAsync(
        string content,
        string suggestedFileName,
        string fileTypeName,
        string extension);
}
