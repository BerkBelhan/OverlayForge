namespace OverlayForge.Core.Enums;

/// <summary>
/// Represents the current state of an overlay window.
/// </summary>
public enum OverlayState
{
    /// <summary>Overlay is visible and interactive.</summary>
    Active,

    /// <summary>Overlay is hidden from view.</summary>
    Hidden,

    /// <summary>Overlay is visible but locked (cannot be moved/resized).</summary>
    Locked,

    /// <summary>Overlay is in click-through mode (mouse passes through).</summary>
    ClickThrough
}
