using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using OverlayForge.Core.Interfaces;
using OverlayForge.Core.Models;

namespace OverlayForge.UI.ViewModels;

/// <summary>
/// Main window ViewModel — coordinates overlay list, hotkeys, presets.
/// </summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly IOverlayManager _overlayManager;
    private readonly IPresetService _presetService;
    private readonly ISettingsService _settingsService;
    private readonly IHotkeyService _hotkeyService;
    private readonly ILogger<MainViewModel> _logger;

    public OverlayListViewModel OverlayList { get; }
    public ObservableCollection<OverlayPreset> Presets { get; } = new();

    [ObservableProperty] private string _statusText = "Ready";
    [ObservableProperty] private bool _isPresetLoading;
    [ObservableProperty] private OverlayPreset? _selectedPreset;

    public MainViewModel(
        IOverlayManager overlayManager,
        IPresetService presetService,
        ISettingsService settingsService,
        IHotkeyService hotkeyService,
        OverlayListViewModel overlayList,
        ILogger<MainViewModel> logger)
    {
        _overlayManager = overlayManager;
        _presetService = presetService;
        _settingsService = settingsService;
        _hotkeyService = hotkeyService;
        _logger = logger;
        OverlayList = overlayList;

        _hotkeyService.HotkeyTriggered += OnHotkeyTriggered;
        _overlayManager.OverlayChanged += (_, _) => UpdateStatus();
    }

    public async Task InitializeAsync()
    {
        await _presetService.LoadPresetsAsync();
        RefreshPresets();

        if (_settingsService.Settings.AutoLoadLastPreset && _settingsService.Settings.LastPresetId.HasValue)
        {
            await LoadPresetByIdAsync(_settingsService.Settings.LastPresetId.Value);
        }
    }

    private void RefreshPresets()
    {
        Presets.Clear();
        foreach (var preset in _presetService.Presets)
            Presets.Add(preset);
    }

    [RelayCommand]
    private async Task SavePresetAsync()
    {
        var name = $"Preset {DateTime.Now:yyyy-MM-dd HH:mm}";
        var preset = await _presetService.SavePresetAsync(name, _overlayManager.Overlays);
        RefreshPresets();
        StatusText = $"Saved preset: {preset.Name}";
        _logger.LogInformation("Saved preset '{Name}'", preset.Name);
    }

    [RelayCommand]
    private async Task LoadSelectedPresetAsync()
    {
        if (SelectedPreset is null) return;
        await LoadPresetByIdAsync(SelectedPreset.Id);
    }

    [RelayCommand]
    private async Task DeleteSelectedPresetAsync()
    {
        if (SelectedPreset is null) return;
        await _presetService.DeletePresetAsync(SelectedPreset.Id);
        RefreshPresets();
        StatusText = "Preset deleted.";
    }

    [RelayCommand]
    private async Task ExportPresetAsync()
    {
        if (SelectedPreset is null) return;

        var dialog = new SaveFileDialog
        {
            Title = "Export Preset",
            Filter = "OverlayForge Preset (*.ofpreset)|*.ofpreset|JSON (*.json)|*.json",
            FileName = $"{SelectedPreset.Name}.ofpreset"
        };

        if (dialog.ShowDialog() == true)
        {
            await _presetService.ExportPresetAsync(SelectedPreset.Id, dialog.FileName);
            StatusText = $"Exported: {dialog.FileName}";
        }
    }

    [RelayCommand]
    private async Task ImportPresetAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Import Preset",
            Filter = "OverlayForge Preset (*.ofpreset;*.json)|*.ofpreset;*.json|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            var preset = await _presetService.ImportPresetAsync(dialog.FileName);
            if (preset is not null)
            {
                RefreshPresets();
                StatusText = $"Imported: {preset.Name}";
            }
        }
    }

    private async Task LoadPresetByIdAsync(Guid presetId)
    {
        IsPresetLoading = true;
        try
        {
            var preset = await _presetService.LoadPresetAsync(presetId);
            if (preset is not null)
            {
                if (_overlayManager is OverlayForge.OverlayEngine.Services.OverlayManager engine)
                    engine.LoadFromPreset(preset.Overlays);

                _settingsService.Settings.LastPresetId = preset.Id;
                await _settingsService.SaveAsync();
                StatusText = $"Loaded preset: {preset.Name}";
            }
        }
        finally
        {
            IsPresetLoading = false;
        }
    }

    private void OnHotkeyTriggered(object? sender, Core.Interfaces.HotkeyTriggeredEventArgs e)
    {
        var activeId = _overlayManager.ActiveOverlayId;

        switch (e.Action)
        {
            case Core.Enums.HotkeyAction.OpacityUp when activeId.HasValue:
                _overlayManager.AdjustOpacity(activeId.Value, _settingsService.Settings.OpacityStep);
                break;

            case Core.Enums.HotkeyAction.OpacityDown when activeId.HasValue:
                _overlayManager.AdjustOpacity(activeId.Value, -_settingsService.Settings.OpacityStep);
                break;

            case Core.Enums.HotkeyAction.ToggleClickThrough when activeId.HasValue:
                _overlayManager.ToggleClickThrough(activeId.Value);
                break;

            case Core.Enums.HotkeyAction.ToggleVisibility when activeId.HasValue:
                _overlayManager.ToggleVisibility(activeId.Value);
                break;

            case Core.Enums.HotkeyAction.ToggleLock when activeId.HasValue:
                _overlayManager.ToggleLock(activeId.Value);
                break;

            case Core.Enums.HotkeyAction.ShowAll:
                _overlayManager.ShowAll();
                break;

            case Core.Enums.HotkeyAction.HideAll:
                _overlayManager.HideAll();
                break;
        }

        UpdateStatus();
    }

    private void UpdateStatus()
    {
        StatusText = $"Overlays: {_overlayManager.Overlays.Count} active";
    }
}
