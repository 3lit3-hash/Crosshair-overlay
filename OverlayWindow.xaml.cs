using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using CrosshairOverlay.Helpers;
using CrosshairOverlay.Models;

namespace CrosshairOverlay;

public partial class OverlayWindow : Window
{
    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    public static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

    public CrosshairConfig Config { get; set; }
    private SettingsWindow _settings;
    
    private bool _isSmartHidden = false;
    private bool _isHiddenByProcess = false;
    private bool _isPanicHidden = false;
    private DispatcherTimer _processTimer;
    private DispatcherTimer _invertTimer;

    public OverlayWindow(SettingsWindow settings)
    {
        InitializeComponent();
        _settings = settings;
        Config = _settings._config;

        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;
        Left = 0;
        Top = 0;

        MouseHook.OnMouseDownAction += (btn) => 
        {
            if (Config.SmartHide && btn == Config.SmartHideBind) { _isSmartHidden = true; UpdateVisibility(); }
        };
        MouseHook.OnMouseUpAction += (btn) => 
        {
            if (Config.SmartHide && btn == Config.SmartHideBind) { _isSmartHidden = false; UpdateVisibility(); }
        };
        KeyboardHook.OnKeyDownAction += (key) => 
        {
            if (Config.SmartHide && key == Config.SmartHideBind) { _isSmartHidden = true; UpdateVisibility(); }
            
            if (key == Config.PanicKeyBind) 
            {
                _isPanicHidden = !_isPanicHidden;
                UpdateVisibility();
            }

            if (key == Config.SwapProfileBind)
            {
                _settings.Dispatcher.Invoke(() => _settings.SwitchToNextProfile());
            }
        };
        KeyboardHook.OnKeyUpAction += (key) => 
        {
            if (Config.SmartHide && key == Config.SmartHideBind) { _isSmartHidden = false; UpdateVisibility(); }
        };
        
        MouseHook.Start();
        KeyboardHook.Start();

        _processTimer = new DispatcherTimer();
        _processTimer.Interval = TimeSpan.FromMilliseconds(500);
        _processTimer.Tick += ProcessTimer_Tick;
        _processTimer.Start();

        _invertTimer = new DispatcherTimer();
        _invertTimer.Interval = TimeSpan.FromMilliseconds(50);
        _invertTimer.Tick += InvertTimer_Tick;
        _invertTimer.Start();
    }

