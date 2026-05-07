using System.Windows;
using System.Windows.Media;
using CrosshairOverlay.Models;

namespace CrosshairOverlay;

public partial class SettingsWindow : Window
{
    private OverlayWindow _overlay;
    private CrosshairConfig _config;
    private bool _loading = true;

    public SettingsWindow()
    {
        InitializeComponent();
        _config = ProfileManager.Load();
        _overlay = new OverlayWindow(_config);
        _overlay.Show();
        LoadUI();
        _loading = false;
        UpdateOverlay();
    }

    private void LoadUI()
    {
        GapSlider.Value = _config.Gap;
        LengthSlider.Value = _config.Length;
        ThicknessSlider.Value = _config.Thickness;
        OpacitySlider.Value = _config.Opacity;
        CenterDotCheck.IsChecked = _config.ShowCenterDot;
        OutlineCheck.IsChecked = _config.Outline;
        ColorBox.Text = $"{_config.Color.R}, {_config.Color.G}, {_config.Color.B}";
    }

    private void UpdateOverlay()
    {
        if (_loading) return;
        _config.Gap = GapSlider.Value;
        _config.Length = LengthSlider.Value;
        _config.Thickness = ThicknessSlider.Value;
        _config.Opacity = OpacitySlider.Value;
        _config.ShowCenterDot = CenterDotCheck.IsChecked ?? false;
        _config.Outline = OutlineCheck.IsChecked ?? false;
        
        var colorParts = ColorBox.Text.Split(',');
        if (colorParts.Length == 3 && 
            byte.TryParse(colorParts[0].Trim(), out byte r) && 
            byte.TryParse(colorParts[1].Trim(), out byte g) && 
            byte.TryParse(colorParts[2].Trim(), out byte b))
        {
            _config.Color = Color.FromRgb(r, g, b);
        }

        ProfileManager.Save(_config);
        _overlay.RedrawCrosshair();
    }

    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdateOverlay();
    private void Check_Changed(object sender, RoutedEventArgs e) => UpdateOverlay();
    private void ColorBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => UpdateOverlay();

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _overlay.Close();
    }
}
