using FluentAssertions;
using OverlayForge.Core.Enums;
using OverlayForge.Core.Models;
using Xunit;

namespace OverlayForge.Core.Tests;

/// <summary>
/// Tests for the HotkeyBinding model.
/// </summary>
public sealed class HotkeyBindingTests
{
    [Fact]
    public void DisplayString_WithCtrlAlt_FormatsCorrectly()
    {
        var binding = new HotkeyBinding
        {
            Action = HotkeyAction.ToggleVisibility,
            Modifiers = HotkeyModifiers.Control | HotkeyModifiers.Alt,
            KeyCode = 0x48,
            KeyName = "H"
        };

        binding.DisplayString.Should().Be("Ctrl+Alt+H");
    }

    [Fact]
    public void DisplayString_WithAllModifiers_FormatsCorrectly()
    {
        var binding = new HotkeyBinding
        {
            Modifiers = HotkeyModifiers.Control | HotkeyModifiers.Alt | HotkeyModifiers.Shift | HotkeyModifiers.Win,
            KeyCode = 0x41,
            KeyName = "A"
        };

        binding.DisplayString.Should().Be("Ctrl+Alt+Shift+Win+A");
    }

    [Fact]
    public void DisplayString_WithNoModifiers_ShowsKeyOnly()
    {
        var binding = new HotkeyBinding
        {
            Modifiers = HotkeyModifiers.None,
            KeyCode = 0x70,
            KeyName = "F1"
        };

        binding.DisplayString.Should().Be("F1");
    }

    [Fact]
    public void HotkeyModifiers_FlagsCombineProperly()
    {
        var modifiers = HotkeyModifiers.Control | HotkeyModifiers.Alt;

        modifiers.HasFlag(HotkeyModifiers.Control).Should().BeTrue();
        modifiers.HasFlag(HotkeyModifiers.Alt).Should().BeTrue();
        modifiers.HasFlag(HotkeyModifiers.Shift).Should().BeFalse();
    }

    [Fact]
    public void NewBinding_HasUniqueId()
    {
        var binding1 = new HotkeyBinding();
        var binding2 = new HotkeyBinding();

        binding1.Id.Should().NotBe(binding2.Id);
    }

    [Fact]
    public void NewBinding_IsEnabledByDefault()
    {
        var binding = new HotkeyBinding();
        binding.IsEnabled.Should().BeTrue();
    }
}