    private void ProcessTimer_Tick(object? sender, EventArgs e)
    {
        IntPtr hwnd = Win32Api.GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return;

        Win32Api.GetWindowThreadProcessId(hwnd, out uint pid);
        if (pid == 0) return;

        string activeName = "";
        try
        {
            var proc = System.Diagnostics.Process.GetProcessById((int)pid);
            activeName = proc.ProcessName.ToLower();
        }
        catch { }

        try
        {
            bool foundGameProfile = false;
            foreach (var kvp in _settings.GetAppState().Profiles)
            {
                var pt = kvp.Value.TargetProcesses.ToLower().Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var tg in pt)
                {
                    if (activeName.Contains(tg.Trim()))
                    {
                        if (_settings.GetAppState().ActiveProfile != kvp.Key)
                        {
                            _settings.Dispatcher.Invoke(() => _settings.SwitchToProfile(kvp.Key));
                        }
                        foundGameProfile = true;
                        break;
                    }
                }
                if (foundGameProfile) break;
            }

            var activeTargets = Config.TargetProcesses.ToLower().Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            bool match = false;
            foreach (var t in activeTargets)
            {
                if (activeName.Contains(t.Trim()))
                {
                    match = true;
                    break;
                }
            }

            if (match)
            {
                if (Win32Api.GetWindowRect(hwnd, out Win32Api.RECT r))
                {
                    if (this.Left != r.Left) this.Left = r.Left;
                    if (this.Top != r.Top) this.Top = r.Top;
                    if (this.Width != r.Width || this.Height != r.Height)
                    {
                        this.Width = r.Width;
                        this.Height = r.Height;
                        RedrawCrosshair();
                    }
                }
            }
            else
            {
                if (!Config.AutoHide)
                {
                    if (this.Left != 0) this.Left = 0;
                    if (this.Top != 0) this.Top = 0;
                    if (this.Width != SystemParameters.PrimaryScreenWidth || this.Height != SystemParameters.PrimaryScreenHeight)
                    {
                        this.Width = SystemParameters.PrimaryScreenWidth;
                        this.Height = SystemParameters.PrimaryScreenHeight;
                        RedrawCrosshair();
                    }
                }
            }

            if (Config.AutoHide)
            {
                if (_isHiddenByProcess == match)
                {
                    _isHiddenByProcess = !match;
                    UpdateVisibility();
                }
            }
            else if (_isHiddenByProcess)
            {
                _isHiddenByProcess = false;
                UpdateVisibility();
            }
        }
        catch { }
    }

    private void InvertTimer_Tick(object? sender, EventArgs e)
    {
        if (!Config.InvertColors || CrosshairCanvas.Visibility != Visibility.Visible) return;
        
        var centerX = (this.Width / 2) + Config.OffsetX;
        var centerY = (this.Height / 2) + Config.OffsetY;

        IntPtr hdc = GetDC(IntPtr.Zero);
        uint pixel = GetPixel(hdc, (int)(this.Left + centerX), (int)(this.Top + centerY));
        ReleaseDC(IntPtr.Zero, hdc);

        byte r = (byte)(pixel & 0x000000FF);
        byte g = (byte)((pixel & 0x0000FF00) >> 8);
        byte b = (byte)((pixel & 0x00FF0000) >> 16);

        var inverted = Color.FromRgb((byte)(255 - r), (byte)(255 - g), (byte)(255 - b));
        
        foreach (var child in CrosshairCanvas.Children) {
            if (child is Shape s && s.Stroke != null) s.Stroke = new SolidColorBrush(inverted) { Opacity = Config.Opacity };
            if (child is Shape s2 && s2.Fill != null && s2.Fill != Brushes.Transparent) s2.Fill = new SolidColorBrush(inverted) { Opacity = Config.Opacity };
        }
    }

    private void UpdateVisibility()
    {
        if (_isHiddenByProcess || _isSmartHidden || _isPanicHidden)
            CrosshairCanvas.Visibility = Visibility.Hidden;
        else
            CrosshairCanvas.Visibility = Visibility.Visible;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var extendedStyle = Win32Api.GetWindowLong(hwnd, Win32Api.GWL_EXSTYLE);
        Win32Api.SetWindowLong(hwnd, Win32Api.GWL_EXSTYLE, extendedStyle | Win32Api.WS_EX_TRANSPARENT);
    }

    public void RedrawCrosshair()
    {
        CrosshairCanvas.Children.Clear();
        UpdateVisibility();

        var brush = new SolidColorBrush(Config.Color) { Opacity = Config.Opacity };
        var outlineBrush = new SolidColorBrush(Colors.Black) { Opacity = Config.Opacity };

        var centerX = (Width / 2) + Config.OffsetX;
        var centerY = (Height / 2) + Config.OffsetY;

        if (Config.ShowCenterDot && Config.ShapeType != 4)
        {
            if (Config.Outline) DrawRect(centerX - Config.DotSize / 2 - 1, centerY - Config.DotSize / 2 - 1, Config.DotSize + 2, Config.DotSize + 2, outlineBrush);
            DrawRect(centerX - Config.DotSize / 2, centerY - Config.DotSize / 2, Config.DotSize, Config.DotSize, brush);
        }

        var t = Config.Thickness;
        var l = Config.Length;
        var g = Config.Gap;

        if (Config.ShapeType == 0)
        {
            DrawLine(centerX - g - l, centerY, centerX - g, centerY, t, brush, Config.Outline, outlineBrush);
            DrawLine(centerX + g, centerY, centerX + g + l, centerY, t, brush, Config.Outline, outlineBrush);
            if (!Config.TShape)
            {
                DrawLine(centerX, centerY - g - l, centerX, centerY - g, t, brush, Config.Outline, outlineBrush);
            }
            DrawLine(centerX, centerY + g, centerX, centerY + g + l, t, brush, Config.Outline, outlineBrush);
        }
        else if (Config.ShapeType == 2)
        {
            DrawCircle(centerX, centerY, l, t, brush, Config.Outline, outlineBrush);
        }
        else if (Config.ShapeType == 3)
        {
            DrawTriangle(centerX, centerY, l, t, brush, Config.Outline, outlineBrush);
        }
        else if (Config.ShapeType == 4 && !string.IsNullOrEmpty(Config.CustomImagePath))
        {
            try
            {
                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(Config.CustomImagePath, UriKind.Absolute);
                bitmap.EndInit();

                var x = centerX - (l / 2);
                var y = centerY - (l / 2);

                var img = new Image { Source = bitmap, Width = l, Height = l, Stretch = Stretch.Uniform, Opacity = Config.Opacity };
                Canvas.SetLeft(img, x);
                Canvas.SetTop(img, y);
                CrosshairCanvas.Children.Add(img);
            }
            catch { }
        }
    }

    private void DrawCircle(double x, double y, double radius, double thickness, Brush brush, bool outl, Brush ob)
    {
        if (outl)
        {
            var outline = new Ellipse { Width = radius * 2, Height = radius * 2, Stroke = ob, StrokeThickness = thickness + 2 };
            Canvas.SetLeft(outline, x - radius);
            Canvas.SetTop(outline, y - radius);
            CrosshairCanvas.Children.Add(outline);
        }
        var ellipse = new Ellipse { Width = radius * 2, Height = radius * 2, Stroke = brush, StrokeThickness = thickness };
        Canvas.SetLeft(ellipse, x - radius);
        Canvas.SetTop(ellipse, y - radius);
        CrosshairCanvas.Children.Add(ellipse);
    }

    private void DrawTriangle(double cx, double cy, double size, double thickness, Brush brush, bool outl, Brush ob)
    {
        var points = new PointCollection
        {
            new Point(cx, cy - size),
            new Point(cx - size, cy + size),
            new Point(cx + size, cy + size)
        };

        if (outl)
        {
            var outline = new Polygon { Points = points, Stroke = ob, StrokeThickness = thickness + 2, Fill = Brushes.Transparent, StrokeLineJoin = PenLineJoin.Round };
            CrosshairCanvas.Children.Add(outline);
        }
        var triangle = new Polygon { Points = points, Stroke = brush, StrokeThickness = thickness, Fill = Brushes.Transparent, StrokeLineJoin = PenLineJoin.Round };
        CrosshairCanvas.Children.Add(triangle);
    }

    private void DrawLine(double x1, double y1, double x2, double y2, double thickness, Brush brush, bool outl, Brush ob)
    {
        if (outl)
        {
            var outline = new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = ob, StrokeThickness = thickness + 2 };
            CrosshairCanvas.Children.Add(outline);
        }
        var line = new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = brush, StrokeThickness = thickness };
        CrosshairCanvas.Children.Add(line);
    }

    private void DrawRect(double x, double y, double w, double h, Brush brush)
    {
        var rect = new Rectangle { Width = w, Height = h, Fill = brush };
        Canvas.SetLeft(rect, x);
        Canvas.SetTop(rect, y);
        CrosshairCanvas.Children.Add(rect);
    }

    protected override void OnClosed(EventArgs e)
    {
        MouseHook.Stop();
        KeyboardHook.Stop();
        if (_processTimer != null) _processTimer.Stop();
        if (_invertTimer != null) _invertTimer.Stop();
        base.OnClosed(e);
    }
}
