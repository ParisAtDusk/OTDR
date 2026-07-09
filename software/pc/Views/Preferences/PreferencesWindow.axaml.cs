using Avalonia.Controls;
using Avalonia.Interactivity;

namespace OTDR.Views.Preferences;

public partial class PreferencesWindow : Window
{
    public PreferencesWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}