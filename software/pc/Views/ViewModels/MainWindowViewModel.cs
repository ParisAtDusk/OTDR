using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OTDR.Views.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    // ---- On Time (ns <-> "1us"-style text) ----
    private double _onTimeNs = 1000.0;
    private string _onTimeStr = "1us";

    public double OnTime
    {
        get => _onTimeNs;
        set
        {
            if (Math.Abs(_onTimeNs - value) < 0.0001) return;
            _onTimeNs = value;
            OnPropertyChanged(nameof(OnTime));

            var formatted = FormatNs(value);
            if (_onTimeStr != formatted)
            {
                _onTimeStr = formatted;
                OnPropertyChanged(nameof(OnTimeStr));
            }
        }
    }

    public string OnTimeStr
    {
        get => _onTimeStr;
        set
        {
            if (_onTimeStr == value) return;

            _onTimeStr = value;
            OnPropertyChanged(nameof(OnTimeStr));

            if (TryParseToNs(value, out double ns))
            {
                OnTime = ns;
            }
        }
    }

    private static string FormatNs(double ns)
    {
        return ns switch
        {
            < 1_000 => $"{ns:0}ns",
            < 1_000_000 => $"{ns / 1_000:0.##}us",
            _ => $"{ns / 1_000_000:0.##}ms"
        };
    }
    private static bool TryParseToNs(string text, out double ns)
    {
        ns = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;

        text = text.Trim().ToLowerInvariant();

        double multiplier;
        string numberPart;

        if (text.EndsWith("ns"))
        {
            multiplier = 1.0;
            numberPart = text[..^2];
        }
        else if (text.EndsWith("us") || text.EndsWith("µs"))
        {
            multiplier = 1_000.0;
            numberPart = text[..^2];
        }
        else if (text.EndsWith("ms"))
        {
            multiplier = 1_000_000.0;
            numberPart = text[..^2];
        }
        else
        {
            return false;
        }

        if (!double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
            return false;

        ns = num * multiplier;
        return true;
    }

    // ---- Other plot parameters ----
    private double? _lineWidth = 1.0;
    public double? LineWidth
    {
        get => _lineWidth;
        set => SetProperty(ref _lineWidth, value ?? 1.0);
    }

    [ObservableProperty]
    private bool showMarkers;
    [ObservableProperty]
    private bool showLegend;
    [ObservableProperty]
    private bool showGrid = true;
}