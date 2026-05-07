using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CrosshairOverlay.Models;

namespace CrosshairOverlay;

public partial class SettingsWindow : Window
{
    private OverlayWindow _overlay;
    private CrosshairConfig _config;
    private bool _loading = true;
    private bool _isBinding = false;

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
        DotSizeSlider.Value = _config.DotSize;
        OpacitySlider.Value = _config.Opacity;
        CenterDotCheck.IsChecked = _config.ShowCenterDot;
        OutlineCheck.IsChecked = _config.Outline;
        TShapeCheck.IsChecked = _config.TShape;
        SmartHideCheck.IsChecked = _config.SmartHide;
        ColorPickerControl.SelectedColor = _config.Color;
        BindButton.Content = _config.SmartHideBind;
        ShapeCombo.SelectedIndex = _config.ShapeType >= 0 && _config.ShapeType <= 3 ? _config.ShapeType : 0;
        
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (_loading) return;
        int shape = ShapeCombo.SelectedIndex;
        
        bool showClassic = shape == 0;
        GapText.Visibility = showClassic ? Visibility.Visible : Visibility.Collapsed;
        GapSlider.Visibility = showClassic ? Visibility.Visible : Visibility.Collapsed;
        TShapeCheck.Visibility = showClassic ? Visibility.Visible : Visibility.Collapsed;

        bool showLength = shape != 1;
        LengthText.Visibility = showLength ? Visibility.Visible : Visibility.Collapsed;
        LengthSlider.Visibility = showLength ? Visibility.Visible : Visibility.Collapsed;
        
        if (shape == 0) LengthText.Text = "Length";
        else if (shape == 2) LengthText.Text = "Radius";
        else if (shape == 3) LengthText.Text = "Size";

        ThicknessText.Visibility = showLength ? Visibility.Visible : Visibility.Collapsed;
        ThicknessSlider.Visibility = showLength ? Visibility.Visible : Visibility.Collapsed;

        bool isDotOnly = shape == 1;
        CenterDotCheck.Visibility = isDotOnly ? Visibility.Collapsed : Visibility.Visible;
        
        bool showDotSize = (CenterDotCheck.IsChecked == true) || isDotOnly;
        DotSizeText.Visibility = showDotSize ? Visibility.Visible : Visibility.Collapsed;
        DotSizeSlider.Visibility = showDotSize ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateOverlay()
    {
        if (_loading) return;
        _config.Gap = GapSlider.Value;
        _config.Length = LengthSlider.Value;
        _config.Thickness = ThicknessSlider.Value;
        _config.DotSize = DotSizeSlider.Value;
        _config.Opacity = OpacitySlider.Value;
        
        bool isDotOnly = ShapeCombo.SelectedIndex == 1;
        _config.ShowCenterDot = isDotOnly ? true : (CenterDotCheck.IsChecked ?? false);
        
        _config.Outline = OutlineCheck.IsChecked ?? false;
        _config.TShape = TShapeCheck.IsChecked ?? false;
        _config.SmartHide = SmartHideCheck.IsChecked ?? false;
        _config.SmartHideBind = BindButton.Content.ToString() ?? "RightButton";
        _config.ShapeType = ShapeCombo?.SelectedIndex ?? 0;
        
        if (ColorPickerControl.SelectedColor.HasValue)
        {
            _config.Color = ColorPickerControl.SelectedColor.Value;
        }

        UpdateVisibility();

        _overlay.RedrawCrosshair();
        if (!_config.SmartHide) 
            _overlay.CrosshairCanvas.Visibility = Visibility.Visible;
    }

    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdateOverlay();
    
    private void Check_Changed(object sender, RoutedEventArgs e) => UpdateOverlay();
    
    private void Combo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => UpdateOverlay();
    
    private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) => UpdateOverlay();

    private void BindButton_Click(object sender, RoutedEventArgs e)
    {
        _isBinding = true;
        BindButton.Content = "Press any key...";
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (_isBinding)
        {
            e.Handled = true;
            _isBinding = false;
            BindButton.Content = e.Key.ToString();
            UpdateOverlay();
        }
        base.OnPreviewKeyDown(e);
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        if (_isBinding && !ReferenceEquals(e.Source, BindButton))
        {
            e.Handled = true;
            _isBinding = false;
            BindButton.Content = e.ChangedButton.ToString() + "Button";
            UpdateOverlay();
        }
        else if (_isBinding && ReferenceEquals(e.Source, BindButton) && e.ChangedButton != MouseButton.Left)
        {
            e.Handled = true;
            _isBinding = false;
            BindButton.Content = e.ChangedButton.ToString() + "Button";
            UpdateOverlay();
        }
        base.OnPreviewMouseDown(e);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        ProfileManager.Save(_config);
        _overlay.Close();
    }
}
