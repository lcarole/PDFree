using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PDFree.Converters;

public class FileSizeConverter : IValueConverter
{
    public static readonly FileSizeConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is long bytes)
        {
            return bytes switch
            {
                < 1024 => $"{bytes} o",
                < 1024 * 1024 => $"{bytes / 1024.0:F1} Ko",
                < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024.0):F1} Mo",
                _ => $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} Go"
            };
        }
        return "—";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
