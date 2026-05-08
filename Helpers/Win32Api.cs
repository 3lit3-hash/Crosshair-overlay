using System;
using System.Runtime.InteropServices;

namespace CrosshairOverlay.Helpers;

public static class Win32Api
{
    public const int WS_EX_TRANSPARENT = 0x00000020;
    public const int GWL_EXSTYLE = -20;

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [StructLayout(LayoutMode.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

    public static System.Windows.Media.Color HsvToRgb(double h, double S, double V)
    {
        double step = h / 60.0;
        int i = (int)Math.Floor(step);
        double f = step - i;
        double p = V * (1.0 - S);
        double q = V * (1.0 - S * f);
        double t = V * (1.0 - S * (1.0 - f));
        double R = 0, G = 0, B = 0;
        switch (i % 6)
        {
            case 0: R = V; G = t; B = p; break;
            case 1: R = q; G = V; B = p; break;
            case 2: R = p; G = V; B = t; break;
            case 3: R = p; G = q; B = V; break;
            case 4: R = t; G = p; B = V; break;
            case 5: R = V; G = p; B = q; break;
        }
        return System.Windows.Media.Color.FromRgb((byte)(R * 255), (byte)(G * 255), (byte)(B * 255));
    }
}
