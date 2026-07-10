using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OTDR.Core.Interfaces;

namespace OTDR.Views.Preferences;

public partial class PreferencesWindow : Window
{
    private readonly ISettingsService _settings;
    public PreferencesWindow(ISettingsService settings)
    {
        InitializeComponent();
        _settings = settings;
    }

    private async void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        await _settings.SaveAsync();
        Close();
    }
}