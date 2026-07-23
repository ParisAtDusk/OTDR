
namespace OTDR.Core.Models.Settings;
public sealed class AppSettings
{
    public enum Theme
    {
        Dark,
        Light,
        Default
    };

    public float LineWidth { get; set; } = 1.0f;
    public bool ShowMarkers { get; set; } = false;
    public bool ShowGrid { get; set; } = true;
    public enum PlottingBackend { LiveCharts, ScottPlot }
    public Theme CurrentTheme { get; set; } = Theme.Default;
    public PlottingBackend PlottingLibrary { get; set; } = PlottingBackend.ScottPlot;
    public double WindowWidth { get; set; } = 1200;
    public double WindowHeight { get; set; } = 800;
}