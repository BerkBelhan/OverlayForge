using OverlayForge.Core.Enums;

namespace OverlayForge.Core.Models;

/// <summary>
/// Application-wide settings and configuration.
/// </summary>
public class AppSettings
{
    /// <summary>Schema version for migration support.</summary>
    public string SchemaVersion { get; set; } = "1.0";

    /// <summary>Global opacity modifier applied to all overlays.</summary>
    public double GlobalOpacityModifier { get; set; } = 1.0;

    /// <summary>Opacity step size for hotkey adjustments (0.01 to 0.5).</summary>
    public double OpacityStep { get; set; } = 0.05;

    /// <summary>Enable hardware acceleration for rendering.</summary>
    public bool UseHardwareAcceleration { get; set; } = true;

    /// <summary>Hotkey bindings list.</summary>
    public List<HotkeyBinding> HotkeyBindings { get; set; } = new();

    /// <summary>Recently opened image files (max 20).</summary>
    public List<string> RecentImages { get; set; } = new();

    /// <summary>Snap distance in pixels for overlay snapping.</summary>
    public int SnapDistance { get; set; } = 10;

    /// <summary>Enable snapping to screen edges.</summary>
    public bool SnapToEdges { get; set; } = true;

    /// <summary>Enable snapping to screen center lines.</summary>
    public bool SnapToCenterLines { get; set; } = true;

    /// <summary>Enable snapping to other overlays.</summary>
    public bool SnapToOtherOverlays { get; set; } = true;

    /// <summary>Show overlays in OBS/screen captures.</summary>
    public bool VisibleInCaptures { get; set; } = false;

    /// <summary>Startup minimized to system tray.</summary>
    public bool StartMinimized { get; set; } = false;

    /// <summary>Minimize to system tray instead of taskbar.</summary>
    public bool MinimizeToTray { get; set; } = true;

    /// <summary>Auto-load last used preset on startup.</summary>
    public bool AutoLoadLastPreset { get; set; } = true;

    /// <summary>ID of the last used preset.</summary>
    public Guid? LastPresetId { get; set; }

    /// <summary>Log level (Verbose, Debug, Information, Warning, Error, Fatal).</summary>
    public string LogLevel { get; set; } = "Information";
}
