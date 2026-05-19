using OverlayForge.Core.Enums;

namespace OverlayForge.Core.Models;

/// <summary>
/// Represents a single overlay instance with all its properties.
/// </summary>
public class OverlayModel
{
    /// <summary>Unique identifier for this overlay.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Display name for this overlay.</summary>
    public string Name { get; set; } = "Overlay";

    /// <summary>Full path to the image file.</summary>
    public string? ImagePath { get; set; }

    /// <summary>Current state of the overlay.</summary>
    public OverlayState State { get; set; } = OverlayState.Active;

    /// <summary>Opacity from 0.0 (transparent) to 1.0 (opaque).</summary>
    public double Opacity { get; set; } = 0.5;

    /// <summary>X position on screen in pixels.</summary>
    public double X { get; set; }

    /// <summary>Y position on screen in pixels.</summary>
    public double Y { get; set; }

    /// <summary>Width of the overlay window in pixels.</summary>
    public double Width { get; set; } = 400;

    /// <summary>Height of the overlay window in pixels.</summary>
    public double Height { get; set; } = 300;

    /// <summary>Z-order index (higher = on top).</summary>
    public int ZOrder { get; set; }

    /// <summary>Whether the overlay is visible.</summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>Whether the overlay is locked (cannot be moved/resized).</summary>
    public bool IsLocked { get; set; }

    /// <summary>Whether mouse events pass through the overlay.</summary>
    public bool IsClickThrough { get; set; }

    /// <summary>Whether to maintain the image's aspect ratio on resize.</summary>
    public bool MaintainAspectRatio { get; set; } = true;

    /// <summary>Rotation angle in degrees (0-360).</summary>
    public double Rotation { get; set; }

    /// <summary>Horizontal flip.</summary>
    public bool FlipHorizontal { get; set; }

    /// <summary>Vertical flip.</summary>
    public bool FlipVertical { get; set; }

    /// <summary>Monitor index this overlay is on (0-based).</summary>
    public int MonitorIndex { get; set; }

    /// <summary>Creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Last modification timestamp.</summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Creates a deep copy of this overlay model.</summary>
    public OverlayModel Clone()
    {
        return new OverlayModel
        {
            Id = Guid.NewGuid(),
            Name = $"{Name} (Copy)",
            ImagePath = ImagePath,
            State = State,
            Opacity = Opacity,
            X = X + 20,
            Y = Y + 20,
            Width = Width,
            Height = Height,
            ZOrder = ZOrder + 1,
            IsVisible = IsVisible,
            IsLocked = IsLocked,
            IsClickThrough = IsClickThrough,
            MaintainAspectRatio = MaintainAspectRatio,
            Rotation = Rotation,
            FlipHorizontal = FlipHorizontal,
            FlipVertical = FlipVertical,
            MonitorIndex = MonitorIndex,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
    }
}
