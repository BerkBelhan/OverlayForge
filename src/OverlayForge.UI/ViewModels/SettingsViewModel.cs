using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using OverlayForge.Core.Enums;
using OverlayForge.Core.Interfaces;
using OverlayForge.Core.Models;

namespace OverlayForge.UI.ViewModels;

/// <summary>
/// ViewModel for the settings window.
/// </summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IHotkeyService _hotkeyService;
    private readonly ILogger<SettingsViewModel> _logger;

    public ObservableCollection<HotkeyBindingViewModel> HotkeyBindings { get; } = new();

    [ObservableProperty] private double _opacityStep;
    [ObservableProperty] private bool _snapToEdges;
    [ObservableProperty] private bool _snapToCenterLines;
    [ObservableProperty] private bool _snapToOtherOverlays;
    [ObservableProperty] private int _snapDistance;
    [ObservableProperty] private bool _minimizeToTray;
    [ObservableProperty] private bool _startMinimized;
    [ObservableProperty] private bool _autoLoadLastPreset;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public SettingsViewModel(
        ISettingsService settingsService,
        IHotkeyService hotkeyService,
        ILogger<SettingsViewModel> logger)
    {
        _settingsService = settingsService;
        _hotkeyService = hotkeyService;
        _logger = logger;

        LoadFromSettings();
    }

    private void LoadFromSettings()
    {
        var s = _settingsService.Settings;
        OpacityStep = s.OpacityStep;
        SnapToEdges = s.SnapToEdges;
        SnapToCenterLines = s.SnapToCenterLines;
        SnapToOtherOverlays = s.SnapToOtherOverlays;
        SnapDistance = s.SnapDistance;
        MinimizeToTray = s.MinimizeToTray;
        StartMinimized = s.StartMinimized;
        AutoLoadLastPreset = s.AutoLoadLastPreset;

        HotkeyBindings.Clear();
        foreach (var binding in s.HotkeyBindings)
            HotkeyBindings.Add(new HotkeyBindingViewModel(binding));
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        var s = _settingsService.Settings;
        s.OpacityStep = OpacityStep;
        s.SnapToEdges = SnapToEdges;
        s.SnapToCenterLines = SnapToCenterLines;
        s.SnapToOtherOverlays = SnapToOtherOverlays;
        s.SnapDistance = SnapDistance;
        s.MinimizeToTray = MinimizeToTray;
        s.StartMinimized = StartMinimized;
        s.AutoLoadLastPreset = AutoLoadLastPreset;
        s.HotkeyBindings = HotkeyBindings.Select(vm => vm.ToModel()).ToList();

        await _settingsService.SaveAsync();

        // Re-register hotkeys
        _hotkeyService.UnregisterAll();
        _hotkeyService.RegisterHotkeys(s.HotkeyBindings);

        StatusMessage = "Settings saved successfully.";
        _logger.LogInformation("Settings saved.");
    }

    [RelayCommand]
    private async Task ResetToDefaultsAsync()
    {
        await _settingsService.ResetToDefaultsAsync();
        LoadFromSettings();
        StatusMessage = "Settings reset to defaults.";
    }
}

/// <summary>
/// ViewModel for a single hotkey binding in the settings UI.
/// </summary>
public sealed partial class HotkeyBindingViewModel : ObservableObject
{
    [ObservableProperty] private HotkeyAction _action;
    [ObservableProperty] private HotkeyModifiers _modifiers;
    [ObservableProperty] private int _keyCode;
    [ObservableProperty] private string _keyName = string.Empty;
    [ObservableProperty] private bool _isEnabled;

    public HotkeyBindingViewModel(HotkeyBinding model)
    {
        Action = model.Action;
        Modifiers = model.Modifiers;
        KeyCode = model.KeyCode;
        KeyName = model.KeyName ?? string.Empty;
        IsEnabled = model.IsEnabled;
    }

    public HotkeyBinding ToModel() => new()
    {
        Action = Action,
        Modifiers = Modifiers,
        KeyCode = KeyCode,
        KeyName = KeyName,
        IsEnabled = IsEnabled
    };

    public string DisplayString => ToModel().DisplayString;
    public string ActionLabel => Action.ToString();
}
