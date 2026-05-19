namespace OverlayForge.Core.Enums;

/// <summary>
/// Snap alignment targets for overlay positioning.
/// </summary>
public enum SnapTarget
{
    /// <summary>No snapping.</summary>
    None,

    /// <summary>Snap to screen edges.</summary>
    ScreenEdges,

    /// <summary>Snap to screen center lines.</summary>
    CenterLines,

    /// <summary>Snap to other overlay boundaries.</summary>
    OtherOverlays
}
