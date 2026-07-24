using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OTDR.Core.Helpers.Converters;

public class ProgressColorConverter : IMultiValueConverter
{
    public static readonly ProgressColorConverter Instance = new();
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 0 && values[0] is not null
            && Application.Current is { } app)
        {
            double progress;
            try
            {
                progress = System.Convert.ToDouble(values[0], culture);
            }
            catch
            {
                return Brushes.DodgerBlue;
            }

            var key = progress >= 100 ? "ProgressCompleteBrush" : "ProgressInProgressBrush";
            if (app.TryFindResource(key, app.ActualThemeVariant, out var brush))
                return brush;
        }
        return Brushes.DodgerBlue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}