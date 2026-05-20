namespace OverlayForge.Core.Models;

/// <summary>
/// Represents a saved configuration preset containing multiple overlays.
/// </summary>
public class OverlayPreset
{
    /// <summary>Unique identifier for this preset.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Display name of the preset.</summary>
    public string Name { get; set; } = "New Preset";

    /// <summary>Optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Overlays included in this preset.</summary>
    public List<OverlayModel> Overlays { get; set; } = new();

    /// <summary>Executable name to auto-load this preset for (e.g., "game.exe").</summary>
    public string? AutoLoadExecutable { get; set; }

    /// <summary>Date the preset was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Date the preset was last modified.</summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Schema version for migration support.</summary>
    public string SchemaVersion { get; set; } = "1.0";
}
