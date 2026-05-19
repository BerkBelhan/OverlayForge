using System.Runtime.InteropServices;

namespace OverlayForge.Win32.Interop;

/// <summary>
/// P/Invoke declarations for Win32 API functions used by OverlayForge.
/// </summary>
internal static partial class NativeMethods
{
    // ─── Window Styles ───────────────────────────────────────────────────────

    internal const int GWL_EXSTYLE = -20;
    internal const int GWL_STYLE = -16;

    internal const int WS_EX_LAYERED = 0x00080000;
    internal const int WS_EX_TRANSPARENT = 0x00000020;
    internal const int WS_EX_TOOLWINDOW = 0x00000080;
    internal const int WS_EX_NOACTIVATE = 0x08000000;
    internal const int WS_EX_TOPMOST = 0x00000008;

    internal const int WS_POPUP = unchecked((int)0x80000000);
    internal const int WS_VISIBLE = 0x10000000;

    // ─── SetWindowPos flags ──────────────────────────────────────────────────

    internal const int SWP_NOSIZE = 0x0001;
    internal const int SWP_NOMOVE = 0x0002;
    internal const int SWP_NOACTIVATE = 0x0010;
    internal const int SWP_SHOWWINDOW = 0x0040;

    internal static readonly IntPtr HWND_TOPMOST = new(-1);
    internal static readonly IntPtr HWND_NOTOPMOST = new(-2);

    // ─── Window messages ─────────────────────────────────────────────────────

    internal const int WM_HOTKEY = 0x0312;
    internal const int WM_CLOSE = 0x0010;
    internal const int WM_NCHITTEST = 0x0084;
    internal const int WM_NCLBUTTONDOWN = 0x00A1;
    internal const int HTCAPTION = 2;
    internal const int HTTRANSPARENT = -1;

    // ─── Layered window attributes ────────────────────────────────────────────

    internal const int LWA_ALPHA = 0x02;
    internal const int LWA_COLORKEY = 0x01;

    // ─── Hotkey modifiers ─────────────────────────────────────────────────────

    internal const int MOD_ALT = 0x0001;
    internal const int MOD_CONTROL = 0x0002;
    internal const int MOD_SHIFT = 0x0004;
    internal const int MOD_WIN = 0x0008;
    internal const int MOD_NOREPEAT = 0x4000;

    // ─── DWM ──────────────────────────────────────────────────────────────────

    [StructLayout(LayoutKind.Sequential)]
    internal struct MARGINS
    {
        public int Left, Right, Top, Bottom;
    }

    // ─── P/Invoke declarations ────────────────────────────────────────────────

    [LibraryImport("user32.dll", SetLastError = true)]
    internal static partial IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [LibraryImport("user32.dll", SetLastError = true)]
    internal static partial IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X, int Y,
        int cx, int cy,
        uint uFlags);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool RegisterHotKey(
        IntPtr hWnd,
        int id,
        uint fsModifiers,
        uint vk);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    [LibraryImport("user32.dll")]
    internal static partial void PostQuitMessage(int nExitCode);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetForegroundWindow(IntPtr hWnd);

    [LibraryImport("dwmapi.dll")]
    internal static partial int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [LibraryImport("dwmapi.dll")]
    internal static partial int DwmSetWindowAttribute(IntPtr hwnd, uint attr, ref int pvAttribute, uint cbAttribute);

    internal const uint DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    internal const int SW_HIDE = 0;
    internal const int SW_SHOW = 5;
}
