using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using OTDR.Core.Interfaces;
using OTDR.Core.Models.Settings;

namespace OTDR.Views.ViewModels;

public partial class PreferencesWindowViewModel : ObservableObject
{
    private readonly ISettingsService _settings = null!;

    public PreferencesWindowViewModel()
    {
        SelectedTheme = AppSettings.Theme.Dark;
        SelectedPlottingBackend = AppSettings.PlottingBackend.LiveCharts;
    }
    public PreferencesWindowViewModel(ISettingsService settings)
    {
        _settings = settings;
        SelectedTheme = _settings.Settings.CurrentTheme;
        SelectedPlottingBackend = _settings.Settings.PlottingLibrary;
    }

    public IReadOnlyList<AppSettings.PlottingBackend> PlottingBackends { get; } =
        Enum.GetValues<AppSettings.PlottingBackend>();
    public IReadOnlyList<AppSettings.Theme> AvailableThemes { get; } =
        Enum.GetValues<AppSettings.Theme>();

    [ObservableProperty]
    private AppSettings.Theme selectedTheme;

    [ObservableProperty]
    private AppSettings.PlottingBackend selectedPlottingBackend;
    partial void OnSelectedThemeChanged(AppSettings.Theme value)
    {
        _ = _settings.SetThemeAsync(value);
        Application.Current!.RequestedThemeVariant = value switch
        {
            AppSettings.Theme.Dark => ThemeVariant.Dark,
            AppSettings.Theme.Light => ThemeVariant.Light,
            _ => ThemeVariant.Default,
        };
    }

    public async Task SaveAsync()
    {
        await _settings.SetPlottingBackend(SelectedPlottingBackend);
        await _settings.SetThemeAsync(SelectedTheme);
    }
}