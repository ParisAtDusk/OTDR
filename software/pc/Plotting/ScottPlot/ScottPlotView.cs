using Avalonia.Controls;
using ScottPlot.Avalonia;
using Avalonia.Media;
using ScottPlot;

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

    public void Clear()             { _avaPlot.Plot.Clear(); _avaPlot.Refresh(); }
    public void AutoScale()         { _avaPlot.Plot.Axes.AutoScale(); _avaPlot.Refresh(); }
    public void SetTitle(string t)  { _avaPlot.Plot.Title(t); _avaPlot.Refresh(); }
    public void ShowGrid(bool show)
    {
        _avaPlot.Plot.Grid.MajorLineColor = show
            ? ScottPlot.Color.FromARGB(0xFFD3D3D3)
            : ScottPlot.Color.FromARGB(0x00000000);
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
    public void SetTheme(ITheme theme)
    {
        var plot = _avaPlot.Plot;
        plot.FigureBackground.Color = ToScottColor(theme.Background);
        plot.DataBackground.Color   = ToScottColor(theme.PanelBackground);
        plot.Axes.Color(ToScottColor(theme.Foreground));
        plot.Grid.MajorLineColor    = ToScottColor(theme.Foreground).WithAlpha(40);
        _seriesColor = ToScottColor(theme.SeriesColor);
        if (_scatter != null)
        {
            _scatter.Color = _seriesColor;
            _avaPlot.Refresh();
        }
    }

    public Avalonia.Media.Color SeriesColor
    {
        set => _seriesColor = ToScottColor(value);
    }

    private static ScottPlot.Color ToScottColor(Avalonia.Media.Color c) =>
        new(c.R, c.G, c.B, c.A);
}