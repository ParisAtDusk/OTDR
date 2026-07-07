using Avalonia.Controls;
using Avalonia.Media;

public interface IPlotView
{
    void PlotScatter(double[] xs, double[] ys);
    void Clear();

    float LineWidth { set; }
    float MarkerSize { set; }
    string? LegendText { set; }  // null = hide
    Color SeriesColor { set; }

    void SetTitle(string title);
    void SetAxisLabels(string x, string y);
    void ShowGrid(bool visible);
    void AutoScale();

    void SavePng(string path, int width, int height);
    void SetTheme(ITheme theme);

    Control AsControl();
}