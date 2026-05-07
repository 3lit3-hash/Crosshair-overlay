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
}
