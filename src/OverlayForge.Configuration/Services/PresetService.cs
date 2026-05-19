using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using OverlayForge.Core.Interfaces;
using OverlayForge.Core.Models;

namespace OverlayForge.Configuration.Services;

/// <summary>
/// Manages overlay presets with JSON persistence.
/// </summary>
public sealed class PresetService : IPresetService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<PresetService> _logger;
    private readonly string _presetsDirectory;
    private readonly List<OverlayPreset> _presets = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public IReadOnlyList<OverlayPreset> Presets => _presets.AsReadOnly();

    public PresetService(IFileSystem fileSystem, ILogger<PresetService> logger, string? presetsDirectory = null)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _presetsDirectory = presetsDirectory
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "OverlayForge",
                "presets");
    }

    public async Task LoadPresetsAsync()
    {
        _presets.Clear();

        if (!_fileSystem.DirectoryExists(_presetsDirectory))
        {
            _fileSystem.CreateDirectory(_presetsDirectory);
            _logger.LogInformation("Created presets directory at {Path}", _presetsDirectory);
            return;
        }

        foreach (var file in _fileSystem.GetFiles(_presetsDirectory, "*.json"))
        {
            try
            {
                var json = await _fileSystem.ReadAllTextAsync(file);
                var preset = JsonSerializer.Deserialize<OverlayPreset>(json, JsonOptions);
                if (preset is not null)
                    _presets.Add(preset);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load preset from {File}", file);
            }
        }

        _logger.LogInformation("Loaded {Count} presets.", _presets.Count);
    }

    public async Task<OverlayPreset> SavePresetAsync(string name, IEnumerable<OverlayModel> overlays, string? description = null)
    {
        var preset = new OverlayPreset
        {
            Name = name,
            Description = description,
            Overlays = overlays.ToList(),
            ModifiedAt = DateTime.UtcNow
        };

        await PersistPresetAsync(preset);

        var existing = _presets.FindIndex(p => p.Id == preset.Id);
        if (existing >= 0)
            _presets[existing] = preset;
        else
            _presets.Add(preset);

        _logger.LogInformation("Saved preset '{Name}' with {Count} overlays.", name, preset.Overlays.Count);
        return preset;
    }

    public async Task<OverlayPreset?> LoadPresetAsync(Guid presetId)
    {
        var preset = _presets.FirstOrDefault(p => p.Id == presetId);
        if (preset is not null) return preset;

        var filePath = GetPresetFilePath(presetId);
        if (!_fileSystem.FileExists(filePath)) return null;

        try
        {
            var json = await _fileSystem.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<OverlayPreset>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load preset {Id}", presetId);
            return null;
        }
    }

    public async Task<bool> DeletePresetAsync(Guid presetId)
    {
        var preset = _presets.FirstOrDefault(p => p.Id == presetId);
        if (preset is null) return false;

        _presets.Remove(preset);
        var filePath = GetPresetFilePath(presetId);

        if (_fileSystem.FileExists(filePath))
        {
            _fileSystem.DeleteFile(filePath);
            _logger.LogInformation("Deleted preset {Id}", presetId);
        }

        return await Task.FromResult(true);
    }

    public async Task ExportPresetAsync(Guid presetId, string filePath)
    {
        var preset = await LoadPresetAsync(presetId);
        if (preset is null) throw new InvalidOperationException($"Preset {presetId} not found.");

        var json = JsonSerializer.Serialize(preset, JsonOptions);
        await _fileSystem.WriteAllTextAsync(filePath, json);
        _logger.LogInformation("Exported preset {Id} to {Path}", presetId, filePath);
    }

    public async Task<OverlayPreset?> ImportPresetAsync(string filePath)
    {
        if (!_fileSystem.FileExists(filePath))
            throw new FileNotFoundException("Preset file not found.", filePath);

        try
        {
            var json = await _fileSystem.ReadAllTextAsync(filePath);
            var preset = JsonSerializer.Deserialize<OverlayPreset>(json, JsonOptions);
            if (preset is null) return null;

            // Assign new ID to avoid conflicts
            preset.Id = Guid.NewGuid();
            preset.ModifiedAt = DateTime.UtcNow;

            await PersistPresetAsync(preset);
            _presets.Add(preset);

            _logger.LogInformation("Imported preset '{Name}' from {Path}", preset.Name, filePath);
            return preset;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import preset from {Path}", filePath);
            return null;
        }
    }

    public OverlayPreset? FindAutoLoadPreset(string executableName)
    {
        return _presets.FirstOrDefault(p =>
            !string.IsNullOrEmpty(p.AutoLoadExecutable) &&
            string.Equals(p.AutoLoadExecutable, executableName, StringComparison.OrdinalIgnoreCase));
    }

    private async Task PersistPresetAsync(OverlayPreset preset)
    {
        if (!_fileSystem.DirectoryExists(_presetsDirectory))
            _fileSystem.CreateDirectory(_presetsDirectory);

        var filePath = GetPresetFilePath(preset.Id);
        var json = JsonSerializer.Serialize(preset, JsonOptions);
        await _fileSystem.WriteAllTextAsync(filePath, json);
    }

    private string GetPresetFilePath(Guid id) =>
        _fileSystem.Combine(_presetsDirectory, $"{id}.json");
}
