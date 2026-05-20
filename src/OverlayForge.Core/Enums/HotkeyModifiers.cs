namespace OverlayForge.Core.Enums;

/// <summary>
/// Modifier keys used in hotkey combinations.
/// </summary>
[Flags]
public enum HotkeyModifiers
{
    /// <summary>No modifier.</summary>
    None = 0,

    /// <summary>Alt key.</summary>
    Alt = 0x0001,

    /// <summary>Control key.</summary>
    Control = 0x0002,

    /// <summary>Shift key.</summary>
    Shift = 0x0004,

    /// <summary>Windows key.</summary>
    Win = 0x0008
}
