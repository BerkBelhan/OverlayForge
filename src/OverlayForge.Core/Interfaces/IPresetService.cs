using OverlayForge.Core.Models;

namespace OverlayForge.Core.Interfaces;

/// <summary>
/// Service for managing overlay presets.
/// </summary>
public interface IPresetService
{
    /// <summary>Gets all saved presets.</summary>
    IReadOnlyList<OverlayPreset> Presets { get; }

    /// <summary>Saves a new preset from current overlay state.</summary>
    Task<OverlayPreset> SavePresetAsync(string name, IEnumerable<OverlayModel> overlays, string? description = null);

    /// <summary>Loads a preset and returns its overlays.</summary>
    Task<OverlayPreset?> LoadPresetAsync(Guid presetId);

    /// <summary>Deletes a preset.</summary>
    Task<bool> DeletePresetAsync(Guid presetId);

    /// <summary>Exports a preset to a file path.</summary>
    Task ExportPresetAsync(Guid presetId, string filePath);

    /// <summary>Imports a preset from a file path.</summary>
    Task<OverlayPreset?> ImportPresetAsync(string filePath);

    /// <summary>Loads all presets from storage.</summary>
    Task LoadPresetsAsync();

    /// <summary>Finds preset to auto-load for a given executable name.</summary>
    OverlayPreset? FindAutoLoadPreset(string executableName);
}
