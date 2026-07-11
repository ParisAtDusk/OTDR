

public sealed class AppSettings
{
    public enum Theme
    {
        Dark,
        Light,
        Default
    };
    public enum PlottingBackend { LiveCharts, ScottPlot }
    public Theme CurrentTheme { get; set; } = Theme.Default;
    public PlottingBackend PlottingLibrary { get; set; } = PlottingBackend.ScottPlot;
    public double WindowWidth { get; set; } = 1200;
    public double WindowHeight { get; set; } = 800;
}