using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OTDR.Views;
using OTDR.Core.Interfaces;

namespace OTDR;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        var services = new ServiceCollection();

        services.AddSingleton<ISettingsService, JsonSettingService>();
        services.AddSingleton<IPlotView, ScottPlotView>();
        services.AddSingleton<IFileDialogService, FileDialogService>();
        services.AddTransient<MainWindow>();
        Services = services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var settingsService = Services.GetRequiredService<ISettingsService>();

            var mainWindow = Services.GetRequiredService<MainWindow>();
            desktop.MainWindow = mainWindow;

            _ = InitializeAsync(settingsService, mainWindow);

            desktop.Exit += async (_, _) =>
            {
                await settingsService.SaveAsync();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task InitializeAsync(
        ISettingsService settingsService,
        MainWindow mainWindow)
    {
        try
        {
            await settingsService.LoadAsync();

            mainWindow.ApplySettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Startup failed: {ex}");
        }
    }
}
