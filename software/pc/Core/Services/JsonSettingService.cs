using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OTDR.Core.Interfaces;

public class JsonSettingService : ISettingsService
{
    private const string FileName = "settings.json";
    private readonly ILogger<JsonSettingService> _logger;
    public AppSettings Settings { get; private set; } = new();

    public JsonSettingService(ILogger<JsonSettingService> logger)
    {
        _logger = logger;
    }

    public async Task LoadAsync()
    {
        _logger.LogInformation("Loading settings from {FileName}", FileName);
        try
        {
            Settings = await JsonStorage.LoadAsync<AppSettings>(FileName) ?? new AppSettings();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings from {FileName}, using defaults", FileName);
            Settings = new AppSettings();
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            await JsonStorage.SaveAsync(FileName, Settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings to {FileName}", FileName);
        }
    }
}