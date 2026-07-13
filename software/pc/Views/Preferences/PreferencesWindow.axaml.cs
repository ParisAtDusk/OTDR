using Avalonia.Controls;
using Avalonia.Interactivity;
using OTDR.Views.ViewModels;

namespace OTDR.Views.Preferences;

public partial class PreferencesWindow : Window
{
    private readonly PreferencesWindowViewModel _vm;

    public PreferencesWindow(PreferencesWindowViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = _vm;
    }

    private async void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        await _vm.SaveAsync();
        Close();
    }
}