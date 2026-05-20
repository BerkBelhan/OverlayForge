using OverlayForge.Core.Models;

namespace OverlayForge.Core.Interfaces;

/// <summary>
/// Service for loading, validating and managing application settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>Current application settings.</summary>
    AppSettings Settings { get; }

    /// <summary>Loads settings from storage.</summary>
    Task LoadAsync();

    /// <summary>Saves settings to storage.</summary>
    Task SaveAsync();

    /// <summary>Resets settings to defaults.</summary>
    Task ResetToDefaultsAsync();

    /// <summary>Adds a path to the recent images list.</summary>
    void AddRecentImage(string path);

    /// <summary>Raised when settings are changed.</summary>
    event EventHandler? SettingsChanged;
}
