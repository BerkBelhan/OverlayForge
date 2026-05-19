using System.Runtime.InteropServices;

namespace OverlayForge.Win32.Interop;

/// <summary>
/// Monitor information for multi-monitor support.
/// </summary>
internal static partial class MonitorHelper
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left, Top, Right, Bottom;
        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MONITORINFO
    {
        public uint cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    internal delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool EnumDisplayMonitors(
        IntPtr hdc,
        IntPtr lprcClip,
        MonitorEnumProc lpfnEnum,
        IntPtr dwData);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [LibraryImport("user32.dll")]
    internal static partial IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    internal const uint MONITOR_DEFAULTTONEAREST = 2;

    /// <summary>
    /// Gets all monitor working areas.
    /// </summary>
    internal static List<System.Windows.Rect> GetAllMonitorWorkAreas()
    {
        var areas = new List<System.Windows.Rect>();

        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (hMon, _, ref rect, _) =>
        {
            var info = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };
            if (GetMonitorInfo(hMon, ref info))
            {
                var work = info.rcWork;
                areas.Add(new System.Windows.Rect(work.Left, work.Top, work.Width, work.Height));
            }
            return true;
        }, IntPtr.Zero);

        return areas;
    }
}
