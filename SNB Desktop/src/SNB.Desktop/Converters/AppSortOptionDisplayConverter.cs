using System;
using System.Globalization;
using Avalonia.Data.Converters;
using SNB.Desktop.Models;

namespace SNB.Desktop.Converters;

public sealed class AppSortOptionDisplayConverter : IValueConverter
{
    public static readonly AppSortOptionDisplayConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AppSortOption sort)
        {
            return $"Sort by: {sort.GetDisplayName()}";
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
