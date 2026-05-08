using System;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using CrosshairOverlay.Models;
using Microsoft.Win32;

namespace CrosshairOverlay;

public partial class SettingsWindow : Window
{
    private OverlayWindow _overlay;
    private AppState _appState;
    public CrosshairConfig _config;
    private bool _loading = true;
    private bool _isBinding = false;

    private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private bool _forceExit = false;

    public SettingsWindow()
    {
        InitializeComponent();
        SetupTrayIcon();
        
        _appState = ProfileManager.Load();
        if (!_appState.Profiles.ContainsKey(_appState.ActiveProfile)) 
            _appState.ActiveProfile = _appState.Profiles.Keys.First();

        _config = _appState.Profiles[_appState.ActiveProfile];
        _overlay = new OverlayWindow(this);
        _overlay.Show();
        
        InitPixelGrid();
        PopulateProfiles();
        _loading = false;
        UpdateOverlay();
        
        System.Windows.Application.Current.SessionEnding += (s, ev) => ProfileManager.Save(_appState);
    }

    public AppState GetAppState() => _appState;

    private void SetupTrayIcon()
    {
        _notifyIcon = new System.Windows.Forms.NotifyIcon();
        try {
            _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
        } catch {
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
        }
        _notifyIcon.Visible = true;
        _notifyIcon.Text = "Crosshair Overlay";

        _notifyIcon.DoubleClick += (s, args) => 
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        };

        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("Open Settings", null, (s, a) => { 
            this.Show(); 
            this.WindowState = WindowState.Normal; 
            this.Activate(); 
        });
        menu.Items.Add("Exit", null, (s, a) => { 
            _forceExit = true;
            _notifyIcon.Visible = false; 
            _notifyIcon.ContextMenuStrip?.Dispose();
            _notifyIcon.Dispose();
            this.Close(); 
        });
        _notifyIcon.ContextMenuStrip = menu;
    }

    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            this.Hide();
            ProfileManager.Save(_appState);
        }
        base.OnStateChanged(e);
    }

    private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left) DragMove();
    }

    private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        _forceExit = true;
        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
        this.Close();
    }

    private void InitPixelGrid()
    {
        for (int i = 0; i < 256; i++)
        {
            var b = new Border 
            { 
                Background = Brushes.Transparent, 
                BorderBrush = new SolidColorBrush(Color.FromArgb(10, 255, 255, 255)), 
                BorderThickness = new Thickness(0.5) 
            };
            b.Tag = i;
            PixelGrid.Children.Add(b);
        }
    }

    private void PixelGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
        {
            if (e.OriginalSource is Border el && el.Tag != null)
            {
                int idx = (int)el.Tag;
                bool fill = e.LeftButton == MouseButtonState.Pressed;
                el.Background = fill ? new SolidColorBrush(_config.Color) : Brushes.Transparent;
                
                char[] chars = _config.PixelGridMatrix.PadRight(256, '0').ToCharArray();
                if (idx < chars.Length) chars[idx] = fill ? '1' : '0';
                _config.PixelGridMatrix = new string(chars);
                UpdateOverlay();
            }
        }
    }
    
    private void ClearGridBtn_Click(object sender, RoutedEventArgs e)
    {
        _config.PixelGridMatrix = new string('0', 256);
        LoadUI();
        UpdateOverlay();
    }

    private void PopulateProfiles()
    {
        _loading = true;
        ProfileCombo.Items.Clear();
        foreach (var key in _appState.Profiles.Keys) ProfileCombo.Items.Add(key);
        ProfileCombo.SelectedItem = _appState.ActiveProfile;
        _loading = false;
        LoadUI();
    }

    public void SwitchToProfile(string profileName)
    {
        if (_appState.Profiles.TryGetValue(profileName, out var newConf))
        {
            _loading = true;
            _appState.ActiveProfile = profileName;
            _config = newConf;
            _overlay.Config = _config;
            ProfileCombo.SelectedItem = profileName;
            LoadUI();
            _loading = false;
            UpdateOverlay();
        }
    }

    public void SwitchToNextProfile()
    {
        var keys = _appState.Profiles.Keys.ToList();
        if (keys.Count <= 1) return;
        int idx = keys.IndexOf(_appState.ActiveProfile);
        idx = (idx + 1) % keys.Count;
        SwitchToProfile(keys[idx]);
    }

    private void LoadUI()
    {
        GapSlider.Value = _config.Gap;
        LengthSlider.Value = _config.Length;
        ThicknessSlider.Value = _config.Thickness;
        DotSizeSlider.Value = _config.DotSize;
        OpacitySlider.Value = _config.Opacity;
        OffsetXSlider.Value = _config.OffsetX;
        OffsetYSlider.Value = _config.OffsetY;
        
        CenterDotCheck.IsChecked = _config.ShowCenterDot;
        OutlineCheck.IsChecked = _config.Outline;
        TShapeCheck.IsChecked = _config.TShape;
        SmartHideCheck.IsChecked = _config.SmartHide;
        AutoHideCheck.IsChecked = _config.AutoHide;
        InvertColorsCheck.IsChecked = _config.InvertColors;

        TargetProcInput.Text = _config.TargetProcesses;
        ColorPickerControl.SelectedColor = _config.Color;
        OutlineColorPickerControl.SelectedColor = _config.OutlineColor;
        
        RgbChromaCheck.IsChecked = _config.RgbChroma;
        RgbSpeedSlider.Value = _config.RgbSpeed;

        if (_config.PixelGridMatrix == null || _config.PixelGridMatrix.Length < 256) 
            _config.PixelGridMatrix = new string('0', 256);
        else if (_config.PixelGridMatrix.Length > 256)
            _config.PixelGridMatrix = _config.PixelGridMatrix.Substring(0, 256);
            
        for(int i = 0; i < 256; i++) {
            if (PixelGrid.Children[i] is Border b) {
                b.Background = _config.PixelGridMatrix[i] == '1' ? new SolidColorBrush(_config.Color) : Brushes.Transparent;
            }
        }

        BindButton.Content = _config.SmartHideBind;
        PanicBindButton.Content = _config.PanicKeyBind;
        SwapBindButton.Content = _config.SwapProfileBind;

        ShapeCombo.SelectedIndex = _config.ShapeType >= 0 && _config.ShapeType <= 5 ? _config.ShapeType : 0;
        
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
        else if (shape == 5) LengthText.Text = "Grid Scale (Size)";

        ThicknessText.Visibility = showLength && shape != 4 && shape != 5 ? Visibility.Visible : Visibility.Collapsed;
        ThicknessSlider.Visibility = showLength && shape != 4 && shape != 5 ? Visibility.Visible : Visibility.Collapsed;

        bool isDotOnly = shape == 1;
        CenterDotCheck.Visibility = isDotOnly || shape == 4 || shape == 5 ? Visibility.Collapsed : Visibility.Visible;
        
        bool showDotSize = (CenterDotCheck.IsChecked == true && shape != 4 && shape != 5) || isDotOnly;
        DotSizeText.Visibility = showDotSize ? Visibility.Visible : Visibility.Collapsed;
        DotSizeSlider.Visibility = showDotSize ? Visibility.Visible : Visibility.Collapsed;

        BrowseImageBtn.Visibility = shape == 4 ? Visibility.Visible : Visibility.Collapsed;
        PixelGridBorder.Visibility = shape == 5 ? Visibility.Visible : Visibility.Collapsed;
        ClearGridBtn.Visibility = shape == 5 ? Visibility.Visible : Visibility.Collapsed;
        
        bool autoHide = AutoHideCheck.IsChecked == true;
        TargetProcText.Visibility = autoHide ? Visibility.Visible : Visibility.Collapsed;
        TargetProcInput.Visibility = autoHide ? Visibility.Visible : Visibility.Collapsed;
        
        bool isChroma = RgbChromaCheck.IsChecked == true;
        RgbSpeedText.Visibility = isChroma ? Visibility.Visible : Visibility.Collapsed;
        RgbSpeedSlider.Visibility = isChroma ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateOverlay()
    {
        if (_loading) return;
        _config.Gap = GapSlider.Value;
        _config.Length = LengthSlider.Value;
        _config.Thickness = ThicknessSlider.Value;
        _config.DotSize = DotSizeSlider.Value;
        _config.Opacity = OpacitySlider.Value;
        _config.OffsetX = OffsetXSlider.Value;
        _config.OffsetY = OffsetYSlider.Value;
        
        bool isDotOnly = ShapeCombo.SelectedIndex == 1;
        _config.ShowCenterDot = isDotOnly ? true : (CenterDotCheck.IsChecked ?? false);
        
        _config.Outline = OutlineCheck.IsChecked ?? false;
        _config.TShape = TShapeCheck.IsChecked ?? false;
        _config.SmartHide = SmartHideCheck.IsChecked ?? false;
        _config.AutoHide = AutoHideCheck.IsChecked ?? false;
        _config.InvertColors = InvertColorsCheck.IsChecked ?? false;
        _config.TargetProcesses = TargetProcInput.Text;
        
        _config.RgbChroma = RgbChromaCheck.IsChecked ?? false;
        _config.RgbSpeed = RgbSpeedSlider.Value;
        if (OutlineColorPickerControl.SelectedColor.HasValue) _config.OutlineColor = OutlineColorPickerControl.SelectedColor.Value;
        
        _config.SmartHideBind = BindButton.Content.ToString() ?? "RightButton";
        _config.PanicKeyBind = PanicBindButton.Content.ToString() ?? "F8";
        _config.SwapProfileBind = SwapBindButton.Content.ToString() ?? "F9";
        
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
    private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateOverlay();
    private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e) => UpdateOverlay();
    private void TextInput_Changed(object sender, TextChangedEventArgs e) => UpdateOverlay();

    private void ProfileCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || ProfileCombo.SelectedItem == null) return;
        SwitchToProfile(ProfileCombo.SelectedItem.ToString() ?? "Default");
    }

    private void NewProfileBtn_Click(object sender, RoutedEventArgs e)
    {
        string name = NewProfileNameInput.Text.Trim();
        if (string.IsNullOrEmpty(name) || _appState.Profiles.ContainsKey(name)) return;
        
        var cloneJson = JsonSerializer.Serialize(_config);
        var cloned = JsonSerializer.Deserialize<CrosshairConfig>(cloneJson) ?? new CrosshairConfig();
        
        _appState.Profiles[name] = cloned;
        PopulateProfiles();
        SwitchToProfile(name);
    }

    private void DelProfileBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_appState.Profiles.Count <= 1) return;
        _appState.Profiles.Remove(_appState.ActiveProfile);
        _appState.ActiveProfile = _appState.Profiles.Keys.First();
        PopulateProfiles();
        SwitchToProfile(_appState.ActiveProfile);
    }

    private void BindButton_Click(object sender, RoutedEventArgs e)
    {
        _isBinding = true;
        BindButton.Content = "Press any key...";
    }
    
    private void PanicBindButton_Click(object sender, RoutedEventArgs e)
    {
        _isBinding = true;
        PanicBindButton.Content = "Press any key...";
    }

    private void SwapBindButton_Click(object sender, RoutedEventArgs e)
    {
        _isBinding = true;
        SwapBindButton.Content = "Press any key...";
    }

    private void BrowseImage_Click(object sender, RoutedEventArgs e)
    {
        var fd = new Microsoft.Win32.OpenFileDialog { Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All Files (*.*)|*.*" };
        if (fd.ShowDialog() == true)
        {
            _config.CustomImagePath = fd.FileName;
            UpdateOverlay();
        }
    }

    protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        if (_isBinding)
        {
            e.Handled = true;
            _isBinding = false;
            
            if (BindButton.Content.ToString() == "Press any key...") BindButton.Content = e.Key.ToString();
            else if (PanicBindButton.Content.ToString() == "Press any key...") PanicBindButton.Content = e.Key.ToString();
            else if (SwapBindButton.Content.ToString() == "Press any key...") SwapBindButton.Content = e.Key.ToString();
            
            UpdateOverlay();
        }
        base.OnPreviewKeyDown(e);
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        if (_isBinding)
        {
            e.Handled = true;
            _isBinding = false;
            var val = e.ChangedButton.ToString() + "Button";
            
            if (BindButton.Content.ToString() == "Press any key...") BindButton.Content = val;
            else if (PanicBindButton.Content.ToString() == "Press any key...") PanicBindButton.Content = val;
            else if (SwapBindButton.Content.ToString() == "Press any key...") SwapBindButton.Content = val;
            
            UpdateOverlay();
        }
        base.OnPreviewMouseDown(e);
    }

    private void ExportBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var exportConfig = JsonSerializer.Deserialize<CrosshairConfig>(JsonSerializer.Serialize(_config));
            if (exportConfig != null) exportConfig.CustomImagePath = "";
            var json = JsonSerializer.Serialize(exportConfig);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var b64 = Convert.ToBase64String(bytes);
            System.Windows.Clipboard.SetText("CHX-" + b64);
            System.Windows.MessageBox.Show("Profile code copied to clipboard!\n(Note: Custom images cannot be dynamically shared).", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch { }
    }

    private void ImportBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var text = System.Windows.Clipboard.GetText();
            if (text.StartsWith("CHX-"))
            {
                var b64 = text.Substring(4);
                var bytes = Convert.FromBase64String(b64);
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                var imported = JsonSerializer.Deserialize<CrosshairConfig>(json);
                if (imported != null)
                {
                    _config = imported;
                    if (_config.PixelGridMatrix == null || _config.PixelGridMatrix.Length < 256) _config.PixelGridMatrix = new string('0', 256);
                    _appState.Profiles[_appState.ActiveProfile] = _config;
                    _overlay.Config = _config;
                    _loading = true;
                    LoadUI();
                    _loading = false;
                    UpdateOverlay();
                    System.Windows.MessageBox.Show("Profile successfully imported!", "Import Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch { }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_forceExit)
        {
            e.Cancel = true;
            this.Hide();
        }
        else
        {
            ProfileManager.Save(_appState);
            _overlay.Close();
        }
    }
}
