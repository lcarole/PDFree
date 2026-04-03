using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PDOff.Converters;

public class EnumToBoolConverter : IValueConverter
{
    public static readonly EnumToBoolConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() == parameter?.ToString();

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true && parameter is string enumStr)
            return Enum.Parse(targetType, enumStr);
        return null;
    }
}
