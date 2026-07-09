using System;
using System.ComponentModel;
using System.Globalization;

namespace OTDR.Views.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
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

            // Only accept text that actually parses to a valid ns value.
            // If it doesn't parse (user is mid-typing, e.g. "1u"), just
            // store the raw text so the TextBox doesn't fight the user,
            // but don't touch OnTime yet.
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

    /// <summary>
    /// Parses strings like "500ns", "1.5us", "1us", "2ms" (case-insensitive,
    /// also accepts "µs") into a nanosecond value.
    /// </summary>
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
    private double _amplitude = 1.0;
    public double Amplitude
    {
        get => _amplitude;
        set { if (Math.Abs(_amplitude - value) < 0.0001) return; _amplitude = value; OnPropertyChanged(nameof(Amplitude)); }
    }

    private double _noise = 0.1;
    public double Noise
    {
        get => _noise;
        set { if (Math.Abs(_noise - value) < 0.0001) return; _noise = value; OnPropertyChanged(nameof(Noise)); }
    }

    private double _pointCount = 200;
    public double PointCount
    {
        get => _pointCount;
        set { if (Math.Abs(_pointCount - value) < 0.0001) return; _pointCount = value; OnPropertyChanged(nameof(PointCount)); }
    }

    private double _lineWidth = 1.0;
    public double LineWidth
    {
        get => _lineWidth;
        set { if (Math.Abs(_lineWidth - value) < 0.0001) return; _lineWidth = value; OnPropertyChanged(nameof(LineWidth)); }
    }

    private bool _showMarkers;
    public bool ShowMarkers
    {
        get => _showMarkers;
        set { if (_showMarkers == value) return; _showMarkers = value; OnPropertyChanged(nameof(ShowMarkers)); }
    }

    private bool _showLegend;
    public bool ShowLegend
    {
        get => _showLegend;
        set { if (_showLegend == value) return; _showLegend = value; OnPropertyChanged(nameof(ShowLegend)); }
    }

    private bool _showGrid = true;
    public bool ShowGrid
    {
        get => _showGrid;
        set { if (_showGrid == value) return; _showGrid = value; OnPropertyChanged(nameof(ShowGrid)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}