using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SNB.Desktop.Converters;

/// <summary>
/// Returns true when the bound value equals the <c>ConverterParameter</c>. Used to toggle the
/// "Active" style class on sidebar nav items based on the shell's <c>ActiveNavItem</c> string.
///
/// Usage: Classes.Active="{Binding ActiveNavItem, Converter={x:Static conv:EqualityConverter.Instance}, ConverterParameter=Settings}"
/// </summary>
public sealed class EqualityConverter : IValueConverter
{
    public static readonly EqualityConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.Equals(value?.ToString(), parameter?.ToString(), StringComparison.Ordinal);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
