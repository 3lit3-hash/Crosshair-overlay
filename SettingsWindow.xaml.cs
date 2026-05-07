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
        TShapeCheck.IsChecked = _config.TShape;
        SmartHideCheck.IsChecked = _config.SmartHide;
        ColorPickerControl.SelectedColor = _config.Color;
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
        _config.TShape = TShapeCheck.IsChecked ?? false;
        _config.SmartHide = SmartHideCheck.IsChecked ?? false;
        
        if (ColorPickerControl.SelectedColor.HasValue)
        {
            _config.Color = ColorPickerControl.SelectedColor.Value;
        }

        _overlay.RedrawCrosshair();
        if (!_config.SmartHide) 
            _overlay.CrosshairCanvas.Visibility = Visibility.Visible;
    }

    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdateOverlay();
    private void Check_Changed(object sender, RoutedEventArgs e) => UpdateOverlay();
    private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) => UpdateOverlay();

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        ProfileManager.Save(_config);
        _overlay.Close();
    }
}
