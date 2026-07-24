using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OTDR.Core.Helpers.Converters;

public class StatusKeyToBrushConverter : IMultiValueConverter
{
    public static readonly StatusKeyToBrushConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 0
            && values[0] is string key
            && Application.Current is { } app
            && app.TryFindResource(key, app.ActualThemeVariant, out var brush))
        {
            return brush;
        }
        return Brushes.Gray;
    }
}