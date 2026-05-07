using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CrosshairOverlay.Helpers;

public static class MouseHook
{
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_RBUTTONUP = 0x0205;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_MBUTTONUP = 0x0208;
    private const int WM_XBUTTONDOWN = 0x020B;
    private const int WM_XBUTTONUP = 0x020C;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    
    private static LowLevelMouseProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    public static event Action<string>? OnMouseDownAction;
    public static event Action<string>? OnMouseUpAction;

    public static void Start()
    {
        _hookID = SetHook(_proc);
    }

    public static void Stop()
    {
        UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(LowLevelMouseProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        {
            var curModule = curProcess.MainModule;
            if (curModule == null) return IntPtr.Zero;
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName ?? ""), 0);
        }
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            if (wParam == (IntPtr)WM_RBUTTONDOWN) OnMouseDownAction?.Invoke("RightButton");
            else if (wParam == (IntPtr)WM_RBUTTONUP) OnMouseUpAction?.Invoke("RightButton");
            else if (wParam == (IntPtr)WM_LBUTTONDOWN) OnMouseDownAction?.Invoke("LeftButton");
            else if (wParam == (IntPtr)WM_LBUTTONUP) OnMouseUpAction?.Invoke("LeftButton");
            else if (wParam == (IntPtr)WM_MBUTTONDOWN) OnMouseDownAction?.Invoke("MiddleButton");
            else if (wParam == (IntPtr)WM_MBUTTONUP) OnMouseUpAction?.Invoke("MiddleButton");
            else if (wParam == (IntPtr)WM_XBUTTONDOWN)
            {
                int mouseData = Marshal.ReadInt32(lParam + 8);
                string btn = (mouseData >> 16) == 1 ? "XButton1" : "XButton2";
                OnMouseDownAction?.Invoke(btn);
            }
            else if (wParam == (IntPtr)WM_XBUTTONUP)
            {
                int mouseData = Marshal.ReadInt32(lParam + 8);
                string btn = (mouseData >> 16) == 1 ? "XButton1" : "XButton2";
                OnMouseUpAction?.Invoke(btn);
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }
}
