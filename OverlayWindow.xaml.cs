using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using CrosshairOverlay.Helpers;
using CrosshairOverlay.Models;

namespace CrosshairOverlay;

public partial class OverlayWindow : Window
{
    public CrosshairConfig Config { get; set; }
    private double _currentSpread = 0;
    private bool _isShooting = false;
    private DispatcherTimer _timer;

    public OverlayWindow(CrosshairConfig config)
    {
        InitializeComponent();
        Config = config;
        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;
        Left = 0;
        Top = 0;

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += Timer_Tick;
        _timer.Start();

        MouseHook.OnLeftDown += () => _isShooting = true;
        MouseHook.OnLeftUp += () => _isShooting = false;
        MouseHook.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_isShooting)
        {
            _currentSpread += 2.0;
            if (_currentSpread > 30) _currentSpread = 30;
        }
        else
        {
            _currentSpread -= 2.0;
            if (_currentSpread < 0) _currentSpread = 0;
        }

        RedrawCrosshair();
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

        var brush = new SolidColorBrush(Config.Color) { Opacity = Config.Opacity };
        var outlineBrush = new SolidColorBrush(Colors.Black) { Opacity = Config.Opacity };

        var centerX = Width / 2;
        var centerY = Height / 2;

        if (Config.ShowCenterDot)
        {
            if (Config.Outline) DrawRect(centerX - Config.DotSize / 2 - 1, centerY - Config.DotSize / 2 - 1, Config.DotSize + 2, Config.DotSize + 2, outlineBrush);
            DrawRect(centerX - Config.DotSize / 2, centerY - Config.DotSize / 2, Config.DotSize, Config.DotSize, brush);
        }

        var t = Config.Thickness;
        var l = Config.Length;
        var g = Config.Gap + _currentSpread;

        DrawLine(centerX - g - l, centerY, centerX - g, centerY, t, brush, Config.Outline, outlineBrush);
        DrawLine(centerX + g, centerY, centerX + g + l, centerY, t, brush, Config.Outline, outlineBrush);
        DrawLine(centerX, centerY - g - l, centerX, centerY - g, t, brush, Config.Outline, outlineBrush);
        DrawLine(centerX, centerY + g, centerX, centerY + g + l, t, brush, Config.Outline, outlineBrush);
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
        base.OnClosed(e);
    }
}
