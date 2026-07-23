using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Styling;
using OTDR.Views.Preferences;
using OTDR.Views.About;
using OTDR.Views.ViewModels;
using OTDR.Core.Interfaces;
using OTDR.Core.Models.Connections;
using OTDR.Core.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Media.Imaging;

namespace OTDR.Views;

public partial class MainWindow : Window
{
    private readonly IPlotView _plot = null!;
    private readonly MainWindowViewModel _vm = null!;
    private readonly ISettingsService _settings = null!;
    private readonly IFileDialogService _fileDialogs = null!;
    private readonly IServiceProvider _services = null!;

    private GridLength _lastSettingsWidth = new(280);
    private bool _settingsVisible = true;

    public MainWindow()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new MainWindowViewModel();
        }
    }

    public MainWindow(ISettingsService settings, IPlotView plot, IFileDialogService fileDialogs,
        IServiceProvider services, MainWindowViewModel vm)
    {
        InitializeComponent();
        _settings = settings;
        _plot = plot;
        _fileDialogs = fileDialogs;
        _services = services;
        _vm = vm;

        PlotContainer.Content = _plot.AsControl();
        Closing += OnWindowClosing;

        DataContext = _vm;
        _vm.PropertyChanged += OnViewModelPropertyChanged;

        SetupInitialPlot();
    }

    public void ApplySettings()
    {
        Width = _settings.Settings.WindowWidth;
        Height = _settings.Settings.WindowHeight;
        Application.Current!.RequestedThemeVariant =
            _settings.Settings.CurrentTheme switch
            {
                AppSettings.Theme.Dark => ThemeVariant.Dark,
                AppSettings.Theme.Light => ThemeVariant.Light,
                _ => ThemeVariant.Default,
            };
        _vm.LineWidth = _settings.Settings.LineWidth;
        _vm.ShowMarkers = _settings.Settings.ShowMarkers;
        _vm.ShowGrid = _settings.Settings.ShowGrid;
    }

    private void SetupInitialPlot()
    {
        _plot.SetTitle("OTDR");
        _plot.SetAxisLabels("Distance [km]", "Signal [dBm]");
        _plot.ShowGrid(_vm.ShowGrid);

        if (_vm.CurrentTrace is { } trace)
            PlotTrace(trace);

        UpdatePlotStyle();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainWindowViewModel.CurrentTrace):
                if (_vm.CurrentTrace is { } trace)
                    PlotTrace(trace);
                break;

            case nameof(MainWindowViewModel.LineWidth):
            case nameof(MainWindowViewModel.ShowMarkers):
            case nameof(MainWindowViewModel.ShowLegend):
                UpdatePlotStyle();
                break;

            case nameof(MainWindowViewModel.ShowGrid):
                _plot.ShowGrid(_vm.ShowGrid);
                break;
        }
    }

    // Pushes new data onto the chart. Called only when CurrentTrace changes
    private void PlotTrace(Core.Models.Acquisition.TraceData trace)
    {
        _plot.PlotScatter(trace.DistanceKm, trace.SignalDbm);
        UpdatePlotStyle();
        _plot.AutoScale();
    }

    // Adjusts rendering of the existing series
    private void UpdatePlotStyle()
    {
        _plot.LineWidth = (float)_vm.LineWidth;
        _plot.MarkerSize = _vm.ShowMarkers ? 5 : 0;
        _plot.LegendText = _vm.ShowLegend ? "Trace" : null;
    }

    private void OnConnectionClick(object? sender, RoutedEventArgs e) => RefreshConnectionMenu();

    private void RefreshConnectionMenu()
    {
        while (ConnectionMenuItem.Items.Count > 2)
            ConnectionMenuItem.Items.RemoveAt(2);

        foreach (var provider in _vm.ConnectionProviders)
        {
            var submenu = new MenuItem { Header = provider.Name };

            foreach (var endpoint in provider.GetConnections())
            {
                var item = new MenuItem { Header = endpoint.DisplayName, Tag = endpoint };
                item.Click += OnEndpointSelected;
                submenu.Items.Add(item);
            }

            if (submenu.Items.Count == 0)
                submenu.Items.Add(new MenuItem { Header = "(none found)", IsEnabled = false });

            ConnectionMenuItem.Items.Add(submenu);
        }
    }

    private void OnEndpointSelected(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: DeviceEndpoint endpoint }) return;

        _vm.SelectedEndpoint = endpoint;
        _vm.ConnectCommand.Execute(null);
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        _plot.Clear();
        InfoText.Text = "Cleared";
        _vm.Averager.Reset();
    }

    private async void OnExportClick(object? sender, RoutedEventArgs e)
    {
        var path = await _fileDialogs.ShowSaveCsvDialogAsync(this);

        if (path is null)
        {
            InfoText.Text = "Export cancelled";
            return;
        }

        InfoText.Text = $"Selected: {path}";
        // TODO: write _vm.CurrentTrace to CSV at path
    }

    private void OnExitClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    private async void OnPreferencesClick(object? sender, RoutedEventArgs e)
    {
        var prefWindow = _services.GetRequiredService<PreferencesWindow>();
        await prefWindow.ShowDialog(this);
    }

    private void OnToggleSettingsClick(object? sender, RoutedEventArgs e)
    {
        _settingsVisible = !_settingsVisible;
        ApplySettingsVisibility();
    }

    private void ApplySettingsVisibility()
    {
        var columnDefs = RootGrid.ColumnDefinitions;

        if (_settingsVisible)
        {
            columnDefs[0].Width = _lastSettingsWidth;
            columnDefs[1].Width = new GridLength(4);
            SettingsPanel.IsVisible = true;
            SettingsSplitter.IsVisible = true;
            SettingsToggleButton.Content = "◀";
        }
        else
        {
            if (columnDefs[0].Width.Value > 0)
                _lastSettingsWidth = columnDefs[0].Width;

            columnDefs[0].Width = new GridLength(0);
            columnDefs[1].Width = new GridLength(0);
            SettingsPanel.IsVisible = false;
            SettingsSplitter.IsVisible = false;
            SettingsToggleButton.Content = "▶";
        }

        SettingsToggleButton.IsChecked = _settingsVisible;
    }

    private void OnDocsClick(object? sender, RoutedEventArgs e) => InfoText.Text = "Documentation (stub)";

    private async void OnAboutClick(object? sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow();
        await aboutWindow.ShowDialog(this);
    }

    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        _vm.Disconnect();

        _settings.Settings.ShowGrid = _vm.ShowGrid;
        _settings.Settings.LineWidth = (float)_vm.LineWidth;
        _settings.Settings.ShowMarkers = _vm.ShowMarkers;
        _settings.Settings.WindowWidth = Width;
        _settings.Settings.WindowHeight = Height;
    }
}