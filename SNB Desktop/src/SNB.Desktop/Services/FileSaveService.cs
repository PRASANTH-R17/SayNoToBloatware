using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace SNB.Desktop.Services;

/// <summary>
/// Saves text using the main window's <see cref="IStorageProvider"/> Save As picker.
/// </summary>
public sealed class FileSaveService : IFileSaveService
{
    public async Task<string?> SaveTextAsync(
        string content,
        string suggestedFileName,
        string fileTypeName,
        string extension)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop
            || desktop.MainWindow is not { } window)
        {
            return null;
        }

        var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export App List",
            SuggestedFileName = suggestedFileName,
            DefaultExtension = extension,
            ShowOverwritePrompt = true,
            FileTypeChoices =
            [
                new FilePickerFileType(fileTypeName)
                {
                    Patterns = [$"*.{extension}"],
                },
            ],
        });

        if (file is null)
        {
            return null;
        }

        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);

        return file.TryGetLocalPath() ?? file.Name;
    }
}
