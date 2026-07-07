using Avalonia.Controls;
using Avalonia.Media;

public sealed record PlotTheme(
    Color Background,
    Color DataBackground,
    Color Foreground,
    Color Grid,
    Color Accent
)
{
    public static PlotTheme Light { get; } = new(
        Background:     Color.FromRgb(0xFF, 0xFF, 0xFF),
        DataBackground: Color.FromRgb(0xFF, 0xFF, 0xFF),
        Foreground:     Color.FromRgb(0x20, 0x20, 0x20),
        Grid:           Color.FromRgb(0xE0, 0xE0, 0xE0),
        Accent:         Color.FromRgb(0x1F, 0x77, 0xB4)
    );

    public static PlotTheme Dark { get; } = new(
        Background:     Color.FromRgb(0x1E, 0x1E, 0x1E),
        DataBackground: Color.FromRgb(0x25, 0x25, 0x25),
        Foreground:     Color.FromRgb(0xE0, 0xE0, 0xE0),
        Grid:           Color.FromRgb(0x40, 0x40, 0x40),
        Accent:         Color.FromRgb(0x4F, 0x9E, 0xE0)
    );
}

public interface IPlotView
{
    void PlotScatter(double[] xs, double[] ys);
    void Clear();
    float LineWidth { set; }
    float MarkerSize { set; }
    string? LegendText { set; }
    void SetTitle(string title);
    void SetAxisLabels(string x, string y);
    void ShowGrid(bool visible);
    void AutoScale();
    void SavePng(string path, int width, int height);
    Control AsControl();

    void ApplyTheme(PlotTheme theme);
}