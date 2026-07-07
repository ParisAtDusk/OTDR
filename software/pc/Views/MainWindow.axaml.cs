using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Styling;
using OTDR.Views.Preferences;
using OTDR.Views.About;

namespace OTDR.Views;

public partial class MainWindow : Window
{
    // Keep a reference to the scatter plot so we can update it in place
    // instead of re-adding a new plot every time a slider moves.
    private readonly IPlotView _plot;
    private readonly Random _random = new();

    // Remembers the settings column's width so it can be restored
    // after being collapsed to zero.
    private GridLength _lastSettingsWidth = new(280);
    private bool _settingsVisible = true;

    public MainWindow() : this(new ScottPlotView()) { } // default

    public MainWindow(IPlotView plot)
    {
        InitializeComponent();
        _plot = plot;
        PlotContainer.Content = _plot.AsControl();
        // Build the initial example plot once the window is constructed.
        SetupInitialPlot();
    }

    // ====================================================================
    // Plot setup
    // ====================================================================

    private void SetupInitialPlot()
    {
        _plot.SetTitle("OTDR");
        _plot.SetAxisLabels("Distance [km]", "Signal [dBm]");

        // ScottPlot 5 shows grid lines by default; this just makes the
        // initial checkbox state (checked) match reality.
        _plot.ShowGrid(true);
        // _plot.Grid.MajorLineColor = Colors.LightGray;

        PlotGeneratedData();
    }

    /// <summary>
    /// Generates example data (a noisy sine wave) using the current
    /// slider values and (re)plots it.
    /// </summary>
    private void PlotGeneratedData()
    {
        double frequency = FrequencySlider.Value;
        double amplitude = AmplitudeSlider.Value;
        double noise = NoiseSlider.Value;
        int count = (int)PointCountSlider.Value;

        double[] xs = new double[count];
        double[] ys = new double[count];

        for (int i = 0; i < count; i++)
        {
            double x = i / (double)count * 10.0; // 0..10
            double y = amplitude * Math.Sin(2 * Math.PI * frequency * x / 10.0);
            y += (_random.NextDouble() - 0.5) * 2 * noise; // add noise
            xs[i] = x;
            ys[i] = y;
        }

        _plot.PlotScatter(xs, ys);
        _plot.LineWidth  = (float)LineWidthSlider.Value;
        _plot.MarkerSize = ShowMarkersCheckBox.IsChecked == true ? 5 : 0;
        _plot.LegendText = ShowLegendCheckBox.IsChecked == true ? "Example series" : null;
        _plot.AutoScale();

        FrequencyValueText.Text = frequency.ToString("0.00");
    }

    // ====================================================================
    // Settings panel handlers
    // ====================================================================

    private void OnShowGridChanged(object? sender, RoutedEventArgs e)
    {
        _plot.ShowGrid(ShowGridCheckBox.IsChecked == true);
    }

    private void OnShowLegendChanged(object? sender, RoutedEventArgs e)
    {
        PlotGeneratedData();
    }

    private void OnShowMarkersChanged(object? sender, RoutedEventArgs e)
    {
        PlotGeneratedData();
    }

    private void OnParameterChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        PlotGeneratedData();
    }

    private void OnPlotClick(object? sender, RoutedEventArgs e)
    {
        InfoText.Text = "Plotted";
        PlotGeneratedData();
    }

    private void OnThemeClick(object? sender, RoutedEventArgs e)
    {
    Application.Current!.RequestedThemeVariant =
        (sender as MenuItem)?.Header switch
        {
            "Dark"  => ThemeVariant.Dark,
            "Light" => ThemeVariant.Light,
            _       => ThemeVariant.Default,
        };
    }

    private void OnStartClick(object? sender, RoutedEventArgs e)
    {
        FrequencySlider.Value = _random.NextDouble() * 5;
        AmplitudeSlider.Value = 0.5 + _random.NextDouble() * 2.5;
        NoiseSlider.Value = _random.NextDouble() * 0.5;
        InfoText.Text = "Randomized";
        PlotGeneratedData();
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        _plot.Clear();
        InfoText.Text = "Cleared";
    }

    private void OnExportClick(object? sender, RoutedEventArgs e)
    {
        // Example of exporting the current chart to a PNG file.
        _plot.SavePng("chart_export.png", 1000, 600);
        InfoText.Text = "Exported to chart_export.png";
    }

    // ====================================================================
    // Menu handlers (File / View / Settings / Help)
    // ====================================================================

    private void OnExitClick(object? sender, RoutedEventArgs e)
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private async void OnPreferencesClick(object? sender, RoutedEventArgs e) {
        var prefWindow = new PreferencesWindow();
        await prefWindow.ShowDialog(this);
    }

    // ====================================================================
    // Settings panel show/hide
    // ====================================================================

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
            // Restore the previous width (or a sensible default) and
            // bring back the splitter column.
            columnDefs[0].Width = _lastSettingsWidth;
            columnDefs[1].Width = new GridLength(4);
            SettingsPanel.IsVisible = true;
            SettingsSplitter.IsVisible = true;
            SettingsToggleButton.Content = "◀";
        }
        else
        {
            // Remember the current width before collapsing so we can
            // restore it later, then collapse both the panel and the
            // splitter column down to zero width.
            if (columnDefs[0].Width.Value > 0)
            {
                _lastSettingsWidth = columnDefs[0].Width;
            }

            columnDefs[0].Width = new GridLength(0);
            columnDefs[1].Width = new GridLength(0);
            SettingsPanel.IsVisible = false;
            SettingsSplitter.IsVisible = false;
            SettingsToggleButton.Content = "▶";
        }

        // Keep the ToggleButton's checked state in sync in case this was
        // triggered from the View menu instead of the button itself.
        SettingsToggleButton.IsChecked = _settingsVisible;
    }

    private void OnDocsClick(object? sender, RoutedEventArgs e) => InfoText.Text = "Documentation (stub)";
    private async void OnAboutClick(object? sender, RoutedEventArgs e) {
        var aboutWindow = new AboutWindow();
        await aboutWindow.ShowDialog(this);
    }
}
