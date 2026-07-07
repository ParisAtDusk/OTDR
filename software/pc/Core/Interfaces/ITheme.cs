using Avalonia.Media;

public interface ITheme
{
    string Name            { get; }
    string FluentVariant   { get; }
    Color Background       { get; }
    Color PanelBackground  { get; }
    Color Foreground       { get; }
    Color Accent           { get; }
    Color Border           { get; }
    Color SeriesColor      { get; }
}