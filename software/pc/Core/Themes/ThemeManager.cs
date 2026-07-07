using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

public class ThemeManager
{
    public void Apply(ITheme theme)
    {
        Application.Current!.RequestedThemeVariant =
            theme.FluentVariant == "Dark" ? ThemeVariant.Dark : ThemeVariant.Light;

        var res = Application.Current.Resources;
        res["AppBackground"]       = new SolidColorBrush(theme.Background);
        res["AppPanelBackground"]  = new SolidColorBrush(theme.PanelBackground);
        res["AppForeground"]       = new SolidColorBrush(theme.Foreground);
        res["AppAccent"]           = new SolidColorBrush(theme.Accent);
        res["AppBorder"]           = new SolidColorBrush(theme.Border);
    }
}