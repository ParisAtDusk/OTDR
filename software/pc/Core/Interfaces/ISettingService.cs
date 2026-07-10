using System.Threading.Tasks;

public interface ISettingsService
{
    AppSettings Settings { get; }

    Task LoadAsync();
    Task SaveAsync();
    public async Task SetThemeAsync(AppSettings.Theme theme)
    {
        Settings.CurrentTheme = theme;
        await SaveAsync();
    }
}