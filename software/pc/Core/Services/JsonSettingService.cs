using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OTDR.Core.Interfaces;

public class JsonSettingService : ISettingsService
{
    private const string FileName = "settings.json";
    public AppSettings Settings { get; private set; } = new();

    public async Task LoadAsync()
    {
        Console.WriteLine("Loading settings");
        try
        {
            Settings = await JsonStorage.LoadAsync<AppSettings>(FileName) ?? new AppSettings();
        }
        catch (Exception ex)
        {
            // TODO: Add logging
            Console.WriteLine("Setting loading failed!");
            Settings = new AppSettings();
        }
    }

    public async Task SaveAsync()
    {
        Console.WriteLine("Saving settings");
        try
        {
            await JsonStorage.SaveAsync(FileName, Settings);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Saving settings failed!");
            // TODO: Add logging
        }
    }
}