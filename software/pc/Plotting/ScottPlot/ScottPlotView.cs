using Avalonia.Controls;
using ScottPlot.Avalonia;
using Avalonia.Styling;

public class ScottPlotView : IPlotView
{
    private readonly AvaPlot _avaPlot = new();
    private ScottPlot.Plottables.Scatter? _scatter;
    private ScottPlot.Color _seriesColor = new(74, 158, 255);

    public void PlotScatter(double[] xs, double[] ys)
    {
        _avaPlot.Plot.Clear();
        _scatter = _avaPlot.Plot.Add.Scatter(xs, ys);
        _scatter.Color = _seriesColor;
        _avaPlot.Refresh();
    }

    public ScottPlotView()
    {
        _avaPlot.AttachedToVisualTree += (_, _) =>
        {
            ApplyTheme(_avaPlot.ActualThemeVariant == ThemeVariant.Dark
                ? PlotTheme.Dark
                : PlotTheme.Light);
        };

        _avaPlot.ActualThemeVariantChanged += (_, _) =>
        {
            var theme = _avaPlot.ActualThemeVariant == ThemeVariant.Dark
                ? PlotTheme.Dark
                : PlotTheme.Light;
            ApplyTheme(theme);
        };
    }


    public void Clear()             { _avaPlot.Plot.Clear(); _avaPlot.Refresh(); }
    public void AutoScale()         { _avaPlot.Plot.Axes.AutoScale(); _avaPlot.Refresh(); }
    public void SetTitle(string t)  { _avaPlot.Plot.Title(t); _avaPlot.Refresh(); }
    public void ShowGrid(bool show)
    {
        if (show){
            ApplyTheme(_avaPlot.ActualThemeVariant == ThemeVariant.Dark
                ? PlotTheme.Dark
                : PlotTheme.Light);
        } else{
            _avaPlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromARGB(0x00000000);
        }
        _avaPlot.Refresh();
    }
    public void SavePng(string path, int w, int h) => _avaPlot.Plot.SavePng(path, w, h);
    public Control AsControl()      => _avaPlot;

    public void SetAxisLabels(string x, string y)
    {
        _avaPlot.Plot.XLabel(x);
        _avaPlot.Plot.YLabel(y);
        _avaPlot.Refresh();
    }

    public float LineWidth
    {
        set { if (_scatter != null) { _scatter.LineWidth = value; _avaPlot.Refresh(); } }
    }

    public float MarkerSize
    {
        set { if (_scatter != null) { _scatter.MarkerSize = value; _avaPlot.Refresh(); } }
    }

    public string? LegendText
    {
        set
        {
            if (_scatter == null) return;
            if (value != null) { _scatter.LegendText = value; _avaPlot.Plot.ShowLegend(); }
            else               { _avaPlot.Plot.HideLegend(); }
            _avaPlot.Refresh();
        }
    }

    public Avalonia.Media.Color SeriesColor
    {
        set => _seriesColor = ToScottColor(value);
    }
     public void ApplyTheme(PlotTheme theme)
    {
        var plt = _avaPlot.Plot;

        plt.FigureBackground.Color = ToScottColor(theme.Background);
        plt.DataBackground.Color   = ToScottColor(theme.DataBackground);

        plt.Axes.Color(ToScottColor(theme.Foreground));
        plt.Grid.MajorLineColor = ToScottColor(theme.Grid);

        plt.Legend.BackgroundColor = ToScottColor(theme.DataBackground);
        plt.Legend.FontColor       = ToScottColor(theme.Foreground);
        plt.Legend.OutlineColor    = ToScottColor(theme.Grid);

        plt.Add.Palette = new ScottPlot.Palettes.Category10(); 

        _avaPlot.Refresh();
    }
    private static ScottPlot.Color ToScottColor(Avalonia.Media.Color c) =>
        new(c.R, c.G, c.B, c.A);
}