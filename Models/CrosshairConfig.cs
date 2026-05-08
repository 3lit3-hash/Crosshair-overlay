using System.Windows.Media;

namespace CrosshairOverlay.Models;

public class CrosshairConfig
{
    public Color Color { get; set; } = Colors.Lime;
    public double Opacity { get; set; } = 1.0;
    public double Thickness { get; set; } = 2.0;
    public double Length { get; set; } = 10.0;
    public double Gap { get; set; } = 5.0;
    public bool ShowCenterDot { get; set; } = true;
    public double DotSize { get; set; } = 2.0;
    public bool Outline { get; set; } = true;
    public bool TShape { get; set; } = false;
    public bool SmartHide { get; set; } = false;
    public string SmartHideBind { get; set; } = "RightButton";
    public int ShapeType { get; set; } = 0;
    public string CustomImagePath { get; set; } = "";
    public string TargetProcesses { get; set; } = "cs2, valorant, r5apex";
    public bool AutoHide { get; set; } = false;

    public double OffsetX { get; set; } = 0;
    public double OffsetY { get; set; } = 0;
    public bool InvertColors { get; set; } = false;
    public string PanicKeyBind { get; set; } = "F8";
    public string SwapProfileBind { get; set; } = "F9";
    
    // Mechanics V5
    public Color OutlineColor { get; set; } = Colors.Black;
    public bool RgbChroma { get; set; } = false;
    public double RgbSpeed { get; set; } = 3.0;
    public string PixelGridMatrix { get; set; } = new string('0', 256);
}
