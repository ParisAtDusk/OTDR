

public sealed class AppSettings
{
    public enum Theme
    {
        Dark,
        Light,
        Default
    };
    public Theme CurrentTheme { get; set; } = Theme.Default;
    public double WindowWidth { get; set; } = 1200;
    public double WindowHeight { get; set; } = 800;
}