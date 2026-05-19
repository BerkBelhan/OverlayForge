using OverlayForge.Core.Models;

namespace OverlayForge.Core.Interfaces;

/// <summary>
/// Service for managing overlay instances.
/// </summary>
public interface IOverlayManager
{
    /// <summary>Gets all active overlays.</summary>
    IReadOnlyList<OverlayModel> Overlays { get; }

    /// <summary>Raised when overlays collection changes.</summary>
    event EventHandler<OverlayChangedEventArgs>? OverlayChanged;

    /// <summary>Creates a new overlay with the given image path.</summary>
    OverlayModel CreateOverlay(string? imagePath = null);

    /// <summary>Removes an overlay by ID.</summary>
    bool RemoveOverlay(Guid id);

    /// <summary>Duplicates an overlay.</summary>
    OverlayModel? DuplicateOverlay(Guid id);

    /// <summary>Gets an overlay by ID.</summary>
    OverlayModel? GetOverlay(Guid id);

    /// <summary>Updates overlay properties.</summary>
    void UpdateOverlay(OverlayModel overlay);

    /// <summary>Brings overlay to front.</summary>
    void BringToFront(Guid id);

    /// <summary>Sends overlay to back.</summary>
    void SendToBack(Guid id);

    /// <summary>Toggles visibility of an overlay.</summary>
    void ToggleVisibility(Guid id);

    /// <summary>Toggles click-through mode of an overlay.</summary>
    void ToggleClickThrough(Guid id);

    /// <summary>Toggles lock state of an overlay.</summary>
    void ToggleLock(Guid id);

    /// <summary>Shows all overlays.</summary>
    void ShowAll();

    /// <summary>Hides all overlays.</summary>
    void HideAll();

    /// <summary>Sets opacity for an overlay.</summary>
    void SetOpacity(Guid id, double opacity);

    /// <summary>Adjusts opacity by delta for an overlay.</summary>
    void AdjustOpacity(Guid id, double delta);

    /// <summary>Gets the currently active/selected overlay ID.</summary>
    Guid? ActiveOverlayId { get; set; }
}

/// <summary>
/// Event arguments for overlay collection changes.
/// </summary>
public class OverlayChangedEventArgs : EventArgs
{
    public OverlayChangeType ChangeType { get; }
    public OverlayModel Overlay { get; }

    public OverlayChangedEventArgs(OverlayChangeType changeType, OverlayModel overlay)
    {
        ChangeType = changeType;
        Overlay = overlay;
    }
}

/// <summary>
/// Type of change to the overlay collection.
/// </summary>
public enum OverlayChangeType
{
    Added,
    Removed,
    Updated,
    Reordered
}
