using OverlayForge.Core.Enums;

namespace OverlayForge.Core.Models;

/// <summary>
/// Represents a global hotkey binding.
/// </summary>
public class HotkeyBinding
{
    /// <summary>Unique identifier for this binding.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Action to perform when hotkey is triggered.</summary>
    public HotkeyAction Action { get; set; }

    /// <summary>Modifier keys required.</summary>
    public HotkeyModifiers Modifiers { get; set; }

    /// <summary>Virtual key code (Windows VK_ constants).</summary>
    public int KeyCode { get; set; }

    /// <summary>Human-readable display string for the key combination.</summary>
    public string DisplayString => BuildDisplayString();

    /// <summary>Whether this hotkey binding is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    private string BuildDisplayString()
    {
        var parts = new List<string>();
        if (Modifiers.HasFlag(HotkeyModifiers.Control)) parts.Add("Ctrl");
        if (Modifiers.HasFlag(HotkeyModifiers.Alt)) parts.Add("Alt");
        if (Modifiers.HasFlag(HotkeyModifiers.Shift)) parts.Add("Shift");
        if (Modifiers.HasFlag(HotkeyModifiers.Win)) parts.Add("Win");
        parts.Add(KeyName ?? $"Key({KeyCode})");
        return string.Join("+", parts);
    }

    /// <summary>Human-readable name for the key (e.g., "Up", "A", "F1").</summary>
    public string? KeyName { get; set; }
}
