using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OTDR.Core.Interfaces;
using OTDR.Core.Models.Settings;

namespace OTDR.Views.ViewModels;

public partial class PreferencesWindowViewModel : ObservableObject
{
    private readonly ISettingsService _settings;

    public PreferencesWindowViewModel(ISettingsService settings)
    {
        _settings = settings;
        SelectedTheme = _settings.Settings.CurrentTheme;
        SelectedPlottingBackend = _settings.Settings.PlottingLibrary;
    }

    public IReadOnlyList<AppSettings.PlottingBackend> PlottingBackends { get; } =
        Enum.GetValues<AppSettings.PlottingBackend>();

    [ObservableProperty]
    private AppSettings.Theme selectedTheme;

    [ObservableProperty]
    private AppSettings.PlottingBackend selectedPlottingBackend;
    public async Task SaveAsync()
    {
        await _settings.SetPlottingBackend(SelectedPlottingBackend);
        await _settings.SetThemeAsync(SelectedTheme);
    }
}