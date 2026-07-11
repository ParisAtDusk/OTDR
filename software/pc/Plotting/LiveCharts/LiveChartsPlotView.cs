// Requires NuGet package: LiveChartsCore.SkiaSharpView.Avalonia
// (pulls in LiveChartsCore, LiveChartsCore.SkiaSharpView, SkiaSharp transitively)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using SkiaSharp;
using OTDR.Core.Interfaces;
using LiveChartsCore.Defaults;

namespace OTDR.Plotting.LiveCharts;

public sealed class LiveChartsPlotView : IPlotView
{
    private readonly CartesianChart _chart;
    private readonly TextBlock _titleBlock;
    private readonly DockPanel _root;

    private readonly LineSeries<ObservablePoint> _series;
    private readonly Axis _xAxis;
    private readonly Axis _yAxis;

    private double[] _xs = Array.Empty<double>();
    private double[] _ys = Array.Empty<double>();
    private float _lineWidth = 2f;
    private float _markerSize = 6f;
    private string? _legendText;
    private string _titleText = string.Empty;
    private string _xLabel = string.Empty;
    private string _yLabel = string.Empty;
    private bool _gridVisible = true;
    private PlotTheme _theme = PlotTheme.Light;

    public LiveChartsPlotView()
    {
        _series = new LineSeries<ObservablePoint>
        {
            Values = Array.Empty<ObservablePoint>(),
            Fill = null,
            LineSmoothness = 0,
            GeometrySize = _markerSize,
            Stroke = new SolidColorPaint(ToSk(_theme.Accent), _lineWidth),
            GeometryStroke = new SolidColorPaint(ToSk(_theme.Accent), _lineWidth),
            GeometryFill = new SolidColorPaint(ToSk(_theme.Accent)),
        };

        _xAxis = new Axis
        {
            Name = _xLabel,
            NamePaint = new SolidColorPaint(ToSk(_theme.Foreground)),
            LabelsPaint = new SolidColorPaint(ToSk(_theme.Foreground)),
            SeparatorsPaint = new SolidColorPaint(ToSk(_theme.Grid)) { StrokeThickness = 1 },
        };

        _yAxis = new Axis
        {
            Name = _yLabel,
            NamePaint = new SolidColorPaint(ToSk(_theme.Foreground)),
            LabelsPaint = new SolidColorPaint(ToSk(_theme.Foreground)),
            SeparatorsPaint = new SolidColorPaint(ToSk(_theme.Grid)) { StrokeThickness = 1 },
        };

        _chart = new CartesianChart
        {
            Series = new ISeries[] { _series },
            XAxes = new[] { _xAxis },
            YAxes = new[] { _yAxis },
            LegendPosition = LegendPosition.Hidden,
            Background = ToAvaloniaBrush(_theme.DataBackground),
            AnimationsSpeed = TimeSpan.FromMilliseconds(200),
        };

        _titleBlock = new TextBlock
        {
            Text = string.Empty,
            FontWeight = FontWeight.SemiBold,
            FontSize = 14,
            Margin = new Thickness(8, 6, 8, 2),
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = ToAvaloniaBrush(_theme.Foreground),
            IsVisible = false,
        };

        _root = new DockPanel
        {
            Background = ToAvaloniaBrush(_theme.Background),
        };
        DockPanel.SetDock(_titleBlock, Dock.Top);
        _root.Children.Add(_titleBlock);
        _root.Children.Add(_chart);

        _root.AttachedToVisualTree += (_, _) =>
        {
            ApplyTheme(_root.ActualThemeVariant == ThemeVariant.Dark
                ? PlotTheme.Dark
                : PlotTheme.Light);
        };

        _root.ActualThemeVariantChanged += (_, _) =>
        {
            var theme = _root.ActualThemeVariant == ThemeVariant.Dark
                ? PlotTheme.Dark
                : PlotTheme.Light;
            ApplyTheme(theme);
        };
    }

    public void PlotScatter(double[] xs, double[] ys)
    {
        if (xs.Length != ys.Length)
            throw new ArgumentException("xs and ys must have the same length.");

        _xs = xs;
        _ys = ys;

        var points = new ObservablePoint[xs.Length];
        for (int i = 0; i < xs.Length; i++)
            points[i] = new ObservablePoint(xs[i], ys[i]);

        _series.Values = points;
    }

    public void Clear()
    {
        _xs = Array.Empty<double>();
        _ys = Array.Empty<double>();
        _series.Values = Array.Empty<ObservablePoint>();
    }

    public float LineWidth
    {
        set
        {
            _lineWidth = value;
            if (_series.Stroke is SolidColorPaint sp) sp.StrokeThickness = value;
            if (_series.GeometryStroke is SolidColorPaint gsp) gsp.StrokeThickness = value;
        }
    }

    public float MarkerSize
    {
        set
        {
            _markerSize = value;
            _series.GeometrySize = value;
        }
    }

    public string? LegendText
    {
        set
        {
            _legendText = value;
            _series.Name = value;
            _chart.LegendPosition = string.IsNullOrEmpty(value)
                ? LegendPosition.Hidden
                : LegendPosition.Top;
        }
    }

