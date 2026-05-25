using Microsoft.Extensions.Logging;
using OverlayForge.Core.Interfaces;
using OverlayForge.Core.Models;
using OverlayForge.OverlayEngine.Windows;

namespace OverlayForge.OverlayEngine.Services;

/// <summary>
/// Core overlay manager: creates, tracks, and controls all overlay windows.
/// Must run on the WPF UI thread.
/// </summary>
public sealed class OverlayManager : IOverlayManager
{
    private readonly ILogger<OverlayManager> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly List<OverlayModel> _overlays = new();
    private readonly Dictionary<Guid, OverlayWindow> _windows = new();

    public IReadOnlyList<OverlayModel> Overlays => _overlays.AsReadOnly();
    public Guid? ActiveOverlayId { get; set; }

    public event EventHandler<OverlayChangedEventArgs>? OverlayChanged;

    public OverlayManager(ILogger<OverlayManager> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public OverlayModel CreateOverlay(string? imagePath = null)
    {
        var model = new OverlayModel
        {
            Name = $"Overlay {_overlays.Count + 1}",
            ImagePath = imagePath,
            ZOrder = _overlays.Count,
            X = 100 + _overlays.Count * 20,
            Y = 100 + _overlays.Count * 20,
            Width = 0,
            Height = 0,
            Opacity = 1.0,
            IsVisible = true,
        };

        _overlays.Add(model);
        CreateWindow(model);
        ActiveOverlayId = model.Id;

        _logger.LogInformation("Created overlay '{Name}' ({Id})", model.Name, model.Id);
        OverlayChanged?.Invoke(this, new OverlayChangedEventArgs(OverlayChangeType.Added, model));
        return model;
    }

    public bool RemoveOverlay(Guid id)
    {
        var model = _overlays.FirstOrDefault(o => o.Id == id);
        if (model is null) return false;

        if (_windows.TryGetValue(id, out var window))
        {
            window.Close();
            _windows.Remove(id);
        }

        _overlays.Remove(model);

        if (ActiveOverlayId == id)
            ActiveOverlayId = _overlays.LastOrDefault()?.Id;

        _logger.LogInformation("Removed overlay '{Name}' ({Id})", model.Name, id);
        OverlayChanged?.Invoke(this, new OverlayChangedEventArgs(OverlayChangeType.Removed, model));
        return true;
    }

    public OverlayModel? DuplicateOverlay(Guid id)
    {
        var original = _overlays.FirstOrDefault(o => o.Id == id);
        if (original is null) return null;

        var clone = original.Clone();
        _overlays.Add(clone);
        CreateWindow(clone);
        ActiveOverlayId = clone.Id;

        _logger.LogInformation("Duplicated overlay '{Name}' as '{Clone}'", original.Name, clone.Name);
        OverlayChanged?.Invoke(this, new OverlayChangedEventArgs(OverlayChangeType.Added, clone));
        return clone;
    }

    public OverlayModel? GetOverlay(Guid id) =>
        _overlays.FirstOrDefault(o => o.Id == id);

    public void UpdateOverlay(OverlayModel overlay)
    {
        var index = _overlays.FindIndex(o => o.Id == overlay.Id);
        if (index < 0) return;

        overlay.ModifiedAt = DateTime.UtcNow;
        _overlays[index] = overlay;

        if (_windows.TryGetValue(overlay.Id, out var window))
            window.UpdateModel(overlay);

        OverlayChanged?.Invoke(this, new OverlayChangedEventArgs(OverlayChangeType.Updated, overlay));
    }

    public void BringToFront(Guid id)
    {
        var model = _overlays.FirstOrDefault(o => o.Id == id);
        if (model is null) return;

        int maxZ = _overlays.Max(o => o.ZOrder);
        model.ZOrder = maxZ + 1;
        UpdateOverlay(model);
    }

    public void SendToBack(Guid id)
    {
        var model = _overlays.FirstOrDefault(o => o.Id == id);
        if (model is null) return;

        int minZ = _overlays.Min(o => o.ZOrder);
        model.ZOrder = minZ - 1;
        UpdateOverlay(model);
    }

    public void ToggleVisibility(Guid id)
    {
        var model = _overlays.FirstOrDefault(o => o.Id == id);
        if (model is null) return;

        model.IsVisible = !model.IsVisible;
        UpdateOverlay(model);
        _logger.LogDebug("Overlay {Id} visibility: {Visible}", id, model.IsVisible);
    }

    public void ToggleClickThrough(Guid id)
    {
        var model = _overlays.FirstOrDefault(o => o.Id == id);
        if (model is null) return;

        model.IsClickThrough = !model.IsClickThrough;
        UpdateOverlay(model);
        _logger.LogDebug("Overlay {Id} click-through: {CT}", id, model.IsClickThrough);
    }

    public void ToggleLock(Guid id)
    {
        var model = _overlays.FirstOrDefault(o => o.Id == id);
        if (model is null) return;

        model.IsLocked = !model.IsLocked;
        UpdateOverlay(model);
        _logger.LogDebug("Overlay {Id} locked: {Locked}", id, model.IsLocked);
    }

    public void ShowAll()
    {
        foreach (var overlay in Overlays.ToList())
        {
            overlay.IsVisible = true;
            UpdateOverlay(overlay);
        }
    }

    public void HideAll()
    {
        foreach (var overlay in Overlays.ToList())
        {
            overlay.IsVisible = false;
            UpdateOverlay(overlay);
        }
    }

    public void SetOpacity(Guid id, double opacity)
    {
        var model = _overlays.FirstOrDefault(o => o.Id == id);
        if (model is null) return;

        model.Opacity = Math.Clamp(opacity, 0.0, 1.0);
        UpdateOverlay(model);
    }

    public void AdjustOpacity(Guid id, double delta)
    {
        var model = _overlays.FirstOrDefault(o => o.Id == id);
        if (model is null) return;

        SetOpacity(id, model.Opacity + delta);
    }

    /// <summary>
    /// Restores all overlays from a preset.
    /// </summary>
    public void LoadFromPreset(IEnumerable<OverlayModel> presetOverlays)
    {
        // Close all existing windows
        foreach (var model in presetOverlays)
        {
           var newOverlay = model.Clone();
            newOverlay.Id = Guid.NewGuid(); // Ensure unique ID for each overlay

            newOverlay.Name = $"Overlay {_overlays.Count + 1} (Preset)";
            
            _overlays.Add(newOverlay);
            CreateWindow(newOverlay);
            OverlayChanged?.Invoke(this, new OverlayChangedEventArgs(OverlayChangeType.Added, newOverlay));           
        }
        _logger.LogInformation("Loaded overlays from preset. Total active {Count}:", _overlays.Count);
    }

    public void AppendPreset(IEnumerable<OverlayModel> presetOverlays)
    {
        foreach (var model in presetOverlays)
        {
            model.Id = Guid.NewGuid(); // Ensure unique ID for each overlay
            _overlays.Add(model);
            CreateWindow(model);
            OverlayChanged?.Invoke(this, new OverlayChangedEventArgs(OverlayChangeType.Added, model));
        }
        _logger.LogInformation("Appended {Count} overlays from preset.", presetOverlays.Count());
    }

    private void CreateWindow(OverlayModel model)
    {
        var windowLogger = _loggerFactory.CreateLogger<OverlayWindow>();
        var window = new OverlayWindow(model, windowLogger);

        window.ModelUpdated += (_, updated) =>
        {
            var idx = _overlays.FindIndex(o => o.Id == updated.Id);
            if (idx >= 0) _overlays[idx] = updated;
            OverlayChanged?.Invoke(this, new OverlayChangedEventArgs(OverlayChangeType.Updated, updated));
        };

        window.CloseRequested += (_, _) => RemoveOverlay(model.Id);

        _windows[model.Id] = window;
        window.Show();
    }
}
