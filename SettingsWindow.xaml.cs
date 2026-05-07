using System;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CrosshairOverlay.Models;
using Microsoft.Win32;

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
        AutoHideCheck.IsChecked = _config.AutoHide;
        TargetProcInput.Text = _config.TargetProcesses;
        ColorPickerControl.SelectedColor = _config.Color;
        BindButton.Content = _config.SmartHideBind;
        ShapeCombo.SelectedIndex = _config.ShapeType >= 0 && _config.ShapeType <= 4 ? _config.ShapeType : 0;
        
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
        else if (shape == 4) LengthText.Text = "Image Scale";

        ThicknessText.Visibility = showLength && shape != 4 ? Visibility.Visible : Visibility.Collapsed;
        ThicknessSlider.Visibility = showLength && shape != 4 ? Visibility.Visible : Visibility.Collapsed;

        bool isDotOnly = shape == 1;
        CenterDotCheck.Visibility = isDotOnly || shape == 4 ? Visibility.Collapsed : Visibility.Visible;
        
        bool showDotSize = (CenterDotCheck.IsChecked == true && shape != 4) || isDotOnly;
        DotSizeText.Visibility = showDotSize ? Visibility.Visible : Visibility.Collapsed;
        DotSizeSlider.Visibility = showDotSize ? Visibility.Visible : Visibility.Collapsed;

        BrowseImageBtn.Visibility = shape == 4 ? Visibility.Visible : Visibility.Collapsed;
        
        bool autoHide = AutoHideCheck.IsChecked == true;
        TargetProcText.Visibility = autoHide ? Visibility.Visible : Visibility.Collapsed;
        TargetProcInput.Visibility = autoHide ? Visibility.Visible : Visibility.Collapsed;
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
        _config.AutoHide = AutoHideCheck.IsChecked ?? false;
        _config.TargetProcesses = TargetProcInput.Text;
        _config.SmartHideBind = BindButton.Content.ToString() ?? "RightButton";
        _config.ShapeType = ShapeCombo?.SelectedIndex ?? 0;
        
        if (ColorPickerControl.SelectedColor.HasValue)
        {
            _config.Color = ColorPickerControl.SelectedColor.Value;
        }

        UpdateVisibility();
        _overlay.RedrawCrosshair();
    }

    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdateOverlay();
    
    private void Check_Changed(object sender, RoutedEventArgs e) => UpdateOverlay();
    
    private void Combo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => UpdateOverlay();
    
    private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) => UpdateOverlay();

    private void TextInput_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e) => UpdateOverlay();

    private void BindButton_Click(object sender, RoutedEventArgs e)
    {
        _isBinding = true;
        BindButton.Content = "Press any key...";
    }

    private void BrowseImage_Click(object sender, RoutedEventArgs e)
    {
        var fd = new OpenFileDialog { Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All Files (*.*)|*.*" };
        if (fd.ShowDialog() == true)
        {
            _config.CustomImagePath = fd.FileName;
            UpdateOverlay();
        }
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

    private void ExportBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var json = JsonSerializer.Serialize(_config);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var b64 = Convert.ToBase64String(bytes);
            Clipboard.SetText("CHX-" + b64);
            MessageBox.Show("Profile code copied to clipboard!", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch { }
    }

    private void ImportBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var text = Clipboard.GetText();
            if (text.StartsWith("CHX-"))
            {
                var b64 = text.Substring(4);
                var bytes = Convert.FromBase64String(b64);
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                var imported = JsonSerializer.Deserialize<CrosshairConfig>(json);
                if (imported != null)
                {
                    _config = imported;
                    _overlay.Config = _config;
                    _loading = true;
                    LoadUI();
                    _loading = false;
                    UpdateOverlay();
                    MessageBox.Show("Profile successfully imported!", "Import Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch { }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        ProfileManager.Save(_config);
        _overlay.Close();
    }
}
