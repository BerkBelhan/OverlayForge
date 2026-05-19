namespace OverlayForge.Core.Enums;

/// <summary>
/// Defines the action triggered by a hotkey.
/// </summary>
public enum HotkeyAction
{
    /// <summary>Increase opacity of active overlay.</summary>
    OpacityUp,

    /// <summary>Decrease opacity of active overlay.</summary>
    OpacityDown,

    /// <summary>Toggle click-through mode on active overlay.</summary>
    ToggleClickThrough,

    /// <summary>Toggle visibility of active overlay.</summary>
    ToggleVisibility,

    /// <summary>Lock or unlock active overlay.</summary>
    ToggleLock,

    /// <summary>Show all overlays.</summary>
    ShowAll,

    /// <summary>Hide all overlays.</summary>
    HideAll,

    /// <summary>Select next overlay.</summary>
    NextOverlay,

    /// <summary>Select previous overlay.</summary>
    PreviousOverlay
}
