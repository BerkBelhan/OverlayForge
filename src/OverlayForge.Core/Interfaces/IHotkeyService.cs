using OverlayForge.Core.Models;
using OverlayForge.Core.Enums;

namespace OverlayForge.Core.Interfaces;

/// <summary>
/// Service for managing global hotkeys.
/// </summary>
public interface IHotkeyService
{
    /// <summary>Raised when a registered hotkey is pressed.</summary>
    event EventHandler<HotkeyTriggeredEventArgs>? HotkeyTriggered;

    /// <summary>Registers all hotkeys from the provided bindings.</summary>
    void RegisterHotkeys(IEnumerable<HotkeyBinding> bindings);

    /// <summary>Unregisters all hotkeys.</summary>
    void UnregisterAll();

    /// <summary>Registers a single hotkey.</summary>
    bool RegisterHotkey(HotkeyBinding binding);

    /// <summary>Unregisters a single hotkey by action.</summary>
    bool UnregisterHotkey(HotkeyAction action);

    /// <summary>Checks if a hotkey combination is already registered.</summary>
    bool IsConflicting(HotkeyModifiers modifiers, int keyCode, HotkeyAction? excludeAction = null);

    /// <summary>Returns all currently registered bindings.</summary>
    IReadOnlyList<HotkeyBinding> RegisteredBindings { get; }
}

/// <summary>
/// Event arguments for hotkey triggers.
/// </summary>
public class HotkeyTriggeredEventArgs : EventArgs
{
    public HotkeyAction Action { get; }
    public HotkeyBinding Binding { get; }

    public HotkeyTriggeredEventArgs(HotkeyAction action, HotkeyBinding binding)
    {
        Action = action;
        Binding = binding;
    }
}