    public void SetTitle(string title)
    {
        _titleText = title;
        _titleBlock.Text = title;
        _titleBlock.IsVisible = !string.IsNullOrEmpty(title);
    }

    public void SetAxisLabels(string x, string y)
    {
        _xLabel = x;
        _yLabel = y;
        _xAxis.Name = x;
        _yAxis.Name = y;
    }

    public void ShowGrid(bool visible)
    {
        _gridVisible = visible;
        var paint = visible ? new SolidColorPaint(ToSk(_theme.Grid)) { StrokeThickness = 1 } : null;
        _xAxis.SeparatorsPaint = paint;
        _yAxis.SeparatorsPaint = paint;
    }

    public void AutoScale()
    {
        _xAxis.MinLimit = null;
        _xAxis.MaxLimit = null;
        _yAxis.MinLimit = null;
        _yAxis.MaxLimit = null;
    }

    public void SavePng(string path, int width, int height)
    {
        var exportSeries = new LineSeries<ObservablePoint>
        {
            Name = _legendText,
            Values = _xs.Select((x, i) => new ObservablePoint(x, _ys[i])).ToArray(),
            Fill = null,
            LineSmoothness = 0,
            GeometrySize = _markerSize,
            Stroke = new SolidColorPaint(ToSk(_theme.Accent), _lineWidth),
            GeometryStroke = new SolidColorPaint(ToSk(_theme.Accent), _lineWidth),
            GeometryFill = new SolidColorPaint(ToSk(_theme.Accent)),
        };

        var gridPaint = _gridVisible
            ? new SolidColorPaint(ToSk(_theme.Grid)) { StrokeThickness = 1 }
            : null;

        var exportXAxis = new Axis
        {
            Name = _xLabel,
            NamePaint = new SolidColorPaint(ToSk(_theme.Foreground)),
            LabelsPaint = new SolidColorPaint(ToSk(_theme.Foreground)),
            SeparatorsPaint = gridPaint,
            MinLimit = _xAxis.MinLimit,
            MaxLimit = _xAxis.MaxLimit,
        };

        var exportYAxis = new Axis
        {
            Name = _yLabel,
            NamePaint = new SolidColorPaint(ToSk(_theme.Foreground)),
            LabelsPaint = new SolidColorPaint(ToSk(_theme.Foreground)),
            SeparatorsPaint = gridPaint,
            MinLimit = _yAxis.MinLimit,
            MaxLimit = _yAxis.MaxLimit,
        };

        int titleHeight = string.IsNullOrEmpty(_titleText) ? 0 : 32;
        int chartHeight = height - titleHeight;

        var skChart = new SKCartesianChart
        {
            Width = width,
            Height = chartHeight,
            Series = new ISeries[] { exportSeries },
            XAxes = new ICartesianAxis[] { exportXAxis },
            YAxes = new ICartesianAxis[] { exportYAxis },
            Background = ToSk(_theme.DataBackground),
            LegendPosition = string.IsNullOrEmpty(_legendText) ? LegendPosition.Hidden : LegendPosition.Top,
        };

        using var chartImage = skChart.GetImage();

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        canvas.Clear(ToSk(_theme.Background));

        if (titleHeight > 0)
        {
            using var font = new SKFont(SKTypeface.Default, 16);
            using var textPaint = new SKPaint { Color = ToSk(_theme.Foreground), IsAntialias = true };
            float textWidth = font.MeasureText(_titleText);
            canvas.DrawText(_titleText, (width - textWidth) / 2f, titleHeight - 10, font, textPaint);
        }

        canvas.DrawImage(chartImage, 0, titleHeight);

        using var finalImage = surface.Snapshot();
        using var data = finalImage.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }

    public Control AsControl() => _root;

    public void ApplyTheme(PlotTheme theme)
    {
        _theme = theme;

        _root.Background = ToAvaloniaBrush(theme.Background);
        _chart.Background = ToAvaloniaBrush(theme.DataBackground);
        _titleBlock.Foreground = ToAvaloniaBrush(theme.Foreground);

        _series.Stroke = new SolidColorPaint(ToSk(theme.Accent), _lineWidth);
        _series.GeometryStroke = new SolidColorPaint(ToSk(theme.Accent), _lineWidth);
        _series.GeometryFill = new SolidColorPaint(ToSk(theme.Accent));

        _xAxis.NamePaint = new SolidColorPaint(ToSk(theme.Foreground));
        _xAxis.LabelsPaint = new SolidColorPaint(ToSk(theme.Foreground));
        _yAxis.NamePaint = new SolidColorPaint(ToSk(theme.Foreground));
        _yAxis.LabelsPaint = new SolidColorPaint(ToSk(theme.Foreground));

        ShowGrid(_gridVisible);
    }

    private static SKColor ToSk(Color c) => new(c.R, c.G, c.B, c.A);

    private static IBrush ToAvaloniaBrush(Color c) => new SolidColorBrush(c);
}