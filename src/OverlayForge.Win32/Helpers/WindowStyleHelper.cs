using System.Windows;
using System.Windows.Interop;
using OverlayForge.Win32.Interop;

namespace OverlayForge.Win32.Helpers;

/// <summary>
/// Extension methods for applying Win32 window styles to WPF windows.
/// </summary>
public static class WindowStyleHelper
{
    /// <summary>
    /// Makes a WPF window an always-on-top overlay with no taskbar button.
    /// Applies WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE.
    /// </summary>
    public static void MakeOverlayWindow(Window window)
    {
        var hwnd = GetHwnd(window);
        if (hwnd == IntPtr.Zero) return;

        var exStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE).ToInt64();
        exStyle |= NativeMethods.WS_EX_LAYERED;
        exStyle |= NativeMethods.WS_EX_TOOLWINDOW;
        exStyle |= NativeMethods.WS_EX_NOACTIVATE;
        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE, new IntPtr(exStyle));

        // Force topmost via SetWindowPos
        NativeMethods.SetWindowPos(
            hwnd,
            NativeMethods.HWND_TOPMOST,
            0, 0, 0, 0,
            NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
    }

    /// <summary>
    /// Enables or disables click-through mode (WS_EX_TRANSPARENT).
    /// </summary>
public static void SetClickThrough(Window window, bool isClickThrough)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero) return;

        // 1. Get the current extended style of the window
        int exStyle = (int)NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE);

        if (isClickThrough)
        {
            // 2a. Add Layered and Transparent flags to make it click-through
            exStyle |= NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TRANSPARENT;
        }
        else
        {
            // 2b. Remove the Transparent flag so it catches clicks again
            exStyle &= ~NativeMethods.WS_EX_TRANSPARENT;
        }

        // 3. Apply the new style
        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GWL_EXSTYLE, new IntPtr(exStyle));

        // CRITICAL FIX: Do NOT call SetLayeredWindowAttributes here!
        // We let WPF handle the actual opacity rendering natively.
    }

    /// <summary>
    /// Sets window opacity using SetLayeredWindowAttributes.
    /// </summary>
    public static void SetWindowOpacity(Window window, double opacity)
    {
        var hwnd = GetHwnd(window);
        if (hwnd == IntPtr.Zero) return;

        // Clamp to valid range
        opacity = Math.Clamp(opacity, 0.0, 1.0);
        byte alpha = (byte)(opacity * 255);

        NativeMethods.SetLayeredWindowAttributes(hwnd, 0, alpha, NativeMethods.LWA_ALPHA);
    }

    /// <summary>
    /// Applies dark mode title bar (Windows 11+).
    /// </summary>
    public static void SetDarkMode(Window window, bool enabled)
    {
        var hwnd = GetHwnd(window);
        if (hwnd == IntPtr.Zero) return;

        int value = enabled ? 1 : 0;
        NativeMethods.DwmSetWindowAttribute(
            hwnd,
            NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE,
            ref value,
            sizeof(int));
    }

    /// <summary>
    /// Gets the Win32 handle for a WPF window.
    /// </summary>
    public static IntPtr GetHwnd(Window window)
    {
        var helper = new WindowInteropHelper(window);
        return helper.EnsureHandle();
    }
}
