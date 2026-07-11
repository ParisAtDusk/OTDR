using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OTDR.Views;
using OTDR.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Serilog;
using OTDR.Plotting.LiveCharts;

namespace OTDR;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        services.AddSingleton<IPlotView>(sp =>
        {
            var settings = sp.GetRequiredService<ISettingsService>().Settings;
            return settings.PlottingLibrary switch
            {
                AppSettings.PlottingBackend.LiveCharts => new LiveChartsPlotView(),
                AppSettings.PlottingBackend.ScottPlot => new ScottPlotView(),
                _ => new ScottPlotView(),
            };
        });

        services.AddSingleton<ISettingsService, JsonSettingService>();
        // services.AddSingleton<IPlotView, LiveChartsPlotView>();
        // services.AddSingleton<IPlotView, ScottPlotView>();
        services.AddSingleton<IFileDialogService, FileDialogService>();
        services.AddTransient<MainWindow>();
        Services = services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var settingsService = Services.GetRequiredService<ISettingsService>();
            Task.Run(() => settingsService.LoadAsync()).GetAwaiter().GetResult();

            var mainWindow = Services.GetRequiredService<MainWindow>();
            desktop.MainWindow = mainWindow;
            mainWindow.ApplySettings();

            // _ = InitializeAsync(settingsService, mainWindow);

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
