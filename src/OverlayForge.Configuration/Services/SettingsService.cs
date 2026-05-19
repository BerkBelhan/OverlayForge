using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using OverlayForge.Core.Interfaces;
using OverlayForge.Core.Models;
using OverlayForge.Core.Enums;

namespace OverlayForge.Configuration.Services;

/// <summary>
/// Manages application settings persistence using JSON.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<SettingsService> _logger;
    private readonly string _settingsPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AppSettings Settings { get; private set; } = new();

    public event EventHandler? SettingsChanged;

    public SettingsService(IFileSystem fileSystem, ILogger<SettingsService> logger, string? settingsPath = null)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _settingsPath = settingsPath
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "OverlayForge",
                "settings.json");
    }

    public async Task LoadAsync()
    {
        try
        {
            if (!_fileSystem.FileExists(_settingsPath))
            {
                _logger.LogInformation("Settings file not found at {Path}, using defaults.", _settingsPath);
                Settings = CreateDefaultSettings();
                await SaveAsync();
                return;
            }

            var json = await _fileSystem.ReadAllTextAsync(_settingsPath);
            var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);

            if (loaded is null)
            {
                _logger.LogWarning("Failed to deserialize settings, using defaults.");
                Settings = CreateDefaultSettings();
                return;
            }

            Settings = MigrateSettings(loaded);
            _logger.LogInformation("Settings loaded from {Path}", _settingsPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings from {Path}. Using defaults.", _settingsPath);
            Settings = CreateDefaultSettings();
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            var dir = _fileSystem.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(dir) && !_fileSystem.DirectoryExists(dir))
                _fileSystem.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(Settings, JsonOptions);
            await _fileSystem.WriteAllTextAsync(_settingsPath, json);
            _logger.LogDebug("Settings saved to {Path}", _settingsPath);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings to {Path}", _settingsPath);
        }
    }

    public async Task ResetToDefaultsAsync()
    {
        Settings = CreateDefaultSettings();
        await SaveAsync();
        _logger.LogInformation("Settings reset to defaults.");
    }

    public void AddRecentImage(string path)
    {
        const int maxRecent = 20;
        Settings.RecentImages.Remove(path);
        Settings.RecentImages.Insert(0, path);
        if (Settings.RecentImages.Count > maxRecent)
            Settings.RecentImages.RemoveRange(maxRecent, Settings.RecentImages.Count - maxRecent);
    }

    private static AppSettings CreateDefaultSettings()
    {
        return new AppSettings
        {
            SchemaVersion = "1.0",
            OpacityStep = 0.05,
            UseHardwareAcceleration = true,
            SnapDistance = 10,
            SnapToEdges = true,
            SnapToCenterLines = true,
            SnapToOtherOverlays = true,
            HotkeyBindings = CreateDefaultHotkeys()
        };
    }

    private static List<HotkeyBinding> CreateDefaultHotkeys()
    {
        return new List<HotkeyBinding>
        {
            new() { Action = HotkeyAction.OpacityUp,          Modifiers = HotkeyModifiers.Control | HotkeyModifiers.Alt, KeyCode = 0x26, KeyName = "Up" },
            new() { Action = HotkeyAction.OpacityDown,        Modifiers = HotkeyModifiers.Control | HotkeyModifiers.Alt, KeyCode = 0x28, KeyName = "Down" },
            new() { Action = HotkeyAction.ToggleClickThrough, Modifiers = HotkeyModifiers.Control | HotkeyModifiers.Alt, KeyCode = 0x43, KeyName = "C" },
            new() { Action = HotkeyAction.ToggleVisibility,   Modifiers = HotkeyModifiers.Control | HotkeyModifiers.Alt, KeyCode = 0x48, KeyName = "H" },
            new() { Action = HotkeyAction.ToggleLock,         Modifiers = HotkeyModifiers.Control | HotkeyModifiers.Alt, KeyCode = 0x4C, KeyName = "L" },
        };
    }

    private static AppSettings MigrateSettings(AppSettings settings)
    {
        // Future migrations: check SchemaVersion and apply changes
        if (settings.SchemaVersion == "1.0") return settings;
        return settings;
    }
}
