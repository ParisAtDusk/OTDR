using Avalonia.Media;
using System.Text.Json.Serialization;

public class JsonTheme : ITheme
{
    [JsonPropertyName("Name")]
    public string Name            { get; set; } = "";

    [JsonPropertyName("FluentVariant")]
    public string FluentVariant   { get; set; } = "Dark";

    [JsonPropertyName("Background")]
    public string BackgroundHex   { get; set; } = "#1a1a2e";

    [JsonPropertyName("PanelBackground")]
    public string PanelBackgroundHex { get; set; } = "#16213e";

    [JsonPropertyName("Foreground")]
    public string ForegroundHex   { get; set; } = "#e0e0e0";

    [JsonPropertyName("Accent")]
    public string AccentHex       { get; set; } = "#4a9eff";

    [JsonPropertyName("Border")]
    public string BorderHex       { get; set; } = "#33ffffff";
    
    [JsonPropertyName("SeriesColor")]
    public string SeriesColorHex { get; set; } = "#4a9eff";

    Color ITheme.SeriesColor     => Color.Parse(SeriesColorHex);
    Color ITheme.Background      => Color.Parse(BackgroundHex);
    Color ITheme.PanelBackground => Color.Parse(PanelBackgroundHex);
    Color ITheme.Foreground      => Color.Parse(ForegroundHex);
    Color ITheme.Accent          => Color.Parse(AccentHex);
    Color ITheme.Border          => Color.Parse(BorderHex);
}