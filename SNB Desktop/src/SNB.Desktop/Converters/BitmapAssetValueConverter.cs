using System;
using System.Globalization;
using System.IO;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SNB.Desktop.Converters;

/// <summary>
/// Converts a string image path into an <see cref="Bitmap"/> for binding to <c>Image.Source</c>.
/// Supports <c>avares://</c> resource URIs and absolute file paths. Returns <c>null</c> for empty
/// or unresolvable paths so the UI can fall back to a placeholder visual instead of crashing — this
/// is what lets the bindable <c>ImagePath</c>/<c>IconPath</c> properties degrade gracefully.
///
/// Usage: Source="{Binding ImagePath, Converter={x:Static conv:BitmapAssetValueConverter.Instance}}"
/// </summary>
public sealed class BitmapAssetValueConverter : IValueConverter
{
    public static readonly BitmapAssetValueConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        try
        {
            if (path.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(path);
                if (AssetLoader.Exists(uri))
                {
                    using var stream = AssetLoader.Open(uri);
                    return new Bitmap(stream);
                }

                return null;
            }

            if (File.Exists(path))
            {
                return new Bitmap(path);
            }
        }
        catch
        {
            // Swallow load failures and fall back to the placeholder visual.
            return null;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
