using System.Threading.Tasks;
using OTDR.Core.Models.Settings;

namespace OTDR.Core.Interfaces;
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
    public async Task SetPlottingBackend(AppSettings.PlottingBackend plottingBackend)
    {
        Settings.PlottingLibrary = plottingBackend;
        await SaveAsync();
    }
}